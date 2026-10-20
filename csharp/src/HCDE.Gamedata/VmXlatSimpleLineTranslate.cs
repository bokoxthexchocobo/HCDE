namespace HCDE.Gamedata;

public sealed class VmSimpleLineTranslation
{
    public int Special { get; set; }
    public uint Flags { get; set; }
    public int[] Args { get; } = new int[5];
}

/// <summary>Simple line translation branch of <c>TranslateLineDef</c> in <c>p_xlat.cpp</c>.</summary>
public static class VmXlatSimpleLineTranslate
{
    public static bool TryApplySimpleTranslation(
        int mapSpecial,
        int tag,
        uint lineFlags,
        VmSimpleLineTranslation translation,
        out int special,
        out uint flags,
        out int activation)
    {
        special = 0;
        flags = lineFlags;
        activation = 0;
        if (mapSpecial == 0 || translation.Special == 0) return false;
        special = translation.Special;
        flags = lineFlags | ((translation.Flags & 0x1fu) << 9);
        if ((translation.Flags & 0x20) != 0) flags |= 1u << 10; // ML_FIRSTSIDEONLY placeholder bit
        activation = 1 << (int)((flags >> 9) & 7u);
        if (activation == 4) activation = 3 | 5; // SPAC_AnyCross -> Impact|PCross simplified
        else if (activation == 5) activation = 3 | 5;
        for (var t = 0; t < translation.Args.Length; t++)
            translation.Args[t] = translation.Args[t] switch
            {
                -2 => tag,
                _ => translation.Args[t]
            };
        return true;
    }
}
