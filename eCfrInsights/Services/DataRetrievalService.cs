using ecfrInsights.Data;
using ecfrInsights.Data.eCFRApi;
using ecfrInsights.Data.Entities;
using ecfrInsights.Xml;

using Microsoft.EntityFrameworkCore;

using System.Collections.Concurrent;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ecfrInsights.Services;

public class DataRetrievalService(IServiceScopeFactory scopeFactory, XmlRetrievalService xmlService, TaskProgressService taskProgressService, DataAnalyticsService _DataAnalysis)
{
    private readonly IServiceScopeFactory _ScopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly XmlRetrievalService _XmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
    private readonly TaskProgressService _TaskProgressService = taskProgressService ?? throw new ArgumentNullException(nameof(taskProgressService));

    public async Task SyncAllAsync(TaskProgress progress, CancellationToken cancellationToken = default)
    {
        try
        {

            await SyncTitlesAsync(cancellationToken);

            _TaskProgressService.UpdateProgress(progress.TaskId, 5, "Titles synced. Starting XML data retrieval.");
            // Step 2: Get current title XML data

            int totalPotential = 55;
            await _XmlService.GetCurrentTitleXMLData(progress, cancellationToken, totalPotential);
            // Step 3: Sync agencies and corrections in parallel
            await SyncAgenciesAsync(cancellationToken);
            await SyncCorrectionsAsync(cancellationToken);
            _TaskProgressService.UpdateProgress(progress.TaskId, 60, "Agencies and Corrections synced.");
            // Step 4: Get historical XML data
            await _XmlService.GetHistoricalXmlData(progress, cancellationToken);
            _TaskProgressService.UpdateProgress(progress.TaskId, 85, "Beginning Analytics Calculations.");

            await _DataAnalysis.CalculateAgencyStatistics(DateTime.Now);

            //this is not needed, but could be done, it takes time and is not critical for this project at this time
            //await _DataAnalysis.CalculateTitleComplexities(DateTime.Now);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SyncAllAsync: {ex.Message}");
            throw;
        }
    }




    public Task SyncAgenciesAsync(CancellationToken cancellationToken = default)
        => GetDataAsync<AgenciesRoot>(Endpoints.GetAllAgencies, "Agencies", cancellationToken);

    public Task SyncCorrectionsAsync(CancellationToken cancellationToken = default)
        => GetDataAsync<CorrectionsRoot>(Endpoints.GetCorrections, "Corrections", cancellationToken);

    public Task SyncTitlesAsync(CancellationToken cancellationToken = default)
        => GetDataAsync<TitlesRoot>(Endpoints.VersionerService.GetTitlesSummary, "Titles", cancellationToken);




    private async Task GetDataAsync<T>(string url, string syncName, CancellationToken cancellationToken)
    {
        using var scope = _ScopeFactory.CreateScope();

        var status = await GetSyncStatusAsync(syncName, scope.ServiceProvider, cancellationToken);
        if (status != null)
        {
            //if this sync was performed within the last 30 minutes, skip it
            if (status.LastSyncedUtc > DateTime.UtcNow.AddMinutes(-5))
            {
                System.Diagnostics.Debug.WriteLine($"Skipping sync for {syncName} as it was last synced at {status.LastSyncedUtc}");
                //   return;
            }
        }

        //get the httpclient factory for this service
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(DataRetrievalService));


        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        await ParseAndStoreDataAsync<T>(content, scope.ServiceProvider, cancellationToken);

        // update sync timestamp for this type
        await UpdateSyncStatusAsync(syncName, DateTime.UtcNow, scope.ServiceProvider, cancellationToken);
    }

    private static async Task ParseAndStoreDataAsync<T>(string data, IServiceProvider provider, CancellationToken cancellationToken)
    {
        //create a new context from the provider to use in this task
        using (var scope = provider.CreateScope())
        using (EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>())
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (typeof(T) == typeof(AgenciesRoot))
                {
                    var jsonData = JsonSerializer.Deserialize<AgenciesRoot>(data);
                    if (jsonData is null)
                    {
                        return;
                    }

                    var currentAgencies = await _context.Agencies.ToListAsync(cancellationToken);

                    List<CfrHierarchy> cfrHierarchies = await _context.CfrHierarchies.ToListAsync(cancellationToken);
                    List<Agency> AgenciesToAdd = [];
                    foreach (var agency in jsonData.Agencies)
                    {
                        var existing = currentAgencies.FirstOrDefault(ca => ca.Slug == agency.Slug);
                        //cast the agency to the entity type
                        var incoming = (Agency)agency;
                        List<CfrHierarchy> filtered = cfrHierarchies.Where(e => agency.CfrReferences.Select(d => d.Title).ToList().Contains(e.TitleNumber)).ToList();

                        incoming.MakeHierarchy(agency, filtered);
                        //update existing from incoming
                        if (existing != null)
                        {
                            existing.Name = incoming.Name;
                            existing.ShortName = incoming.ShortName;
                            existing.DisplayName = incoming.DisplayName;
                            existing.SortableName = incoming.SortableName;
                            existing.ParentSlug = incoming.ParentSlug;
                            _context.Agencies.Update(existing);
                        }
                        else
                        {
                            if (incoming.AgencyHierarchies.Count > 0)
                            {
                                AgenciesToAdd.Add(incoming);

                            }
                            else
                            {
                                //write this
                                System.Diagnostics.Debug.WriteLine($"Did not add the agency {incoming.DisplayName} because it did not match with a hierarchical record");
                            }
                        }
                    }
                    //check agency refs against existing cfr numbers and remove any that don't exist

                    List<Agency> agenciesToRemove = [];
                    AgenciesToAdd.ForEach(agency =>
                    {
                        if (agency.AgencyHierarchies == null )
                        {
                            agenciesToRemove.Add(agency);
                            System.Diagnostics.Debug.WriteLine($"Did not add the agency {agency.DisplayName} because it did not match with a hierarchical record");
                            return;
                        }
                        if (agency.AgencyHierarchies.Any(h => h.CfrReferenceNumber == null))
                        {
                            agenciesToRemove.Add(agency);
                            System.Diagnostics.Debug.WriteLine($"Did not add the agency {agency.DisplayName} because it did not match with a hierarchical record");
                        }
                        
                    });
                    if(agenciesToRemove.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found {agenciesToRemove.Count} agencies to remove because they did not match with a hierarchical record. These will not be added to the database.");
                        AgenciesToAdd = AgenciesToAdd.Except(agenciesToRemove).ToList();
                    }


                   //filter duplicate agencyhierarchical records and keep only one copy of each unique record

                    Dictionary<string,int> totalDuplicatesRemoved =new();
                    AgenciesToAdd.ForEach(agency =>
                    {
                        if (agency.AgencyHierarchies != null && agency.AgencyHierarchies.Count > 0)
                        {
                            var uniqueHierarchies = agency.AgencyHierarchies
                                .GroupBy(h => new { h.CfrReferenceNumber, h.Slug })
                                .Select(g => g.First())
                                .ToList();
                            var removed = agency.AgencyHierarchies.Count - uniqueHierarchies.Count;
                            if (removed != 0)
                            {
                                totalDuplicatesRemoved.Add(agency.Name, removed);
                            }
                            agency.AgencyHierarchies = uniqueHierarchies;
                        }
                    });

                    if (totalDuplicatesRemoved.Sum(e=>e.Value) > 0)
                    {
                        foreach(var item in totalDuplicatesRemoved)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found and removed {item.Value} duplicate agency hierarchical records from the {item.Key} agency.");

                        }
                    }

                    await _context.Agencies.AddRangeAsync(AgenciesToAdd);
                }
                else if (typeof(T) == typeof(CorrectionsRoot))
                {
                    var corrections = JsonSerializer.Deserialize<CorrectionsRoot?>(data);
                    if (corrections is null)
                    {
                        return;
                    }
                    //get last correction from database and insert any new corrections that are not in the database
                    var lastCorrectionNumber = await _context.Corrections.MaxAsync(c => (int?)c.Id, cancellationToken) ?? 0;
                    var newCorrections = corrections.Corrections
                        .Where(c => c.Id > lastCorrectionNumber).ToList();
                    //parse and store new corrections
                    List<Correction> correctionsToAdd = [];
                    foreach (var correction in newCorrections)
                    {
                        Correction c = (Correction)correction;
                        correctionsToAdd.Add(c);
                    }
                    _context.Corrections.AddRange(correctionsToAdd);


                }
                else if (typeof(T) == typeof(TitlesRoot) || typeof(T) == typeof(TitlesRoot))
                {
                    var titles = JsonSerializer.Deserialize<TitlesRoot>(data);

                    if (titles is null)
                    {
                        return;
                    }
                    //sync the titles by comparing the incoming titles with the existing titles in the database and updating or adding as necessary
                    var currentTitles = await _context.CfrTitles.ToListAsync(cancellationToken);

                    foreach (var title in titles.Titles)
                    {
                        var existing = currentTitles.FirstOrDefault(ct => ct.Number == title.Number);
                        var incoming = (CfrTitle)title;
                        if (existing != null)
                        {
                            existing.Number = incoming.Number;
                            existing.Name = incoming.Name;
                            existing.LatestIssueDate = incoming.LatestIssueDate;
                            existing.LatestAmendedOn = incoming.LatestAmendedOn;
                            existing.UpToDateAsOf = incoming.UpToDateAsOf;
                            existing.Reserved = incoming.Reserved;
                            existing.SectionCount = incoming.SectionCount;
                            existing.CfrReferences = incoming.CfrReferences;
                            _context.CfrTitles.Update(existing);
                        }
                        else
                        {
                            //convert the incoming data to a hash and save it
                            incoming.XmlDocumentHash = ComputeDocumentHash(incoming);
                            _context.CfrTitles.Add(incoming);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T)} is not supported for data parsing.");
                }

                await _context.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                //log exception
                System.Diagnostics.Debug.WriteLine($"Error parsing and storing data for type {typeof(T)}: {ex.Message}");
            }
        }

    }
    private static string ComputeDocumentHash<T>(T doc)
    {
        // Deterministic JSON settings
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Serialize the document
        var json = JsonSerializer.Serialize(doc, options);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Compute SHA-256
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(bytes);

        // Convert to hex string
        return Convert.ToHexString(hashBytes);
    }


    private static async Task<SyncStatus?> GetSyncStatusAsync(string name, IServiceProvider provider, CancellationToken cancellationToken)
    {
        SyncStatus? retValue = null;
        using var scope = provider.CreateScope();
        using EcfrContext _context = scope.ServiceProvider.GetRequiredService<EcfrContext>();
        try
        {
            retValue = await _context.SyncStatuses.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating sync status for {name}: {ex.Message}");
            return null;
        }
        return retValue;
    }
    private static async Task UpdateSyncStatusAsync(string name, DateTime utcNow, IServiceProvider provider, CancellationToken cancellationToken)
    {
        //create a new context from the provider to use in this task
        using (var scope = provider.CreateScope())
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

}