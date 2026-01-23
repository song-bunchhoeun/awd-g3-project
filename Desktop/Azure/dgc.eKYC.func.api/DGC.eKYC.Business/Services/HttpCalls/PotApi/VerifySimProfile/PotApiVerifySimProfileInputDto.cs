namespace DGC.eKYC.Business.Services.HttpCalls.PotApi.VerifySimProfile;

public class PotApiVerifySimProfileInputPersonDocumentDto
{
    public string Phone { get; set; }

    public string DocumentId { get; set; }

    public string? DocumentRevision { get; set; }
    public DateTime? DocumentIssueDate { get; set; }
    public DateTime DocumentExpireDate { get; set; }
    public string? DocumentPhotoUrl { get; set; }
    public string? DocumentOwnerPhotoUrl { get; set; }
    public string? DocumentOwnerLivePhotoUrl { get; set; }
    public decimal? FaceMatchPercentage { get; set; }
    public DateTime CreatedDate { get; set; }

    public PotApiVerifySimProfileInputPersonDto Person { get; set; }
}

public class PotApiVerifySimProfileInputPersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FirstNameKh { get; set; }
    public string LastNameKh { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? Address { get; set; }
}