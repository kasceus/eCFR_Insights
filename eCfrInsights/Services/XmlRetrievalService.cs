using ecfrInsights.Data;
using ecfrInsights.Data.eCFRApi;
using ecfrInsights.Data.Entities;
using ecfrInsights.Xml;

using Microsoft.EntityFrameworkCore;

using System.Collections.Concurrent;
using System.Threading;

namespace ecfrInsights.Services;

public class XmlRetrievalService(IServiceScopeFactory scopeFactory, TaskProgressService taskProgressService)
{
    private readonly IServiceScopeFactory _ScopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly TaskProgressService _TaskProgressService = taskProgressService ?? throw new ArgumentNullException(nameof(taskProgressService));
    private static readonly SemaphoreSlim _sqliteGate = new SemaphoreSlim(4); // allow 4 tasks at a time
    /// <summary>
    /// Downloads and stores XML for all current titles.
    /// </summary>
    /// 
    public async Task GetCurrentTitleXMLData(TaskProgress progress, CancellationToken cancellationToken = default, int? totalPotential = null)
    {
        List<CfrTitle> titles = [];

        //create a new context from the provider to use in this task
        using var scope = _ScopeFactory.CreateScope();

        using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
        {
            titles = await _context.CfrTitles.AsNoTracking().ToListAsync(cancellationToken);
        }

        ConcurrentBag<string> documentHahses = [];
        int incomingPercentage = progress.ProgressPercentage;
        var steps = titles.Count;

        for (int i = 0; i < steps; i++)
        {
            var title = titles[i];
            try
            {
                documentHahses.Add(title.XmlDocumentHash);

                DateTime date = title.LatestIssueDate ?? DateTime.Now;
                
                await DownloadAndStoreTitleXmlAsync(title.Number, date, documentHahses, cancellationToken);

                // Calculate progress as percentage within this task
                double progressWithinTask = (double)(i + 1) / steps * 100;

                int finalPercent;
                if (totalPotential.HasValue)
                {
                    // Map progressWithinTask (0-100) to the allocated range (incomingPercentage to incomingPercentage+totalPotential)
                    finalPercent = incomingPercentage + (int)(progressWithinTask * totalPotential.Value / 100);
                }
                else
                {
                    // If no totalPotential, use progressWithinTask as-is
                    finalPercent = (int)progressWithinTask;
                }

                _TaskProgressService.UpdateProgress(progress.TaskId, finalPercent, $"Downloaded {i + 1}/{steps} current titles");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading XML for title {title.Number}: {ex.Message}");
            }
        }

        await UpdateSyncStatusAsync("TitleXML", DateTime.UtcNow, _ScopeFactory, cancellationToken);

    }

    private static async Task UpdateSyncStatusAsync(string name, DateTime utcNow, IServiceScopeFactory scopeFactory, CancellationToken cancellationToken)
    {
        //create a new context from the provider to use in this task
        using var scope = scopeFactory.CreateScope();
        using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var status = await _context.SyncStatuses.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
                if (status == null)
                {
                    status = new ecfrInsights.Data.Entities.SyncStatus { Name = name, LastSyncedUtc = utcNow };
                    _context.SyncStatuses.Add(status);
                }
                else
                {
                    status.LastSyncedUtc = utcNow;
                    _context.SyncStatuses.Update(status);
                }

                await _context.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"Error updating sync status for {name}: {ex.Message}");

            }
        }
    }
    public async Task GetHistoricalXmlData(TaskProgress? taskProgress = null, CancellationToken cancellationToken = default)
    {
        taskProgress ??= _TaskProgressService.StartTask("Historical XML Download");
        int currentProgressPercent = taskProgress.ProgressPercentage;//this may come in at higher than 0;
        int incomingPercentage = currentProgressPercent;
        try
        {
            List<CfrTitle> titles = [];
            Dictionary<int, DateTime> historicalDateDict = [];

            // Create a scope just for initial data retrieval, dispose before launching background tasks
            using (var scope = _ScopeFactory.CreateScope())
            using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
            {
                titles = await _context.CfrTitles.AsNoTracking().ToListAsync(cancellationToken);
                var historicalDates = await _context.Corrections.AsNoTracking()
                    .Select(c => new { c.Title, c.ErrorCorrected })
                    .Distinct()
                    .ToListAsync(cancellationToken);
                historicalDateDict = historicalDates
               .ToDictionary(x => x.Title, x => x.ErrorCorrected);
            }


            List<Task> tasks = [];
            ConcurrentBag<string> documentHahses = [];
            int totalTasks = 0;

            // Calculate total tasks
            foreach (var title in titles)
            {
                totalTasks += 1; // for firstDate
                totalTasks += historicalDateDict.Count;
            }

            int taskWeight = totalTasks > 0 ? (100 - incomingPercentage) / totalTasks : 0;

            int completedTasks = 0;
            object lockObj = new object();
            foreach (var title in titles)
            {
                documentHahses.Add(title.XmlDocumentHash);
                try
                {
                    var firstDate = new DateTime(2025, 1, 1);
                    var titleNumber = title.Number;  // Capture to avoid closure issues
                    //get all of these historical documents at once
                    Task first = DownloadAndStoreTitleHNistoryXmlAsync(titleNumber, firstDate, documentHahses, cancellationToken)
                        .ContinueWith(_ =>
                        {
                            lock (lockObj)
                            {
                                completedTasks++;
                                double taskWeightPercent = totalTasks > 0 ? (double)(100 - incomingPercentage) / totalTasks : 0;
                                int percentage = incomingPercentage + (int)(completedTasks * taskWeightPercent);
                                _TaskProgressService.UpdateProgress(taskProgress.TaskId, percentage, $"Downloaded {completedTasks}/{totalTasks} historical files for date {firstDate}");
                            }
                        });
                    tasks.Add(first);

                    List<DateTime> historicalDates = historicalDateDict.Where(e => e.Key == title.Number).Select(e => e.Value).Where(e => e > firstDate && e < title.LatestIssueDate).ToList();

                    foreach (var historicalDate in historicalDates)
                    {
                        var capturedDate = historicalDate;  // Capture to avoid closure issues
                        tasks.Add(Task.Run(async () =>
                        {
                            await _sqliteGate.WaitAsync(cancellationToken);
                            try
                            {
                                await DownloadAndStoreTitleHNistoryXmlAsync(titleNumber, capturedDate, documentHahses, cancellationToken);


                                lock (lockObj)
                                {
                                    completedTasks++;
                                    double taskWeightPercent = totalTasks > 0 ? (double)(100 - incomingPercentage) / totalTasks : 0;
                                    int percentage = incomingPercentage + (int)(completedTasks * taskWeightPercent);

                                    _TaskProgressService.UpdateProgress(taskProgress.TaskId, percentage, $"Downloaded {completedTasks}/{totalTasks} files for date {capturedDate}");
                                }
                            }
                            finally
                            {
                                _sqliteGate.Release();
                            }
                        }));

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error downloading XML for title {title.Number}: {ex.Message}");
                }
            }
            await Task.WhenAll(tasks);//wait for all retrievals to finish

            await UpdateSyncStatusAsync("HistoricalXML", DateTime.UtcNow, _ScopeFactory, cancellationToken);
            _TaskProgressService.CompleteTask(taskProgress.TaskId, "Historical XML download completed successfully");
        }
        catch (Exception ex)
        {
            _TaskProgressService.FailTask(taskProgress.TaskId, $"Historical XML download failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Downloads and stores XML for a specific title at a specific date.
    /// Parses the XML into CfrHierarchies and tracks document history.
    /// </summary>
    public async Task DownloadAndStoreTitleXmlAsync(int titleNumber, DateTime asOfDate, ConcurrentBag<string>? hashList = null, CancellationToken cancellationToken = default)
    {
        using var scope = _ScopeFactory.CreateScope();


        using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var client = httpClientFactory.CreateClient(nameof(XmlRetrievalService));
               

                var vs = new Endpoints.VersionerService();
                var url = vs.GetXMLForTitle(titleNumber, asOfDate);

                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return;

                var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse XML into document and references
                var (parsedDoc, parsedRefs, parsedHash, rawXml) = CfrXmlParser.Parse<CfrTitle, CfrHierarchy>(xmlContent, titleNumber, hashList);

                var nullRefs = parsedRefs.Where(e => e.CfrReferenceNumber == null).ToList();
                if (nullRefs.Any())
                {
                    Console.WriteLine("null");
                }
                // Load existing document
                CfrTitle? existing = await _context.CfrTitles
                    .Include(d => d.CfrReferences)
                    .FirstOrDefaultAsync(d => d.Number == titleNumber, cancellationToken);

                if (existing == null)
                {
                    foreach (var r in parsedRefs.Where(e => e.CfrReferenceNumber != null))
                        parsedDoc.CfrReferences.Add(r);
                    _context.CfrTitles.Add(parsedDoc);

                }
                else
                {
                    // Document exists - check if it has changed
                    if (existing.XmlDocumentHash != parsedHash)
                    {
                        _context.CfrHierarchies.RemoveRange(existing.CfrReferences);
                        existing.CfrReferences.Clear();
                        // Update document with new data
                        existing.Name = parsedDoc.Name;
                        existing.SectionCount = parsedDoc.SectionCount;
                        existing.XmlDocumentHash = parsedHash;
                        existing.LatestAmendedOn = parsedDoc.LatestAmendedOn;
                        existing.LatestIssueDate = parsedDoc.LatestIssueDate;
                        existing.UpToDateAsOf = parsedDoc.UpToDateAsOf;
                        foreach (var r in parsedRefs.Where(e => e.CfrReferenceNumber != null))
                            existing.CfrReferences.Add(r);
                        _context.CfrTitles.Update(existing);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

            }
            catch (InvalidOperationException existing)
            {
                //document is already hashed and the hash matches, so we can skip processing
                System.Diagnostics.Debug.WriteLine($"{existing.Message}");
                _context.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing XML for title {titleNumber} on {asOfDate:yyyy-MM-dd}: {ex.Message}");
                //stop tracking changes and rollback ef state
                _context.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

    }
    public async Task DownloadAndStoreTitleHNistoryXmlAsync(int titleNumber, DateTime asOfDate, ConcurrentBag<string>? hashList = null, CancellationToken cancellationToken = default)
    {
        using var scope = _ScopeFactory.CreateScope();


        using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var client = httpClientFactory.CreateClient(nameof(XmlRetrievalService));

                //check to see if the historical record already exists, if so, skip it
                CfrTitleHistory? existing = await _context.CfrTitleHistories
                 .Include(d => d.CfrHierarchyHistories)
                 .FirstOrDefaultAsync(d => d.Number == titleNumber, cancellationToken);
                if (existing != null && existing.LatestIssueDate <= asOfDate)
                {
                    //log that this one already is downloaded
                    System.Diagnostics.Debug.WriteLine($"Already have the record for {titleNumber} and date {asOfDate}");

                    return;
                }


                var vs = new Endpoints.VersionerService();
                var url = vs.GetXMLForTitle(titleNumber, asOfDate);

                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return;

                var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse XML into document and references
                var (parsedDoc, parsedRefs, parsedHash, rawXml) = CfrXmlParser.Parse<CfrTitleHistory, CfrHierarchyHistory>(xmlContent, titleNumber, hashList);

                var nullRefs = parsedRefs.Where(e => e.CfrReferenceNumber == null).ToList();
                if (nullRefs.Any())
                {
                    Console.WriteLine("null");
                }
                // Load existing document


                if (existing == null)
                {
                    foreach (var r in parsedRefs.Where(e => e.CfrReferenceNumber != null))
                        parsedDoc.CfrHierarchyHistories.Add(r);
                    _context.CfrTitleHistories.Add(parsedDoc);

                }
                else
                {
                    // Document exists - check if it has changed
                    if (existing.XmlDocumentHash != parsedHash)
                    {
                        _context.CfrHierarchyHistories.RemoveRange(existing.CfrHierarchyHistories);
                        existing.CfrHierarchyHistories.Clear();
                        // Update document with new data
                        existing.Name = parsedDoc.Name;
                        existing.SectionCount = parsedDoc.SectionCount;
                        existing.XmlDocumentHash = parsedHash;
                        existing.LatestAmendedOn = parsedDoc.LatestAmendedOn;
                        existing.LatestIssueDate = parsedDoc.LatestIssueDate;
                        existing.UpToDateAsOf = parsedDoc.UpToDateAsOf;
                        foreach (var r in parsedRefs.Where(e => e.CfrReferenceNumber != null))
                            existing.CfrHierarchyHistories.Add(r);
                        _context.CfrTitleHistories.Update(existing);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

            }
            catch (InvalidOperationException existing)
            {
                //document is already hashed and the hash matches, so we can skip processing
                System.Diagnostics.Debug.WriteLine($"{existing.Message}");
                _context.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing XML for title {titleNumber} on {asOfDate:yyyy-MM-dd}: {ex.Message}");
                //stop tracking changes and rollback ef state
                _context.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

    }
}
