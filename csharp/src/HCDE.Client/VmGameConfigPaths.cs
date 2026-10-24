namespace HCDE.Client;

/// <summary><c>FGameConfigFile::GetConfigPath</c>.</summary>
public static class VmGameConfigPaths
{
    public static string GetConfigPath(VmCommandLineArgs? args, bool tryProg, Func<bool, string> getDefaultPath)
    {
        var overridePath = args?.CheckValue("-config");
        if (!string.IsNullOrEmpty(overridePath)) return overridePath;
        return getDefaultPath(tryProg);
    }
}
