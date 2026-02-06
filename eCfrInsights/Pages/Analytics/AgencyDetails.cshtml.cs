using ecfrInsights.Data.Entities;
using ecfrInsights.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ecfrInsights.Pages.Analytics;

public class AgencyDetailsModel(IHttpClientFactory factory, AgencyService agencyService) : PageModel
{
    private readonly HttpClient _http = factory.CreateClient("default");

    public AgencyStatistics? Agency { get; set; }
    public string AgencyRefs = "";
    public async Task<IActionResult> OnGet(string slug)
    {
        string url = $"{Request.Scheme}://{Request.Host}/api/analytics/agencies";

        var agencies = await _http.GetFromJsonAsync<List<AgencyStatistics>>(url);

        Agency = agencies?.FirstOrDefault(a => a.Slug == slug);



        if (Agency == null)
            return NotFound();
        //get the Hierarchy ref for the agency

        var refs = await agencyService.GetRefs(slug);
        AgencyRefs = string.Join(", ", refs);
        return Page();
    }
}
