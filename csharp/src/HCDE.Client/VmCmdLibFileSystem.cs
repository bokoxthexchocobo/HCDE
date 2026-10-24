namespace HCDE.Client;

/// <summary>Native <c>cmdlib.cpp</c> filesystem probes (managed IO backend).</summary>
public static class VmCmdLibFileSystem
{
    public static bool FileExists(string filename) => File.Exists(filename);

    public static bool DirExists(string filename) => Directory.Exists(filename);

    public static bool GetFileInfo(string pathname, out long size, out DateTime lastWriteUtc)
    {
        size = 0;
        lastWriteUtc = default;
        if (string.IsNullOrEmpty(pathname)) return false;
        if (!File.Exists(pathname)) return false;
        var info = new FileInfo(pathname);
        size = info.Length;
        lastWriteUtc = info.LastWriteTimeUtc;
        return true;
    }
}
