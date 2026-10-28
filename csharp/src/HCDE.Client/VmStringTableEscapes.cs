namespace HCDE.Client;

/// <summary><c>FStringTable::ProcessEscapes</c>.</summary>
public static class VmStringTableEscapes
{
    public const char ColorEscape = '\u001c';

    public static string Process(string input)
    {
        Span<char> buffer = stackalloc char[input.Length];
        var written = Process(input.AsSpan(), buffer);
        return buffer[..written].ToString();
    }

    public static int Process(ReadOnlySpan<char> input, Span<char> output)
    {
        var op = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c != '\\')
            {
                output[op++] = c;
                continue;
            }

            if (++i >= input.Length) break;
            c = input[i];
            c = c switch
            {
                'n' => '\n',
                'c' => ColorEscape,
                'r' => '\r',
                't' => '\t',
                'x' => ParseHexByte(input, ref i),
                '\n' => '\0',
                _ => c
            };
            if (c == '\0') continue;
            output[op++] = c;
        }

        return op;
    }

    private static char ParseHexByte(ReadOnlySpan<char> input, ref int i)
    {
        var value = 0;
        for (var n = 0; n < 2 && i + 1 < input.Length; n++)
        {
            var cc = input[++i];
            if (cc is >= '0' and <= '9') value = (value << 4) + (cc - '0');
            else if (cc is >= 'a' and <= 'f') value = (value << 4) + (10 + cc - 'a');
            else if (cc is >= 'A' and <= 'F') value = (value << 4) + (10 + cc - 'A');
            else
            {
                i--;
                break;
            }
        }

        return value == 0 ? '\0' : (char)value;
    }
}
