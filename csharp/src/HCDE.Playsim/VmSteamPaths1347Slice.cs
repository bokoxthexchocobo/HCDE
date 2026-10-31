namespace HCDE.Playsim;

/// <summary>
/// GOG, Steam, and Bethesda registry install paths from <c>i_steam.cpp</c> (1-289).
/// Chunk 1347. The registry read is the query this file performs. The 64-bit
/// build inserts Wow6432Node; the 32-bit build does not.
/// </summary>
public static class VmSteamPaths1347Slice
{
    public const int RegSz = 1;
    public const string HiveLocalMachine = "HKLM";
    public const string HiveCurrentUser = "HKCU";

    public static bool QueryPathKey(RegStore1347 store, string hive, string keyPath, string valueName, out string value)
    {
        value = "";
        if (!store.TryOpen(hive, keyPath, out var key))
            return false;
        if (key.Size(valueName, out var type, out var pathLen) && type == RegSz && pathLen != 0 && key.Read(valueName, out var text))
            value = text;
        key.Closed = true;
        return value.Length > 0;
    }
}

public sealed class RegKey1347
{
    public int Type = VmSteamPaths1347Slice.RegSz;
    public int PathLen = 2;
    public string Text = "";
    public bool SizeOk = true;
    public bool ReadOk = true;
    public bool Closed;
    public int SizeQueries;
    public int ReadQueries;
    public string LastName = "";

    public bool Size(string name, out int type, out int pathLen)
    {
        SizeQueries++;
        LastName = name;
        type = Type;
        pathLen = PathLen;
        return SizeOk;
    }

    public bool Read(string name, out string text)
    {
        ReadQueries++;
        LastName = name;
        text = Text;
        return ReadOk;
    }
}

public sealed class RegStore1347
{
    public Dictionary<(string Hive, string Path), RegKey1347> Keys = new();
    public List<string> Opens = new();

    public bool TryOpen(string hive, string path, out RegKey1347 key)
    {
        Opens.Add(hive + ":" + path);
        if (Keys.TryGetValue((hive, path), out var found))
        {
            key = found;
            return true;
        }

        key = new RegKey1347();
        return false;
    }
}

public sealed class VmSteamPaths1347
{
    public bool Win64 = true;
    public RegStore1347 Store = new();

    public string GogRoot => Win64 ? "Software\\Wow6432Node\\GOG.com\\Games" : "Software\\GOG.com\\Games";

    public string BethesdaRoot => Win64
        ? "Software\\Wow6432Node\\Bethesda Softworks\\Bethesda.net"
        : "Software\\Bethesda Softworks\\Bethesda.net";

    public List<string> GogPaths()
    {
        var result = new List<string>();
        Push(result, "1435827232", "");
        Push(result, "2015545325", "/DOOM_Data/StreamingAssets");
        Push(result, "1435848814", "/doom2");
        Push(result, "1426071866", "/DOOM II_Data/StreamingAssets");
        Push(result, "1413291984", "");
        var finalPath = Read("1435848742");
        if (finalPath != null)
        {
            result.Add(finalPath + "/TNT");
            result.Add(finalPath + "/Plutonia");
        }

        Push(result, "1135892318", "/base/wads");
        Push(result, "1432899949", "");
        Push(result, "1290366318", "");
        Push(result, "1247951670", "");
        Push(result, "1983497091", "");
        return result;
    }

    public string SteamPath()
    {
        if (VmSteamPaths1347Slice.QueryPathKey(Store, VmSteamPaths1347Slice.HiveCurrentUser, "Software\\Valve\\Steam", "SteamPath", out var path))
            return path;
        if (VmSteamPaths1347Slice.QueryPathKey(Store, VmSteamPaths1347Slice.HiveLocalMachine, "Software\\Valve\\Steam", "InstallPath", out path))
            return path;
        return "";
    }

    public List<string> BethesdaPaths()
    {
        var result = new List<string>();
        if (!VmSteamPaths1347Slice.QueryPathKey(Store, VmSteamPaths1347Slice.HiveLocalMachine, BethesdaRoot, "installLocation", out var path))
            return result;
        path += "/games/";
        string[] dirs =
        {
            "DOOM_Classic_2019/base",
            "DOOM_Classic_2019/rerelease/DOOM_Data/StreamingAssets",
            "DOOM_II_Classic_2019/base",
            "DOOM_II_Classic_2019/rerelease/DOOM II_Data/StreamingAssets",
            "DOOM 3 BFG Edition/base/wads",
            "Heretic Shadow of the Serpent Riders/base",
            "Hexen/base",
            "Hexen Deathkings of the Dark Citadel/base",
        };
        foreach (var dir in dirs)
            result.Add(path + dir);
        return result;
    }

    void Push(List<string> result, string gameId, string suffix)
    {
        var path = Read(gameId);
        if (path != null)
            result.Add(path + suffix);
    }

    string? Read(string gameId)
    {
        if (VmSteamPaths1347Slice.QueryPathKey(Store, VmSteamPaths1347Slice.HiveLocalMachine, GogRoot + "\\" + gameId, "Path", out var path))
            return path;
        return null;
    }
}
