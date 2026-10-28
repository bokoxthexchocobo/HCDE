namespace HCDE.Client;

/// <summary><c>CheckSkipGameOptionBlock</c> from <c>doommenu.cpp</c>.</summary>
public static class VmMenuOptionFilter
{
    public static bool ShouldSkipBlock(string blockName, bool drawReadThis, bool swapMenu)
    {
        if (string.Equals(blockName, "ReadThis", StringComparison.OrdinalIgnoreCase))
            return drawReadThis;
        if (string.Equals(blockName, "Swapmenu", StringComparison.OrdinalIgnoreCase))
            return swapMenu;
        return false;
    }
}
