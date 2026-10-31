namespace HCDE.Client;

/// <summary>
/// Raven colorset arm, <c>ParseAMColors</c>, the thin-triangle glyph, arrow-lump scale, and
/// the <c>AM_StaticInit</c> clears from <c>am_map.cpp</c> (<b>751–900</b>). Chunk <b>1126</b>.
/// </summary>
public static class VmAmMapColorParse1126Slice
{
    public const double PlayerRadius = 16;
    public const int ArrowScale = 18;

    public static int ArrowRadiusUnits()
    {
        return (int)((8 * PlayerRadius) / 7);
    }

    public static double ScaleArrowComponent(double component)
    {
        return component * ArrowRadiusUnits();
    }

    public readonly record struct Segment(double Ax, double Ay, double Bx, double By);

    public static Segment[] ThinTriangle() =>
    [
        new(-0.5, -0.7, 1, 0),
        new(1, 0, -0.5, 0.7),
        new(-0.5, 0.7, -0.5, -0.7)
    ];

    public static void ClearStaticArrows(
        List<Segment> mapArrow,
        List<Segment> cheatMapArrow,
        List<Segment> cheatKey,
        List<Segment> easyKey)
    {
        mapArrow.Clear();
        cheatMapArrow.Clear();
        cheatKey.Clear();
        easyKey.Clear();
    }

    public static List<Segment> ParseArrowLump(string? lumpText)
    {
        var arrow = new List<Segment>();
        if (string.IsNullOrEmpty(lumpText))
            return arrow;

        var i = 0;
        while (Skip(lumpText, ref i))
        {
            Expect(lumpText, ref i, '(');
            var ax = ReadFloat(lumpText, ref i);
            Expect(lumpText, ref i, ',');
            var ay = ReadFloat(lumpText, ref i);
            Expect(lumpText, ref i, ')');
            Expect(lumpText, ref i, ',');
            Expect(lumpText, ref i, '(');
            var bx = ReadFloat(lumpText, ref i);
            Expect(lumpText, ref i, ',');
            var by = ReadFloat(lumpText, ref i);
            Expect(lumpText, ref i, ')');
            var scale = ArrowRadiusUnits();
            arrow.Add(new Segment(ax * scale, ay * scale, bx * scale, by * scale));
        }

        return arrow;
    }

    public enum BasePalette
    {
        None,
        Doom,
        Raven,
        Strife
    }

    public sealed class ColorsetEdit
    {
        public bool Defined { get; set; }
        public bool Whitened { get; set; }
        public bool DisplayLocks { get; set; }
        public BasePalette Base { get; set; }
        public bool AnyColorWritten { get; set; }
        public Dictionary<int, uint> Colors { get; } = new();
        public string? Error { get; set; }
    }

    public static (string Palette, bool ShowLocks) RavenArm()
    {
        return ("raven", true);
    }

    public static IReadOnlyList<string> ColorNames() =>
    [
        "Background", "YourColor", "WallColor", "TwoSidedWallColor", "FloorDiffWallColor",
        "CeilingDiffWallColor", "ExtraFloorWallColor", "ThingColor", "ThingColor_Item",
        "ThingColor_CountItem", "ThingColor_Monster", "ThingColor_NocountMonster",
        "ThingColor_Friend", "SpecialWallColor", "SecretWallColor", "GridColor", "XHairColor",
        "NotSeenColor", "LockedColor", "IntraTeleportColor", "InterTeleportColor",
        "SecretSectorColor", "UnexploredSecretColor", "SectorFillColor", "PortalColor",
        "AlmostBackgroundColor"
    ];

    public static ColorsetEdit ParseAmColors(IReadOnlyList<string> tokens, Func<string, uint> resolveColor)
    {
        var edit = new ColorsetEdit { Defined = true, Whitened = true };
        var index = 0;
        if (!TryTake(tokens, ref index, out var open) || open != "{")
        {
            edit.Error = "expected '{'";
            return edit;
        }

        while (TryTake(tokens, ref index, out var token))
        {
            if (token == "}")
                return edit;

            if (!TryTake(tokens, ref index, out var eq) || eq != "=")
            {
                edit.Error = "expected '='";
                return edit;
            }

            if (token.Equals("base", StringComparison.OrdinalIgnoreCase))
            {
                if (edit.AnyColorWritten)
                {
                    edit.Error = "'base' must be specified before the first color";
                    return edit;
                }

                if (!TryTake(tokens, ref index, out var baseName))
                {
                    edit.Error = "expected base name";
                    return edit;
                }

                if (baseName.Equals("doom", StringComparison.OrdinalIgnoreCase))
                {
                    edit.Base = BasePalette.Doom;
                    edit.DisplayLocks = false;
                    edit.Whitened = false;
                }
                else if (baseName.Equals("raven", StringComparison.OrdinalIgnoreCase))
                {
                    edit.Base = BasePalette.Raven;
                    edit.DisplayLocks = true;
                    edit.Whitened = false;
                }
                else if (baseName.Equals("strife", StringComparison.OrdinalIgnoreCase))
                {
                    edit.Base = BasePalette.Strife;
                    edit.DisplayLocks = false;
                    edit.Whitened = false;
                }
                else
                {
                    edit.Error = "Unknown value for 'base'. Must be 'Doom', 'Strife' or 'Raven'.";
                    return edit;
                }
            }
            else if (token.Equals("showlocks", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryTake(tokens, ref index, out var flag))
                {
                    edit.Error = "expected showlocks flag";
                    return edit;
                }

                if (flag.Equals("false", StringComparison.OrdinalIgnoreCase))
                    edit.DisplayLocks = false;
                else if (flag.Equals("true", StringComparison.OrdinalIgnoreCase))
                    edit.DisplayLocks = true;
                else
                {
                    edit.Error = "expected true or false";
                    return edit;
                }
            }
            else
            {
                var colorIndex = -1;
                var names = ColorNames();
                for (var i = 0; i < names.Count; i++)
                {
                    if (token.Equals(names[i], StringComparison.OrdinalIgnoreCase))
                    {
                        colorIndex = i;
                        break;
                    }
                }

                if (colorIndex < 0)
                {
                    edit.Error = $"Unknown key '{token}'";
                    return edit;
                }

                if (!TryTake(tokens, ref index, out var colorText))
                {
                    edit.Error = "expected color";
                    return edit;
                }

                var resolved = resolveColor(colorText);
                edit.Colors[colorIndex] = PackOpaque(resolved);
                edit.AnyColorWritten = true;
            }
        }

        edit.Error = "expected '}'";
        return edit;
    }

    public static uint PackOpaque(uint rgb)
    {
        var r = (rgb >> 16) & 0xff;
        var g = (rgb >> 8) & 0xff;
        var b = rgb & 0xff;
        return (uint)((255 << 24) | (r << 16) | (g << 8) | b);
    }

    private static bool TryTake(IReadOnlyList<string> tokens, ref int index, out string token)
    {
        if (index >= tokens.Count)
        {
            token = "";
            return false;
        }

        token = tokens[index];
        index++;
        return true;
    }

    private static bool Skip(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
        return index < text.Length;
    }

    private static void Expect(string text, ref int index, char expected)
    {
        Skip(text, ref index);
        if (index >= text.Length || text[index] != expected)
            throw new FormatException($"expected '{expected}'");
        index++;
    }

    private static double ReadFloat(string text, ref int index)
    {
        Skip(text, ref index);
        var start = index;
        if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            index++;
        while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.'))
            index++;
        var slice = text[start..index];
        return double.Parse(slice, System.Globalization.CultureInfo.InvariantCulture);
    }
}
