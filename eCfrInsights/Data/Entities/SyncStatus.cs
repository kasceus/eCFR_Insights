namespace ecfrInsights.Data.Entities;

public class SyncStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime LastSyncedUtc { get; set; }
}