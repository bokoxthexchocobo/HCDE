namespace HCDE.Scripting;

/// <summary>Prepares scanner input using FScanner::PrepareScript's text rules.</summary>
public static class VmScriptText
{
    public static string Prepare(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // Native removes a UTF-8 BOM only when at least one byte follows it.
        if (source.Length > 1 && source[0] == '\uFEFF') source = source[1..];
        if (source.EndsWith('\n')) return source;
        return source.EndsWith('\0') ? source[..^1] + "\n" : source + "\n";
    }
}
