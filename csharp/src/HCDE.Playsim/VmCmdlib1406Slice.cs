namespace HCDE.Playsim;

/// <summary>
/// copystring, file and directory probes, and the one-level search from
/// cmdlib.cpp 92-241. Chunk 1406.
/// GetFileInfo starts at line 242 and stays outside this band.
/// WideString and the stat and access calls stay callees.
/// </summary>
public static class VmCmdlib1406Slice
{
    public const int BandStart = 92;
    public const int BandEnd = 241;
    public const int IfDir = 0x4000;
    public const int ReadMode = 4;
    public const int GetFileInfoStart = 242;
    public const string ScanMatch = "*";

    public readonly record struct StatQuery(string Path, bool Wide);

    public readonly record struct StatResult(bool Ok, int Mode);

    public readonly record struct AccessQuery(string? Path, int Mode, bool Wide);

    public readonly record struct Listed(string FilePath, bool IsDirectory, bool IsHidden, bool IsSystem);

    public readonly record struct ScanQuery(string Path, string Match, bool NoSubdir, bool ReadHidden);

    public readonly record struct ScanResult(bool Ok, IReadOnlyList<Listed> Entries);

    public static char[] CopyString(string? text)
    {
        if (text == null)
            return new[] { '\0' };
        int end = text.IndexOf('\0');
        int length = end >= 0 ? end : text.Length;
        var buffer = new char[length + 1];
        text.CopyTo(0, buffer, 0, length);
        buffer[length] = '\0';
        return buffer;
    }

    public static bool FileExists(
        string? filename,
        bool windows,
        Func<string?, string>? wide,
        Func<StatQuery, StatResult> stat)
    {
        return DirEntryExists(filename, true, windows, wide, stat, out bool isDirectory) && !isDirectory;
    }

    public static bool DirExists(
        string? filename,
        bool windows,
        Func<string?, string>? wide,
        Func<StatQuery, StatResult> stat)
    {
        return DirEntryExists(filename, true, windows, wide, stat, out bool isDirectory) && isDirectory;
    }

    public static string RecursiveFileExists(
        string path,
        string file,
        bool windows,
        Func<string?, string>? wide,
        Func<StatQuery, StatResult> stat,
        Func<ScanQuery, ScanResult> scan)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (scan == null)
            throw new ArgumentNullException(nameof(scan));
        string joined = path + "/" + file;
        if (FileExists(joined, windows, wide, stat))
            return joined;
        ScanResult found = scan(new ScanQuery(path, ScanMatch, false, false));
        if (!found.Ok)
            return "";
        if (found.Entries == null)
            throw new InvalidOperationException("ScanDirectory");
        foreach (Listed entry in found.Entries)
        {
            if (entry.IsDirectory && !entry.IsHidden && !entry.IsSystem)
            {
                string child = entry.FilePath + "/" + file;
                if (FileExists(child, windows, wide, stat))
                    return child;
            }
        }
        return "";
    }

    public static bool FileReadable(
        string? filename,
        bool windows,
        Func<string?, string>? wide,
        Func<AccessQuery, int> access)
    {
        if (access == null)
            throw new ArgumentNullException(nameof(access));
        if (windows)
        {
            if (wide == null)
                throw new ArgumentNullException(nameof(wide));
            string probed = wide(filename) ?? throw new InvalidOperationException("WideString");
            return access(new AccessQuery(probed, ReadMode, true)) == 0;
        }
        return access(new AccessQuery(filename, ReadMode, false)) == 0;
    }

    public static bool DirEntryExists(
        string? pathname,
        bool writeDirectory,
        bool windows,
        Func<string?, string>? wide,
        Func<StatQuery, StatResult> stat,
        out bool isDirectory)
    {
        isDirectory = false;
        if (string.IsNullOrEmpty(pathname))
            return false;
        if (stat == null)
            throw new ArgumentNullException(nameof(stat));
        string probed = pathname;
        if (windows)
        {
            if (wide == null)
                throw new ArgumentNullException(nameof(wide));
            probed = wide(pathname) ?? throw new InvalidOperationException("WideString");
        }
        StatResult info = stat(new StatQuery(probed, windows));
        if (writeDirectory)
            isDirectory = (info.Mode & IfDir) != 0;
        return info.Ok;
    }
}
