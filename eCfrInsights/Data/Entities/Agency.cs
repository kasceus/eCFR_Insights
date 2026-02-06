using System.Collections.Generic;
using System.Text.Json.Serialization;
using ecfrInsights.Data.Entities;
using ecfrInsights.Data.eCFRApi;
using System.ComponentModel.DataAnnotations.Schema;
namespace ecfrInsights.Data.Entities;

/// <summary>
/// This is the data for an individual agency retrieved from the eCFR API.
/// </summary>
public partial class Agency
{
    public string Name { get; set; } = default!;

    public string? ShortName { get; set; }

    public string DisplayName { get; set; } = default!;

    public string SortableName { get; set; } = default!;

    public string Slug { get; set; } = default!;

    // Navigation property for self-referencing parent-child relationship
    [JsonIgnore]
    public Agency? Parent { get; set; }

    [JsonIgnore]
    public string? ParentSlug { get; set; }

    // Navigation property: One GraphsModel has many child Agencies
    public List<Agency> Children { get; set; } = [];

    public ICollection<AgencyHierarchy> AgencyHierarchies { get; set; } = [];
    [NotMapped]
    public virtual ICollection<AgencyStatistics> Statistics { get; set; } = [];

    public static implicit operator Agency(ApiAgency agency)
    {

        var agencyRet = new Agency
        {
            Name = agency.Name,
            ShortName = agency.ShortName,
            DisplayName = agency.DisplayName,
            SortableName = agency.SortableName,
            Slug = agency.Slug,
            Children = agency.Children.ConvertAll(child => (Agency)child),

        };
        return agencyRet;
    }
}

public static class AgencyExtensions
{
    public static void MakeHierarchy(this Agency parent, ApiAgency apiAgency, List<CfrHierarchy> hierarchies)
    {
        foreach (var cfrReference in apiAgency.CfrReferences)
        {
            string? chapter = null;
            string? subChapter = null;
            string? subTitle = null;
            string? part = null;



            var hierarchy = hierarchies.Where(h => h.TitleNumber == cfrReference.Title);
            //need to add to the query if the fields are not null to narrow it down
            if (!string.IsNullOrWhiteSpace(cfrReference.Subtitle))
            {
                subTitle = cfrReference.Subtitle;
                hierarchy = hierarchies.Where(h => h.Subtitle == cfrReference.Subtitle);
            }

            if (!string.IsNullOrWhiteSpace(cfrReference.Chapter))
            {
                chapter = cfrReference.Chapter;
                hierarchy = hierarchies.Where(h => h.Chapter == cfrReference.Chapter);
            }
            if (!string.IsNullOrWhiteSpace(cfrReference.Subchapter))
            {
                subChapter = cfrReference.Subchapter;
                hierarchy = hierarchies.Where(h => h.Subchapter == cfrReference.Subchapter);
            }
            if (!string.IsNullOrWhiteSpace(cfrReference.Part))
            {
                part = cfrReference.Part;
                hierarchy = hierarchies.Where(h => h.Part == cfrReference.Part);
            }

            AgencyHierarchy? agencyHierarchy = null;
            if (hierarchy.Count() > 1)
            {
                agencyHierarchy = new AgencyHierarchy()
                {
                    CfrReferenceNumber = hierarchy.OrderBy(e => e.CfrReferenceNumber.Length).First().CfrReferenceNumber,
                    Slug = parent.Slug
                };
            }
            else if (hierarchy.Count() == 1)
            {
                agencyHierarchy = new AgencyHierarchy()
                {
                    CfrReferenceNumber = hierarchy.First().CfrReferenceNumber,
                    Slug = parent.Slug
                };
            }
            else
            {
                //cfrHierarchical item not found. need to create it here, too.
                CfrHierarchy newHierarchy = new CfrHierarchy()
                {
                    TitleNumber = cfrReference.Title,
                    Subtitle = subTitle,
                    Chapter = chapter,
                    Subchapter = subChapter,
                    Part = part,
                    Reserved = true ,
                    CfrReferenceNumber= CfrHierarchy.BuildReferenceNumber(cfrReference.Title, subTitle, chapter, subChapter, part, null, null, null)
                };
                
                newHierarchy.CfrReferenceTitle= $"Reserved {parent.Name} - {newHierarchy.CfrReferenceNumber}".Trim();
                
                agencyHierarchy = new()
                {
                    CfrReferenceNumber = newHierarchy.CfrReferenceNumber,
                    Slug = parent.Slug,
                    CfrHierarchy=newHierarchy
                };
            }
            if (agencyHierarchy != null)
                parent.AgencyHierarchies.Add(agencyHierarchy);
        }
    }

}