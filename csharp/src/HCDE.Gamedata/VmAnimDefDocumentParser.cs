namespace HCDE.Gamedata;

/// <summary><c>InitAnimDefs</c> / <c>ParseAnim</c> header from <c>animations.cpp</c>.</summary>
public sealed class VmAnimDefEntry
{
    public string Kind { get; set; } = "";
    public string BaseTexture { get; set; } = "";
    public bool Optional { get; set; }
    public bool BaseMissing { get; set; }
    public string AnimationType { get; set; } = "Forward";
    public List<VmAnimDefFrame> PicFrames { get; } = [];
    public VmAnimDefRangeAnim? Range { get; set; }
    public int DefinedMode { get; set; }
}

public sealed class VmAnimDefFrame
{
    public string FrameTexture { get; set; } = "";
    public uint SpeedMin { get; set; } = 1;
    public uint SpeedRange { get; set; }
}

public sealed class VmAnimDefRangeAnim
{
    public string EndTexture { get; set; } = "";
    public uint SpeedMin { get; set; }
    public uint SpeedRange { get; set; }
    public string Direction { get; set; } = "Forward";
}

public interface IAnimDefTextureCatalog
{
    bool TextureExists(string name, bool isFlat);
    int GetTextureIndex(string name, bool isFlat);
}

public static class VmAnimDefDocumentParser
{
    public static bool TryParseStatement(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefEntry entry, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected ANIMDEFS keyword."); return false; }
        var head = tokens[i++];
        if (head.Equals("flat", StringComparison.OrdinalIgnoreCase) || head.Equals("texture", StringComparison.OrdinalIgnoreCase))
        {
            entry.Kind = head.ToLowerInvariant();
            return TryParseAnimHeader(tokens, ref i, textures, entry, isFlat: head.Equals("flat", StringComparison.OrdinalIgnoreCase), errors);
        }

        if (head.Equals("switch", StringComparison.OrdinalIgnoreCase))
            return VmAnimDefSwitchParser.TryParseSwitch(tokens, ref i, textures, errors);
        if (head.Equals("warp", StringComparison.OrdinalIgnoreCase) || head.Equals("warp2", StringComparison.OrdinalIgnoreCase))
            return TryParseWarp(tokens, ref i, textures, type2: head.Equals("warp2", StringComparison.OrdinalIgnoreCase), errors);
        if (head.Equals("canvastexture", StringComparison.OrdinalIgnoreCase)
            || head.Equals("cameratexture", StringComparison.OrdinalIgnoreCase)
            || head.Equals("animatedDoor", StringComparison.OrdinalIgnoreCase)
            || head.Equals("skyoffset", StringComparison.OrdinalIgnoreCase)
            || head.Equals("firetexture", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"ANIMDEFS directive '{head}' is not ported in this chunk.");
            return false;
        }

        errors.Add($"Unknown ANIMDEFS keyword '{head}'.");
        return false;
    }

    private static bool TryParseAnimHeader(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefEntry entry, bool isFlat, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected texture name."); return false; }
        if (tokens[i].Equals("optional", StringComparison.OrdinalIgnoreCase))
        {
            entry.Optional = true;
            i++;
        }

        if (i >= tokens.Count) { errors.Add("Expected texture name."); return false; }
        entry.BaseTexture = tokens[i++];
        var exists = textures.TextureExists(entry.BaseTexture, isFlat);
        if (!exists)
        {
            if (entry.Optional)
                entry.BaseMissing = true;
            else
                errors.Add($"ANIMDEFS: Can't find {entry.BaseTexture}");
        }

        return errors.Count == 0;
    }

    public static bool TryParseAnimModifier(IReadOnlyList<string> tokens, ref int i, VmAnimDefEntry entry, List<string> errors)
    {
        if (i >= tokens.Count) return true;
        var word = tokens[i];
        if (word.Equals("Oscillate", StringComparison.OrdinalIgnoreCase))
        {
            if (entry.AnimationType.Equals("Random", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("You cannot use \"random\" and \"oscillate\" together in a single animation.");
                return false;
            }

            entry.AnimationType = "OscillateUp";
            i++;
            return true;
        }

        if (word.Equals("Random", StringComparison.OrdinalIgnoreCase))
        {
            if (entry.AnimationType.Equals("OscillateUp", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("You cannot use \"random\" and \"oscillate\" together in a single animation.");
                return false;
            }

            entry.AnimationType = "Random";
            i++;
        }

        return errors.Count == 0;
    }

    public static bool TryParseAnimClause(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefEntry entry, bool isFlat, List<string> errors)
    {
        if (i >= tokens.Count) return true;
        var word = tokens[i];
        if (word.Equals("range", StringComparison.OrdinalIgnoreCase))
        {
            if (entry.DefinedMode == 2)
            {
                errors.Add("You cannot use \"pic\" and \"range\" together in a single animation.");
                return false;
            }

            if (entry.DefinedMode == 1)
            {
                errors.Add("You can only use one \"range\" per animation.");
                return false;
            }

            entry.DefinedMode = 1;
            i++;
            return TryParseRangeAnim(tokens, ref i, textures, entry, isFlat, errors);
        }

        if (word.Equals("pic", StringComparison.OrdinalIgnoreCase))
        {
            if (entry.DefinedMode == 1)
            {
                errors.Add("You cannot use \"pic\" and \"range\" together in a single animation.");
                return false;
            }

            entry.DefinedMode = 2;
            i++;
            return TryParsePicFrame(tokens, ref i, textures, entry, isFlat, errors);
        }

        return TryParseAnimModifier(tokens, ref i, entry, errors);
    }

    public static bool TryParsePicFrame(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefEntry entry, bool isFlat, List<string> errors)
    {
        var frameTex = ParseFramenum(tokens, ref i, textures, entry.BaseTexture, isFlat, entry.BaseMissing, errors);
        if (!TryParseTime(tokens, ref i, out var min, out var max, errors)) return false;
        if (!entry.BaseMissing && textures.TextureExists(entry.BaseTexture, isFlat))
            entry.PicFrames.Add(new VmAnimDefFrame { FrameTexture = frameTex, SpeedMin = min, SpeedRange = max - min });
        return errors.Count == 0;
    }

    public static bool TryParseRangeAnim(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefEntry entry, bool isFlat, List<string> errors)
    {
        var end = ParseFramenum(tokens, ref i, textures, entry.BaseTexture, isFlat, entry.BaseMissing, errors);
        if (!TryParseTime(tokens, ref i, out var min, out var max, errors)) return false;
        var direction = "Forward";
        if (!entry.BaseMissing && textures.TextureExists(entry.BaseTexture, isFlat)
            && textures.TextureExists(end, isFlat))
        {
            var baseIndex = textures.GetTextureIndex(entry.BaseTexture, isFlat);
            var endIndex = textures.GetTextureIndex(end, isFlat);
            if (endIndex < baseIndex) direction = "Backward";
        }

        entry.Range = new VmAnimDefRangeAnim { EndTexture = end, SpeedMin = min, SpeedRange = max - min, Direction = direction };
        return errors.Count == 0;
    }

    public static string ParseFramenum(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        string baseTexture, bool isFlat, bool allowMissing, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected frame texture."); return ""; }
        var token = tokens[i++];
        if (int.TryParse(token, out var offset))
            return $"{baseTexture}+{offset - 1}";
        if (!textures.TextureExists(token, isFlat) && !allowMissing)
            errors.Add($"Unknown texture {token}");
        return token;
    }

    public static bool TryParseTime(IReadOnlyList<string> tokens, ref int i, out uint min, out uint max, List<string> errors)
    {
        min = max = 1;
        if (i >= tokens.Count) { errors.Add("Must specify a duration for animation frame"); return false; }
        var unit = tokens[i++];
        if (unit.Equals("tics", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count || !double.TryParse(tokens[i++], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var tics))
            {
                errors.Add("Expected tics value.");
                return false;
            }

            var ms = (uint)(tics * 1000 / 35.0);
            min = max = ms;
            return true;
        }

        if (unit.Equals("rand", StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 >= tokens.Count) { errors.Add("Expected rand min/max."); return false; }
            if (!double.TryParse(tokens[i++], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lo)
                || !double.TryParse(tokens[i++], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var hi))
            {
                errors.Add("Expected rand min/max.");
                return false;
            }

            min = (uint)(lo * 1000 / 35.0);
            max = (uint)(hi * 1000 / 35.0);
            return true;
        }

        errors.Add("Must specify a duration for animation frame");
        return false;
    }

    private static bool TryParseWarp(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures, bool type2, List<string> errors)
    {
        var isFlat = false;
        if (i < tokens.Count && (tokens[i].Equals("flat", StringComparison.OrdinalIgnoreCase)
                                 || tokens[i].Equals("texture", StringComparison.OrdinalIgnoreCase)))
        {
            isFlat = tokens[i].Equals("flat", StringComparison.OrdinalIgnoreCase);
            i++;
        }

        if (i >= tokens.Count) { errors.Add("Expected texture name for warp."); return false; }
        var name = tokens[i++];
        if (!textures.TextureExists(name, isFlat) && !errors.Contains($"ANIMDEFS: Can't find {name}"))
            errors.Add($"ANIMDEFS: Can't find {name}");
        if (i < tokens.Count && double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            i++;
        return errors.Count == 0;
    }
}
