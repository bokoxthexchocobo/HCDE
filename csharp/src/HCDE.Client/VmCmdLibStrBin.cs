namespace HCDE.Client;

/// <summary>Native <c>cmdlib.cpp</c> <c>strbin</c> / <c>strbin1</c>.</summary>
public static class VmCmdLibStrBin
{
    public static string Unescape(string start)
    {
        ArgumentNullException.ThrowIfNull(start);
        Span<char> buffer = stackalloc char[start.Length];
        var length = Unescape(start, buffer);
        return new string(buffer[..length]);
    }

    public static int Unescape(string source, Span<char> destination)
    {
        var write = 0;
        var index = 0;
        while (index < source.Length)
        {
            var c = source[index++];
            if (c != '\\')
            {
                if (write >= destination.Length) throw new ArgumentException("Destination too small.");
                destination[write++] = c;
                continue;
            }

            if (index >= source.Length) break;
            var esc = source[index++];
            switch (esc)
            {
                case 'a': destination[write++] = '\a'; break;
                case 'b': destination[write++] = '\b'; break;
                case 'c': destination[write++] = '\x1c'; break;
                case 'f': destination[write++] = '\f'; break;
                case 'n': destination[write++] = '\n'; break;
                case 't': destination[write++] = '\t'; break;
                case 'r': destination[write++] = '\r'; break;
                case 'v': destination[write++] = '\v'; break;
                case '?': destination[write++] = '?'; break;
                case '\n': break;
                case 'x' or 'X':
                    var hex = 0;
                    for (var i = 0; i < 2 && index < source.Length; i++)
                    {
                        var ch = source[index];
                        if (ch is >= '0' and <= '9') hex = (hex << 4) + (ch - '0');
                        else if (ch is >= 'a' and <= 'f') hex = (hex << 4) + (10 + ch - 'a');
                        else if (ch is >= 'A' and <= 'F') hex = (hex << 4) + (10 + ch - 'A');
                        else break;
                        index++;
                    }

                    destination[write++] = (char)hex;
                    break;
                case >= '0' and <= '7':
                    var oct = esc - '0';
                    for (var i = 0; i < 2 && index < source.Length; i++)
                    {
                        var ch = source[index];
                        if (ch is >= '0' and <= '7') { oct = (oct << 3) + (ch - '0'); index++; }
                        else break;
                    }

                    destination[write++] = (char)oct;
                    break;
                default:
                    destination[write++] = esc;
                    break;
            }
        }

        return write;
    }
}
