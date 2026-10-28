namespace HCDE.Gamedata;

/// <summary>FTeam runtime helpers (gaps) from <c>teaminfo.cpp</c>.</summary>
public static class VmTeamInfoRuntime
{
    public const int MaxPlayers = 32;

    public static bool TryChangeTeam(
        bool multiplayer,
        bool teamplay,
        uint playerNum,
        bool playerInGame,
        int currentTeam,
        uint newTeam,
        int teamCount)
    {
        if (!multiplayer || !teamplay) return false;
        if (playerNum >= MaxPlayers || !playerInGame) return false;
        if (!VmTeamInfoParser.IsValidTeamIndex(newTeam, teamCount)) return false;
        if (currentTeam == newTeam) return false;
        return true;
    }

    public static string GetTeamName(VmTeamDefinition team) => team.Name;

    public static int GetPlayerColor(VmTeamDefinition team) => team.PlayerColor;

    public static bool GetAllowCustomPlayerColor(VmTeamDefinition team) => team.AllowCustomPlayerColor;
}
