namespace HCDE.Client;

/// <summary>Native <c>FGameConfigFile::ReadCVars</c>.</summary>
public static class VmGameConfigCvars
{
    public static void ReadSection(VmConfigFile config, IDictionary<string, string> sink)
    {
        while (config.NextInSection(out var key, out var value))
            sink[key] = value;
    }

    public static void ReadGlobalSettings(VmConfigFile config, IDictionary<string, string> sink)
    {
        if (config.SetSection("GlobalSettings.Unknown")) ReadSection(config, sink);
        if (config.SetSection("GlobalSettings")) ReadSection(config, sink);
    }
}
