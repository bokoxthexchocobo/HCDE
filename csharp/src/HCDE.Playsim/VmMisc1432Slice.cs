namespace HCDE.Playsim;

/// <summary>
/// M_FindResponseFile through M_LoadDefaults from m_misc.cpp 156-406.
/// The comment stripper above this band is already credited. The PCX header starts at line 414.
/// </summary>
public static class VmMisc1432Slice
{
    public const int BandStart = 156;
    public const int BandEnd = 406;
    public const int ResponseLimit = 100;
    public const int StripWhitespace = 1;
    public const int SkipEmpty = 2;

    public sealed class Config
    {
        public string Path = "";
        public string Game = "";
        public bool WriteOk = true;
        public List<string> Ops = new();
    }

    public sealed class State
    {
        public List<string> Args = new();
        public Dictionary<string, string> Files = new(StringComparer.Ordinal);
        public List<string> Lines = new();
        public Config? Config;
        public List<string> SavedPaths = new();
        public List<bool> IniRetries = new();
        public int RetryAt;
        public List<string> FailedPaths = new();
        public string? Opened;
        public string Errno = "Permission denied";
    }

    public static void FindResponse(State s)
    {
        if (s.Args == null) throw new NullReferenceException();
        int added = 0;
        int i = 1;
        while (i < s.Args.Count)
        {
            string arg = s.Args[i] ?? throw new NullReferenceException();
            if (arg.Length == 0 || arg[0] != '@') { i++; continue; }
            string name = arg.Substring(1);
            int argc = 0;
            string text = "";
            if (added < ResponseLimit)
            {
                if (!s.Files.TryGetValue(name, out string? raw))
                    s.Lines.Add("No such response file (" + name + ")!\n");
                else
                {
                    s.Lines.Add("Found response file " + name + "!\n");
                    text = Uncomment(raw, StripWhitespace | SkipEmpty);
                    Parse(text, null, out argc);
                }
            }
            else s.Lines.Add("Ignored response file " + name + ".\n");
            if (argc != 0)
            {
                var parsed = new List<string>();
                Parse(text, parsed, out _);
                var next = new List<string>();
                for (int index = 0; index < i; index++) next.Add(s.Args[index]);
                next.AddRange(parsed);
                for (int index = i + 1; index < s.Args.Count; index++) next.Add(s.Args[index]);
                s.Args = next;
                if (++added == ResponseLimit) s.Lines.Add("Response file limit of " + ResponseLimit + " hit.\n");
            }
            else s.Args.RemoveAt(i);
        }
        if (added > 0)
        {
            s.Lines.Add("Added " + added + " response file" + (added > 1 ? "s" : "") + ", now have " + s.Args.Count + " command-line args:\n");
            for (int k = 1; k < s.Args.Count; k++) s.Lines.Add(s.Args[k] + "\n");
        }
    }

    public static int Parse(string? args, List<string>? argv, out int count)
    {
        if (args == null) throw new NullReferenceException();
        count = 0;
        int size = 0;
        int i = 0;
        while (true)
        {
            while (At(args, i) <= ' ' && At(args, i) != 0) i++;
            if (At(args, i) == 0) break;
            if (At(args, i) == '"')
            {
                var token = new System.Text.StringBuilder();
                i++;
                char stuff;
                do
                {
                    stuff = At(args, i);
                    i++;
                    if (stuff == '\\' && At(args, i) == '"') { stuff = '"'; i++; }
                    else if (stuff == '"') stuff = '\0';
                    else if (stuff == 0) i--;
                    if (argv != null && stuff != 0) token.Append(stuff);
                    size++;
                }
                while (stuff != 0);
                argv?.Add(token.ToString());
                count++;
            }
            else
            {
                int start = i++;
                while (At(args, i) != 0 && At(args, i) > ' ' && At(args, i) != '"') i++;
                int end = i;
                argv?.Add(args.Substring(start, end - start));
                size += end - start + 1;
                count++;
            }
        }
        return size;
    }

    public static bool Save(State s, string? filename)
    {
        if (s.Config == null) return true;
        if (filename != null)
        {
            s.SavedPaths.Add(s.Config.Path);
            s.Config.Path = filename;
        }
        s.Config.Ops.Add("global");
        if (s.Config.Game.Length != 0) s.Config.Ops.Add("game");
        bool success = s.Config.WriteOk;
        s.Config.Ops.Add(success ? "write" : "fail");
        if (filename != null) s.Config.Path = filename;
        return success;
    }

    public static void SaveFinal(State s)
    {
        if (s.Config == null) return;
        while (!Save(s, null) && IniFailed(s)) { }
        s.Config = null;
    }

    public static void WriteIni(State s, IReadOnlyList<string> argv)
    {
        string? filename = argv.Count == 1 ? null : argv[1];
        if (!Save(s, filename)) s.Lines.Add("Writing config failed: " + s.Errno + "\n");
        else s.Lines.Add("Config saved.\n");
    }

    public static void OpenConfig(State s)
    {
        Save(s, null);
        if (s.Config == null) throw new NullReferenceException();
        s.Opened = Folder(s.Config.Path);
    }

    public static void Load(State s)
    {
        s.Config = new Config();
        s.Config.Ops.Add("setup");
    }

    public static string Uncomment(string? from, int flags)
    {
        if (from == null) throw new NullReferenceException();
        bool strip = (flags & StripWhitespace) != 0;
        bool skip = (flags & SkipEmpty) != 0;
        var output = new System.Text.StringBuilder();
        foreach (string line in SplitLines(from))
        {
            int hash = line.IndexOf('#');
            if (hash == 0) continue;
            int end = line.Length;
            if (hash >= 0)
            {
                int start = 0;
                while (start < hash && Space(line[start])) start++;
                if (start == hash) end = hash;
            }
            string body = end == line.Length ? line : line.Substring(0, end);
            if (skip && body.Length == 0) continue;
            int a = 0;
            int b = body.Length;
            if (strip)
            {
                while (a < b && Space(body[a])) a++;
                while (b > a && Space(body[b - 1])) b--;
            }
            if (skip && b == a) continue;
            output.Append(body, a, b - a);
            output.Append(' ');
        }
        return output.ToString();
    }

    static bool IniFailed(State s)
    {
        if (s.Config == null) throw new NullReferenceException();
        s.FailedPaths.Add(s.Config.Path);
        if (s.RetryAt >= s.IniRetries.Count) return false;
        return s.IniRetries[s.RetryAt++];
    }

    static string Folder(string? path)
    {
        if (path == null) throw new NullReferenceException();
        if (path.Length == 0) throw new NullReferenceException();
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/') src--;
        return path.Substring(0, src);
    }

    static char At(string args, int index) => (uint)index < (uint)args.Length ? args[index] : '\0';

    static bool Space(char c) => c is ' ' or '\t' or '\n' or '\r' or '\v' or '\f';

    static List<string> SplitLines(string from)
    {
        var lines = new List<string>();
        int start = 0;
        for (int i = 0; i < from.Length; i++)
        {
            if (from[i] != '\n') continue;
            lines.Add(from.Substring(start, i - start));
            start = i + 1;
        }
        if (start < from.Length) lines.Add(from.Substring(start));
        return lines;
    }
}
