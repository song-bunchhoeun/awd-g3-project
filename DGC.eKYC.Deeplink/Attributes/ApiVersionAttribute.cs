namespace DGC.eKYC.Deeplink.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ApiVersionAttribute : Attribute
{
    public string Version { get; }
    public DateOnly ExpectedDate { get; }

    public ApiVersionAttribute(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("version is required", nameof(version));

        Version = version;

        // Expect ISO date format yyyy-MM-dd (date-only)
        if (!DateOnly.TryParseExact(version, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
        {
            throw new ArgumentException("ApiVersion must be an ISO date in format yyyy-MM-dd, e.g. 2025-05-08", nameof(version));
        }

        ExpectedDate = parsed;
    }
}