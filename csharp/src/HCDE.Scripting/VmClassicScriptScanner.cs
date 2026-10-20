namespace HCDE.Scripting;

/// <summary>Native <c>sc_man.cpp</c> <c>CheckString</c> / pushback behavior.</summary>
public sealed class VmClassicScriptScanner
{
    private readonly string[] tokens;
    private int position;
    private bool pushedBack;

    public VmClassicScriptScanner(string source) =>
        tokens = Tokenize(source);

    public string? Current { get; private set; }

    public bool GetString()
    {
        if (pushedBack)
        {
            pushedBack = false;
            return Current is not null;
        }

        if (position >= tokens.Length)
        {
            Current = null;
            return false;
        }

        Current = tokens[position++];
        return true;
    }

    public bool CheckString(string expected)
    {
        if (!GetString())
            return false;
        if (string.Equals(Current, expected, StringComparison.OrdinalIgnoreCase))
            return true;
        UnGet();
        return false;
    }

    public void UnGet() => pushedBack = position > 0 || Current is not null;

    private static string[] Tokenize(string source)
    {
        var tokens = new List<string>();
        var index = 0;
        while (index < source.Length)
        {
            while (index < source.Length && char.IsWhiteSpace(source[index])) index++;
            if (index >= source.Length) break;
            if (source[index] == '"')
            {
                index++;
                var start = index;
                while (index < source.Length && source[index] != '"') index++;
                tokens.Add(source[start..index]);
                if (index < source.Length) index++;
                continue;
            }

            var tokenStart = index;
            while (index < source.Length && !char.IsWhiteSpace(source[index]) && source[index] != '"')
                index++;
            tokens.Add(source[tokenStart..index]);
        }

        return tokens.ToArray();
    }
}
