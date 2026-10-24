namespace HCDE.Gamedata;

/// <summary>DEH/BEX patch text helpers from <c>d_dehacked.cpp</c> (<c>ReadChars</c>, <c>ReplaceSpecialChars</c>, <c>GetLine</c>).</summary>
public static class VmDehackedPatchLexer
{
    public static string ReadChars(string patch, ref int offset, int size)
    {
        if (size <= 0) return "";
        var buffer = new char[size];
        var written = 0;
        while (written < size && offset < patch.Length)
        {
            var ch = patch[offset++];
            if (ch == '\r') continue;
            buffer[written++] = ch;
        }

        return new string(buffer, 0, written);
    }

    public static string ReplaceSpecialChars(string input)
    {
        var output = new System.Text.StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c != '\\')
            {
                output.Append(c);
                continue;
            }

            if (++i >= input.Length) break;
            switch (input[i])
            {
                case 'n' or 'N':
                    output.Append('\n');
                    break;
                case 't' or 'T':
                    output.Append('\t');
                    break;
                case 'r' or 'R':
                    output.Append('\r');
                    break;
                case 'x' or 'X':
                    output.Append(ParseHexByte(input, ref i));
                    break;
                case >= '0' and <= '7':
                    output.Append(ParseOctalByte(input, ref i));
                    break;
                default:
                    output.Append(input[i]);
                    break;
            }
        }

        return output.ToString();
    }

    public static string SkipWhite(string line) => line.TrimStart();

    public static string StripWhite(string line) => line.TrimEnd();

    public enum PatchLineKind
    {
        None,
        KeyValue,
        TwoWord
    }

    public static bool TryParsePatchLine(string raw, out string line1, out string line2, out PatchLineKind kind)
    {
        line1 = "";
        line2 = "";
        kind = PatchLineKind.None;
        var trimmed = SkipWhite(raw);
        if (trimmed.Length == 0 || trimmed[0] == '#') return false;

        var eq = trimmed.IndexOf('=');
        if (eq >= 0)
        {
            line1 = trimmed[..eq].TrimEnd();
            if (line1.Length == 0) return false;
            line2 = SkipWhite(trimmed[(eq + 1)..]);
            if (line2.Length == 0) return false;
            kind = PatchLineKind.KeyValue;
            return true;
        }

        var space = trimmed.IndexOf(' ');
        if (space < 0) return false;
        line1 = trimmed[..space];
        line2 = SkipWhite(trimmed[(space + 1)..]);
        kind = PatchLineKind.TwoWord;
        return true;
    }

    private static char ParseHexByte(string input, ref int i)
    {
        var value = 0;
        for (var n = 0; n < 2 && i + 1 < input.Length; n++)
        {
            var p = input[++i];
            value <<= 4;
            if (p is >= '0' and <= '9') value += p - '0';
            else if (p is >= 'a' and <= 'f') value += 10 + p - 'a';
            else if (p is >= 'A' and <= 'F') value += 10 + p - 'A';
            else { i--; break; }
        }

        return (char)value;
    }

    private static char ParseOctalByte(string input, ref int i)
    {
        var value = 0;
        for (var n = 0; n < 3; n++)
        {
            var p = input[i];
            if (p is < '0' or > '7') break;
            value <<= 3;
            value += p - '0';
            if (n < 2 && i + 1 < input.Length) i++;
        }

        return (char)value;
    }
}
