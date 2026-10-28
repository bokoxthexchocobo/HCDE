namespace HCDE.Gamedata;

public sealed class VmDehackedFramePatchTarget
{
    public int Tics { get; set; }
    public int Misc1 { get; set; }
    public int Misc2 { get; set; }
    public int Frame { get; set; }
    public int StateFlags { get; set; }
    public long[] Args { get; } = new long[8];
    public int ArgsUsed { get; set; }
}

/// <summary><c>PatchFrame</c> from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchFrame
{
    public static int GetDefaultTics(int frameNum, int fallback) =>
        frameNum switch { 47 => 5, 48 => 4, _ => fallback };

    public static bool TryApplyProperty(string key, string value, VmDehackedFramePatchTarget target)
    {
        if (!long.TryParse(value, out var val64)) return false;
        var val = (int)val64;
        if (key.Equals("Duration", StringComparison.OrdinalIgnoreCase) && key.Length == 8)
        {
            target.Tics = Math.Clamp(val, -1, short.MaxValue);
            return true;
        }

        if (key.Equals("Unknown 1", StringComparison.OrdinalIgnoreCase))
        {
            target.Misc1 = val;
            return true;
        }

        if (key.Equals("Unknown 2", StringComparison.OrdinalIgnoreCase))
        {
            target.Misc2 = val;
            return true;
        }

        if (key.Equals("Sprite number", StringComparison.OrdinalIgnoreCase))
        {
            target.Misc1 = val;
            return true;
        }

        if (key.Equals("Next frame", StringComparison.OrdinalIgnoreCase))
        {
            target.Frame = val;
            return true;
        }

        if (key.Equals("Sprite subnumber", StringComparison.OrdinalIgnoreCase))
        {
            target.Frame = val;
            return true;
        }

        if (key.StartsWith("Args", StringComparison.OrdinalIgnoreCase) && key.Length == 5 &&
            key[4] is >= '1' and <= '8')
        {
            var arg = key[4] - '1';
            target.Args[arg] = val64;
            target.ArgsUsed |= 1 << arg;
            return true;
        }

        if (key.Equals("MBF21 Bits", StringComparison.OrdinalIgnoreCase))
        {
            if (uint.TryParse(value, out var bits))
                ApplyFrameBits(bits, [( "SKILL5FAST", 1 )], target);
            return true;
        }

        return false;
    }

    public static void ApplyFrameBits(uint value, IReadOnlyList<(string Name, int BitValue)> sbits, VmDehackedFramePatchTarget target)
    {
        var mask = 0;
        var flags = 0;
        for (var i = 0; i < sbits.Count; i++)
        {
            mask |= sbits[i].BitValue;
            if ((value & (1u << i)) != 0) flags |= sbits[i].BitValue;
        }

        target.StateFlags = (target.StateFlags & ~mask) | flags;
    }

    public static void FinalizeFrame(VmDehackedFramePatchTarget target)
    {
        target.StateFlags |= 1;
        if ((target.Frame & 0x7fff) > 63) target.Frame &= 0x3f;
        target.Frame &= 0x3f;
        if ((target.Frame & 0x8000) != 0) target.StateFlags |= 0x8000;
    }
}
