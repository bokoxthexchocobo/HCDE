using System.Text;

namespace HCDE.MapLoader;

internal static class UdmfAdditionalIds
{
    internal static IReadOnlyList<int> Parse(string text, int primary, bool sector)
    {
        var ids = new List<int>();
        foreach (var token in Tokens(text))
        {
            if (!TryNumber(token, out var id)) break;
            if (id != 0 && (sector || id != -1) && id != primary && !ids.Contains(id)) ids.Add(id);
        }
        return ids.AsReadOnly();
    }

    // FScanner defaults to classic Hexen tokenization for the nested tag string.
    private static IEnumerable<string> Tokens(string text)
    {
        // PrepareScript strips a UTF-8 BOM and guarantees a terminal newline.
        if (text.Length > 1 && text[0] == '\uFEFF') text = text[1..];
        if (!text.EndsWith('\n'))
            text = text.EndsWith('\0') ? text[..^1] + "\n" : text + "\n";
        var offset = 0;
        while (offset < text.Length)
        {
            if (text[offset] <= ' ') { offset++; continue; }
            var rest = text.AsSpan(offset);
            if (text[offset] == ';' || rest.StartsWith("//", StringComparison.Ordinal)
                || rest.StartsWith("#region", StringComparison.Ordinal) || rest.StartsWith("#endregion", StringComparison.Ordinal))
            {
                var end = text.IndexOf('\n', offset);
                offset = end < 0 ? text.Length : end + 1;
                continue;
            }
            if (rest.StartsWith("/*", StringComparison.Ordinal))
            {
                var end = text.IndexOf("*/", offset + 2, StringComparison.Ordinal);
                if (end < 0) yield break;
                offset = end + 2;
                continue;
            }
            if (text[offset] == '"')
            {
                var token = new StringBuilder();
                offset++;
                while (offset < text.Length && text[offset] != '"')
                {
                    if (text[offset] == '\\' && offset + 1 < text.Length && text[offset + 1] == '"') offset++;
                    if (text[offset] == '\r' && offset + 1 < text.Length && text[offset + 1] == '\n') offset++;
                    token.Append(text[offset++]);
                }
                if (offset < text.Length) offset++;
                yield return token.ToString();
                continue;
            }
            var start = offset;
            while (offset < text.Length && text[offset] > ' ' && text[offset] is not ('{' or '}' or '|' or '=' or '"' or ';'))
            {
                if (text[offset] == '/' && (offset + 1 == text.Length || text[offset + 1] <= ' '
                    || text[offset + 1] is '/' or '*' or '{' or '}' or '|' or '=' or '"' or ';')) break;
                offset++;
            }
            if (offset == start) offset++;
            yield return text[start..offset];
        }
    }

    // strtoll(base 0) saturates at signed 64-bit limits before native casts to int.
    private static bool TryNumber(string token, out int number)
    {
        number = 0;
        if (token == "MAXINT") { number = int.MaxValue; return true; }
        var offset = 0;
        while (offset < token.Length && token[offset] is ' ' or '\t' or '\n' or '\r' or '\v' or '\f') offset++;
        if (offset == token.Length) return false;
        var negative = token[offset] == '-';
        if (token[offset] is '-' or '+') offset++;
        if (offset == token.Length) return false;
        var radix = token[offset] == '0' ? 8 : 10;
        if (offset + 1 < token.Length && token[offset] == '0' && token[offset + 1] is 'x' or 'X')
        { radix = 16; offset += 2; }
        if (offset == token.Length) return false;
        var limit = negative ? 1UL << 63 : (ulong)long.MaxValue;
        ulong value = 0;
        for (; offset < token.Length; offset++)
        {
            var c = token[offset];
            var digit = c is >= '0' and <= '9' ? c - '0'
                : c is >= 'a' and <= 'f' ? c - 'a' + 10 : c is >= 'A' and <= 'F' ? c - 'A' + 10 : -1;
            if (digit < 0 || digit >= radix) return false;
            value = value > (limit - (ulong)digit) / (ulong)radix ? limit : value * (ulong)radix + (ulong)digit;
        }
        var signed = negative ? unchecked(-(long)value) : (long)value;
        number = unchecked((int)signed);
        return true;
    }
}
