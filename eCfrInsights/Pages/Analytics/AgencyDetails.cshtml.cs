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
        string url = $"http://localhost:8080/api/analytics/agencies";

        List<AgencyStatistics>? agencies = await _http.GetFromJsonAsync<List<AgencyStatistics>>(url);

        Agency = agencies?.FirstOrDefault(a => a.Slug == slug);



        if (Agency == null)
        {
            return NotFound();
        }
        //get the Hierarchy ref for the agency

        List<string> refs = await agencyService.GetRefs(slug);
        AgencyRefs = string.Join(", ", refs);
        return Page();
    }
}
