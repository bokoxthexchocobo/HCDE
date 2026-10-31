namespace HCDE.Playsim;

/// <summary>
/// Save-list scan, GZDoom 4.14 import, and the save-name comment from
/// <c>loadsavemenu.cpp</c> (1-285). Chunk 1310.
/// </summary>
public sealed class VmLoadSaveMenu1310
{
    public const int OldSaveStringSize = 24;
    public const int MinSaveVer = 4556;
    public const int SaveVer = 4560;
    public const string GameSig = "HCDE";
    public const string SaveExt = "zds";

    static int _oldSort;
    static readonly VmLoadSaveMenu1310 Shared = new(new Host());

    readonly Host _host;

    public List<Node> Saves { get; } = new();
    public int LastSaved { get; private set; }
    public int LastAccessed { get; private set; }
    public string? QuickSave { get; private set; }
    public int SortOrder { get; set; }

    public VmLoadSaveMenu1310(Host host)
    {
        _host = host;
    }

    public static VmLoadSaveMenu1310 GetManager() => Shared;

    public static void ResetProcess()
    {
        _oldSort = 0;
    }

    public static bool CheckGzdoom(string engine, string software)
    {
        const string prefix = "GZDoom g";
        if (!software.StartsWith(prefix, StringComparison.Ordinal))
            return false;
        if (software.Length < 11)
            return false;
        int major = software[8] - '0';
        if (major != 4 || software[9] != '.')
            return false;
        int minor = software[10] - '0';
        if (software.Length > 11 && software[11] >= '0' && software[11] <= '9')
            minor = minor * 10 + (software[11] - '0');
        if (minor > 14)
            return false;
        if (minor == 14 && software.Length > 13)
        {
            int patch = software[13] - '0';
            if (software.Length > 14 && software[14] >= '0' && software[14] <= '9')
                return false;
            if (patch > 2)
                return false;
        }

        return true;
    }

    public void ReadSaveStrings()
    {
        if (_oldSort != SortOrder)
        {
            Saves.Clear();
            _oldSort = SortOrder;
        }

        if (Saves.Count != 0)
            return;
        LastSaved = -1;
        LastAccessed = -1;
        QuickSave = null;
        if (!_host.Scan(_host.Folder, "*." + SaveExt, true))
            return;
        foreach (string entry in _host.Entries)
        {
            SaveFile? save = _host.Open(entry);
            if (save == null)
                continue;
            int info = save.Find("info.json");
            if (info < 0)
                continue;
            byte[] data = save.Read(info);
            if (!_host.OpenReader(data, out Arc arc))
                continue;
            int savever = arc.SaveVersion;
            string engine = arc.Get("Engine");
            string iwad = arc.Get("Game WAD");
            string title = arc.Get("Title");
            string created = arc.Get("Creation Time");
            string uuid = arc.Get("GameUUID");
            var allow = arc.HasAllow ? arc.Allow : new List<string>();
            if (string.Equals(engine, "GZDOOM", StringComparison.Ordinal))
            {
                if (!CheckGzdoom(engine, arc.Get("Software")))
                    continue;
            }
            else if ((!engine.Equals(GameSig, StringComparison.OrdinalIgnoreCase) && IndexOfIgnore(allow, GameSig) < 0)
                || savever > SaveVer)
            {
                continue;
            }

            bool oldVer = false;
            bool missing = false;
            if (savever < MinSaveVer)
            {
                oldVer = true;
            }
            else if (iwad.Equals(_host.Iwad, StringComparison.OrdinalIgnoreCase))
            {
                missing = !_host.CheckWads(arc, false);
            }
            else
            {
                continue;
            }

            Saves.Add(new Node
            {
                Filename = entry,
                Uuid = uuid,
                OldVersion = oldVer,
                MissingWads = missing,
                Title = title,
                Created = created,
            });
        }
    }

    public void PerformLoad(string file, bool flag)
    {
        _host.Load(file, flag);
    }

    public void PerformSave(string file, string title)
    {
        _host.Save(file, title);
    }

    public static string ExtractComment(string time, string comment)
    {
        string text = time;
        if (time.Length > 0)
            text += "\n";
        text += comment;
        return text;
    }

    public string BuildSaveName(string prefix, int slot)
    {
        return _host.BuildName($"{prefix}{slot:D2}");
    }

    static int IndexOfIgnore(List<string> values, string needle)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].Equals(needle, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public sealed class Node
    {
        public string Filename { get; init; } = "";
        public string Uuid { get; init; } = "";
        public bool OldVersion { get; init; }
        public bool MissingWads { get; init; }
        public string Title { get; init; } = "";
        public string Created { get; init; } = "";
    }

    public sealed class Arc
    {
        public int SaveVersion { get; init; }
        public bool HasAllow { get; init; }
        public List<string> Allow { get; } = new();
        public Dictionary<string, string> Strings { get; } = new();

        public string Get(string key) => Strings.TryGetValue(key, out string? value) ? value : "";
    }

    public sealed class SaveFile
    {
        public int Info { get; init; }
        public byte[] Data { get; init; } = Array.Empty<byte>();
        public bool ReaderOk { get; init; } = true;
        public Arc Arc { get; init; } = new();

        public int Find(string name) => name == "info.json" ? Info : -1;

        public byte[] Read(int index)
        {
            if (index < 0)
                throw new InvalidOperationException("info");
            return Data;
        }
    }

    public sealed class Host
    {
        public string Folder { get; set; } = "";
        public string Iwad { get; set; } = "";
        public bool ScanOk { get; set; } = true;
        public List<string> Entries { get; } = new();
        public Dictionary<string, SaveFile?> Files { get; } = new();
        public List<string> Calls { get; } = new();
        public bool WadsPresent { get; set; } = true;

        public bool Scan(string folder, string filter, bool recurse)
        {
            Calls.Add($"scan:{folder}:{filter}:{recurse}");
            return ScanOk;
        }

        public SaveFile? Open(string path)
        {
            Files.TryGetValue(path, out SaveFile? file);
            return file;
        }

        public bool OpenReader(byte[] data, out Arc arc)
        {
            foreach (SaveFile? file in Files.Values)
            {
                if (file != null && ReferenceEquals(file.Data, data))
                {
                    arc = file.Arc;
                    return file.ReaderOk;
                }
            }

            arc = new Arc();
            return false;
        }

        public bool CheckWads(Arc arc, bool strict)
        {
            Calls.Add($"wads:{arc.Get("Title")}:{strict}");
            return arc.Get("Title") != "gone";
        }

        public void Load(string file, bool flag)
        {
            Calls.Add($"load:{file}:{flag}");
        }

        public void Save(string file, string title)
        {
            Calls.Add($"save:{file}:{title}");
        }

        public string BuildName(string stem)
        {
            Calls.Add($"name:{stem}");
            return "path/" + stem + "." + SaveExt;
        }
    }
}
