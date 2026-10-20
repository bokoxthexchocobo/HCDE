namespace HCDE.Gamedata;

/// <summary><c>PatchThing</c> actor resolution from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingContext
{
    public static bool TryResolveThingType(int thingy, IReadOnlyList<string> infoNames, out string? typeName)
    {
        if (thingy <= 0 || thingy > infoNames.Count)
        {
            typeName = null;
            return false;
        }

        typeName = infoNames[thingy - 1];
        return !string.IsNullOrEmpty(typeName);
    }
}
