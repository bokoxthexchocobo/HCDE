using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Directory lump scan from <c>file_directory.cpp</c> (<b>1–194</b>). Chunk <b>1227</b>.
/// Backup names are skipped after a C-locale fold. The relative path is stored
/// unchanged and the base path is the prefix of the first scanned entry.
/// Open still succeeds when the scan fails. A cached read joins that base to
/// the entry's original relative path.
/// </summary>
public sealed class VmDirScanEntry1227
{
    public string FileName = "";
    public string FilePath = "";
    public string FilePathRel = "";
    public ulong Length;
    public bool IsDirectory;
}

public sealed class VmDirLump1227
{
    public string FileName = "";
    public ulong Length;
    public ulong CompressedSize;
    public int Flags;
    public int ResourceId;
    public int Method;
    public int Namespace;
    public int Position;
}

public sealed class VmDirFilter1227
{
    public Func<string, string, bool>? FileNameCheck;
}

public sealed class VmDirReader1227
{
    public bool IsOpen;
    public bool Cached;
    public string Path = "";
    public byte[] Data = Array.Empty<byte>();
}

public sealed class VmDirHooks1227
{
    public Func<string, string> FullPath = static path => path;
    public Func<string, string, List<VmDirScanEntry1227>?> Scan =
        static (dir, match) => new List<VmDirScanEntry1227>();
    public Func<string, string> Normalize = static rel => rel;
    public Func<string, byte[]?> OpenBytes = static path => Array.Empty<byte>();
    public Action<VmDirectory1227>? AfterPost;
    public int ScanErrno;
}

public sealed class VmDirectory1227
{
    public const int FlagFullPath = 2;
    public const int MethodStored = 0;
    public const int NamespaceGlobal = 0;
    public const int ReaderCached = 2;
    public const ulong MaxStoredLength = 0x7fffffff;

    public readonly List<string> Log = new();
    public readonly List<VmDirLump1227> Entries = new();
    public readonly List<string> SystemFilePath = new();
    public string FileName = "";
    public bool NoSubdir;
    public string BasePath = "";
    public bool HasBase;
    public int NumLumps;
    public VmDirHooks1227 Hooks;

    public VmDirectory1227(string directory, bool nosubdir, VmDirHooks1227 hooks)
    {
        Hooks = hooks;
        NoSubdir = nosubdir;
        string fn = hooks.FullPath(directory);
        if (fn[^1] != '/')
            fn += "/";
        FileName = fn;
        Log.Add("ctor file=" + Bracket(FileName) + " nosubdir=" + (NoSubdir ? 1 : 0));
    }

    public static VmDirectory1227? Check(string filename, bool nosubdir, VmDirFilter1227? filter, VmDirHooks1227 hooks)
    {
        var rf = new VmDirectory1227(filename, nosubdir, hooks);
        if (rf.Open(filter))
        {
            rf.Log.Add("check keep=1 nosubdir=" + (rf.NoSubdir ? 1 : 0) + " lumps=" + rf.NumLumps);
            return rf;
        }

        rf.Log.Add("check drop");
        return null;
    }

    public bool Open(VmDirFilter1227? filter)
    {
        NumLumps = AddDirectory(FileName, filter);
        Log.Add("hash no_reader=1 lumps=" + NumLumps);
        Log.Add("post lumps=" + NumLumps);
        Hooks.AfterPost?.Invoke(this);
        Log.Add("open ok=1 lumps=" + NumLumps);
        return true;
    }

    public VmDirReader1227 GetEntryReader(uint entry, int readerType, int readerFlags)
    {
        _ = readerFlags;
        var reader = new VmDirReader1227();
        if (entry < (uint)NumLumps)
        {
            string fn = BasePath + SystemFilePath[Entries[(int)entry].Position];
            byte[]? data = Hooks.OpenBytes(fn);
            if (data == null)
            {
                Log.Add("reader entry=" + entry + " type=" + readerType + " path=" + Bracket(fn) + " open=0");
                return reader;
            }

            reader.IsOpen = true;
            reader.Path = fn;
            reader.Data = data;
            if (readerType == ReaderCached)
            {
                reader.Cached = true;
                Log.Add("reader entry=" + entry + " type=" + readerType + " path=" + Bracket(fn) + " open=1 cached=1 data=" + Hex(data));
            }
            else
            {
                Log.Add("reader entry=" + entry + " type=" + readerType + " path=" + Bracket(fn) + " open=1 cached=0");
            }
        }
        else
        {
            Log.Add("reader entry=" + entry + " type=" + readerType + " empty");
        }

        return reader;
    }

    int AddDirectory(string dirpath, VmDirFilter1227? filter)
    {
        int count = 0;
        Log.Add("scan dir=" + Bracket(dirpath) + " match=[*]");
        List<VmDirScanEntry1227>? list = Hooks.Scan(dirpath, "*");
        if (list == null)
        {
            Log.Add("msg 1 Could not scan '" + dirpath + "': " + StrError(Hooks.ScanErrno));
        }
        else
        {
            HasBase = false;
            BasePath = "";
            Entries.Clear();
            SystemFilePath.Clear();
            Log.Add("alloc n=" + list.Count);
            foreach (VmDirScanEntry1227 entry in list)
            {
                if (!HasBase)
                {
                    int full = entry.FilePath.LastIndexOf(entry.FilePathRel, StringComparison.Ordinal);
                    BasePath = full < 0 ? entry.FilePath : entry.FilePath.Substring(0, full);
                    HasBase = true;
                    Log.Add("base=" + Bracket(BasePath));
                }

                if (!entry.IsDirectory)
                {
                    string fi = Fold(entry.FileName);
                    if (fi.Contains(".orig", StringComparison.Ordinal) ||
                        fi.Contains(".bak", StringComparison.Ordinal) ||
                        fi.Contains(".cache", StringComparison.Ordinal))
                    {
                        Log.Add("bak " + Bracket(fi));
                        continue;
                    }

                    if (filter == null || filter.FileNameCheck == null || filter.FileNameCheck(fi, entry.FilePath))
                    {
                        if (entry.Length > MaxStoredLength)
                        {
                            Log.Add("msg 2 " + entry.FilePath + " is larger than 2GB and will be ignored");
                            continue;
                        }

                        SystemFilePath.Add(entry.FilePathRel);
                        var lump = new VmDirLump1227
                        {
                            FileName = Hooks.Normalize(entry.FilePathRel),
                            Length = entry.Length,
                            CompressedSize = entry.Length,
                            Flags = FlagFullPath,
                            ResourceId = -1,
                            Method = MethodStored,
                            Namespace = NamespaceGlobal,
                            Position = count,
                        };
                        Entries.Add(lump);
                        Log.Add(
                            "add pos=" + count +
                            " rel=" + Bracket(entry.FilePathRel) +
                            " norm=" + Bracket(lump.FileName) +
                            " len=" + entry.Length +
                            " comp=" + lump.CompressedSize +
                            " flags=2 id=-1 method=0 ns=0");
                        count++;
                    }
                    else
                    {
                        Log.Add("reject " + Bracket(fi) + " " + Bracket(entry.FilePath));
                    }
                }
                else
                {
                    Log.Add("dir " + Bracket(entry.FileName));
                }
            }
        }

        return count;
    }

    static string Fold(string name)
    {
        var chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c >= 'A' && c <= 'Z')
                chars[i] = (char)(c + 32);
        }

        return new string(chars);
    }

    static string StrError(int err)
    {
        if (err == 2)
            return "No such file or directory";
        if (err == 13)
            return "Permission denied";
        return "Unknown error " + err;
    }

    static string Hex(byte[] data)
    {
        var sb = new StringBuilder(data.Length * 2);
        for (int i = 0; i < data.Length; i++)
            sb.Append(data[i].ToString("x2"));
        return sb.ToString();
    }

    public static string Esc(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (c >= 32 && c < 127 && c != '\\')
                sb.Append(c);
            else
                sb.Append("\\x").Append(((int)c).ToString("x2"));
        }

        return sb.ToString();
    }

    public static string Bracket(string text) => "[" + Esc(text) + "]";
}
