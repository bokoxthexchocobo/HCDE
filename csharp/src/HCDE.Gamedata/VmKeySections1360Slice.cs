namespace HCDE.Gamedata;

/// <summary>
/// KEYCONF commands in <c>src/gamedata/keysections.cpp</c> lines 78-323.
/// <c>LoadKeys</c> (lines 39-55) stays a hook. The ini name is truncated only for the title search.
/// </summary>
public sealed class VmKeySections1360Slice
{
    public const int PcfNoMenu = 1;

    public sealed class KeySection
    {
        public string Title { get; set; } = "";
        public string Ini { get; set; } = "";
        public List<KeyAction> Actions { get; } = [];
    }

    public sealed class KeyAction
    {
        public string Title { get; set; } = "";
        public string Command { get; set; } = "";
    }

    public struct PlayerClass
    {
        public object? Type { get; set; }
        public int Flags { get; set; }
    }

    public bool ParsingKeyConf { get; set; }
    public bool SetSlotStrict { get; set; }
    public int CurrentKeySection { get; set; } = -1;
    public List<KeySection> KeySections { get; } = [];
    public List<string> KeyConfWeapons { get; } = [];
    public List<PlayerClass> PlayerClasses { get; } = [];
    public List<string> Messages { get; } = [];
    public List<string> Submitted { get; } = [];
    public Action<string, bool>? LoadKeys { get; set; }
    public Func<string, object?>? FindActor { get; set; }
    public Func<object?, string, bool>? ValidatePlayerClass { get; set; }

    public void AddKeySection(params string[] argv)
    {
        if (!ParsingKeyConf)
            return;
        if (argv.Length != 3)
        {
            Messages.Add("Usage: addkeysection <menu section name> <ini name>\n");
            return;
        }

        var name = argv[2];
        if (name.Length > 32)
        {
            Messages.Add(
                $"WARNING: {name} is too long as an ini name! The ini name should be 32 bytes or less.\n");
            name = name[..32];
        }

        for (var i = 0; i < KeySections.Count; i++)
        {
            if (string.Equals(KeySections[i].Title, name, StringComparison.OrdinalIgnoreCase))
            {
                CurrentKeySection = i;
                return;
            }
        }

        CurrentKeySection = KeySections.Count;
        KeySections.Add(new KeySection { Title = argv[1], Ini = argv[2] });
        if (LoadKeys is null)
            throw new InvalidOperationException("LoadKeys");
        LoadKeys(argv[2], false);
        LoadKeys(argv[2], true);
    }

    public void AddMenuKey(params string[] argv)
    {
        if (!ParsingKeyConf)
            return;
        if (argv.Length != 3)
        {
            Messages.Add("Usage: addmenukey <description> <command>\n");
            return;
        }

        if (CurrentKeySection == -1 || CurrentKeySection >= KeySections.Count)
        {
            Messages.Add("You must use addkeysection first.\n");
            return;
        }

        if ((uint)CurrentKeySection >= (uint)KeySections.Count)
            throw new IndexOutOfRangeException("KeySections");

        var sect = KeySections[CurrentKeySection];
        sect.Actions.Add(new KeyAction { Title = argv[1], Command = argv[2] });
    }

    public void LoadWadSettings(IReadOnlyList<string> lumps)
    {
        ParsingKeyConf = true;
        KeySections.Clear();
        KeyConfWeapons.Clear();
        foreach (var lump in lumps)
        {
            var inBlock = false;
            var conf = 0;
            while (conf < lump.Length)
            {
                var linePos = 0;
                while (conf + linePos < lump.Length
                    && lump[conf + linePos] != '\n'
                    && lump[conf + linePos] != '\r')
                {
                    linePos++;
                }

                var line = lump.Substring(conf, linePos);
                conf += linePos;
                while (conf < lump.Length && (lump[conf] == '\n' || lump[conf] == '\r'))
                    conf++;
                Submitted.Add(FilterLine(line, ref inBlock));
            }
        }

        ParsingKeyConf = false;
    }

    public void ClearIwadPlayerClasses(object? ti)
    {
        for (var i = 0; i < PlayerClasses.Count; i++)
        {
            if (ReferenceEquals(PlayerClasses[i].Type, ti))
            {
                for (var j = i; j < PlayerClasses.Count - 1; j++)
                    PlayerClasses[j] = PlayerClasses[j + 1];
                PlayerClasses.RemoveAt(PlayerClasses.Count - 1);
            }
        }
    }

    public void ClearPlayerClasses()
    {
        if (!ParsingKeyConf)
            return;
        if (SetSlotStrict)
        {
            PlayerClasses.Clear();
            return;
        }

        ClearIwadPlayerClasses(RequireFind("DoomPlayer"));
        ClearIwadPlayerClasses(RequireFind("HereticPlayer"));
        ClearIwadPlayerClasses(RequireFind("StrifePlayer"));
        ClearIwadPlayerClasses(RequireFind("FighterPlayer"));
        ClearIwadPlayerClasses(RequireFind("ClericPlayer"));
        ClearIwadPlayerClasses(RequireFind("MagePlayer"));
        ClearIwadPlayerClasses(RequireFind("ChexPlayer"));
    }

    public void AddPlayerClass(params string[] argv)
    {
        if (!(ParsingKeyConf && argv.Length > 1))
            return;

        var ti = RequireFind(argv[1]);
        if (ValidatePlayerClass is null)
            throw new InvalidOperationException("ValidatePlayerClass");
        if (!ValidatePlayerClass(ti, argv[1]))
            return;

        for (var i = 0; i < PlayerClasses.Count; i++)
        {
            if (ReferenceEquals(PlayerClasses[i].Type, ti))
                return;
        }

        var flags = 0;
        for (var arg = 2; arg < argv.Length; arg++)
        {
            if (string.Equals(argv[arg], "nomenu", StringComparison.OrdinalIgnoreCase))
                flags |= PcfNoMenu;
            else
                Messages.Add($"Unknown flag '{argv[arg]}' for player class '{argv[1]}'\n");
        }

        PlayerClasses.Add(new PlayerClass { Type = ti, Flags = flags });
    }

    object? RequireFind(string name)
    {
        if (FindActor is null)
            throw new InvalidOperationException("FindActor");
        return FindActor(name);
    }

    static string FilterLine(string line, ref bool inBlock)
    {
        var filtered = new List<char>(line.Length);
        var prev = '\0';
        var inQuote = false;
        for (var pos = 0; pos < line.Length; pos++)
        {
            var ch = line[pos];
            if (inBlock)
            {
                if (prev == '*' && ch == '/')
                {
                    inBlock = false;
                    prev = '\0';
                }
                else
                {
                    prev = ch;
                }

                continue;
            }

            if (ch == '"')
            {
                inQuote = !inQuote;
            }
            else if (!inQuote && prev == '/' && ch == '/')
            {
                if (filtered.Count > 0)
                    filtered.RemoveAt(filtered.Count - 1);
                break;
            }
            else if (!inQuote && prev == '/' && ch == '*')
            {
                if (filtered.Count > 0)
                    filtered.RemoveAt(filtered.Count - 1);
                inBlock = true;
                prev = '\0';
                continue;
            }

            filtered.Add(ch);
            prev = ch;
        }

        return new string(filtered.ToArray());
    }
}
