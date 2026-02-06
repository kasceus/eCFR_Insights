using ecfrInsights.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using System.Data.Common;

namespace ecfrInsights.Data;

public class EcfrContext(DbContextOptions<EcfrContext> options) : DbContext(options)
{


    /// <summary>
    /// Agencies
    /// </summary>
    public DbSet<Agency> Agencies { get; set; }

    /// <summary>
    /// GraphsModel Hierarchy Relations table
    /// </summary>
    public DbSet<AgencyHierarchy> AgencyHierarchies { get; set; }

    /// <summary>
    /// GraphsModel Statistics
    /// </summary>
    public DbSet<AgencyStatistics> AgencyStatistics { get; set; }
  

    /// <summary>
    /// Document Hierarchical data table
    /// </summary>
    public DbSet<CfrHierarchy> CfrHierarchies { get; set; }
    /// <summary>
    /// Historical data for hierarchical data
    /// </summary>
    public DbSet<CfrHierarchyHistory> CfrHierarchyHistories { get; set; }
    /// <summary>
    /// Title Documents
    /// </summary>
    public DbSet<CfrTitle> CfrTitles { get; set; }

    /// <summary>
    /// History of Title Documents
    /// </summary>
    public DbSet<CfrTitleHistory> CfrTitleHistories { get; set; }

    /// <summary>
    /// Corrections for Titles and their hierarchies
    /// </summary>
    public DbSet<Correction> Corrections { get; set; }

    /// <summary>
    /// Status of the different Synchronization processes
    /// </summary>
    public DbSet<SyncStatus> SyncStatuses { get; set; }

    public DbSet<CfrTitleComplexity> CfrTitleComplexities { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.AddInterceptors(new BusyTimeoutInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        IEnumerable<Type> configurations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract &&
                           type.GetInterfaces().Any(i => i.IsGenericType &&
                                                         i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));

        foreach (Type? configType in configurations)
        {
            Type entityType = configType.GetInterfaces()
                                       .First(i => i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                                       .GetGenericArguments()[0];

            object? instance = Activator.CreateInstance(configType);

            System.Reflection.MethodInfo? applyConfigurationMethod = typeof(ModelBuilder)
                .GetMethod(nameof(ModelBuilder.ApplyConfiguration))?
                .MakeGenericMethod(entityType);

            applyConfigurationMethod?.Invoke(modelBuilder, [instance]);
        }
    }
}
public class BusyTimeoutInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA busy_timeout = 5000;";
        cmd.CommandText = "PRAGMA journal_mode = WAL;";
        cmd.ExecuteNonQuery();
    }
}
