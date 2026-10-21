namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> player movement and cheat cases (<c>d_net.cpp</c> 9010–9091).</summary>
public static class VmDNetDoCommandPlayerMisc
{
    public const uint BtJump = 1;

    public static bool ShouldToggleCrouchOnLevelNative(int gameState, int levelState) => gameState == levelState;

    public static bool CrouchRequiresLivePlayerNative(object? mobj, int playerState, int liveState) =>
        mobj is not null && playerState == liveState;

    public static bool CrouchBlockedByJumpNative(uint oldButtons, uint jumpMask) => (oldButtons & jumpMask) != 0;

    public static int SelectCrouchDirectionNative(int crouchDir) => crouchDir < 0 ? 1 : -1;

    public static bool ShouldRunMorphCommandNative(int player, int consolePlayer) => player == consolePlayer;

    public static bool ShouldPrintSettingsControllerAddNative(int consolePlayer, int playernum) =>
        consolePlayer == playernum;

    public static bool ShouldPrintAuthorityControllerMessageNative(bool localAuthority, int playernum, int consolePlayer) =>
        localAuthority && playernum != consolePlayer;

    public static bool KillClassRequiresResolvedActorNative(object? cls) => cls is not null;

    public static bool RemoveClassRequiresActorDescendantNative(object? cls, bool isActorDescendant) =>
        cls is not null && isActorDescendant;
}
