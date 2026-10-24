namespace HCDE.Gamedata;

public sealed class VmLineFlagTranslation
{
    public bool IsMask { get; set; }
    public uint NewValue { get; set; }
}

/// <summary>Line-flag translation loops at the start of <c>TranslateLineDef</c> in <c>p_xlat.cpp</c>.</summary>
public static class VmXlatLineFlagTranslate
{
    public static uint ApplyMaskPass(uint flags, IReadOnlyList<VmLineFlagTranslation> rules, bool skipMbf21Line11, uint compatFlags2)
    {
        var flags1 = flags;
        for (var i = 0; i < 16 && i < rules.Count; i++)
        {
            if ((flags & (1u << i)) == 0 || !rules[i].IsMask) continue;
            if (i == 11 && skipMbf21Line11 && (compatFlags2 & (1u << 0)) == 0) continue;
            flags1 &= rules[i].NewValue;
        }

        return flags1;
    }

    public static (uint NewFlags, uint NewFlags2, bool Passthrough, float? Alpha) ApplyValuePass(
        uint flags1,
        IReadOnlyList<VmLineFlagTranslation> rules)
    {
        uint newFlags = 0, newFlags2 = 0;
        var passthrough = false;
        float? alpha = null;
        for (var i = 0; i < 16 && i < rules.Count; i++)
        {
            if ((flags1 & (1u << i)) == 0 || rules[i].IsMask) continue;
            var val = rules[i].NewValue;
            switch ((int)val)
            {
                case -1:
                    passthrough = true;
                    break;
                case -2:
                    alpha = 0.75f;
                    break;
                case -3:
                    alpha = 0.25f;
                    break;
                default:
                    if ((val & 0x80000000) != 0 && val != 0x80000000) newFlags2 |= val;
                    else newFlags |= val;
                    break;
            }
        }

        return (newFlags, newFlags2, passthrough, alpha);
    }

    public static void ApplyZeroSpecial(int[] args)
    {
        for (var i = 1; i < args.Length; i++) args[i] = 0;
    }
}
