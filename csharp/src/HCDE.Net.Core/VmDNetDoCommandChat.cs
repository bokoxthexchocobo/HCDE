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

    public enum DoCommandCaseKind
    {
        Say,
        Taunt,
        GiveCheat,
        TakeCheat,
        SetInv,
        WarpCheat,
        GenericCheat,
        Changemap,
        Suicide,
        Centerview,
    }

    public readonly record struct SayCaseInput(
        int ShowChat,
        ulong MutedMask,
        int Player,
        int Who,
        bool Deathmatch,
        bool Teamplay,
        int SpeakerTeam,
        int ConsoleTeam,
        bool NoBoldChat);

    public static (bool BreakCase, bool AllowGlobal, bool AllowTeam, bool UseBold) EvaluateSayCase(SayCaseInput input)
    {
        if (ShouldDropSayNative(input.ShowChat, input.MutedMask, input.Player))
            return (true, false, false, false);
        if (IsGlobalSayNative(input.Who))
        {
            if (ShouldBreakGlobalChatWhenDisabledNative(input.ShowChat))
                return (true, false, false, false);
            return (false, true, false, ShouldUseBoldChatNative(input.Who, input.NoBoldChat));
        }

        if (ShouldBreakTeamChatWhenDisabledNative(input.ShowChat))
            return (true, false, false, false);
        if (!SameTeamForTeamChatNative(input.Deathmatch, input.SpeakerTeam, input.ConsoleTeam))
            return (true, false, false, false);
        var bold = ShouldUseBoldChatNative(input.Who, input.NoBoldChat);
        return (false, false, true, bold);
    }

    public readonly record struct CheatCaseInput(
        bool RejectPredator,
        bool SvCheats,
        int Player,
        int ConsolePlayer,
        int DebugLevel);

    public static (bool Execute, bool LogRemote, bool LogExecute, bool LogRejected) EvaluateGiveCheatCase(
        CheatCaseInput input)
    {
        if (!ShouldExecuteGiveCheatNative(input.RejectPredator))
            return (false, false, false, ShouldLogCheatRejectedNative(input.DebugLevel));
        var execute = ShouldInvokeGiveCheatNative(input.RejectPredator, input.SvCheats);
        return (
            execute,
            ShouldLogRemoteGiveNative(input.Player, input.ConsolePlayer),
            execute && ShouldLogCheatExecuteNative(input.DebugLevel),
            !execute && ShouldLogCheatRejectedNative(input.DebugLevel));
    }

    public static (bool Execute, bool LogRejected) EvaluateTakeCheatCase(CheatCaseInput input) =>
        ShouldExecuteTakeCheatNative(input.RejectPredator)
            ? (ShouldInvokeGiveCheatNative(input.RejectPredator, input.SvCheats), false)
            : (false, ShouldLogCheatRejectedNative(input.DebugLevel));

    public static (bool Execute, bool ConsumePadding) EvaluateSetInvCase(bool rejectPredator) =>
        ShouldExecuteSetInvCheatNative(rejectPredator)
            ? (true, false)
            : (false, MustConsumeSetInvPaddingWhenRejectedNative(rejectPredator));

    public static (bool Execute, bool LogExecute, bool LogRejected) EvaluateGenericCheatCase(CheatCaseInput input)
    {
        if (!ShouldExecuteGenericCheatNative(input.RejectPredator))
            return (false, false, ShouldLogGenericCheatRejectedNative(input.DebugLevel));
        var execute = ShouldInvokeGiveCheatNative(input.RejectPredator, input.SvCheats);
        return (
            execute,
            execute && ShouldLogGenericCheatExecuteNative(input.DebugLevel),
            !execute && ShouldLogGenericCheatRejectedNative(input.DebugLevel));
    }

    public readonly record struct DoCommandCaseInput(
        DoCommandCaseKind Kind,
        SayCaseInput Say,
        CheatCaseInput Cheat,
        string? TauntVariant,
        object? TauntPawn,
        int LevelFlags,
        int ChangemapCheatFlag,
        int GameState,
        int LevelState,
        bool Paused,
        int PlayerState,
        int DeadState,
        int Cmd,
        int CenterviewCmd);

    public readonly record struct DoCommandCaseOutcome(
        bool Handled,
        bool BreakCase,
        int LevelFlags,
        string? TauntSound);

    public static DoCommandCaseOutcome RunDoCommandCase(DoCommandCaseInput input)
    {
        switch (input.Kind)
        {
            case DoCommandCaseKind.Say:
            {
                var say = EvaluateSayCase(input.Say);
                return new DoCommandCaseOutcome(true, say.BreakCase, input.LevelFlags, null);
            }
            case DoCommandCaseKind.Taunt:
            {
                if (!ShouldPlayTauntOnPawnNative(input.TauntPawn))
                    return new DoCommandCaseOutcome(true, false, input.LevelFlags, null);
                return new DoCommandCaseOutcome(true, false, input.LevelFlags, FormatTauntSoundNative(input.TauntVariant));
            }
            case DoCommandCaseKind.GiveCheat:
            {
                var give = EvaluateGiveCheatCase(input.Cheat);
                return new DoCommandCaseOutcome(true, !give.Execute, input.LevelFlags, null);
            }
            case DoCommandCaseKind.TakeCheat:
            {
                var take = EvaluateTakeCheatCase(input.Cheat);
                return new DoCommandCaseOutcome(true, !take.Execute, input.LevelFlags, null);
            }
            case DoCommandCaseKind.SetInv:
            {
                var setInv = EvaluateSetInvCase(input.Cheat.RejectPredator);
                return new DoCommandCaseOutcome(true, setInv.ConsumePadding, input.LevelFlags, null);
            }
            case DoCommandCaseKind.WarpCheat:
                return new DoCommandCaseOutcome(
                    true,
                    !ShouldExecuteWarpCheatNative(input.Cheat.RejectPredator),
                    input.LevelFlags,
                    null);
            case DoCommandCaseKind.GenericCheat:
            {
                var generic = EvaluateGenericCheatCase(input.Cheat);
                return new DoCommandCaseOutcome(true, !generic.Execute, input.LevelFlags, null);
            }
            case DoCommandCaseKind.Changemap:
                return new DoCommandCaseOutcome(
                    true,
                    false,
                    ApplyChangemapCheatFlagNative(input.LevelFlags, input.ChangemapCheatFlag),
                    null);
            case DoCommandCaseKind.Suicide:
                return new DoCommandCaseOutcome(
                    true,
                    !ShouldRunSuicideCommandNative(
                        input.GameState,
                        input.LevelState,
                        input.Paused,
                        input.PlayerState,
                        input.DeadState),
                    input.LevelFlags,
                    null);
            case DoCommandCaseKind.Centerview:
                return new DoCommandCaseOutcome(
                    true,
                    false,
                    input.LevelFlags,
                    null);
            default:
                return new DoCommandCaseOutcome(false, false, input.LevelFlags, null);
        }
    }
}
