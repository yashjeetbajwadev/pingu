namespace pingu.Helpers;

public static class Helper
{
    public static string GetEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Environment variable '{key}' not found.");
        }

        return value;
    }
}