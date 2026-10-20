namespace HCDE.Gamedata;

public sealed class VmLineFlagTranslation
{
    public uint NewValue { get; set; }
    public bool IsMask { get; set; }
}

/// <summary>Line flag translation prefix of <c>FLevelLocals::TranslateLineDef</c> in <c>p_xlat.cpp</c>.</summary>
public static class VmXlatLineFlagTranslate
{
    public static uint ApplyMaskPass(uint flags, IReadOnlyList<VmLineFlagTranslation> translations)
    {
        var flags1 = flags;
        for (var i = 0; i < 16 && i < translations.Count; i++)
        {
            if ((flags & (1u << i)) != 0 && translations[i].IsMask)
                flags1 &= translations[i].NewValue;
        }

        return flags1;
    }

    public static (uint NewFlags, uint NewFlags2, bool Passthrough, double Alpha) ApplyValuePass(
        uint flags1,
        IReadOnlyList<VmLineFlagTranslation> translations)
    {
        uint newFlags = 0, newFlags2 = 0;
        var passthrough = false;
        var alpha = 1.0;
        for (var i = 0; i < 16 && i < translations.Count; i++)
        {
            if ((flags1 & (1u << i)) == 0 || translations[i].IsMask) continue;
            var val = translations[i].NewValue;
            switch (val)
            {
                case 0xFFFFFFFF:
                    passthrough = true;
                    break;
                case 0xFFFFFFFE:
                    alpha = 0.75;
                    break;
                case 0xFFFFFFFD:
                    alpha = 0.25;
                    break;
                default:
                    if ((val & 0x80000000) != 0 && val != 0x80000000) newFlags2 |= val;
                    else newFlags |= val;
                    break;
            }
        }

        return (newFlags, newFlags2, passthrough, alpha);
    }

    public static void ApplyZeroSpecial(int tag, int[] args, out int special, out uint flags, out int arg0)
    {
        special = 0;
        flags = 0;
        arg0 = tag;
        if (args.Length > 1) Array.Clear(args, 1, args.Length - 1);
    }
}
