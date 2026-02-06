
using ecfrInsights.Data;

using Microsoft.EntityFrameworkCore;

namespace ecfrInsights.Services;

public class AgencyService(EcfrContext context)
{
    internal async Task<List<string>> GetRefs(string slug)
    {
        return await context.Agencies.Where(e => e.Slug == slug).SelectMany(e => e.AgencyHierarchies).Select(e => e.CfrReferenceNumber).ToListAsync();
    }
}
