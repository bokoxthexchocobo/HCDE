namespace HCDE.Client;

/// <summary><c>CollectDefaultSearchPaths</c> from <c>gameconfigfile.cpp</c> (portable subset).</summary>
public static class VmGameConfigSearchPaths
{
    private static readonly string[] LinuxGameDirs = ["/games/gzdoom", "/games/doom", "/doom"];

    public static IReadOnlyList<string> Collect(
        string? homeDir,
        string? dataDir,
        string shareDir = "/usr/local/share",
        string defaultShareDir = "/usr/local/share")
    {
        var paths = new List<string>();
        if (OperatingSystem.IsMacOS())
        {
            if (!string.IsNullOrEmpty(homeDir)) paths.Add(homeDir);
            paths.Add("$PROGDIR");
            return paths;
        }

        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD())
        {
            if (!string.IsNullOrEmpty(homeDir)) paths.Add("$HOME");
            paths.Add("$PROGDIR");
            return paths;
        }

        var shareDirChanged = !string.Equals(shareDir, defaultShareDir, StringComparison.Ordinal);
        paths.Add("$PROGDIR");
        dataDir ??= "";
        foreach (var gameDir in LinuxGameDirs)
        {
            paths.Add(dataDir + gameDir);
            paths.Add(shareDir + gameDir);
            if (shareDirChanged) paths.Add(defaultShareDir + gameDir);
            paths.Add("/usr/share" + gameDir);
        }

        return paths;
    }
}
