namespace HCDE.Gamedata;

public sealed class VmTeamDefinition
{
    public string Name { get; set; } = "";
    public int PlayerColor { get; set; }
    public string TextColor { get; set; } = "";
    public string Logo { get; set; } = "";
    public bool AllowCustomPlayerColor { get; set; }
}

public static class VmTeamInfoParser
{
    public static bool TryParseDocument(IReadOnlyList<string> tokens, ref int i, List<VmTeamDefinition> teams, string activeGame, List<string> errors)
    {
        while (i < tokens.Count)
        {
            var word = tokens[i++];
            if (word.Equals("ClearTeams", StringComparison.OrdinalIgnoreCase))
            {
                teams.Clear();
                continue;
            }

            if (word.Equals("Team", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseTeam(tokens, ref i, teams, activeGame, errors)) return false;
                continue;
            }

            errors.Add($"Unknown TEAMINFO command '{word}'.");
            return false;
        }

        if (teams.Count < 2)
            errors.Add("At least two teams must be defined in TEAMINFO.");
        return errors.Count == 0;
    }

    public static bool TryParseTeam(IReadOnlyList<string> tokens, ref int i, List<VmTeamDefinition> teams, string activeGame, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected team name."); return false; }
        var team = new VmTeamDefinition { Name = tokens[i++] };
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after team name."); return false; }
        i++;
        var valid = -1;
        while (i < tokens.Count && tokens[i] != "}")
        {
            var option = tokens[i++];
            if (option.Equals("Game", StringComparison.OrdinalIgnoreCase))
            {
                if (i >= tokens.Count) return false;
                var game = tokens[i++];
                if (game.Equals("Any", StringComparison.OrdinalIgnoreCase)) valid = 1;
                else if (VmGameInfoParser.CheckGame(game, activeGame)) valid = 1;
                else if (valid == -1) valid = 0;
            }
            else if (option.Equals("PlayerColor", StringComparison.OrdinalIgnoreCase))
            {
                if (i < tokens.Count) team.PlayerColor = ParseColor(tokens[i++]);
            }
            else if (option.Equals("TextColor", StringComparison.OrdinalIgnoreCase))
            {
                if (i < tokens.Count) team.TextColor = $"[{tokens[i++]}]";
            }
            else if (option.Equals("Logo", StringComparison.OrdinalIgnoreCase))
            {
                if (i < tokens.Count) team.Logo = tokens[i++];
            }
            else if (option.Equals("AllowCustomPlayerColor", StringComparison.OrdinalIgnoreCase))
                team.AllowCustomPlayerColor = true;
            else if (option.Equals("PlayerStartThingNumber", StringComparison.OrdinalIgnoreCase))
            {
                if (i < tokens.Count) i++;
            }
            else
            {
                if (i < tokens.Count) i++;
            }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (valid != 0) teams.Add(team);
        return true;
    }

    private static int ParseColor(string token) =>
        token.StartsWith('#') && int.TryParse(token[1..], System.Globalization.NumberStyles.HexNumber, null, out var rgb)
            ? rgb
            : 0;

    public static bool IsValidTeamIndex(uint teamIndex, int teamCount) =>
        teamIndex < (uint)teamCount;

    public static int GetTeamTextColor(VmTeamDefinition team, Func<string, int?>? parseFontColor = null)
    {
        if (string.IsNullOrEmpty(team.TextColor)) return VmSkillInfoRuntime.CrUntranslated;
        return parseFontColor?.Invoke(team.TextColor) ?? VmSkillInfoRuntime.CrUntranslated;
    }

    public static IReadOnlyList<string> FormatTeamList(IReadOnlyList<VmTeamDefinition> teams)
    {
        var lines = new List<string> { "Defined teams are as follows:" };
        for (var i = 0; i < teams.Count; i++)
            lines.Add($"{i} : {teams[i].Name}");
        lines.Add("End of team list.");
        return lines;
    }
}
