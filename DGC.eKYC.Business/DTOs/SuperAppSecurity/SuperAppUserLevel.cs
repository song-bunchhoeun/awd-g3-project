namespace DGC.eKYC.Business.DTOs.SecurityBaseRequest;

public class SuperAppUserLevel
{
    public const string GuestLevel = "0";
    public const string L0 = "L0";
    public const string L1 = "L1";
    public const string L2 = "L2";
    public const string LevelOne = "1";
    public const string LevelTwo = "2";

    public static bool IsValid(object? levelValue)
    {
        var value = GetSuperAppUserLevel(levelValue);

        return !string.IsNullOrEmpty(value);
    }

    public static string? GetSuperAppUserLevel(object? levelValue)
    {
        var levelStr = levelValue?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(levelStr))
        {
            return null;
        }

        levelStr = levelStr.Trim();

        if (int.TryParse(levelStr, out var value))
        {
            levelStr = value.ToString();
        }

        if (string.Equals(levelStr, GuestLevel, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(levelStr, L0, StringComparison.OrdinalIgnoreCase))
        {
            return L0;
        }
        if (string.Equals(levelStr, LevelOne, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(levelStr, L1, StringComparison.OrdinalIgnoreCase))
        {
            return L1;
        }
        if (string.Equals(levelStr, LevelTwo, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(levelStr, L2, StringComparison.OrdinalIgnoreCase))
        {
            return L2;
        }

        return null;
    }
}