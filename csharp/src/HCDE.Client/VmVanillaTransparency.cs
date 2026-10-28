namespace HCDE.Client;

/// <summary>Native <c>r_vanillatrans.cpp</c> lump-driven transparency mode.</summary>
public static class VmVanillaTransparency
{
    public static bool UseVanillaTransparency(
        int cvarValue,
        ref VmVanillaTransparencyProbeState state,
        VmLumpFinder findLump)
    {
        if (state.FirstTime)
        {
            var lastLump = 0;
            findLump("ZSCRIPT", ref lastLump);
            if (findLump("ZSCRIPT", ref lastLump) < 0)
            {
                lastLump = 0;
                state.FoundDehacked = findLump("DEHACKED", ref lastLump) >= 0;
                lastLump = 0;
                state.FoundDecorate = findLump("DECORATE", ref lastLump) >= 0;
                state.FoundZScript = false;
            }
            else
            {
                state.FoundZScript = true;
                state.FoundDehacked = false;
                state.FoundDecorate = false;
            }

            state.FirstTime = false;
        }

        return cvarValue switch
        {
            0 => false,
            1 => true,
            _ => state.FoundDehacked || (!state.FoundDecorate && cvarValue == 3),
        };
    }

    public static void ResetProbe(ref VmVanillaTransparencyProbeState state) => state.FirstTime = true;
}

public delegate int VmLumpFinder(string name, ref int lastLump);

public struct VmVanillaTransparencyProbeState
{
    public bool FirstTime = true;
    public bool FoundDehacked;
    public bool FoundDecorate;
    public bool FoundZScript;

    public VmVanillaTransparencyProbeState()
    {
    }
}
