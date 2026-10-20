namespace HCDE.Client;

/// <summary><c>ArchiveGameData</c> / <c>ArchiveGlobalData</c> from <c>gameconfigfile.cpp</c>.</summary>
public static class VmGameConfigArchive
{
    public static void ArchiveGameData(
        VmConfigFile config,
        string gameName,
        bool modSetup,
        bool netgame,
        bool isHost,
        IReadOnlyDictionary<string, string> cvarArchive,
        VmKeyBindings bindings,
        VmKeyBindings doubleBindings,
        VmKeyBindings automapBindings,
        IReadOnlyDictionary<string, string>? aliases = null)
    {
        WriteCvarSection(config, VmGameConfigGameSetup.GameSection(gameName, "Player"), cvarArchive);
        if (modSetup)
            WriteCvarSection(config, $"{gameName}.Player.Mod", cvarArchive);
        WriteCvarSection(config, VmGameConfigGameSetup.GameSection(gameName, "ConsoleVariables"), cvarArchive);

        if (!netgame || isHost)
        {
            var serverSection = VmGameConfigGameSetup.GameSection(gameName, netgame ? "NetServerInfo" : "LocalServerInfo");
            WriteCvarSection(config, serverSection, cvarArchive);
            if (modSetup)
                WriteCvarSection(config, $"{gameName}.{(netgame ? "NetServerInfo" : "LocalServerInfo")}.Mod", cvarArchive);
        }

        WriteCvarSection(config, VmGameConfigGameSetup.GameSection(gameName, "ConfigOnlyVariables"), cvarArchive);
        if (modSetup)
            WriteCvarSection(config, $"{gameName}.ConfigOnlyVariables.Mod", cvarArchive);
        WriteCvarSection(config, VmGameConfigGameSetup.GameSection(gameName, "UnknownConsoleVariables"), cvarArchive);

        if (aliases is not null)
            WriteAliases(config, VmGameConfigGameSetup.GameSection(gameName, "ConsoleAliases"), aliases);

        ArchiveBindings(config, gameName, "Bindings", bindings);
        ArchiveBindings(config, gameName, "DoubleBindings", doubleBindings);
        ArchiveBindings(config, gameName, "AutomapBindings", automapBindings);
    }

    public static void ArchiveGlobalData(VmConfigFile config, string lastRunVersion, string release, IReadOnlyDictionary<string, string> globalCvars)
    {
        config.SetSection("LastRun", allowCreate: true);
        config.ClearCurrentSection();
        config.SetValueForKey("Version", lastRunVersion);
        config.SetValueForKey("Release", release);
        WriteCvarSection(config, "GlobalSettings", globalCvars);
        WriteCvarSection(config, "GlobalSettings.Unknown", globalCvars);
    }

    private static void WriteCvarSection(VmConfigFile config, string section, IReadOnlyDictionary<string, string> values)
    {
        config.SetSection(section, allowCreate: true);
        config.ClearCurrentSection();
        foreach (var (key, value) in values) config.SetValueForKey(key, value);
    }

    private static void WriteAliases(VmConfigFile config, string section, IReadOnlyDictionary<string, string> aliases)
    {
        config.SetSection(section, allowCreate: true);
        config.ClearCurrentSection();
        foreach (var (name, command) in aliases)
        {
            config.SetValueForKey("Name", name, duplicates: true);
            config.SetValueForKey("Command", command, duplicates: true);
        }
    }

    private static void ArchiveBindings(VmConfigFile config, string gameName, string label, VmKeyBindings bindings)
    {
        config.SetSection(VmGameConfigGameSetup.GameSection(gameName, label), allowCreate: true);
        bindings.ArchiveBindings(config);
    }
}
