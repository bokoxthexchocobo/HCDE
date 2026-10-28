namespace HCDE.Playsim;

/// <summary>
/// CVAR clamps and `sv_gametype` multiplayer rule mapping from <c>g_game.cpp</c> (**225–374**).
/// </summary>
public static class VmGGameCvars996Slice
{
    public const int DisplayNameTagsMax = 3;
    public const int SvGameTypeMax = 4;
    public const int SvGameTypeDeathmatch = 1;
    public const int SvGameTypeTeamDeathmatch = 2;
    public const int SvGameTypeCtfPlaceholder = 3;
    public const int SvGameTypeInvasion = 4;

    public readonly struct MultiplayerRuleFlags
    {
        public bool Deathmatch { get; init; }
        public bool Teamplay { get; init; }
        public bool UsesCtfPlaceholderRules { get; init; }
    }

    public readonly struct GameSessionFlags
    {
        public bool SendPause { get; init; }
        public bool SendSave { get; init; }
        public bool SendTurn180 { get; init; }
        public bool UserGame { get; init; }
        public bool InSave { get; init; }

        public bool BlocksExitCommands => InSave;

        public bool HasPendingNetworkEvents => SendPause || SendSave || SendTurn180;
    }

    public static int NormalizeDisplayNameTags(int value) =>
        value < 0 || value > DisplayNameTagsMax ? 0 : value;

    public static bool ShouldClampDisplayNameTags(int value) =>
        value < 0 || value > DisplayNameTagsMax;

    public static int ClampSvGameType(int value) =>
        value < 0 || value > SvGameTypeMax ? 0 : value;

    public static bool ShouldRejectSvGameType(int value) =>
        value < 0 || value > SvGameTypeMax;

    public static MultiplayerRuleFlags ResolveMultiplayerRulesFromSvGameType(int svGameType) =>
        svGameType switch
        {
            SvGameTypeDeathmatch => new() { Deathmatch = true, Teamplay = false },
            SvGameTypeTeamDeathmatch => new() { Deathmatch = true, Teamplay = true },
            SvGameTypeCtfPlaceholder => new()
            {
                Deathmatch = true,
                Teamplay = true,
                UsesCtfPlaceholderRules = true,
            },
            SvGameTypeInvasion => new() { Deathmatch = false, Teamplay = false },
            _ => new() { Deathmatch = false, Teamplay = false },
        };

    public static bool IsInvasionGameType(int svGameType) =>
        svGameType == SvGameTypeInvasion;

    public static bool UsesTeamDeathmatchRules(int svGameType) =>
        svGameType is SvGameTypeTeamDeathmatch or SvGameTypeCtfPlaceholder;

    public static GameSessionFlags CollectGameSessionFlags(
        bool sendPause,
        bool sendSave,
        bool sendTurn180,
        bool userGame,
        bool inSave) =>
        new()
        {
            SendPause = sendPause,
            SendSave = sendSave,
            SendTurn180 = sendTurn180,
            UserGame = userGame,
            InSave = inSave,
        };
}
