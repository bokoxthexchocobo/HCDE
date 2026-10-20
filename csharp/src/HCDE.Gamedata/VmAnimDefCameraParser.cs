namespace HCDE.Gamedata;

/// <summary><c>ParseCameraTexture</c> / <c>ParseCanvasTexture</c> from <c>animations.cpp</c>.</summary>
public sealed class VmAnimDefCameraTexture
{
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Hdr { get; set; }
    public double FitWidth { get; set; }
    public double FitHeight { get; set; }
    public bool WorldPanning { get; set; }
    public bool Translucent { get; set; }
}

public static class VmAnimDefCameraParser
{
    public static bool TryParseCamera(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        VmAnimDefCameraTexture camera, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected camera texture name."); return false; }
        camera.Name = tokens[i++];
        if (i + 1 >= tokens.Count) { errors.Add("Expected camera width/height."); return false; }
        if (!int.TryParse(tokens[i++], out var width) || !int.TryParse(tokens[i++], out var height))
        {
            errors.Add("Expected camera width/height.");
            return false;
        }

        camera.Width = width;
        camera.Height = height;
        camera.FitWidth = textures.TextureExists(camera.Name, isFlat: true) ? width : width;
        camera.FitHeight = height;
        while (i < tokens.Count)
        {
            var word = tokens[i];
            if (word.Equals("hdr", StringComparison.OrdinalIgnoreCase)) { camera.Hdr = true; i++; continue; }
            if (word.Equals("fit", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i + 1 >= tokens.Count) { errors.Add("Expected fit dimensions."); return false; }
                camera.FitWidth = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
                camera.FitHeight = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
                continue;
            }

            if (word.Equals("WorldPanning", StringComparison.OrdinalIgnoreCase)) { camera.WorldPanning = true; i++; continue; }
            if (word.Equals("translucent", StringComparison.OrdinalIgnoreCase)) { camera.Translucent = true; i++; continue; }
            break;
        }

        return errors.Count == 0;
    }
}
