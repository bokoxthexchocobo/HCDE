namespace HCDE.Client;

/// <summary>Portable <c>FGameConfigFile::DoGameSetup</c> section and alias parsing.</summary>
public static class VmGameConfigGameSetup
{
    public static string GameSection(string gameName, string subsection) => $"{gameName}.{subsection}";

    public static void ReadGameCvarSections(VmConfigFile config, string gameName, IDictionary<string, string> sink)
    {
        foreach (var subsection in new[]
                 {
                     "UnknownConsoleVariables", "ConfigOnlyVariables", "ConsoleVariables",
                     "LocalServerInfo", "Player"
                 })
        {
            if (config.SetSection(GameSection(gameName, subsection)))
                VmGameConfigCvars.ReadSection(config, sink);
        }
    }

    public static Dictionary<string, string> ReadConsoleAliases(VmConfigFile config, string gameName)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!config.SetSection(GameSection(gameName, "ConsoleAliases"))) return aliases;

        string? name = null;
        while (config.NextInSection(out var key, out var value))
        {
            if (string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
                name = value;
            else if (string.Equals(key, "Command", StringComparison.OrdinalIgnoreCase) && name is not null)
            {
                aliases[name] = value;
                name = null;
            }
        }

        return aliases;
    }

    public static void ApplyHcdeUiBaseline(double lastRunVersion, IDictionary<string, string> cvars)
    {
        if (lastRunVersion >= 232) return;
        cvars["uiscale"] = "1";
        cvars["screenblocks"] = "12";
        cvars["m_cleanscale"] = "true";
    }
}
