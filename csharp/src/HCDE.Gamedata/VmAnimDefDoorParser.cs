namespace HCDE.Gamedata;

/// <summary><c>ParseAnimatedDoor</c> from <c>animations.cpp</c>.</summary>
public sealed class VmAnimDefDoor
{
    public string BaseTexture { get; set; } = "";
    public string OpenSound { get; set; } = "";
    public string CloseSound { get; set; } = "";
    public List<string> FrameTextures { get; } = [];
}

public static class VmAnimDefDoorParser
{
    public static bool TryParseDoor(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures, VmAnimDefDoor door, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected door base texture."); return false; }
        door.BaseTexture = tokens[i++];
        if (!textures.TextureExists(door.BaseTexture, isFlat: false))
            errors.Add($"Unknown texture {door.BaseTexture}");
        while (i < tokens.Count)
        {
            var word = tokens[i];
            if (word.Equals("opensound", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i < tokens.Count) door.OpenSound = tokens[i++];
                continue;
            }

            if (word.Equals("closesound", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i < tokens.Count) door.CloseSound = tokens[i++];
                continue;
            }

            if (word.Equals("pic", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i < tokens.Count) door.FrameTextures.Add(tokens[i++]);
                continue;
            }

            if (word.Equals("allowdecals", StringComparison.OrdinalIgnoreCase)) { i++; continue; }
            break;
        }

        return errors.Count == 0;
    }
}
