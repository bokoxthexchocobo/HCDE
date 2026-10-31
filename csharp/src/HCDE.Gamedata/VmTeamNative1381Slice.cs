namespace HCDE.Gamedata;

/// <summary>
/// Team validity, team change, getters, the team-list command, and the VM
/// natives from <c>teaminfo.cpp</c> 232-434. <c>ClearTeams</c> stays on 228-231.
/// <c>TeamChanged</c>, translation build, font-color parse, and texture lookup
/// are callees.
/// </summary>
public sealed class VmTeamNative1381Slice
{
    public const int MaxPlayers = 64;
    public const int CrUndefined = -1;
    public const int CrUntranslated = 11;
    public const int TextureAny = 0;
    public const string TeamListCommand = "teamlist";

    public readonly record struct Export(string ClassName, string ScriptName, string Native, int Bits);

    public static readonly Export[] Exports =
    [
        new("", "Teams", "Teams", 0),
        new("FTeam", "mName", "m_Name", 0),
    ];

    public static readonly string[] Natives =
    [
        "IsValid", "ChangeTeam", "GetPlayerColor", "GetTextColor",
        "GetLogo", "GetLogoName", "AllowsCustomPlayerColor",
    ];

    public sealed class Team
    {
        public string Name = "";
        public int PlayerColor;
        public string TextColor = "";
        public string Logo = "";
        public bool AllowCustomPlayerColor;
    }

    public bool Multiplayer { get; set; }
    public bool Teamplay { get; set; }
    public bool[] InGame { get; } = new bool[MaxPlayers];
    public int[] CurrentTeam { get; } = new int[MaxPlayers];
    public List<Team> Teams { get; } = [];
    public List<string> Lines { get; } = [];
    public List<string> Trace { get; } = [];
    public Func<string, int, int, int>? ParseFontColor { get; set; }
    public Func<string, int, int>? CheckTexture { get; set; }

    public bool IsValid(uint id) => id < (uint)Teams.Count;

    public bool ChangeTeam(uint player, uint newTeam)
    {
        if (!Multiplayer || !Teamplay || player >= (uint)MaxPlayers || !InGame[(int)player]
            || !IsValid(newTeam) || (uint)CurrentTeam[(int)player] == newTeam)
            return false;
        var stored = unchecked((int)newTeam);
        Trace.Add("changed " + player + " " + stored);
        CurrentTeam[(int)player] = stored;
        Trace.Add("translation " + player);
        return true;
    }

    public string GetName(Team team) => team.Name;

    public int GetPlayerColor(Team team) => team.PlayerColor;

    public int GetTextColor(Team team)
    {
        if (team.TextColor.Length == 0)
            return CrUntranslated;
        if (ParseFontColor == null)
            throw new InvalidOperationException("V_ParseFontColor");
        var parsed = ParseFontColor(team.TextColor, 0, 0);
        if (parsed == CrUndefined)
        {
            Lines.Add(
                "GetTextColor: Undefined color '" + team.TextColor
                + "' in definition of team '" + team.Name + "'.\n");
            return CrUntranslated;
        }

        return parsed;
    }

    public string GetLogo(Team team) => team.Logo;

    public bool GetAllowCustomPlayerColor(Team team) => team.AllowCustomPlayerColor;

    public int LogoIndex(Team team)
    {
        if (team.Logo.Length == 0)
            return -1;
        if (CheckTexture == null)
            throw new InvalidOperationException("CheckForTexture");
        return CheckTexture(team.Logo, TextureAny);
    }

    public void TeamList()
    {
        Lines.Add("Defined teams are as follows:\n");
        for (var i = 0; i < Teams.Count; i++)
            Lines.Add(i + " : " + GetName(Teams[i]) + "\n");
        Lines.Add("End of team list.\n");
    }

    public int CallIsValid(uint id, int numret, out int value)
        => ReturnInt(numret, IsValid(id) ? 1 : 0, out value);

    public int CallChangeTeam(uint player, uint newTeam, int numret, out int value)
        => ReturnInt(numret, ChangeTeam(player, newTeam) ? 1 : 0, out value);

    public int CallGetPlayerColor(Team? self, int numret, out int value)
        => ReturnInt(numret, GetPlayerColor(Require(self)), out value);

    public int CallGetTextColor(Team? self, int numret, out int value)
        => ReturnInt(numret, GetTextColor(Require(self)), out value);

    public int CallGetLogo(Team? self, int numret, out int value)
        => ReturnInt(numret, LogoIndex(Require(self)), out value);

    public int CallGetLogoName(Team? self, int numret, out string? value)
    {
        var logo = GetLogo(Require(self));
        if (numret > 0)
        {
            value = logo;
            return 1;
        }

        value = null;
        return 0;
    }

    public int CallAllowsCustom(Team? self, int numret, out int value)
        => ReturnInt(numret, GetAllowCustomPlayerColor(Require(self)) ? 1 : 0, out value);

    public void WriteLogoName(Team? self, out string value) => value = GetLogo(Require(self));

    static Team Require(Team? self)
    {
        if (self == null)
            throw new InvalidOperationException("In function parameter self");
        return self;
    }

    static int ReturnInt(int numret, int raw, out int value)
    {
        if (numret > 0)
        {
            value = raw;
            return 1;
        }

        value = 0;
        return 0;
    }
}
