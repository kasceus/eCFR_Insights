using System.ComponentModel;

namespace ecfrInsights.Data.eCFRApi;

/// <summary>
/// Endpoints for pulling from eCFR Api.
/// </summary>
/// <remarks>Documentation can be found at https://www.ecfr.gov/developers/documentation/api/v1#/</remarks>
public class Endpoints
{
    public const string BaseUrl = "https://www.ecfr.gov/api";
    public const string GetAllAgencies = $"{BaseUrl}/admin/v1/agencies.json";
    public const string GetCorrections = $"{BaseUrl}/admin/v1/corrections.json";

    public class VersionerService
    {
        /// <summary>
        /// Makes the url for the ancestry endpoint for a given title and date. This endpoint returns the ancestry of a regulation for a given title and date.
        /// </summary>
        /// <param name="title">title to search</param>
        /// <param name="date">date to search</param>
        /// <returns>string for the endpoint</returns>
        [Description("Ancestors route returns all ancestors (including self) from a given level through the top title node.")]
        public string GetAncestryForTitleAndDate(int title, DateTime date){
            string DateParsed = date.ToString("yyyy-MM-dd");
            return $"{BaseUrl}/versioner/v1/ancestry/{DateParsed}/title-{title}.json"; 
        }
        [Description("Source XML for a title or subset of a title. Requests can be for entire titles or part level and below." +
            " Downloadable XML document is returned for title requests. Processed XML is returned if part, subpart, section, or appendix is requested")]
        
        public string GetXMLForTitle(int title, DateTime date)
        {
            string DateParsed = date.ToString("yyyy-MM-dd");
            return $"{BaseUrl}/versioner/v1/full/{DateParsed}/title-{title}.xml";
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The Title service can be used to determine the status of each individual title and of 
        /// the overall status of title imports and reprocessings. It returns an array of all titles 
        /// containing a hash for each with the name of the title, the latest amended date, latest issue date, 
        /// up-to-date date, reserved status, and if applicable, processing in progress status. 
        /// The meta data returned indicates the latest issue date and whether titles are currently being reprocessed.
        /// </remarks>
        [Description("Summary information about each title")]
        public const string GetTitlesSummary = $"{BaseUrl}/versioner/v1/titles.json";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        /// <remarks>
        /// Returns the content versions meeting the specified criteria. Each content object includes its identifier,
        /// parent hierarchy, last amendment date and issue date it was last updated. Queries return content versions 
        /// on an issue date, or before or on a specific issue date lte or on or after gte a specific issue date. 
        /// The gte and lte parameters may be combined. Use of the on parameter precludes use of gte or lte. 
        /// In the response, the date field is identical to amendment_date and is deprecated.</remarks>
        public string GetVersionsForTitle(int title)
        {
            return $"{BaseUrl}/versioner/v1/versions/title-{title}.json";
        }
    }
}
