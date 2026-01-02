using DGC.eKYC.Business.DTOs.SuperAppSecurity;

namespace DGC.eKYC.Business.Services.HashService;

public interface IHashCompute
{
    bool ValidateCheckSum(string initData);
    void ValidateSecurityFieldInput(SuperAppSecurityBaseInput input);
    void PopulateSecurityField(SuperAppSecurityBaseInput input, string initData);
}

