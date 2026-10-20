namespace HCDE.Client;

/// <summary><c>CVarSearchContainsNoCase</c> from <c>c_cvars.cpp</c>.</summary>
public static class VmCvarSearch
{
    public static bool ContainsNoCase(string? haystack, string? needle)
    {
        if (string.IsNullOrEmpty(needle)) return true;
        if (string.IsNullOrEmpty(haystack)) return false;
        return haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }
}
