namespace HCDE.Gamedata;

public sealed class VmSectorMaskRule
{
    public int Mask { get; set; }
    public int Op { get; set; }
    public int Shift { get; set; }
}

public sealed class VmSectorTranslation
{
    public int NewType { get; set; }
    public bool BitmaskAllowed { get; set; }
}

/// <summary><c>FLevelLocals::TranslateSectorSpecial</c> from <c>p_xlat.cpp</c>.</summary>
public static class VmXlatSectorTranslate
{
    public static int TranslateSectorSpecial(
        int special,
        IReadOnlyList<VmSectorMaskRule> masks,
        IReadOnlyList<VmSectorTranslation> translations)
    {
        var mask = 0;
        for (var i = masks.Count - 1; i >= 0; i--)
        {
            var rule = masks[i];
            var newMask = special & rule.Mask;
            if (newMask == 0) continue;
            special &= ~newMask;
            if (rule.Op == 1) newMask <<= rule.Shift;
            else if (rule.Op == -1) newMask >>= rule.Shift;
            else if (rule.Op == 0 && rule.Shift == 1) newMask = 0;
            mask |= newMask;
        }

        if ((uint)special < (uint)translations.Count)
        {
            var tr = translations[special];
            if (!tr.BitmaskAllowed && mask != 0) special = 0;
            else special = tr.NewType;
        }

        return special | mask;
    }
}
