namespace HCDE.Gamedata;

public sealed class VmBoomTranslator
{
    public int FirstLineType { get; set; }
    public int LastLineType { get; set; }
    public int NewSpecial { get; set; }
    public List<VmBoomArg> Args { get; } = [];
}

public sealed class VmBoomArg
{
    public int ArgNum { get; set; }
    public byte ConstantValue { get; set; }
    public bool OrExisting { get; set; }
    public int ListSize { get; set; }
    public uint AndValue { get; set; }
    public int[] ResultFilter { get; } = [];
    public byte[] ResultValue { get; } = [];
}

/// <summary>Boomish branch of <c>TranslateLineDef</c> in <c>p_xlat.cpp</c>.</summary>
public static class VmXlatBoomTranslate
{
    public static bool TryTranslateBoomish(
        int special,
        int tag,
        uint flags,
        bool passthrough,
        IReadOnlyList<VmBoomTranslator> boomish,
        out int newSpecial,
        out uint newFlags,
        out int activation,
        int[] args)
    {
        newSpecial = 0;
        newFlags = flags;
        activation = 0;
        for (var i = 0; i < boomish.Count; i++)
        {
            var b = boomish[i];
            if (special < b.FirstLineType || special > b.LastLineType) continue;
            newSpecial = b.NewSpecial;
            activation = MapTriggerType(special & 7, passthrough, ref newFlags);
            args[0] = tag;
            for (var j = 1; j < args.Length; j++) args[j] = 0;
            ApplyBoomArgs(special, ref newFlags, args, b.Args);
            if ((special & 7) is 4 or 5)
            {
                if (newSpecial == 12) args[2] |= 128;
                else args[0] = 0;
            }

            return true;
        }

        return false;
    }

    private static int MapTriggerType(int trigger, bool passthrough, ref uint flags)
    {
        switch (trigger)
        {
            case 1: flags |= 1u << 11; return 1;
            case 0: return 1;
            case 3:
            case 5: flags |= 1u << 11;
                return passthrough ? 6 : 2;
            case 2:
            case 4: return passthrough ? 6 : 2;
            case 6: flags |= 1u << 11; return 3;
            default: return 0;
        }
    }

    private static void ApplyBoomArgs(int special, ref uint flags, int[] args, IReadOnlyList<VmBoomArg> boomArgs)
    {
        foreach (var arg in boomArgs)
        {
            var destIndex = arg.ArgNum < 4 ? arg.ArgNum + 1 : -1;
            byte val = arg.ConstantValue;
            if (arg.ListSize > 0)
            {
                for (var k = 0; k < arg.ListSize; k++)
                {
                    if ((special & arg.AndValue) == arg.ResultFilter[k]) val = arg.ResultValue[k];
                }
            }

            if (destIndex >= 0) args[destIndex] = arg.OrExisting ? args[destIndex] | val : val;
            else
            {
                var flagTemp = (int)((flags >> 9) & 0x3f);
                flagTemp = arg.OrExisting ? flagTemp | val : val;
                flags = (flags & ~0x7e00u) | (uint)(flagTemp << 9);
            }
        }
    }
}
