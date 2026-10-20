namespace HCDE.Client;

/// <summary><c>DoModSetup</c> and <c>ReadNetVars</c> from <c>gameconfigfile.cpp</c>.</summary>
public static class VmGameConfigModSetup
{
    public static bool DoModSetup(VmConfigFile config, string gameName, IDictionary<string, string> sink)
    {
        var any = false;
        if (config.SetSection($"{gameName}.Player.Mod"))
        {
            VmGameConfigCvars.ReadSection(config, sink);
            any = true;
        }

        if (config.SetSection($"{gameName}.LocalServerInfo.Mod"))
        {
            VmGameConfigCvars.ReadSection(config, sink);
            any = true;
        }

        if (config.SetSection($"{gameName}.ConfigOnlyVariables.Mod"))
        {
            VmGameConfigCvars.ReadSection(config, sink);
            any = true;
        }

        return any;
    }

    public static void ReadNetVars(
        VmConfigFile config,
        string gameName,
        IDictionary<string, string> sink,
        bool modSetup)
    {
        if (config.SetSection(VmGameConfigGameSetup.GameSection(gameName, "NetServerInfo")))
            VmGameConfigCvars.ReadSection(config, sink);
        if (modSetup && config.SetSection($"{gameName}.NetServerInfo.Mod"))
            VmGameConfigCvars.ReadSection(config, sink);
    }
}
