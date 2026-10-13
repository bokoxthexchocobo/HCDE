using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace HCDE.Client;

public static class VmReplayGainIni
{
    private static readonly Regex Number = new(@"^[+-]?(?:(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?|infinity|inf|nan(?:\([a-z0-9_]*\))?)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex HexNumber = new(@"^(?<sign>[+-]?)0x(?<digits>(?:[0-9a-f]+(?:\.[0-9a-f]*)?|\.[0-9a-f]+))(?:p(?<exponent>[+-]?[0-9]+))?",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<KeyValuePair<string, float>> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var entries = new List<KeyValuePair<string, float>>();
        string? section = null;
        foreach (var raw in text.Replace("\r", "").Split('\n'))
        {
            var line = raw.Split('\0')[0].Trim();
            if (line.Length < 2 || line.StartsWith('#') || line.StartsWith("//", StringComparison.Ordinal)) continue;
            if (line[0] == '[')
            {
                section = line[1..];
                if (section.EndsWith(']')) section = section[..^1];
                continue;
            }
            if (section is null) throw new InvalidDataException("Gain configuration entry precedes a section.");
            var equal = line.IndexOf('=');
            if (equal <= 0) continue;
            var value = line[(equal + 1)..].TrimStart();
            if (value.StartsWith("<<<", StringComparison.Ordinal))
                throw new InvalidDataException("Multiline gain configuration is not supported.");
            if (!section.Equals("Gains", StringComparison.OrdinalIgnoreCase)) continue;
            entries.Add(new(line[..equal].TrimEnd(), ParseNumber(value)));
        }
        return entries.AsReadOnly();
    }

    internal static float ParseNumber(string value)
    {
        value = value.TrimStart(' ', '\t', '\r', '\n', '\v', '\f');
        var hex = HexNumber.Match(value);
        if (hex.Success) return ParseHex(hex);
        var match = Number.Match(value);
        var token = match.Value;
        var unsignedToken = token.TrimStart('+', '-');
        var gain = !match.Success ? 0 : unsignedToken.StartsWith("nan", StringComparison.OrdinalIgnoreCase)
            ? ParseNan(token, unsignedToken) : unsignedToken.StartsWith("inf", StringComparison.OrdinalIgnoreCase)
            ? token.StartsWith('-') ? float.NegativeInfinity : float.PositiveInfinity
            : (float)double.Parse(token, CultureInfo.InvariantCulture);
        return gain;
    }

    private static float ParseNan(string token, string unsignedToken)
    {
        var negative = token.StartsWith('-');
        var bits = 0x7fc00000;
        if (OperatingSystem.IsWindows())
        {
            if (unsignedToken.Equals("nan(ind)", StringComparison.OrdinalIgnoreCase))
                return BitConverter.Int32BitsToSingle(unchecked((int)0xffc00000));
            // UCRT's generic NaN payload becomes all-one mantissa bits after float conversion.
            if (!unsignedToken.Equals("nan(snan)", StringComparison.OrdinalIgnoreCase)) bits = 0x7fffffff;
        }
        if (negative) bits |= int.MinValue;
        return BitConverter.Int32BitsToSingle(bits);
    }

    private static float ParseHex(Match match)
    {
        var digits = match.Groups["digits"].Value;
        var point = digits.IndexOf('.');
        var fractionDigits = point < 0 ? 0 : digits.Length - point - 1;
        var integer = BigInteger.Parse("0" + digits.Replace(".", ""), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        var exponentToken = match.Groups["exponent"].Value;
        long exponent = 0;
        if (exponentToken.Length != 0 && !long.TryParse(exponentToken, NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out exponent)) exponent = exponentToken[0] == '-' ? long.MinValue : long.MaxValue;
        // Retain a rounded binary64 significand before applying the binary scale.
        var shift = (int)Math.Max(0, integer.GetBitLength() - 53);
        if (shift > 0)
        {
            var retained = integer >> shift;
            var remainder = integer - (retained << shift);
            var halfway = BigInteger.One << (shift - 1);
            if (remainder > halfway || remainder == halfway && !retained.IsEven) retained++;
            integer = retained;
        }
        var adjustment = shift - 4L * fractionDigits;
        var scale = exponent > long.MaxValue - Math.Max(0, adjustment) ? long.MaxValue
            : exponent < long.MinValue - Math.Min(0, adjustment) ? long.MinValue : exponent + adjustment;
        var result = (float)Math.ScaleB((double)integer, (int)Math.Clamp(scale, int.MinValue, int.MaxValue));
        return match.Groups["sign"].Value == "-" ? -result : result;
    }

    public static void ReadFileOnce(VmReplayGainCache cache, string path)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(path);
        cache.ReadOnce(() =>
        {
            string text;
            try { text = File.ReadAllText(path); }
            catch (FileNotFoundException) { return []; }
            catch (DirectoryNotFoundException) { return []; }
            return Parse(text);
        });
    }

    public static string Merge(string text, IReadOnlyList<KeyValuePair<string, string>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _ = Parse(text);
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        var start = lines.FindIndex(line => line.Trim().Equals("[Gains]", StringComparison.OrdinalIgnoreCase));
        if (start < 0)
        {
            if (lines[^1].Length != 0) lines.Add("");
            lines.Add("[Gains]");
            start = lines.Count - 1;
        }
        var end = start + 1;
        while (end < lines.Count && !lines[end].TrimStart().StartsWith('[')) end++;
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key) || entry.Key.IndexOfAny(['=', '\r', '\n', '\0']) >= 0
                || entry.Value.IndexOfAny(['\r', '\n', '\0']) >= 0)
                throw new InvalidDataException("Gain entry cannot be represented as a single INI line.");
            var match = -1;
            for (var i = start + 1; i < end; i++)
            {
                var line = lines[i].TrimStart();
                if (line.StartsWith('#') || line.StartsWith("//", StringComparison.Ordinal)) continue;
                var equal = line.IndexOf('=');
                if (equal > 0 && line[..equal].TrimEnd().Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                { match = i; break; }
            }
            var replacement = entry.Key + "=" + entry.Value;
            if (match >= 0) lines[match] = replacement;
            else { lines.Insert(end, replacement); end++; }
        }
        return string.Join("\n", lines).TrimEnd('\n') + "\n";
    }

    public static void SaveFile(VmReplayGainCache cache, string path)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(path);
        var text = "";
        try { text = File.ReadAllText(path); }
        catch (FileNotFoundException) { }
        cache.Save(entries => File.WriteAllText(path, Merge(text, entries)));
    }
}
