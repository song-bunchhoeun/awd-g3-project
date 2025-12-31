using DGC.eKYC.Business.Services.HttpCalls.PotApi.VerifySimProfile;

namespace DGC.eKYC.Business.Services.HttpCalls.PointOfTruthHttpCall;

public interface IPointOfTruthHttpCall
{
    Task<PotApiVerifySimProfileOutputDto?> VerifySimProfileAsync(
        string ssykAccessId,
        PotApiVerifySimProfileInputPersonDocumentDto inputDto,
        CancellationToken cancellationToken);
}