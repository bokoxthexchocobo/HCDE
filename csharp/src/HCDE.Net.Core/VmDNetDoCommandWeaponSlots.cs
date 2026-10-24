namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> weapon slot cases (<c>d_net.cpp</c> 9099–9133).</summary>
public static class VmDNetDoCommandWeaponSlots
{
    public static bool ShouldClearWeaponSlotNative(int slot, int numWeaponSlots) =>
        slot >= 0 && slot < numWeaponSlots;

    public static bool ShouldAddWeaponToSlotNative(int slot, object? weaponClass) =>
        slot >= 0 && weaponClass is not null;

    public static bool UseConsolePlayerForSlotAddNative(int player, int consolePlayer) => player == consolePlayer;

    public static bool ShouldReadSlotPlayerNumberNative(int cmd, int demSetSlotPnum) => cmd == demSetSlotPnum;

    public static int ResolveSlotPlayerNative(int cmd, int demSetSlotPnum, int defaultPlayer, int streamPlayer) =>
        cmd == demSetSlotPnum ? streamPlayer : defaultPlayer;

    public static bool ShouldIterateWeaponSlotCountNative(int count) => count > 0;

    public static int DecrementWeaponSlotCountNative(int count) => count - 1;

    public static bool ShouldAddSlotDefaultNative(int cmd, int demAddSlotDefault) => cmd == demAddSlotDefault;

    public static int ReadPitchLimitUpDegreesNative(int streamByte) => -streamByte;

    public static (int MinDegrees, int MaxDegrees) ReadPitchLimitPairNative(int upStreamByte, int downStreamByte) =>
        (ReadPitchLimitUpDegreesNative(upStreamByte), downStreamByte);
}
