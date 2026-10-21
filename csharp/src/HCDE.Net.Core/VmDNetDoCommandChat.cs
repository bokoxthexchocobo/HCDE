namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> chat, taunt, and cheat guard cases (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandChat
{
    public const int MsgTeam = 1;
    public const int MsgBold = 2;
    public const int ChatDisabled = 0;

    public static bool ShouldDropSayNative(int showChat, ulong mutedMask, int player) =>
        showChat == ChatDisabled || ((mutedMask >> player) & 1) != 0;

    public static bool IsGlobalSayNative(int who) => (who & MsgTeam) == 0;

    public static bool ShouldPrintGlobalChatNative(int showChat) => showChat >= 1;

    public static bool ShouldPrintTeamChatNative(int showChat) => showChat >= 2;

    public static bool ShouldUseBoldChatNative(int who, bool noBoldChat) => (who & MsgBold) != 0 && !noBoldChat;

    public static bool SameTeamForTeamChatNative(bool deathmatch, int speakerTeam, int consoleTeam) =>
        !deathmatch || speakerTeam == consoleTeam;

    public static string FormatTauntSoundNative(string? variant)
    {
        if (variant is { Length: > 0 })
            return $"*taunt-{variant}";
        return "*taunt";
    }

    public static bool ShouldPlayTauntOnPawnNative(object? pawn) => pawn is not null;

    public static bool ShouldExecuteGiveCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool ShouldLogRemoteGiveNative(int player, int consolePlayer) => player != consolePlayer;

    public static string FormatRemoteGiveMessageNative(string template, string playerName, string item) =>
        template.Replace("%s", playerName, StringComparison.Ordinal);

    public static bool ShouldExecuteTakeCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool ShouldExecuteSetInvCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool MustConsumeSetInvPaddingWhenRejectedNative(bool rejectPredator) => rejectPredator;

    public static bool ShouldLogCheatExecuteNative(int debugLevel) => debugLevel >= 2;

    public static bool ShouldLogCheatRejectedNative(int debugLevel) => debugLevel >= 2;


    public static bool ShouldPrintTeamPrefixNative(bool deathmatch, bool teamplay) => deathmatch && teamplay;

    public static bool ShouldPrintAllPrefixNative(bool deathmatch, bool teamplay) => deathmatch && teamplay;

    public static bool ShouldBreakAfterSayWhenDisabledNative(bool dropped) => dropped;

    public static bool ShouldBreakGlobalChatWhenDisabledNative(int showChat) => showChat < 1;

    public static bool ShouldBreakTeamChatWhenDisabledNative(int showChat) => showChat < 2;

    public static bool ShouldInvokeGiveCheatNative(bool rejectPredator, bool svCheats) => !rejectPredator && svCheats;

    public static bool ShouldSkipMutedPlayerNative(ulong mutedMask, int player) => ((mutedMask >> player) & 1) != 0;


    public static bool ShouldExecuteWarpCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool ShouldExecuteGenericCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool ShouldLogGenericCheatExecuteNative(int debugLevel) => debugLevel >= 2;

    public static bool ShouldLogGenericCheatRejectedNative(int debugLevel) => debugLevel >= 2;

    public static int ApplyChangemapCheatFlagNative(int levelFlags, int changemapCheatFlag) =>
        levelFlags | changemapCheatFlag;

    public static bool ShouldSetCenteringOnCenterviewCommandNative(int cmd, int centerviewCmd) =>
        cmd == centerviewCmd;

    public static bool ShouldRunSuicideCommandNative(int gameState, int levelState, bool paused, int playerState, int deadState) =>
        gameState == levelState && !paused && playerState != deadState;

}
