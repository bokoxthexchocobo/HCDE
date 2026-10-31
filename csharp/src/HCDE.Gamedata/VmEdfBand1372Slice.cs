using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>
/// Thing-header dehacked tail, reverb properties and header, and the
/// thing-block scan through the nested opener from <c>hcde_edf.cpp</c>
/// 757-976. Lines 729-756 and 977-980 stay chunk 18.
/// </summary>
public sealed class VmEdfBand1372Slice
{
    public int Properties;
    public int UnsupportedProperties;
    public int InvalidProperties;
    public int ThingDeltas;
    public int ThingTypes;
    public int NestedDepth;
    public List<(string Part, int Line)> Prepared { get; } = [];
    public List<Issue> Issues { get; } = [];
    public Func<int, string?>? FileName { get; set; }

    static readonly Dictionary<string, Action<Reverb, float>> FloatKeys = new()
    {
        ["roomsize"] = (reverb, value) => reverb.Room = value,
        ["damping"] = (reverb, value) => reverb.Damping = value,
        ["wetscale"] = (reverb, value) => reverb.wetscale = value,
        ["dryscale"] = (reverb, value) => reverb.dryscale = value,
        ["width"] = (reverb, value) => reverb.width = value,
        ["eq.lowfreq"] = (reverb, value) => reverb.LFReference = value,
        ["eq.highfreq"] = (reverb, value) => reverb.HFReference = value,
        ["eq.lowgain"] = (reverb, value) => reverb.eq_lowgain = value,
        ["eq.midgain"] = (reverb, value) => reverb.eq_midgain = value,
        ["eq.highgain"] = (reverb, value) => reverb.eq_highgain = value,
    };

    public void ParseThingHeader(Thing thing, string header)
    {
        var brace = header.IndexOf('{');
        var tokens = Tokenize(brace < 0 ? header : header[..brace]);
        if (tokens.Count < 2)
        {
            return;
        }

        thing.Name = tokens[1];
        for (var i = 2; i < tokens.Count; i++)
        {
            if (tokens[i] != ":")
            {
                continue;
            }

            if (i + 1 < tokens.Count)
            {
                thing.BaseName = tokens[i + 1];
            }

            if (FirstInt(tokens, i + 2, out var value))
            {
                thing.DoomedNum = value;
                for (var j = i + 2; j < tokens.Count; j++)
                {
                    if (!StrictInt(tokens[j], out value))
                    {
                        continue;
                    }

                    for (var k = j + 1; k < tokens.Count; k++)
                    {
                        if (StrictInt(tokens[k], out value))
                        {
                            thing.DehackedNum = value;
                            return;
                        }
                    }

                    return;
                }
            }

            return;
        }
    }

    public void ParseReverbProperty(Reverb reverb, string text)
    {
        var tokens = Tokenize(text);
        if (tokens.Count == 0)
        {
            return;
        }

        var key = Lower(tokens[0]);
        if (key.Length == 0)
        {
            return;
        }

        Properties++;
        if (key == "+equalized")
        {
            reverb.DecayHFLimit = 1f;
        }
        else if (key == "-equalized")
        {
            reverb.DecayHFLimit = 0f;
        }
        else if (key == "predelay")
        {
            if (FirstInt(tokens, 1, out var delay))
            {
                reverb.predelay = delay;
            }
        }
        else if (FloatKeys.TryGetValue(key, out var apply))
        {
            if (tokens.Count >= 2)
            {
                apply(reverb, Stoff(tokens[1]));
            }
        }
        else
        {
            UnsupportedProperties++;
            RecordIssue("reverb", reverb.Name + " property '" + key + "' is not supported", reverb.SourceLump, reverb.Line);
        }
    }

    public void ParseReverbHeader(Reverb reverb, string header)
    {
        var brace = header.IndexOf('{');
        var tokens = Tokenize(brace < 0 ? header : header[..brace]);
        if (tokens.Count < 2)
        {
            return;
        }

        reverb.Name = tokens[1];
        var colon = false;
        for (var i = 2; i < tokens.Count; i++)
        {
            if (tokens[i] == ":")
            {
                colon = true;
                continue;
            }

            if (!colon || !StrictInt(tokens[i], out var id))
            {
                continue;
            }

            if (reverb.Id1 == -1)
            {
                reverb.Id1 = id;
            }
            else if (reverb.Id2 == -1)
            {
                reverb.Id2 = id;
                break;
            }
        }
    }

    public Thing? ParseThingBlock(string header, IReadOnlyList<SourceLine> body, int lump, int headerLine, bool isDelta)
    {
        var thing = new Thing { SourceLump = lump, Line = headerLine, IsDelta = isDelta };
        ParseThingHeader(thing, header);
        if (thing.Name.Length == 0)
        {
            InvalidProperties++;
            RecordIssue(isDelta ? "thingdelta" : "thingtype", "missing thing name", lump, thing.Line);
            return null;
        }

        if (isDelta)
        {
            ThingDeltas++;
        }
        else
        {
            ThingTypes++;
        }

        Prepared.Clear();
        NestedDepth = 0;
        var inHeredoc = false;
        foreach (var line in body)
        {
            if (TryPrepare(ref inHeredoc, line.Text, out var part))
            {
                Prepared.Add((part, line.Line));
            }
        }

        return thing;
    }

    public bool TryPrepare(ref bool inHeredoc, string text, out string part)
    {
        var trimmed = Trim(text);
        part = "";
        if (trimmed.Length == 0)
        {
            return false;
        }

        if (inHeredoc)
        {
            if (trimmed.Contains("\"@", StringComparison.Ordinal))
            {
                inHeredoc = false;
            }

            return false;
        }

        var open = trimmed.IndexOf('{');
        part = open < 0 ? trimmed : trimmed[..open];
        return true;
    }

    public void RecordIssue(string? context, string detail, int lump, int line)
    {
        if (Issues.Count >= 20)
        {
            return;
        }

        string source;
        if (lump < 0)
        {
            source = "<unknown>";
        }
        else if (FileName is null)
        {
            throw new InvalidOperationException("GetFileFullName");
        }
        else
        {
            source = FileName(lump) ?? "<unknown>";
        }

        Issues.Add(new Issue { Source = source, Context = context ?? "<unknown>", Detail = detail, Line = line });
    }

    public static string Trim(string value)
    {
        var first = 0;
        while (first < value.Length && IsSpace(value[first]))
        {
            first++;
        }

        var last = value.Length;
        while (last > first && IsSpace(value[last - 1]))
        {
            last--;
        }

        return value[first..last];
    }

    public static string Lower(string value)
    {
        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= 'A' and <= 'Z')
            {
                chars[i] = (char)(chars[i] + 32);
            }
        }

        return new string(chars);
    }

    public static List<string> Tokenize(string value)
    {
        var tokens = new List<string>();
        var current = "";
        var inQuote = false;
        var quote = '\0';
        var escape = false;
        foreach (var ch in value)
        {
            if (escape)
            {
                current += ch;
                escape = false;
                continue;
            }

            if (inQuote)
            {
                if (ch == '\\')
                {
                    escape = true;
                }
                else if (ch == quote)
                {
                    inQuote = false;
                }
                else
                {
                    current += ch;
                }

                continue;
            }

            if (ch is '"' or '\'')
            {
                inQuote = true;
                quote = ch;
                continue;
            }

            if (IsSpace(ch) || ch is '{' or '}' or '(' or ')' or ',' or '=' or ';' || ch == ':')
            {
                if (current.Length != 0)
                {
                    tokens.Add(current);
                    current = "";
                }

                if (ch == ':')
                {
                    tokens.Add(":");
                }

                continue;
            }

            current += ch;
        }

        if (current.Length != 0)
        {
            tokens.Add(current);
        }

        return tokens;
    }

    public static bool StrictInt(string value, out int result)
    {
        result = 0;
        if (value.Length == 0)
        {
            return false;
        }

        var i = 0;
        while (i < value.Length && IsSpace(value[i]))
        {
            i++;
        }

        if (i >= value.Length)
        {
            return false;
        }

        var neg = value[i] == '-';
        if (value[i] is '+' or '-')
        {
            i++;
        }

        if (i >= value.Length)
        {
            return false;
        }

        var basis = 10;
        if (value[i] == '0')
        {
            if (i + 1 < value.Length && value[i + 1] is 'x' or 'X')
            {
                basis = 16;
                i += 2;
                if (i >= value.Length || Digit(value[i]) < 0)
                {
                    return false;
                }
            }
            else
            {
                basis = 8;
            }
        }

        long acc = 0;
        var any = false;
        for (; i < value.Length; i++)
        {
            var digit = Digit(value[i]);
            if (digit < 0 || digit >= basis)
            {
                break;
            }

            if (acc > (long.MaxValue - digit) / basis)
            {
                return false;
            }

            any = true;
            acc = (acc * basis) + digit;
        }

        if (!any || i != value.Length)
        {
            return false;
        }

        if (neg)
        {
            acc = -acc;
        }

        if (acc < int.MinValue || acc > int.MaxValue)
        {
            return false;
        }

        result = (int)acc;
        return true;
    }

    public static bool FirstInt(IReadOnlyList<string> tokens, int start, out int result)
    {
        for (var i = start; i < tokens.Count; i++)
        {
            if (StrictInt(tokens[i], out result))
            {
                return true;
            }
        }

        result = 0;
        return false;
    }

    public static float Stoff(string text)
    {
        var i = 0;
        while (i < text.Length && IsSpace(text[i]))
        {
            i++;
        }

        if (i >= text.Length)
        {
            throw new FormatException("stof");
        }

        var start = i;
        if (text[i] is '+' or '-')
        {
            i++;
        }

        var saw = false;
        while (i < text.Length && text[i] is >= '0' and <= '9')
        {
            saw = true;
            i++;
        }

        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && text[i] is >= '0' and <= '9')
            {
                saw = true;
                i++;
            }
        }

        if (i < text.Length && text[i] is 'e' or 'E')
        {
            var exp = i + 1;
            if (exp < text.Length && text[exp] is '+' or '-')
            {
                exp++;
            }

            var digits = exp;
            while (exp < text.Length && text[exp] is >= '0' and <= '9')
            {
                exp++;
            }

            if (exp > digits)
            {
                i = exp;
            }
        }

        if (!saw || !float.TryParse(text[start..i], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || float.IsInfinity(value))
        {
            throw saw ? new OverflowException("stof") : new FormatException("stof");
        }

        return value;
    }

    static bool IsSpace(char ch) => ch is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';

    static int Digit(char ch)
    {
        if (ch is >= '0' and <= '9')
        {
            return ch - '0';
        }

        if (ch is >= 'a' and <= 'f')
        {
            return ch - 'a' + 10;
        }

        if (ch is >= 'A' and <= 'F')
        {
            return ch - 'A' + 10;
        }

        return -1;
    }

    public sealed class Thing
    {
        public string Name = "";
        public string BaseName = "";
        public int DoomedNum = -1;
        public int DehackedNum = -1;
        public int SourceLump = -1;
        public int Line;
        public bool IsDelta;
    }

    public sealed class Reverb
    {
        public string Name = "";
        public int Id1 = -1;
        public int Id2 = -1;
        public int SourceLump = -1;
        public int Line;
        public float Room = 0.5f;
        public float Damping = 1f;
        public float DecayHFLimit = 1f;
        public float HFReference = 5000f;
        public float LFReference = 250f;
        public float wetscale = 0.33333334f;
        public float dryscale;
        public float width = 1f;
        public int predelay;
        public float eq_lowgain = 1f;
        public float eq_midgain = 1f;
        public float eq_highgain = 1f;
    }

    public sealed class SourceLine
    {
        public string Text = "";
        public int Line;
    }

    public sealed class Issue
    {
        public string Source = "";
        public string Context = "";
        public string Detail = "";
        public int Line;
    }
}
