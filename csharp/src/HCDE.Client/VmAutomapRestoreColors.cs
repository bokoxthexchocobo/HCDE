namespace HCDE.Client;

/// <summary><c>CCMD(am_restorecolors)</c> from <c>am_map.cpp</c> (578–593).</summary>
public static class VmAutomapRestoreColors
{
    public static int StandardCvarSlotCount => 16;

    public static int OverlayCvarSlotCount => 16;

    public static bool ShouldResetStandardSlot(int index, int slotCount) => index < slotCount;

    public static bool ShouldResetOverlaySlot(int index, int slotCount) => index < slotCount;

    public static bool ShouldResetSectorFillAlpha() => true;

    public static bool ShouldResetOverlaySectorFillAlpha() => true;
}
