namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> chat/taunt/print/cheat cases (<c>d_net.cpp</c> 8538–8693).</summary>
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

    public static bool ShouldExecuteTakeCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool ShouldExecuteSetInvCheatNative(bool rejectPredator) => !rejectPredator;

    public static bool MustConsumeSetInvPaddingWhenRejectedNative(bool rejectPredator) => rejectPredator;

    public static bool ShouldLogCheatExecuteNative(int debugLevel) => debugLevel >= 2;

    public static bool ShouldLogCheatRejectedNative(int debugLevel) => debugLevel >= 2;

    public static bool ShouldPlayChatSoundNative(bool noChatSound) => !noChatSound;

    public static bool ShouldPrintTeamPrefixNative(bool deathmatch, bool teamplay) => deathmatch && teamplay;

    public static bool ShouldPrintAllPrefixNative(bool deathmatch, bool teamplay) => deathmatch && teamplay;

    public static int ReadWhoByteNative(int who) => who;

    public static bool ShouldBreakAfterSayWhenDisabledNative(bool dropped) => dropped;

    public static bool ShouldBreakGlobalChatWhenDisabledNative(int showChat) => showChat < 1;

    public static bool ShouldBreakTeamChatWhenDisabledNative(int showChat) => showChat < 2;

    public static bool ShouldInvokeMusicChangeNative() => true;

    public static bool ShouldInvokePrintNative() => true;

    public static bool ShouldInvokeCenterPrintNative() => true;

    public static bool ShouldReadUserInfoChangeNative() => true;

    public static bool ShouldApplyServerInfoChangeNative(bool xorMode) => true;

    public static string FormatRemoteGiveMessageNative(string template, string playerName, string item) =>
        template.Replace("%s", playerName, StringComparison.Ordinal);

    public static bool ShouldInvokeGiveCheatNative(bool rejectPredator, bool svCheats) => !rejectPredator && svCheats;

    public static bool ShouldSkipMutedPlayerNative(ulong mutedMask, int player) => ((mutedMask >> player) & 1) != 0;

    public static bool ShouldAdvanceStreamAfterSayNative(bool dropped) => !dropped;

    public static bool ShouldReadGiveAmountNative() => true;

    public static bool ShouldReadSetInvAmountNative() => true;
}
