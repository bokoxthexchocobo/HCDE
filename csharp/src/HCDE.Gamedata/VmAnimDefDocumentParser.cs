namespace HCDE.Gamedata;

/// <summary><c>InitAnimDefs</c> / <c>ParseAnim</c> header from <c>animations.cpp</c>.</summary>
public sealed class VmAnimDefEntry
{
    public string Kind { get; set; } = "";
    public string BaseTexture { get; set; } = "";
    public bool Optional { get; set; }
    public bool BaseMissing { get; set; }
    public string AnimationType { get; set; } = "Forward";
}

public interface IAnimDefTextureCatalog
{
    bool TextureExists(string name, bool isFlat);
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

        if (head.Equals("switch", StringComparison.OrdinalIgnoreCase)
            || head.Equals("warp", StringComparison.OrdinalIgnoreCase)
            || head.Equals("warp2", StringComparison.OrdinalIgnoreCase)
            || head.Equals("canvastexture", StringComparison.OrdinalIgnoreCase)
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
}
