namespace HCDE.Gamedata;

/// <summary><c>PatchPointer</c> Boom hack and codep frame from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchPointer
{
    public static int ResolveBoomPointerZero(string line2, IReadOnlyList<int> codePConv)
    {
        var open = line2.IndexOf('(');
        if (open < 0) return 0;
        var space = line2.IndexOf(' ', open + 1);
        if (space < 0) return 0;
        var close = line2.IndexOf(')', space);
        if (close < 0) return 0;
        if (!int.TryParse(line2.AsSpan(space + 1, close - space - 1), out var index)) return 0;
        for (var i = 0; i < codePConv.Count; i++)
            if (codePConv[i] == index) return i;
        return 0;
    }

    public static bool TryMapActionIndex(int index, int actionCount, out int clamped)
    {
        if (index < 0 || index >= actionCount) { clamped = -1; return false; }
        clamped = index;
        return true;
    }
}
