using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using PhoneNumbers;

namespace DGC.eKYC.Business.Mapper;

public static partial class Mapper
{
    private static readonly DateTime DateTimeDefault = new(1900, 1, 1);
    private static readonly PhoneNumberUtil PhoneNumberService = PhoneNumberUtil.GetInstance();

    public static string ToIdNumberCleaned(this string? idNumber, bool? expectedKhNid = false)
    {
        if (idNumber is null) return string.Empty;

        var expectedReplaceableSymbols = new[] { "!", "@", "#", "$", ")" };

        var mnoIdNumberZeroSpace = new string(idNumber.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        var mnoIdNumberRevisionFree = mnoIdNumberZeroSpace.Split("(").FirstOrDefault();

        foreach (var symbol in expectedReplaceableSymbols)
        {
            idNumber = mnoIdNumberRevisionFree!.Replace(symbol, "").Trim();
        }

        if (int.TryParse(idNumber, out var intIdNumber))
        {
            idNumber = intIdNumber.ToString();
        }
        else if (expectedKhNid == true)
        {
            throw new CustomHttpResponseException(400,
                new ErrorResponse("invalid-kh-id-number", "Your Cambodian ID Number is not parsable as a number"));
        }

        return idNumber;
    }

    public static string ToMsIsdnCleaned(this string msIsdn)
    {
        try
        {
            var parsedMsIsdn = PhoneNumberService.Parse(msIsdn, "KH");
            return $"{parsedMsIsdn.CountryCode}{parsedMsIsdn.NationalNumber}";
        }
        catch
        {
            throw new CustomHttpResponseException(400,
                new ErrorResponse("unparsable-msisdn", "MSISDN is not parsable by google libphonenumber"));
        }
    }

    public static string ToGenderCleaned(this string? gender)
    {
        // Because Gender has already been cleaned when it first run through OriginalInput to PotBaseObject
        // Therefore, it should be safe to assume
        if (string.IsNullOrEmpty(gender)) return string.Empty;

        return gender[0].ToString().ToUpperInvariant();
    }

    public static string ToDateOfBirthCleaned(this DateOnly dateOfBirth)
    {
        return dateOfBirth.ToString("yyyy-MM-dd");
    }

    public static DateOnly ToDateOfBirthCleaned(this string dateOfBirthStr)
    {
        if (string.IsNullOrEmpty(dateOfBirthStr) || dateOfBirthStr == "0001-01-01")
        {
            return DateOnly.FromDateTime(DateTimeDefault);
        }

        return DateOnly.ParseExact(dateOfBirthStr, "yyyy-MM-dd");
    }
}