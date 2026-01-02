using DGC.eKYC.Business.Validations.ModelState;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DGC.eKYC.Business.DTOs.SuperAppSecurity;

public class SuperAppSecurityBaseInput : SuperAppSecurityBaseFormattedInput
{
    [Required]
    public string InitData { get; set; }
}

[ValidateNever]
public class SuperAppSecurityBaseFormattedInput
{
    [JsonIgnore]
    public string? DeviceId { get; set; }

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public string? CheckSum { get; set; }

    [JsonIgnore]
    [SuperAppUserLevelValidation]
    public object? UserLevel { get; set; }

    [JsonIgnore]
    public double? TimeStamp { get; set; }
}
