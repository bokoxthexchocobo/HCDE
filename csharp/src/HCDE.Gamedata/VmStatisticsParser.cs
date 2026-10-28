namespace HCDE.Gamedata;

public sealed class VmStatisticsEpisode
{
    public string Header { get; set; } = "";
    public string Name { get; set; } = "";
    public List<VmStatisticsSession> Sessions { get; } = [];
}

public sealed class VmStatisticsSession
{
    public string PlayerName { get; set; } = "";
    public string Info { get; set; } = "";
    public int TimeNeededTics { get; set; }
    public int Skill { get; set; }
    public List<VmStatisticsLevelEntry> Levels { get; } = [];
}

public sealed class VmStatisticsLevelEntry
{
    public string Name { get; set; } = "";
    public string Info { get; set; } = "";
    public int TimeNeededTics { get; set; }
}

public static class VmStatisticsParser
{
    public const int TicRate = 35;

    public static int ParseTimeToTics(string hhmmss)
    {
        var parts = hhmmss.Split(':');
        if (parts.Length != 3) return 0;
        var h = int.Parse(parts[0]);
        var m = int.Parse(parts[1]);
        var s = int.Parse(parts[2]);
        return (((h * 60) + m) * 60 + s) * TicRate;
    }

    public static bool TryParseDocument(IReadOnlyList<string> tokens, ref int i, List<VmStatisticsEpisode> episodes, List<string> errors)
    {
        while (i < tokens.Count)
        {
            var ep = new VmStatisticsEpisode { Header = tokens[i++] };
            if (i >= tokens.Count) return false;
            ep.Name = tokens[i++];
            if (i >= tokens.Count || tokens[i] != "{") return false;
            i++;
            while (i < tokens.Count && tokens[i] != "}")
            {
                var session = new VmStatisticsSession
                {
                    PlayerName = tokens[i++],
                    Info = tokens[i++]
                };
                session.TimeNeededTics = ParseTimeToTics(tokens[i++]);
                session.Skill = int.Parse(tokens[i++]);
                if (i < tokens.Count && tokens[i] == "{")
                {
                    i++;
                    while (i < tokens.Count && tokens[i] != "}")
                    {
                        session.Levels.Add(new VmStatisticsLevelEntry
                        {
                            Name = tokens[i++],
                            Info = tokens[i++],
                            TimeNeededTics = ParseTimeToTics(tokens[i++])
                        });
                    }

                    if (i < tokens.Count && tokens[i] == "}") i++;
                }

                ep.Sessions.Add(session);
            }

            if (i < tokens.Count && tokens[i] == "}") i++;
            episodes.Add(ep);
        }

        return errors.Count == 0;
    }

    public static int CompareEpisodeNames(string a, string b) =>
        string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
}
