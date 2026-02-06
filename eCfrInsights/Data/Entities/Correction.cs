using ecfrInsights.Data.eCFRApi;

namespace ecfrInsights.Data.Entities;

/// <summary>
/// Database entity for stored corrections from the eCFR API.
/// Each correction is associated with one or more CFR references.
/// </summary>
public partial class Correction
{
    public int Id { get; set; }

    public string CorrectiveAction { get; set; } = default!;

    public DateTime ErrorCorrected { get; set; }

    public DateTime ErrorOccurred { get; set; }

    public string FrCitation { get; set; } = default!;

    public int Position { get; set; }

    public bool DisplayInToc { get; set; }
    //key to link to the title document, this will be used to link to the actual document that is being corrected
    public int Title { get; set; }

    public int Year { get; set; }

    public DateTime LastModified { get; set; }

    public CfrTitle CfrDocument { get; set; } = default!;

    public static explicit operator Correction(ApiCorrection v)
    {

        Correction ret = new()
        {
            Id = v.Id,
            CorrectiveAction = v.CorrectiveAction,
            ErrorCorrected = DateTime.TryParse(v.ErrorCorrected, out var ec) ? ec : default,
            ErrorOccurred = DateTime.TryParse(v.ErrorOccurred, out var eo) ? eo : default,
            DisplayInToc = v.DisplayInToc,
            FrCitation = v.FrCitation,
            Position = v.Position,
            Title = v.Title,
            Year = v.Year,
            LastModified = DateTime.TryParse(v.LastModified, out var lm) ? lm : default,
        };

        return ret;
    }
}
