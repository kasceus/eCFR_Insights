using ecfrInsights.Data.Entities;

using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Text.Json;

namespace ecfrInsights.Pages
{
    public class GraphsModel : PageModel
    {

        private readonly HttpClient _http;

        public string JsonHierarchy { get; set; } = "{}";

        public GraphsModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("default");
        }

        public async Task OnGet()
        {
            string url = $"http://localhost:8080/api/analytics/agencies";
            List<AgencyStatistics>? agencies = await _http.GetFromJsonAsync<List<AgencyStatistics>>(url);

            var hierarchy = new
            {
                name = "Agencies",
                children = agencies.Select(a => new
                {
                    name = a.AgencyName,
                    value = a.ComplexityScore,
                    slug = a.Slug,
                    totalHierarchies = a.TotalHierarchies,
                    totalWords = a.TotalWords,
                    totalSubAgencies = a.TotalSubAgencies
                })
            };

            JsonHierarchy = JsonSerializer.Serialize(hierarchy);
        }

    }
}
