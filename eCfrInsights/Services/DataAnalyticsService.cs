using ecfrInsights.Data;
using ecfrInsights.Data.Entities;

using Microsoft.EntityFrameworkCore;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ecfrInsights.Services;

public class DataAnalyticsService(EcfrContext context)
{

    private async Task RemoveTodaysTitleCalculations(DateTime? date)
    {
        date??= DateTime.Now;
        DateTime fromDate = date.Value.Date;
        DateTime toDate = fromDate.AddDays(1).AddTicks(-15);
        var existing = await context.CfrTitleComplexities
            .Where(c => c.DateComputed >= fromDate && c.DateComputed <= toDate).ToListAsync();
        if (existing != null)
        {
            context.CfrTitleComplexities.RemoveRange(existing);
            await context.SaveChangesAsync();
        }
    }
    public async Task<List<CfrTitleComplexity>> CalculateTitleComplexities(DateTime? date)
    {

        await RemoveTodaysTitleCalculations(date);
        // ---------------------------------------------
        // 1. Load all Title IDs and names
        // ---------------------------------------------
        var titleKeys = await context.CfrTitles
            .AsNoTracking()
            .Select(t => new { t.Number, t.Name })
            .ToListAsync();

        // ---------------------------------------------
        // 2. Batch query: HierarchicalCount
        // ---------------------------------------------
        var hierarchicalCounts = await context.CfrHierarchies
            .AsNoTracking()
            .GroupBy(r => r.TitleNumber)
            .Select(g => new
            {
                Title = g.Key,
                Count = g.SelectMany(r => r.AgencyHierarchies).Count()
            })
            .ToDictionaryAsync(x => x.Title, x => x.Count);

        // ---------------------------------------------
        // 3. Batch query: TotalAgencies
        // ---------------------------------------------
        var agencyCounts = await context.CfrHierarchies
                                .AsNoTracking()
                                .SelectMany(r => r.AgencyHierarchies)
                                .Where(e => e.CfrHierarchy != null)
                                .GroupBy(h => h.CfrHierarchy.TitleNumber)
                                .Select(g => new
                                {
                                    Title = g.Key,
                                    Count = g.Select(h => h.Slug).Distinct().Count()
                                })
                                .ToDictionaryAsync(x => x.Title, x => x.Count);


        // ---------------------------------------------
        // 4. Batch query: Wordcount
        // ---------------------------------------------
        var wordCounts = await context.CfrHierarchies
              .AsNoTracking()
              .SelectMany(h => h.ChildReferences)
              .Select(cr => new
              {
                  cr.TitleNumber,
                  cr.ReferenceContent
              })
              .ToListAsync();   // <-- forces client evaluation

        var grouped = wordCounts
            .GroupBy(x => x.TitleNumber)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x =>
                    x.ReferenceContent == null
                        ? 0
                        : x.ReferenceContent.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                )
            );


        // ---------------------------------------------
        // 5. Batch query: TotalCorrections (after startDate)
        // ---------------------------------------------
        DateTime startDate = new DateTime(2025, 1, 1);

        var correctionCounts = await context.Corrections
            .AsNoTracking()
            .Where(c => c.ErrorCorrected >= startDate)
            .GroupBy(c => c.Title)
            .Select(g => new
            {
                Title = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Title, x => x.Count);

        // ---------------------------------------------
        // 6. Build raw complexity objects
        // ---------------------------------------------
        var raw = new List<CfrTitleComplexity>();

        foreach (var titleDoc in titleKeys)
        {
            raw.Add(new CfrTitleComplexity
            {
                Title = titleDoc.Number,
                HierarchicalCount = hierarchicalCounts.GetValueOrDefault(titleDoc.Number, 0),
                TotalAgencies = agencyCounts.GetValueOrDefault(titleDoc.Number, 0),
                Wordcount = grouped.GetValueOrDefault(titleDoc.Number, 0),
                TotalCorrections = correctionCounts.GetValueOrDefault(titleDoc.Number, 0),
                TitleText=titleDoc.Name
            });
        }

        // ---------------------------------------------
        // 7. Normalize each metric across all titles
        // ---------------------------------------------
        Normalize(raw, t => t.HierarchicalCount, (t, v) => t.NormHierarchical = v);
        Normalize(raw, t => t.TotalAgencies, (t, v) => t.NormAgencies = v);
        Normalize(raw, t => t.Wordcount, (t, v) => t.NormWordcount = v);
        Normalize(raw, t => t.TotalCorrections, (t, v) => t.NormCorrections = v);

        // ---------------------------------------------
        // 8. Compute final complexity score
        // ---------------------------------------------
        foreach (var t in raw)
        {
            t.ComplexityScore =
                (t.NormHierarchical * 2) +
                (t.NormAgencies * 3) +
                (t.NormWordcount * 1) +
                (t.NormCorrections * 5);
        }
        //add the data to the database
        var existingList = await context.CfrTitleComplexities.ToListAsync();

        raw.ForEach(record =>
        {
            var existing = existingList.FirstOrDefault(r => r.Title == record.Title);
            if (existing != null)
            {
                existing.HierarchicalCount = record.HierarchicalCount;
                existing.TotalAgencies = record.TotalAgencies;
                existing.Wordcount = record.Wordcount;
                existing.TotalCorrections = record.TotalCorrections;
                existing.NormHierarchical = record.NormHierarchical;
                existing.NormAgencies = record.NormAgencies;
                existing.NormWordcount = record.NormWordcount;
                existing.NormCorrections = record.NormCorrections;
                existing.ComplexityScore = record.ComplexityScore;
                existing.DateComputed = DateTime.UtcNow;
            }
            else
                context.CfrTitleComplexities.Add(record);
        });
        await context.SaveChangesAsync();
        return raw;
    }

    internal async Task<List<CfrTitleComplexity>> GetTitleComplexities(DateTime? date)
    {
        DateTime fromDate = date?.Date ?? DateTime.Now.Date;
        DateTime toDate = fromDate.AddDays(1).AddTicks(-15);

        return await context.CfrTitleComplexities.Where(e=>e.DateComputed>= fromDate && e.DateComputed <=toDate).ToListAsync();
    }


    private async Task RemoveTodaysAgencyCalculations(DateTime date)
    {
        DateTime fromDate = date.Date;
        DateTime toDate = date.Date.AddDays(1).AddTicks(-15);
        var existing = await context.AgencyStatistics
            .Where(c => c.ForDate >= fromDate && c.ForDate <= toDate).ToListAsync();
        if (existing != null)
        {
            context.AgencyStatistics.RemoveRange(existing);
            await context.SaveChangesAsync();
        }
    }
    public async Task<List<AgencyStatistics>> CalculateAgencyStatistics(DateTime? forDate)
    {
        forDate ??= DateTime.Now;

        await RemoveTodaysAgencyCalculations(forDate.Value);
        // ---------------------------------------------
        // 1. Load all agency slugs + names
        // ---------------------------------------------
        var agencies = await context.Agencies
            .AsNoTracking()
            .Select(a => new { a.Slug, a.Name })
            .ToListAsync();

        // ---------------------------------------------
        // 2. TotalHierarchies per agency
        // ---------------------------------------------
        var hierarchyCounts = await context.AgencyHierarchies
            .AsNoTracking()
            .GroupBy(h => h.Slug)
            .Select(g => new
            {
                Slug = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Slug, x => x.Count);

        // ---------------------------------------------
        // 3. TotalSubAgencies per agency
        // ---------------------------------------------
        var subAgencyCounts = await context.Agencies
            .AsNoTracking()
            .Where(h => h.Children != null)
            .GroupBy(h => h.Slug)
            .Select(g => new
            {
                Slug = g.Key,
                Count = g.Select(h => h.Children)
                         .Where(p => p != null)
                         .Distinct()
                         .Count()
            })
            .ToDictionaryAsync(x => x.Slug, x => x.Count);

        // ---------------------------------------------
        // 4. TotalWords (client-side wordcount)
        // ---------------------------------------------
        var roots = await context.AgencyHierarchies
            .AsNoTracking()
            .Where(h => h.CfrHierarchy != null)
            .Include(h => h.CfrHierarchy)
            .ToListAsync();

        var wordRows = new List<(string Slug, string Text)>();

        foreach (var h in roots)
        {
            await LoadChildren(h.CfrHierarchy!);

            foreach (var text in ExtractAllText(h.CfrHierarchy!))
                wordRows.Add((h.Slug, text));
        }


        var wordCounts = wordRows
            .GroupBy(x => x.Slug)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x =>
                    x.Text == null
                        ? 0
                        : x.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                )
            );
           

        // ---------------------------------------------
        // 5. Build raw stats
        // ---------------------------------------------
        var stats = new List<AgencyStatistics>();

        foreach (var a in agencies)
        {
            stats.Add(new AgencyStatistics
            {
                Slug = a.Slug,
                AgencyName = a.Name,
                ForDate = forDate.Value,
                TotalHierarchies = hierarchyCounts.GetValueOrDefault(a.Slug, 0),
                TotalSubAgencies = subAgencyCounts.GetValueOrDefault(a.Slug, 0),
                TotalWords = wordCounts.GetValueOrDefault(a.Slug, 0)
            });
        }

        // ---------------------------------------------
        // 6. Normalize metrics
        // ---------------------------------------------
        Normalize(stats, s => s.TotalHierarchies, (s, v) => s.NormHierarchical = v);
        Normalize(stats, s => s.TotalSubAgencies, (s, v) => s.NormAgencies = v);
        Normalize(stats, s => s.TotalWords, (s, v) => s.NormWordcount = v);

        // ---------------------------------------------
        // 7. Compute final complexity score
        // ---------------------------------------------
        foreach (var s in stats)
            s.CalculateComplexityScore();

        context.AgencyStatistics.AddRange(stats);
        try
        {
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            // Handle exceptions as needed (e.g., log the error)
            Console.WriteLine($"Error saving agency statistics: {ex.Message}");
        }
        return stats;
    }


    // -----------------------------------------------------------
    // Shared normalization helper
    // -----------------------------------------------------------
    private void Normalize<T>(
        List<T> items,
        Func<T, double> selector,
        Action<T, double> assign)
    {
        double min = items.Min(selector);
        double max = items.Max(selector);
        double range = max - min;

        foreach (var item in items)
        {
            double value = selector(item);
            double normalized = range == 0 ? 0 : (value - min) / range;
            assign(item, normalized);
        }
    }

    internal async Task<List<AgencyStatistics>> GetAgencyStatistics(DateTime? date)
    {
        date ??= DateTime.Now;
        DateTime fromdate = date.Value.Date;
        DateTime todate = fromdate.AddDays(1).AddTicks(-15);


        return await context.AgencyStatistics
            .AsNoTracking()
            .Where(s => s.ForDate >= fromdate && s.ForDate <= todate)
            .ToListAsync();
    }
    async Task LoadChildren(CfrHierarchy node)
    {
        await context.Entry(node)
            .Collection(n => n.ChildReferences)
            .LoadAsync();

        foreach (var child in node.ChildReferences)
            await LoadChildren(child);
    }
    IEnumerable<string> ExtractAllText(CfrHierarchy node)
    {
        foreach (var child in node.ChildReferences)
        {
            if (!string.IsNullOrWhiteSpace(child.ReferenceContent))
                yield return child.ReferenceContent;

            foreach (var text in ExtractAllText(child))
                yield return text;
        }
    }

}
