using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// FormatGUID, myasctime, CreatePath, RemoveFile, and RemoveDir from
/// cmdlib.cpp 467-645. Chunk 1393.
/// strbin starts at line 646 and stays outside this band.
/// This tree compiles the non-Windows CreatePath.
/// </summary>
public static class VmCmdlib1393Slice
{
    public const int MkdirMode = 493;
    public const int StrbinStart = 646;
    public const string StrbinSignature = "int strbin (char *str)";
    public const string AscTimeFormat = "%F %T";
    public const string UnknownTime = "Unknown\n";
    public static string ReadableTime = "";

    public readonly record struct GuidParts(uint Data1, ushort Data2, ushort Data3, byte D0, byte D1, byte D2, byte D3, byte D4, byte D5, byte D6, byte D7);

    public readonly record struct LocalStamp(int Year, int Month, int Day, int Hour, int Minute, int Second);

    public readonly record struct FsCall(string Path, bool Wide);

    public static string FormatGuid(GuidParts guid, int buffSize)
    {
        if (buffSize < 0)
            throw new ArgumentOutOfRangeException(nameof(buffSize));
        string full = string.Format(
            CultureInfo.InvariantCulture,
            "{{{0:x8}-{1:x4}-{2:x4}-{3:x2}{4:x2}-{5:x2}{6:x2}{7:x2}{8:x2}{9:x2}{10:x2}}}",
            guid.Data1, guid.Data2, guid.Data3,
            guid.D0, guid.D1, guid.D2, guid.D3, guid.D4, guid.D5, guid.D6, guid.D7);
        if (buffSize == 0)
            return "";
        int keep = Math.Min(full.Length, buffSize - 1);
        return full.Substring(0, keep);
    }

    public static string MyAscTime(LocalStamp? local)
    {
        if (local == null)
            ReadableTime = UnknownTime;
        else
        {
            LocalStamp t = local.Value;
            ReadableTime = string.Format(
                CultureInfo.InvariantCulture,
                "{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);
        }
        return ReadableTime;
    }

    public static void CreatePath(string fn, bool windows, Func<string, bool> exists, Func<string, int, int> mkdir)
    {
        if (fn == null)
            throw new ArgumentNullException(nameof(fn));
        if (exists == null)
            throw new ArgumentNullException(nameof(exists));
        if (mkdir == null)
            throw new ArgumentNullException(nameof(mkdir));
        if (windows)
            CreatePathWindows(fn, mkdir);
        else
            CreatePathPosix(fn, exists, mkdir);
    }

    public static void RemoveFile(string file, bool windows, Action<FsCall> remove)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (remove == null)
            throw new ArgumentNullException(nameof(remove));
        remove(new FsCall(file, windows));
    }

    public static int RemoveDir(string file, bool windows, Func<FsCall, int> remove)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (remove == null)
            throw new ArgumentNullException(nameof(remove));
        return remove(new FsCall(file, windows));
    }

    private static void CreatePathPosix(string fn, Func<string, bool> exists, Func<string, int, int> mkdir)
    {
        if (fn.Length > 0 && fn[0] == '/' && fn.Length == 1)
            return;
        char[] chars = fn.ToCharArray();
        int p = 0;
        while (true)
        {
            int slash = -1;
            for (int i = p + 1; i < chars.Length; i++)
            {
                if (chars[i] == '/')
                {
                    slash = i;
                    break;
                }
            }
            if (slash >= 0)
                chars[slash] = '\0';
            string copy = CutAtNull(chars);
            if (!exists(copy) && mkdir(copy, MkdirMode) == -1)
                return;
            if (slash >= 0)
                chars[slash] = '/';
            else
                break;
            p = slash;
        }
    }

    private static void CreatePathWindows(string fn, Func<string, int, int> mkdir)
    {
        if (fn.Length == 0)
            throw new ArgumentException(nameof(fn));
        char tail = fn[fn.Length - 1];
        if (tail != '\\' && tail != '/')
            DoCreatePathWindows(fn + "/", mkdir);
        else
            DoCreatePathWindows(fn, mkdir);
    }

    private static void DoCreatePathWindows(string fn, Func<string, int, int> mkdir)
    {
        SplitPath(fn, out string drive, out string dir);
        if (dir.Length == 0)
            return;
        string path = drive + dir;
        if (path.Length == 0)
            return;
        int end = path.Length;
        while (end > 0 && (path[end - 1] == '/' || path[end - 1] == '\\'))
            end--;
        if (end == 0)
            return;
        path = path.Substring(0, end);
        DoCreatePathWindows(path, mkdir);
        mkdir(path, -1);
    }

    private static void SplitPath(string fn, out string drive, out string dir)
    {
        drive = "";
        int rest = 0;
        if (fn.Length >= 2 && fn[1] == ':' && IsDrive(fn[0]))
        {
            drive = fn.Substring(0, 2);
            rest = 2;
        }
        int last = -1;
        for (int i = rest; i < fn.Length; i++)
        {
            if (fn[i] == '/' || fn[i] == '\\')
                last = i;
        }
        if (last < rest)
            dir = "";
        else
            dir = fn.Substring(rest, last - rest + 1);
    }

    private static bool IsDrive(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    private static string CutAtNull(char[] chars)
    {
        int n = 0;
        while (n < chars.Length && chars[n] != '\0')
            n++;
        return new string(chars, 0, n);
    }
}
