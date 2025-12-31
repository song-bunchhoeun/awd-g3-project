using System.Text.Json.Serialization;

namespace DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiResponse;

public class HuaweiRrPassportOcrApiResponse
{
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
    
    public string Surname { get; set; }

    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }

    [JsonPropertyName("passport_number")]
    public string IdNumber { get; set; }

    [JsonPropertyName("date_of_birth")] 
    public DateOnly BirthDate { get; set; }

    public string Sex { get; set; }

    [JsonPropertyName("date_of_expiry")]
    public DateOnly? ExpiryDate { get; set; }


    [JsonPropertyName("machine_code")]
    public string MachineCode1 { get; set; }

    [JsonPropertyName("machine_code2")]
    public string MachineCode2 { get; set; }

    public DateOnly? DateOfIssue { get; set; }
    public string BirthPlace { get; set; }

    [JsonPropertyName("passport_type")]
    public string IdcardType { get; set; }

    public object Confidence { get; set; }

    public string Nationality { get; set; }

    [JsonPropertyName("issuing_authority")]
    public string IssuingAuthority { get; set; }

    [JsonPropertyName("place_of_issue")]
    public string PlaceOfIssue { get; set; }
}


public class HuaweiRrKhNidOcrApiResponse
{
    [JsonPropertyName("id_number")]
    public string IdNumber { get; set; }

    public string Address { get; set; }

    public bool IsNameConsistent { get; set; }

    [JsonPropertyName("birth_date")]
    public string BirthDate { get; set; }

    public string Description { get; set; }

    [JsonPropertyName("portrait_image")]
    public string PortraitImage { get; set; }

    [JsonPropertyName("issue_date")]
    public string IssueDate { get; set; }

    [JsonPropertyName("detect_blur_result")]
    public bool DetectBlurResult { get; set; }

    [JsonPropertyName("score_info")]
    public object ScoreInfo { get; set; }

    [JsonPropertyName("detect_glare_result")]
    public bool DetectGlareResult { get; set; }

    [JsonPropertyName("detect_reproduce_result")]
    public bool DetectReproduceResult { get; set; }

    [JsonPropertyName("detect_blocking_within_border_result")]
    public bool DetectBlockingWithinBorderResult { get; set; }

    public string Height { get; set; }

    [JsonPropertyName("machine_code1")]
    public string MachineCode1 { get; set; }

    [JsonPropertyName("machine_code2")]
    public string MachineCode2 { get; set; }

    [JsonPropertyName("machine_code3")]
    public string MachineCode3 { get; set; }

    [JsonPropertyName("adjusted_image")]
    public string AdjustedImage { get; set; }

    public string Sex { get; set; }

    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; }

    public object Confidence { get; set; }

    [JsonPropertyName("birth_place")]
    public string BirthPlace { get; set; }

    [JsonPropertyName("idcard_type")]
    public string IdcardType { get; set; }

    [JsonPropertyName("portrait_location")]
    public List<List<int>> PortraitLocation { get; set; }

    [JsonPropertyName("detect_tampering_result")]
    public bool DetectTamperingResult { get; set; }

    [JsonPropertyName("isIDConsistent")]
    public bool IsIDConsistent { get; set; }

    [JsonPropertyName("name_kh")]
    public string NameKh { get; set; }

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; }

    [JsonPropertyName("detect_border_integrity_result")]
    public bool DetectBorderIntegrityResult { get; set; }
}


