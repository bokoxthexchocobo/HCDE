namespace HCDE.Client;

/// <summary>
/// <c>D_AddWildFile</c>, <c>D_AddConfigFiles</c>, <c>D_AddDirectory</c>,
/// <c>BaseFileSearch</c>, and <c>D_FileNotFound</c> from <c>findfile.cpp</c> 165-391.
/// Lines 119-164 stay chunk 137. <c>D_AddFile</c>'s stat walk stays injected.
/// Linux treats only <c>/</c> as a separator. <c>PATH_SEPARATOR</c> defaults to <c>:</c>.
/// </summary>
public sealed class VmFindFile1369Slice
{
    public const int RequireNone = 0;
    public const int RequireIwad = 1;
    public const int RequireFile = 2;
    public const int RequireOptFile = 4;

    public string ProgDir { get; set; } = "";
    public int ExitOnNotFound { get; set; } = RequireIwad | RequireFile;
    public char PathSeparator { get; set; } = ':';
    public Func<string, bool>? DirExists { get; set; }
    public Func<string, string>? NicePath { get; set; }
    public Func<string, string>? ExpandEnv { get; set; }
    public Func<string, string, string>? RecursiveExists { get; set; }
    public Func<string, string, bool, IReadOnlyList<ScanHit>?>? Scan { get; set; }
    public Action<IList<string>, string, bool, int, bool, ConfigFile?>? AddFile { get; set; }
    public Action<string>? Fatal { get; set; }
    public List<string> Trace { get; } = [];
    public List<string> Prints { get; } = [];
    public string? LastFound { get; private set; }
    public string? FatalMessage { get; private set; }

    public sealed class ScanHit
    {
        public string FilePath { get; init; } = "";
        public bool IsDirectory { get; init; }
    }

    public sealed class ConfigFile
    {
        public readonly record struct Position(int Section, int Cursor);

        readonly List<(string Name, List<(string Key, string Value)> Rows)> _sections = [];
        int _section = -1;
        int _cursor;

        public void Add(string section, string key, string value)
        {
            var index = Find(section);
            if (index < 0)
            {
                _sections.Add((section, []));
                index = _sections.Count - 1;
            }

            _sections[index].Rows.Add((key, value));
        }

        public bool SetSection(string? name)
        {
            if (name == null)
                throw new InvalidOperationException("section");
            var index = Find(name);
            if (index < 0)
                return false;
            _section = index;
            _cursor = 0;
            return true;
        }

        public bool Next(out string key, out string value)
        {
            if (_section < 0 || _cursor >= _sections[_section].Rows.Count)
            {
                key = "";
                value = "";
                return false;
            }

            (key, value) = _sections[_section].Rows[_cursor];
            _cursor++;
            return true;
        }

        public Position Capture() => new(_section, _cursor);

        public void Restore(Position position)
        {
            _section = position.Section;
            _cursor = position.Cursor;
        }

        int Find(string name)
        {
            for (var i = 0; i < _sections.Count; i++)
            {
                if (string.Equals(_sections[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }
    }

    public void AddWildFile(IList<string> wads, string? value, string? extension, ConfigFile? config, bool optional)
    {
        if (string.IsNullOrEmpty(value))
            return;
        var foundPath = BaseFileSearch(value, extension, false, config);
        if (foundPath != null)
        {
            CallAdd(wads, foundPath, config, optional);
            return;
        }

        var path = ExtractFilePath(value);
        var name = VmFindFileWildcard.ApplyWildcardExtensionFilter(ExtractFileBase(value), extension);
        if (path.Length == 0)
            path = ".";
        var found = false;
        var list = CallScan(path, name);
        if (list != null)
        {
            foreach (var entry in list)
            {
                CallAdd(wads, entry.FilePath, config, optional);
                found = true;
            }
        }

        if (!found)
            FileNotFound(optional ? RequireOptFile : RequireFile, optional ? "optional wad" : "wad", value);
    }

    public void AddConfigFiles(IList<string> wads, string? section, string? extension, ConfigFile? config, bool optional)
    {
        if (config == null)
            return;
        if (section == null)
            throw new InvalidOperationException("section");
        if (!config.SetSection(section))
            return;
        while (config.Next(out var key, out var value))
        {
            if (!key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                continue;
            var position = config.Capture();
            AddWildFile(wads, Expand(value), extension, config, optional);
            config.Restore(position);
        }
    }

    public void AddDirectory(IList<string> wads, string? dir, string? filespec, ConfigFile? config, bool optional)
    {
        if (dir == null)
            throw new InvalidOperationException("dir");
        var list = CallScan(dir, string.IsNullOrEmpty(filespec) ? "*.wad" : filespec);
        if (list == null)
            return;
        foreach (var entry in list)
        {
            if (!entry.IsDirectory)
                CallAdd(wads, entry.FilePath, config, optional);
        }
    }

    public string? BaseFileSearch(string? file, string? ext, bool lookFirstInProgDir, ConfigFile? config)
    {
        if (string.IsNullOrEmpty(file))
            return null;
        if (lookFirstInProgDir)
        {
            if (ProgDir.Length == 0)
                throw new InvalidOperationException("progdir");
            var candidate = ProgDir + (ProgDir[^1] == '/' ? "" : "/") + file;
            if (Exists(candidate))
                return Remember(candidate);
        }

        if (Exists(file))
            return Remember(file);
        if (config != null && config.SetSection("FileSearch.Directories"))
        {
            while (config.Next(out var key, out var value))
            {
                if (key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                {
                    var joined = Joined(Nice(value), file);
                    if (joined != null && Exists(joined))
                        return Remember(joined);
                }
                else if (key.Equals("RecursivePath", StringComparison.OrdinalIgnoreCase))
                {
                    var dir = Nice(value);
                    if (dir.Length == 0)
                        continue;
                    if (dir[^1] == '/')
                        dir = dir[..^1];
                    var path = Recurse(dir, file);
                    if (path.Length > 0)
                        return Remember(path);
                }
                else if (key.Equals("PathList", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var dirname in SplitSkipEmpty(Expand(value), PathSeparator))
                    {
                        var joined = Joined(Nice(dirname), file);
                        if (joined != null && Exists(joined))
                            return Remember(joined);
                    }
                }
            }
        }

        if (ext == null)
            return null;
        var tmp = file;
        DefaultExtension(ref tmp, ext);
        return BaseFileSearch(tmp, null, lookFirstInProgDir, config);
    }

    public void FileNotFound(int test, string? type, string? file)
    {
        if (type == null)
            throw new InvalidOperationException("type");
        if (file == null)
            throw new InvalidOperationException("file");
        Prints.Add($"{type} not found: {file}\n");
        if ((ExitOnNotFound & test) == 0)
            return;
        FatalMessage =
            $"Cannot find {type} '{file}'\nTo ignore this error and continue, set cvar i_exit_on_not_found to {ExitOnNotFound - test}\n";
        Trace.Add("I_FatalError");
        if (Fatal == null)
            throw new InvalidOperationException("I_FatalError");
        Fatal(FatalMessage);
    }

    string? Remember(string path)
    {
        LastFound = path;
        return path;
    }
    bool Exists(string path)
    {
        if (DirExists == null)
            throw new InvalidOperationException("DirEntryExists");
        Trace.Add("DirEntryExists");
        return DirExists(path);
    }
    string Nice(string? value)
    {
        if (value == null || NicePath == null)
            throw new InvalidOperationException("NicePath");
        Trace.Add("NicePath");
        return NicePath(value) ?? "";
    }
    string Expand(string? value)
    {
        if (value == null || ExpandEnv == null)
            throw new InvalidOperationException("ExpandEnvVars");
        Trace.Add("ExpandEnvVars");
        return ExpandEnv(value) ?? "";
    }
    string Recurse(string dir, string file)
    {
        if (RecursiveExists == null)
            throw new InvalidOperationException("RecursiveFileExists");
        Trace.Add("RecursiveFileExists");
        return RecursiveExists(dir, file) ?? "";
    }
    IReadOnlyList<ScanHit>? CallScan(string path, string name)
    {
        if (Scan == null)
            throw new InvalidOperationException("ScanDirectory");
        Trace.Add("ScanDirectory");
        return Scan(path, name, true);
    }
    void CallAdd(IList<string> wads, string file, ConfigFile? config, bool optional)
    {
        if (AddFile == null)
            throw new InvalidOperationException("D_AddFile");
        Trace.Add("D_AddFile");
        AddFile(wads, file, true, -1, optional, config);
    }
    static string? Joined(string dir, string file)
    {
        if (dir.Length == 0)
            return null;
        return dir + (dir[^1] == '/' ? "" : "/") + file;
    }
    static void DefaultExtension(ref string path, string extension)
    {
        var index = path.Length - 1;
        while (index > 0 && path[index] != '/')
        {
            if (path[index] == '.')
                return;
            index--;
        }

        path += extension;
    }

    static string ExtractFilePath(string path)
    {
        var src = path.Length == 0 ? 0 : path.Length - 1;
        while (src > 0 && path[src - 1] != '/')
            src--;
        return path[..src];
    }

    static string ExtractFileBase(string path)
    {
        if (path.Length == 0)
            return "";
        var src = path.Length - 1;
        while (src > 0 && path[src - 1] != '/')
            src--;
        return path[src..];
    }

    static List<string> SplitSkipEmpty(string text, char separator)
    {
        var tokens = new List<string>();
        if (text.Length == 0)
            return tokens;
        var start = 0;
        for (var i = 0; i <= text.Length; i++)
        {
            if (i != text.Length && text[i] != separator)
                continue;
            if (i != start)
                tokens.Add(text[start..i]);
            start = i + 1;
        }

        return tokens;
    }
}
