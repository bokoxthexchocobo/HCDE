namespace HCDE.Net.Core;

/// <summary>CCMD unmute and settings-controller mirrors (<c>d_net.cpp</c> 10462–10667).</summary>
public static class VmDNetCcmdSettings
{
    public static bool UnmuteArgcValidNative(int argc) => argc >= 2;

    public static bool RejectWithoutMultiplayerNative(bool multiplayer) => !multiplayer;

    public static ulong ClearMuteClientMaskNative(ulong mutedClients, int player) =>
        mutedClients & ~MuteClientMaskNative(player);

    public static ulong MuteClientMaskNative(int player) => 1ul << player;

    public static bool UnmutePlayerNumberInvalidNative(int playerNumber, int maxPlayers) =>
        playerNumber < 0 || playerNumber >= maxPlayers;

    public static bool ShouldPushUnmuteTargetNative(int player, int consolePlayer, int listSize, int findIndex) =>
        player != consolePlayer && findIndex >= listSize;

    public static bool ShouldUnmuteTargetPlayerNative(int player, int consolePlayer, bool inGame) =>
        player != consolePlayer && inGame;

    public static bool ShouldReportUnmutePlayerNotInGameNative(bool inGame) => !inGame;

    public static bool RejectWithoutNetgameNative(bool netgame) => !netgame;

    public static bool RejectWithoutAuthorityNative(bool authority) => !authority;

    public static bool SettingsArgcValidNative(int argc) => argc >= 2;

    public static bool ShouldEmitAddControllerNative(bool settingsController, bool add) =>
        add && !settingsController;

    public static bool ShouldEmitRemoveControllerNative(bool settingsController, bool add) =>
        !add && settingsController;

    public static bool ShouldRejectAlreadyControllerNative(bool settingsController, bool add) =>
        settingsController && add;

    public static bool ShouldRejectNotControllerNative(bool settingsController, bool add) =>
        !settingsController && !add;

    public static bool ShouldRejectClientNotInGameForSettingsNative(bool inGame) => !inGame;

    public static bool ListSettingsControllerEligibleNative(bool authoritySlot, bool settingsController) =>
        !authoritySlot && settingsController;

    public static bool ListSettingsEmptyNative(int controllerCount) => controllerCount == 0;

    public static bool MarkConsolePlayerInListNative(int client, int consolePlayer) => client == consolePlayer;

    public static bool ShouldPushClientForRemoveAllNative(bool authoritySlot, bool settingsController) =>
        !authoritySlot && settingsController;

    public static bool SettingsClientNumberInvalidNative(int clientNumber, int maxPlayers) =>
        clientNumber < 0 || clientNumber >= maxPlayers;

    public static bool SettingsPushClientNative(bool authoritySlot, int listSize, int findIndex) =>
        !authoritySlot && findIndex >= listSize;

    public static bool ShouldPrintUnmuteUsageNative(int argc) => !UnmuteArgcValidNative(argc);

    public static bool ShouldPrintAddSettingsUsageNative(int argc) => !SettingsArgcValidNative(argc);

    public static bool ShouldPrintRemoveSettingsUsageNative(int argc) => !SettingsArgcValidNative(argc);

    public static bool ShouldWriteAddControllerNative(bool settingsController, bool add, bool inGame, bool authoritySlot) =>
        !authoritySlot
        && inGame
        && !ShouldRejectAlreadyControllerNative(settingsController, add)
        && ShouldEmitAddControllerNative(settingsController, add);

    public static bool ShouldWriteRemoveControllerNative(bool settingsController, bool add, bool inGame, bool authoritySlot) =>
        !authoritySlot
        && inGame
        && !ShouldRejectNotControllerNative(settingsController, add)
        && ShouldEmitRemoveControllerNative(settingsController, add);

    public static bool ShouldPrintNoSettingsControllersNative(int controllerCount) =>
        ListSettingsEmptyNative(controllerCount);

    public static bool ShouldRejectInvalidUnmuteIntNative(bool validInt) => !validInt;

    public static bool ShouldRejectInvalidSettingsClientIntNative(bool validInt) => !validInt;

    public static bool ShouldSkipUnmuteWhenNotInGameNative(int player, int consolePlayer, bool inGame) =>
        !ShouldUnmuteTargetPlayerNative(player, consolePlayer, inGame);

    public static bool ShouldEnqueueSettingsClientNative(
        bool authoritySlot,
        int listSize,
        int findIndex,
        bool validInt,
        int clientNumber,
        int maxPlayers) =>
        !authoritySlot
        && findIndex >= listSize
        && validInt
        && clientNumber >= 0
        && clientNumber < maxPlayers;
}
