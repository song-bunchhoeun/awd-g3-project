namespace DGC.eKYC.Business.Services.HttpCalls.PotApi.VerifySimProfile;

public class PotApiVerifySimProfileOutputDto
{
    public string OperationId { get; set; }
    public bool IsVerified { get; set; }
    public int Flag { get; set; }
    public DateTime Timestamp { get; set; }
}