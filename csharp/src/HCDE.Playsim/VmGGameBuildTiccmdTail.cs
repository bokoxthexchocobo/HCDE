namespace HCDE.Playsim;

/// <summary><c>G_BuildTiccmd</c> net flush tail from <c>g_game.cpp</c> (1101–1128).</summary>
public static class VmGGameBuildTiccmdTail
{
    public const int MovementShift = 8;

    public static int ScaleMovementAxis(int axis) => axis << MovementShift;

    public static (int forward, int side) ScaleMovementPair(int forward, int side) =>
        (ScaleMovementAxis(forward), ScaleMovementAxis(side));

    public static bool ShouldTraceInput(int gametic, int lastTraceTic, int ticRate) =>
        gametic - lastTraceTic >= ticRate;

    public static bool HasNonZeroMovement(int forward, int side, int up, uint buttons) =>
        forward != 0 || side != 0 || up != 0 || buttons != 0;

    public static bool ShouldWriteWeaponSelect(int sendWeaponSlot, int none) => sendWeaponSlot != none;

    public static bool ShouldWriteFlechette(bool wantsFlechette) => wantsFlechette;

    public static void ClearWeaponSelect(ref int sendWeaponSlot, int none) => sendWeaponSlot = none;

    public static void ClearFlechetteRequest(ref bool wantsFlechette) => wantsFlechette = false;
}
