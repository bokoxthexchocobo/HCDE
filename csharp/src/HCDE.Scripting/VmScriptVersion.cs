using System.Text;

namespace HCDE.Scripting;

/// <summary>Reads the leading ZScript version directive for call-site compatibility.</summary>
public static class VmScriptVersion
{
    public static Version Read(string source, Version? maximumVersion = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        source = VmScriptText.Prepare(source);
        var position = 0;
        SkipTrivia(source, ref position);
        var start = position;
        while (position < source.Length && source[position] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_') position++;
        if (!source.AsSpan(start, position - start).Equals("version", StringComparison.OrdinalIgnoreCase))
            return CheckSupported(new Version(2, 3, 0), maximumVersion);
        SkipTrivia(source, ref position, classicString: true);
        if (position >= source.Length) throw new FormatException("Bad version directive.");
        string versionText;
        if (source[position] == '"')
        {
            position++;
            var text = new StringBuilder();
            while (position < source.Length && source[position] != '"')
            {
                if (source[position] == '\\' && position + 1 < source.Length && source[position + 1] == '"') position++;
                else if (source[position] == '\r' && position + 1 < source.Length && source[position + 1] == '\n') position++;
                text.Append(source[position++]);
            }
            if (position == source.Length) throw new FormatException("Bad version directive.");
            versionText = text.ToString();
        }
        else
        {
            start = position;
            while (position < source.Length && source[position] > ' ' && !"{}|=\";".Contains(source[position]))
            {
                if (source.AsSpan(position).StartsWith("//", StringComparison.Ordinal)
                    || source.AsSpan(position).StartsWith("/*", StringComparison.Ordinal)) break;
                position++;
            }
            versionText = source[start..position];
        }
        var parts = versionText.Split('.');
        if (parts.Length is < 2 or > 3) throw new FormatException("Bad version directive.");
        var numbers = new int[3];
        for (var i = 0; i < parts.Length; i++)
            numbers[i] = ReadComponent(parts[i]);
        // Native revision passes through int16_t into uint32_t; wrapped revisions cannot fit System.Version.
        if (numbers[2] > short.MaxValue) throw new FormatException("Wrapped script revisions are unsupported.");
        return CheckSupported(new Version(numbers[0], numbers[1], numbers[2]), maximumVersion);
    }

    private static int ReadComponent(string value)
    {
        if (value.Length == 0) return 0;
        var index = 0;
        while (index < value.Length && value[index] is ' ' or '\t' or '\n' or '\r' or '\v' or '\f') index++;
        var negative = index < value.Length && value[index] == '-';
        if (index < value.Length && value[index] is '+' or '-') index++;
        var firstDigit = index;
        var magnitude = 0;
        while (index < value.Length && value[index] is >= '0' and <= '9')
        {
            magnitude = Math.Min(ushort.MaxValue, magnitude * 10 + value[index++] - '0');
        }
        if (index == firstDigit || index != value.Length || magnitude == ushort.MaxValue || negative && magnitude != 0)
            throw new FormatException("Bad version directive.");
        return magnitude;
    }

    private static Version CheckSupported(Version version, Version? maximum)
    {
        if (maximum is not null && version > new Version(maximum.Major, maximum.Minor, Math.Max(0, maximum.Build)))
            throw new NotSupportedException($"Script version {version} exceeds supported version {maximum}.");
        return version;
    }

    private static void SkipTrivia(string source, ref int position, bool classicString = false)
    {
        while (position < source.Length)
        {
            if (source[position] <= ' ') { position++; continue; }
            if (source.AsSpan(position).StartsWith("//", StringComparison.Ordinal) || classicString && source[position] == ';')
            {
                var newline = source.IndexOf('\n', position + 1);
                position = newline < 0 ? source.Length : newline + 1;
                continue;
            }
            if (source.AsSpan(position).StartsWith("#region", StringComparison.Ordinal)
                || source.AsSpan(position).StartsWith("#endregion", StringComparison.Ordinal))
            {
                var newline = source.IndexOf('\n', position);
                if (newline < 0) break;
                position = newline + 1;
                continue;
            }
            if (source.AsSpan(position).StartsWith("/*", StringComparison.Ordinal))
            {
                var end = source.IndexOf("*/", position + 2, StringComparison.Ordinal);
                if (end < 0) throw new FormatException("Unterminated script comment.");
                position = end + 2;
                continue;
            }
            break;
        }
    }
}
