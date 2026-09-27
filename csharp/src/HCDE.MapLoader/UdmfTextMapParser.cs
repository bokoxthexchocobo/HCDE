using System.Globalization;
using System.Text;

namespace HCDE.MapLoader;

public sealed class UdmfVertex
{
    public double X { get; set; }
    public double Y { get; set; }
}

public sealed class UdmfLinedef
{
    public int V1 { get; set; }
    public int V2 { get; set; }
    public int SideFront { get; set; } = -1;
    public int SideBack { get; set; } = -1;
    public int Special { get; set; }
    public int Id { get; set; }
    public bool Blocking { get; set; }
    public bool BlockEverything { get; set; }
    public bool BlockSight { get; set; }
    public bool BlockHitscan { get; set; }
    public bool BlockProjectiles { get; set; }
    public bool BlockSound { get; set; }
    public bool TwoSided { get; set; }
    public bool PlayerCross { get; set; }
    public bool PlayerUse { get; set; }
    public bool PassUse { get; set; }
    public bool RepeatSpecial { get; set; }
    public int Arg0 { get; set; }
    public int Arg1 { get; set; }
    public int Arg2 { get; set; }
    public int Arg3 { get; set; }
    public int Arg4 { get; set; }
}

public sealed class UdmfSidedef
{
    public int Sector { get; set; }
    public string TextureTop { get; set; } = "-";
    public string TextureBottom { get; set; } = "-";
    public string TextureMiddle { get; set; } = "-";
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}

public sealed class UdmfSector
{
    public double HeightFloor { get; set; }
    public double HeightCeiling { get; set; }
    public string TextureFloor { get; set; } = "-";
    public string TextureCeiling { get; set; } = "-";
    public int LightLevel { get; set; }
    public int Special { get; set; }
    public int Id { get; set; }
}

public sealed class UdmfThing
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Height { get; set; }
    public double Angle { get; set; }
    public int Type { get; set; }
    public int Id { get; set; }
    public int Special { get; set; }
    public bool Skill1 { get; set; }
    public bool Ambush { get; set; }
    public bool Skill2 { get; set; }
    public bool Skill3 { get; set; }
    public bool Skill4 { get; set; }
    public bool Skill5 { get; set; }
    public bool Single { get; set; }
    public bool Coop { get; set; }
    public bool Dm { get; set; }
    public int Arg0 { get; set; }
    public int Arg1 { get; set; }
    public int Arg2 { get; set; }
    public int Arg3 { get; set; }
    public int Arg4 { get; set; }
}

public sealed class UdmfTextMap
{
    public string Namespace { get; init; } = "";
    public IReadOnlyList<UdmfVertex> Vertices { get; init; } = Array.Empty<UdmfVertex>();
    public IReadOnlyList<UdmfLinedef> Linedefs { get; init; } = Array.Empty<UdmfLinedef>();
    public IReadOnlyList<UdmfSidedef> Sidedefs { get; init; } = Array.Empty<UdmfSidedef>();
    public IReadOnlyList<UdmfSector> Sectors { get; init; } = Array.Empty<UdmfSector>();
    public IReadOnlyList<UdmfThing> Things { get; init; } = Array.Empty<UdmfThing>();
}

/// <summary>
/// UDMF TEXTMAP reader for the standard vertex, linedef, sidedef, sector, and thing blocks.
/// Unknown keys and unknown blocks are skipped. Mirrors the assignment grammar in <c>udmf.cpp</c>.
/// </summary>
public static class UdmfTextMapParser
{
    public static bool TryParse(ReadOnlySpan<byte> utf8, out UdmfTextMap map, out string? error)
    {
        var text = Encoding.UTF8.GetString(utf8);
        return TryParse(text, out map, out error);
    }

    public static bool TryParse(string text, out UdmfTextMap map, out string? error)
    {
        map = new UdmfTextMap();
        error = null;
        var parser = new Parser(text);
        try { return parser.TryParse(out map, out error); }
        catch (OverflowException)
        {
            map = new UdmfTextMap();
            error = "udmf-integer-out-of-range-or-fraction";
            return false;
        }
    }

    private sealed class Parser
    {
        private readonly string _text;
        private int _index;

        public Parser(string text) => _text = text ?? string.Empty;

        public bool TryParse(out UdmfTextMap map, out string? error)
        {
            var namespaceName = "";
            var vertices = new List<UdmfVertex>();
            var linedefs = new List<UdmfLinedef>();
            var sidedefs = new List<UdmfSidedef>();
            var sectors = new List<UdmfSector>();
            var things = new List<UdmfThing>();

            while (true)
            {
                SkipTrivia();
                if (Eof)
                    break;

                if (!TryReadIdentifier(out var name))
                {
                    map = new UdmfTextMap();
                    error = $"Expected identifier at {_index}.";
                    return false;
                }

                SkipTrivia();
                if (Peek('='))
                {
                    _index++;
                    if (!TryReadValue(out var value, out error))
                    {
                        map = new UdmfTextMap();
                        return false;
                    }

                    if (!Expect(';'))
                    {
                        map = new UdmfTextMap();
                        error = "Expected ';' after assignment.";
                        return false;
                    }

                    if (name.Equals("namespace", StringComparison.OrdinalIgnoreCase) && value.Text is not null)
                        namespaceName = value.Text;
                    continue;
                }

                if (!Peek('{'))
                {
                    map = new UdmfTextMap();
                    error = $"Expected '=' or '{{' after '{name}'.";
                    return false;
                }

                if (!TryReadBlock(out var fields, out error))
                {
                    map = new UdmfTextMap();
                    return false;
                }

                switch (name.ToLowerInvariant())
                {
                    case "vertex":
                        vertices.Add(ReadVertex(fields));
                        break;
                    case "linedef":
                        linedefs.Add(ReadLinedef(fields));
                        break;
                    case "sidedef":
                        sidedefs.Add(ReadSidedef(fields));
                        break;
                    case "sector":
                        sectors.Add(ReadSector(fields));
                        break;
                    case "thing":
                        things.Add(ReadThing(fields));
                        break;
                }
            }

            map = new UdmfTextMap
            {
                Namespace = namespaceName,
                Vertices = vertices,
                Linedefs = linedefs,
                Sidedefs = sidedefs,
                Sectors = sectors,
                Things = things,
            };
            error = null;
            return true;
        }

        private bool TryReadBlock(out Dictionary<string, Value> fields, out string? error)
        {
            fields = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            error = null;
            if (!Expect('{'))
            {
                error = "Expected '{'.";
                return false;
            }

            while (true)
            {
                SkipTrivia();
                if (Peek('}'))
                {
                    _index++;
                    return true;
                }

                if (Eof)
                {
                    error = "Unterminated block.";
                    return false;
                }

                if (!TryReadIdentifier(out var key))
                {
                    error = "Expected field name inside block.";
                    return false;
                }

                SkipTrivia();
                if (!Expect('='))
                {
                    error = $"Expected '=' after '{key}'.";
                    return false;
                }

                if (!TryReadValue(out var value, out error))
                    return false;

                if (!Expect(';'))
                {
                    error = $"Expected ';' after '{key}'.";
                    return false;
                }

                fields[key] = value;
            }
        }

        private static UdmfVertex ReadVertex(Dictionary<string, Value> fields) => new()
        {
            X = Number(fields, "x"),
            Y = Number(fields, "y"),
        };

        private static UdmfLinedef ReadLinedef(Dictionary<string, Value> fields) => new()
        {
            V1 = Int(fields, "v1"),
            V2 = Int(fields, "v2"),
            SideFront = Int(fields, "sidefront", -1),
            SideBack = Int(fields, "sideback", -1),
            Special = Int(fields, "special"),
            Id = Int(fields, "id"),
            Blocking = Bool(fields, "blocking"),
            BlockEverything = Bool(fields, "blockeverything"),
            BlockSight = Bool(fields, "blocksight"),
            BlockHitscan = Bool(fields, "blockhitscan"),
            BlockProjectiles = Bool(fields, "blockprojectiles"),
            BlockSound = Bool(fields, "blocksound"),
            TwoSided = Bool(fields, "twosided"),
            PlayerCross = Bool(fields, "playercross"),
            PlayerUse = Bool(fields, "playeruse"),
            PassUse = Bool(fields, "passuse"),
            RepeatSpecial = Bool(fields, "repeatspecial"),
            Arg0 = Int(fields, "arg0"),
            Arg1 = Int(fields, "arg1"),
            Arg2 = Int(fields, "arg2"),
            Arg3 = Int(fields, "arg3"),
            Arg4 = Int(fields, "arg4"),
        };

        private static UdmfSidedef ReadSidedef(Dictionary<string, Value> fields) => new()
        {
            Sector = Int(fields, "sector"),
            TextureTop = Text(fields, "texturetop", "-"),
            TextureBottom = Text(fields, "texturebottom", "-"),
            TextureMiddle = Text(fields, "texturemiddle", "-"),
            OffsetX = Int(fields, "offsetx"),
            OffsetY = Int(fields, "offsety"),
        };

        private static UdmfSector ReadSector(Dictionary<string, Value> fields) => new()
        {
            HeightFloor = Number(fields, "heightfloor"),
            HeightCeiling = Number(fields, "heightceiling"),
            TextureFloor = Text(fields, "texturefloor", "-"),
            TextureCeiling = Text(fields, "textureceiling", "-"),
            LightLevel = Int(fields, "lightlevel"),
            Special = Int(fields, "special"),
            Id = Int(fields, "id"),
        };

        private static UdmfThing ReadThing(Dictionary<string, Value> fields) => new()
        {
            X = Number(fields, "x"),
            Y = Number(fields, "y"),
            Height = Number(fields, "height"),
            Angle = Number(fields, "angle"),
            Type = Int(fields, "type"),
            Id = Int(fields, "id"),
            Special = Int(fields, "special"),
            Skill1 = Bool(fields, "skill1"),
            Ambush = Bool(fields, "ambush"),
            Skill2 = Bool(fields, "skill2"),
            Skill3 = Bool(fields, "skill3"),
            Skill4 = Bool(fields, "skill4"),
            Skill5 = Bool(fields, "skill5"),
            Single = Bool(fields, "single"),
            Coop = Bool(fields, "coop"),
            Dm = Bool(fields, "dm"),
            Arg0 = Int(fields, "arg0"),
            Arg1 = Int(fields, "arg1"),
            Arg2 = Int(fields, "arg2"),
            Arg3 = Int(fields, "arg3"),
            Arg4 = Int(fields, "arg4"),
        };

        private static double Number(Dictionary<string, Value> fields, string key) =>
            fields.TryGetValue(key, out var value) ? value.Number : 0;

        private static int Int(Dictionary<string, Value> fields, string key, int fallback = 0)
        {
            if (!fields.TryGetValue(key, out var value)) return fallback;
            if (!double.IsFinite(value.Number) || value.Number != Math.Truncate(value.Number)) throw new OverflowException();
            return checked((int)value.Number);
        }

        private static bool Bool(Dictionary<string, Value> fields, string key, bool fallback = false) =>
            fields.TryGetValue(key, out var value) ? value.Boolean : fallback;

        private static string Text(Dictionary<string, Value> fields, string key, string fallback) =>
            fields.TryGetValue(key, out var value) && value.Text is not null ? value.Text : fallback;

        private bool TryReadValue(out Value value, out string? error)
        {
            SkipTrivia();
            value = default;
            error = null;
            if (Eof)
            {
                error = "Expected value.";
                return false;
            }

            if (Peek('"'))
            {
                if (!TryReadString(out var text))
                {
                    error = "Unterminated string.";
                    return false;
                }

                value = Value.FromText(text);
                return true;
            }

            if (StartsWithKeyword("true"))
            {
                _index += 4;
                value = Value.FromBool(true);
                return true;
            }

            if (StartsWithKeyword("false"))
            {
                _index += 5;
                value = Value.FromBool(false);
                return true;
            }

            if (!TryReadNumber(out var number))
            {
                error = "Expected number, string, or boolean.";
                return false;
            }

            value = Value.FromNumber(number);
            return true;
        }

        private bool TryReadNumber(out double number)
        {
            number = 0;
            var start = _index;
            if (Peek('+') || Peek('-'))
                _index++;

            var digits = false;
            while (!Eof && char.IsDigit(_text[_index]))
            {
                digits = true;
                _index++;
            }

            if (Peek('.'))
            {
                _index++;
                while (!Eof && char.IsDigit(_text[_index]))
                {
                    digits = true;
                    _index++;
                }
            }

            if (Peek('e') || Peek('E'))
            {
                var exponent = _index;
                _index++;
                if (Peek('+') || Peek('-'))
                    _index++;
                var expDigits = false;
                while (!Eof && char.IsDigit(_text[_index]))
                {
                    expDigits = true;
                    _index++;
                }

                if (!expDigits)
                    _index = exponent;
            }

            if (!digits)
            {
                _index = start;
                return false;
            }

            return double.TryParse(
                _text[start.._index],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number);
        }

        private bool TryReadString(out string text)
        {
            text = "";
            if (!Expect('"'))
                return false;

            var builder = new StringBuilder();
            while (!Eof)
            {
                var ch = _text[_index++];
                if (ch == '"')
                {
                    text = builder.ToString();
                    return true;
                }

                if (ch == '\\' && !Eof)
                {
                    builder.Append(_text[_index++]);
                    continue;
                }

                builder.Append(ch);
            }

            return false;
        }

        private bool TryReadIdentifier(out string name)
        {
            name = "";
            if (Eof || !IsIdentStart(_text[_index]))
                return false;

            var start = _index;
            _index++;
            while (!Eof && IsIdentPart(_text[_index]))
                _index++;

            name = _text[start.._index];
            return true;
        }

        private bool StartsWithKeyword(string keyword)
        {
            if (_index + keyword.Length > _text.Length)
                return false;
            if (!string.Equals(_text.Substring(_index, keyword.Length), keyword, StringComparison.Ordinal))
                return false;
            var end = _index + keyword.Length;
            return end >= _text.Length || !IsIdentPart(_text[end]);
        }

        private void SkipTrivia()
        {
            while (!Eof)
            {
                var ch = _text[_index];
                if (char.IsWhiteSpace(ch))
                {
                    _index++;
                    continue;
                }

                if (ch == '/' && _index + 1 < _text.Length && _text[_index + 1] == '/')
                {
                    _index += 2;
                    while (!Eof && _text[_index] != '\n')
                        _index++;
                    continue;
                }

                if (ch == '/' && _index + 1 < _text.Length && _text[_index + 1] == '*')
                {
                    _index += 2;
                    while (_index + 1 < _text.Length && !(_text[_index] == '*' && _text[_index + 1] == '/'))
                        _index++;
                    if (_index + 1 < _text.Length)
                        _index += 2;
                    else
                        _index = _text.Length;
                    continue;
                }

                return;
            }
        }

        private bool Expect(char ch)
        {
            SkipTrivia();
            if (!Peek(ch))
                return false;
            _index++;
            return true;
        }

        private bool Peek(char ch) => !Eof && _text[_index] == ch;

        private bool Eof => _index >= _text.Length;

        private static bool IsIdentStart(char ch) => char.IsLetter(ch) || ch == '_';

        private static bool IsIdentPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
    }

    private readonly struct Value
    {
        public double Number { get; init; }
        public bool Boolean { get; init; }
        public string? Text { get; init; }

        public static Value FromNumber(double number) => new() { Number = number };
        public static Value FromBool(bool value) => new() { Boolean = value };
        public static Value FromText(string text) => new() { Text = text };
    }
}
