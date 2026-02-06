using ecfrInsights.Data;
using ecfrInsights.Data.Entities;
using ecfrInsights.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class SyncStatusModel(EcfrContext context,  DataRetrievalService _dataService, XmlRetrievalService _xmlService, TaskProgressService _progressService) : PageModel
{
    public IList<SyncStatus> Statuses { get; private set; } = new List<SyncStatus>();

    public async Task OnGetAsync()
    {
        Statuses = await context.SyncStatuses.OrderBy(s => s.Name).ToListAsync();
    }

    public IActionResult OnPostRunNow()
    {
        var progress = _progressService.StartTask("Full Sync (Titles, XML, Agencies, Corrections)");
        
        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunSyncWithProgress(progress));
        
        return Redirect($"/Sync?taskId={progress.TaskId}");
    }

    public IActionResult OnPostRunXml()
    {
        var progress = _progressService.StartTask("XML Sync");
        
        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunXmlSyncWithProgress(progress));
        
        return Redirect($"/Sync?taskId={progress.TaskId}");
    }
    public IActionResult OnPostRunXmlHistorical()
    {
        var progress = _progressService.StartTask("Historical XML Sync");

        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunHistoricalXmlSyncWithProgress(progress));

        return Redirect($"/Sync?taskId={progress.TaskId}");
    }
    public IActionResult OnPostRunTitles()
    {
        var progress = _progressService.StartTask("Titles Sync");
        
        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunTitlesSyncWithProgress(progress));
        
        return Redirect($"/Sync?taskId={progress.TaskId}");
    }
    public IActionResult OnPostRunCorrections()
    {
        var progress = _progressService.StartTask("Corrections Sync");

        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunCorrectionsSyncWithProgress(progress));

        return Redirect($"/Sync?taskId={progress.TaskId}");
    }

    private async Task RunCorrectionsSyncWithProgress(TaskProgress progress)
    {
        _progressService.UpdateProgress(progress.TaskId, 0, "Starting download of corrections data...");
        await _dataService.SyncCorrectionsAsync();
        _progressService.CompleteTask(progress.TaskId, "Sync completed successfully!");

    }
    public IActionResult OnPostRunAgencies()
    {
        var progress = _progressService.StartTask("Agencies Sync");

        // Fire off the sync task without waiting
        _ = Task.Run(async () => await RunAgenciesSyncWithProgress(progress));

        return Redirect($"/Sync?taskId={progress.TaskId}");
    }

    private async Task RunAgenciesSyncWithProgress(TaskProgress progress)
    {
        _progressService.UpdateProgress(progress.TaskId, 0, "Starting download of Agencies data...");
        await _dataService.SyncAgenciesAsync();
        _progressService.CompleteTask(progress.TaskId, "Sync completed successfully!");
    }

    private async Task RunSyncWithProgress(TaskProgress progress)
    {
        try
        {
           
            _progressService.UpdateProgress(progress.TaskId, 0, "Starting full sync...");
            
            await _dataService.SyncAllAsync(progress);            
            
            _progressService.CompleteTask(progress.TaskId, "Sync completed successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during sync: {ex.Message}");
            _progressService.FailTask(progress.TaskId, ex.Message);
        }
    }

    private async Task RunXmlSyncWithProgress(TaskProgress progress)
    {
        try
        {
            _progressService.UpdateProgress(progress.TaskId, 0, "Starting XML sync...");
            
            await _xmlService.GetCurrentTitleXMLData(progress);
            
            _progressService.CompleteTask(progress.TaskId, "XML sync completed successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during XML sync: {ex.Message}");
            _progressService.FailTask(progress.TaskId, ex.Message);
        }
    }
    private async Task RunHistoricalXmlSyncWithProgress(TaskProgress progress)
    {
        try
        {
            _progressService.UpdateProgress(progress.TaskId, 0, "Starting XML sync...");

            await _xmlService.GetHistoricalXmlData(progress);

            _progressService.CompleteTask(progress.TaskId, "XML sync completed successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during XML sync: {ex.Message}");
            _progressService.FailTask(progress.TaskId, ex.Message);
        }
    }

    private async Task RunTitlesSyncWithProgress(TaskProgress progress)
    {
        try
        {
            _progressService.UpdateProgress(progress.TaskId, 0, "Starting titles sync...");
            
            _progressService.UpdateProgress(progress.TaskId, 50, "Fetching titles from API...");
            await _dataService.SyncTitlesAsync();
            
            _progressService.CompleteTask(progress.TaskId, "Titles sync completed successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during titles sync: {ex.Message}");
            _progressService.FailTask(progress.TaskId, ex.Message);
        }
    }
}