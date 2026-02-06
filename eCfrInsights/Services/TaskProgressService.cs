using System.Collections.Concurrent;

namespace ecfrInsights.Services;

/// <summary>
/// Represents the current progress of a background task
/// </summary>
public class TaskProgress
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public string TaskName { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    public int ProgressPercentage { get; set; } = 0;
    public string CurrentMessage { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Service for tracking the progress of long-running background tasks
/// </summary>
public class TaskProgressService
{
    private static readonly ConcurrentDictionary<string, TaskProgress> _tasks = new();
    
    /// <summary>
    /// Starts a new task and returns its TaskProgress object for updates
    /// </summary>
    public TaskProgress StartTask(string taskName)
    {
        var progress = new TaskProgress
        {
            TaskName = taskName,
            Status = TaskStatus.InProgress,
            StartTime = DateTime.UtcNow,
            ProgressPercentage=0
        };
        
        _tasks.TryAdd(progress.TaskId, progress);
        return progress;
    }

    /// <summary>
    /// Gets the progress of a specific task by ID
    /// </summary>
    public TaskProgress? GetTaskProgress(string taskId)
    {
        _tasks.TryGetValue(taskId, out var progress);
        return progress;
    }

    /// <summary>
    /// Updates the progress of a task
    /// </summary>
    public void UpdateProgress(string taskId, int percentage, string message)
    {
        if (_tasks.TryGetValue(taskId, out var progress))
        {
            progress.ProgressPercentage = Math.Min(Math.Max(percentage, 0), 100);
            progress.CurrentMessage = message;
        }
    }

    /// <summary>
    /// Marks a task as completed
    /// </summary>
    public void CompleteTask(string taskId, string? message = null)
    {
        if (_tasks.TryGetValue(taskId, out var progress))
        {
            progress.Status = TaskStatus.Completed;
            progress.ProgressPercentage = 100;
            progress.EndTime = DateTime.UtcNow;
            if (message != null)
            {
                progress.CurrentMessage = message;
            }
        }
    }

    /// <summary>
    /// Marks a task as failed with an error message
    /// </summary>
    public void FailTask(string taskId, string errorMessage)
    {
        if (_tasks.TryGetValue(taskId, out var progress))
        {
            progress.Status = TaskStatus.Failed;
            progress.ErrorMessage = errorMessage;
            progress.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Cancels a task
    /// </summary>
    public void CancelTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var progress))
        {
            progress.Status = TaskStatus.Cancelled;
            progress.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets all active tasks
    /// </summary>
    public List<TaskProgress> GetAllActiveTasks()
    {
        return _tasks.Values
            .Where(t => t.Status == TaskStatus.InProgress)
            .ToList();
    }

    /// <summary>
    /// Gets all tasks (active and completed)
    /// </summary>
    public List<TaskProgress> GetAllTasks()
    {
        return _tasks.Values.ToList();
    }

    /// <summary>
    /// Clears old completed tasks (older than specified time)
    /// </summary>
    public void ClearOldTasks(TimeSpan olderThan)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);
        var oldTasks = _tasks
            .Where(kvp => kvp.Value.EndTime.HasValue && kvp.Value.EndTime < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var taskId in oldTasks)
        {
            _tasks.TryRemove(taskId, out _);
        }
    }

    /// <summary>
    /// Clears all tasks
    /// </summary>
    public void ClearAllTasks()
    {
        _tasks.Clear();
    }
}
