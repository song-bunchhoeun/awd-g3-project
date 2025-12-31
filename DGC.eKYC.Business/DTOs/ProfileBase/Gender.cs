namespace DGC.eKYC.Business.DTOs.ProfileBase;

public static class Gender
{
    public const string M = "M";
    public const string F = "F";

    public static readonly string[] AllowedGender = [M, F];

    public static bool IsValid(object? value)
    {
        return value switch
        {
            string strValue => string.Compare(strValue, M, StringComparison.CurrentCultureIgnoreCase) == 0 ||
                               string.Compare(strValue, F, StringComparison.CurrentCultureIgnoreCase) == 0,
            _ => false
        };
    }
}