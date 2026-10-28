namespace HCDE.Gamedata;

public sealed class VmVoxelOptions
{
    public double Scale { get; set; } = 1;
    public int DroppedSpin { get; set; }
    public int PlacedSpin { get; set; }
    public double AngleOffsetDeg { get; set; } = 90;
    public double XOffset { get; set; }
    public double YOffset { get; set; }
    public double ZOffset { get; set; }
    public bool OverridePalette { get; set; }
    public bool PitchFromMomentum { get; set; }
    public bool UseActorPitch { get; set; }
    public bool UseActorRoll { get; set; }
}

public sealed class VmVoxelSpriteBinding
{
    public int SpriteIndex { get; set; }
    public int Frame { get; set; } = 255;
}

/// <summary><c>VOX_ReadSpriteNames</c>, <c>VOX_ReadOptions</c>, binding apply from <c>voxeldef.cpp</c>.</summary>
public static class VmVoxelDefParser
{
    public const int MaxSpriteFrames = 29;

    public static bool TryReadSpriteNames(
        IReadOnlyList<string> tokens,
        ref int i,
        List<VmVoxelSpriteBinding> sprites,
        List<string> errors,
        Func<string, int>? resolveSpriteIndex = null)
    {
        sprites.Clear();
        resolveSpriteIndex ??= _ => 0;
        while (i < tokens.Count)
        {
            if (tokens[i] == "=")
            {
                if (sprites.Count == 0) errors.Add("No sprites specified for voxel.");
                i++;
                return errors.Count == 0;
            }

            var name = tokens[i++];
            if (name.Length != 4 && name.Length != 5)
            {
                errors.Add($"Sprite name \"{name}\" is wrong size.");
                continue;
            }

            var frameChar = name.Length == 5 ? char.ToUpperInvariant(name[4]) : '\0';
            if (name.Length == 5 && (frameChar < 'A' || frameChar >= 'A' + MaxSpriteFrames))
            {
                errors.Add($"Sprite frame {frameChar} is invalid.");
                continue;
            }

            var frame = name.Length == 4 ? 255 : frameChar - 'A';
            var sprIndex = resolveSpriteIndex(name[..4]);
            if (sprIndex >= 0)
                sprites.Add(new VmVoxelSpriteBinding { SpriteIndex = sprIndex, Frame = frame });
        }

        if (sprites.Count != 0) errors.Add("Unexpected end of voxel sprite list.");
        return false;
    }

    public static bool TryReadOptions(IReadOnlyList<string> tokens, ref int i, VmVoxelOptions opts, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "{")
        {
            errors.Add("Expected voxel option block.");
            return false;
        }

        i++;
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("scale", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedNumber(tokens, ref i, out var scale, requireEquals: true)) return false;
                opts.Scale = scale;
            }
            else if (key.Equals("spin", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedInt(tokens, ref i, out var spin)) return false;
                opts.DroppedSpin = opts.PlacedSpin = spin;
            }
            else if (key.Equals("placedspin", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedInt(tokens, ref i, out var spin)) return false;
                opts.PlacedSpin = spin;
            }
            else if (key.Equals("droppedspin", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedInt(tokens, ref i, out var spin)) return false;
                opts.DroppedSpin = spin;
            }
            else if (key.Equals("angleoffset", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedNumber(tokens, ref i, out var deg, requireEquals: true)) return false;
                opts.AngleOffsetDeg = deg + 90;
            }
            else if (key.Equals("xoffset", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedNumber(tokens, ref i, out var v, requireEquals: true)) return false;
                opts.XOffset = v;
            }
            else if (key.Equals("yoffset", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedNumber(tokens, ref i, out var v, requireEquals: true)) return false;
                opts.YOffset = v;
            }
            else if (key.Equals("zoffset", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadSignedNumber(tokens, ref i, out var v, requireEquals: true)) return false;
                opts.ZOffset = v;
            }
            else if (key.Equals("overridepalette", StringComparison.OrdinalIgnoreCase)) opts.OverridePalette = true;
            else if (key.Equals("pitchfrommomentum", StringComparison.OrdinalIgnoreCase)) opts.PitchFromMomentum = true;
            else if (key.Equals("useactorpitch", StringComparison.OrdinalIgnoreCase)) opts.UseActorPitch = true;
            else if (key.Equals("useactorroll", StringComparison.OrdinalIgnoreCase)) opts.UseActorRoll = true;
            else
            {
                errors.Add($"Unknown voxel option '{key}'.");
                if (i < tokens.Count && tokens[i] == "=") i += 2;
            }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        return true;
    }

    public static void ApplyVoxelToSprites(
        IReadOnlyList<VmVoxelSpriteBinding> sprites,
        object voxelDef,
        Action<int, int, object> addVoxel)
    {
        foreach (var binding in sprites)
        {
            if (binding.Frame == 255)
            {
                for (var j = MaxSpriteFrames - 1; j >= 0; j--)
                    addVoxel(binding.SpriteIndex, j, voxelDef);
            }
            else addVoxel(binding.SpriteIndex, binding.Frame, voxelDef);
        }
    }

    private static bool TryReadSignedInt(IReadOnlyList<string> tokens, ref int i, out int value)
    {
        value = 0;
        if (!TryReadSignedNumber(tokens, ref i, out var num, requireEquals: true)) return false;
        value = (int)num;
        return true;
    }

    private static bool TryReadSignedNumber(IReadOnlyList<string> tokens, ref int i, out double value, bool requireEquals)
    {
        value = 0;
        if (requireEquals)
        {
            if (i >= tokens.Count || tokens[i] != "=") return false;
            i++;
        }

        var mul = 1.0;
        if (i < tokens.Count && tokens[i] == "-")
        {
            mul = -1;
            i++;
        }

        if (i >= tokens.Count) return false;
        value = mul * double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }
}
