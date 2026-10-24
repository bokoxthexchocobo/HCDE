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

    public static bool ShouldErrorWhenKickedSelfNative(int kickedPlayer, int consolePlayer) =>
        kickedPlayer == consolePlayer;

    public static bool ShouldDisconnectKickedPlayerNative(int kickedPlayer, int consolePlayer, bool inGame) =>
        kickedPlayer != consolePlayer && inGame;

    public static bool ShouldRevertCameraToMobjNative(object? mobj) => mobj is not null;

    public static bool ShouldReadiedCommandNative(int player, int maxPlayers) =>
        player >= 0 && player < maxPlayers;

    public static int NegatePitchLimitDegreesNative(int degrees) => -degrees;

    public static bool CrouchRequiresUnfrozenPlayerNative(bool totallyFrozen) => !totallyFrozen;

    public static bool ShouldPrintMorphResultNative(int player, int consolePlayer) => player == consolePlayer;

    public static bool ShouldPrintDelControllerNative(int consolePlayer, int playernum) =>
        consolePlayer == playernum;

    public static bool KillClassHasReplacementNative(object? cls, object? replacement) =>
        cls is not null && replacement is not null && !ReferenceEquals(cls, replacement);

    public static bool RemoveClassHasReplacementNative(object? cls, object? replacement) =>
        cls is not null && replacement is not null && !ReferenceEquals(cls, replacement);

    public static bool IsConversationCommandNative(int cmd, int demConvReply, int demConvClose, int demConvNull) =>
        cmd == demConvReply || cmd == demConvClose || cmd == demConvNull;

    public static bool ShouldFinishGameOnCommandNative(int gameState, int levelState) => gameState == levelState;

    public static bool ShouldReadNetEventArgsNative(int argCount, int maxArgs) =>
        argCount >= 0 && argCount <= maxArgs;

    public static bool ShouldRouteNetEventToConsoleNative(bool predatorHandled) => !predatorHandled;

    public static bool ZscCmdBufferNonEmptyNative(int size) => size > 0;

    public static bool ShouldApplyNextSkillNative(int nextSkill) => nextSkill >= 0;

    public static bool ShouldSelectWeaponNative(int player, int maxPlayers) =>
        player >= 0 && player < maxPlayers;

    public static bool ShouldUseFlechetteNative(int player, int maxPlayers) =>
        player >= 0 && player < maxPlayers;

    public static bool ShouldPrintKickDisconnectNative(int kickedPlayer, int consolePlayer, bool inGame) =>
        kickedPlayer != consolePlayer && inGame;

    public static bool MorphMessageNonEmptyNative(string msg) => msg.Length > 0;
}
