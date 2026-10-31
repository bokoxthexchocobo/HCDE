namespace HCDE.Playsim;

/// <summary>
/// Argument registration, help text, raw argv, and the single-name
/// <c>CheckParm</c> from <c>m_argv.cpp</c> (1-335). Chunk 1336.
/// <c>TestArgList</c> at line 336 stays chunk 123. Section order is the
/// <c>FName</c> index order from registration.
/// </summary>
public sealed class VmArgv1336
{
    public const string Off = "\x1b[0m";
    public const string Bold = "\x1b[1m";
    public const string Dim = "\x1b[2m";

    public sealed class Arg
    {
        public string Name { get; init; } = "";
        public string Section { get; set; } = "";
        public int SectionIndex { get; set; }
        public string Summary { get; set; } = "";
        public string Usage { get; set; } = "";
        public string Details { get; set; } = "";
        public bool Advanced { get; set; }
    }

    public sealed class Catalog
    {
        readonly Dictionary<string, Arg> available = new(StringComparer.Ordinal);
        readonly Dictionary<string, int> names = new(StringComparer.Ordinal);
        int nextName;

        public int? TerminalColumns { get; set; }
        public List<string> Printed { get; } = new();
        public Func<Args, IReadOnlyList<Arg>, int, int>? CheckParmMany { get; set; }

        public IReadOnlyDictionary<string, Arg> Available => available;

        public Arg Register(string name, string section, string summary, string usage, string details, bool advanced = false)
        {
            if (available.ContainsKey(name))
                throw new InvalidOperationException("FArg already registered: " + name);

            var arg = new Arg
            {
                Name = name,
                Summary = summary,
                Usage = usage,
                Advanced = advanced,
            };
            if (section.Length == 0)
            {
                arg.Section = Intern("Unknown");
                arg.Advanced = true;
            }
            else
            {
                arg.Section = Intern(section);
            }

            arg.SectionIndex = names[arg.Section];
            arg.Details = details.Length > 0 ? details : summary;
            available.Add(name, arg);
            return arg;
        }

        public void PrintHelp(bool full)
        {
            var fullwidth = TerminalColumns ?? 80;
            var sections = new List<string>();
            var bySection = new Dictionary<string, Dictionary<string, Arg>>(StringComparer.Ordinal);
            var maxlen = 0;
            foreach (var pair in available)
            {
                if (pair.Value.Advanced && !full)
                    continue;
                if (!bySection.TryGetValue(pair.Value.Section, out var sectionMap))
                {
                    sectionMap = new Dictionary<string, Arg>(StringComparer.Ordinal);
                    bySection.Add(pair.Value.Section, sectionMap);
                    sections.Add(pair.Value.Section);
                }

                sectionMap.Add(pair.Key, pair.Value);
                if (maxlen < pair.Key.Length)
                    maxlen = pair.Key.Length;
            }

            sections.Sort((a, b) => names[a].CompareTo(names[b]));
            var indent = maxlen + 1;
            var breaklines = indent >= fullwidth / 2;
            if (breaklines)
                indent = 4;
            maxlen = fullwidth - indent - 1;

            foreach (var sectionName in sections)
            {
                var section = bySection[sectionName];
                var lines = new List<string>(section.Count);
                foreach (var pair in section)
                    lines.Add(pair.Value.Name);
                lines.Sort(StringComparer.Ordinal);

                var head = sectionName.Length == 0 ? "" : AsciiUpper(sectionName[..1]);
                var tail = sectionName.Length <= 1 ? "" : AsciiLower(sectionName[1..]);
                Printed.Add("\n" + Bold + head + tail + " options:" + Off + "\n");

                if (breaklines)
                {
                    foreach (var line in lines)
                    {
                        var arg = section[line];
                        var usage = !full || arg.Usage.Length == 0
                            ? ""
                            : " " + Dim + arg.Usage + Off;
                        var blurb = full ? arg.Details : arg.Summary;
                        Printed.Add("\n" + Bold + arg.Name + Off + usage + "\n" + blurb + "\n");
                    }
                }
                else
                {
                    if (!full)
                        Printed.Add("\n");
                    foreach (var line in lines)
                    {
                        var arg = section[line];
                        var desc = !full
                            ? arg.Summary
                            : arg.Usage.Length == 0
                                ? arg.Details
                                : Dim + arg.Usage + Off + "\n" + arg.Details;
                        var left = AsciiLower(arg.Name);
                        if (full)
                            Printed.Add("\n");
                        var nextSpace = 0;
                        do
                        {
                            string right;
                            if (IndexOf(desc, "\n", nextSpace + 1) != -1 || LongerThan(desc, maxlen))
                            {
                                int space;
                                int nextBreak;
                                do
                                {
                                    space = nextSpace;
                                    nextBreak = IndexOf(desc, "\n", nextSpace + 1);
                                    nextSpace = IndexOf(desc, " ", nextSpace + 1);
                                    if (nextBreak > -1 && nextBreak < nextSpace)
                                    {
                                        space = nextBreak;
                                        break;
                                    }
                                }
                                while (nextSpace > -1 && nextSpace < maxlen);

                                right = Left(desc, space);
                                desc = Mid(desc, space + 1);
                                nextSpace = 0;
                            }
                            else
                            {
                                right = desc;
                                desc = "";
                            }

                            Printed.Add(Bold + Pad(left, indent) + Off + " " + right + "\n");
                            left = "";
                        }
                        while (desc.Length > 0);
                    }
                }
            }
        }

        string Intern(string text)
        {
            if (!names.ContainsKey(text))
                names.Add(text, nextName++);
            return text;
        }
    }

    public sealed class Args
    {
        public List<string> Argv { get; private set; } = new();
        public Catalog? Catalog { get; init; }

        public Args()
        {
            Argv = new List<string>();
        }

        public Args(Args other)
        {
            Argv = new List<string>(other.Argv);
            Catalog = other.Catalog;
        }

        public Args(int argc, string?[] argv)
        {
            SetRawArgs(argc, argv);
        }

        public Args(Catalog catalog, int argc, string?[]? argv)
        {
            Catalog = catalog;
            if (argv != null && argc > 0)
            {
                Argv.EnsureCapacity(Argv.Count + argc);
                for (var i = 0; i < argc; i++)
                    Argv.Add(argv[i] ?? "");
            }
        }

        public Args Assign(Args other)
        {
            Argv = new List<string>(other.Argv);
            return this;
        }

        public void SetRawArgs(int argc, string?[] argv)
        {
            var next = new List<string>(argc);
            for (var i = 0; i < argc; i++)
                next.Add(argv[i] ?? "");
            Argv = next;
        }

        public void FlushArgs()
        {
            Argv.Clear();
        }

        public int CheckParm(Arg check, int start = 1)
        {
            if (Catalog?.CheckParmMany == null)
                throw new InvalidOperationException("CheckParm");
            return Catalog.CheckParmMany(this, new[] { check }, start);
        }
    }

    static bool LongerThan(string desc, int maxlen)
    {
        var limit = unchecked((uint)maxlen);
        return (uint)desc.Length > limit;
    }

    static int IndexOf(string text, string needle, int start)
    {
        if (start > 0 && text.Length <= start)
            return -1;
        if (start < 0)
            start = 0;
        return text.IndexOf(needle, start, StringComparison.Ordinal);
    }

    static string Left(string text, int count)
    {
        if (count < 0 || count > text.Length)
            return text;
        return text[..count];
    }

    static string Mid(string text, int pos)
    {
        if (pos < 0 || pos >= text.Length)
            return "";
        return text[pos..];
    }

    static string Pad(string text, int width)
    {
        if (text.Length >= width)
            return text;
        return new string(' ', width - text.Length) + text;
    }

    static string AsciiLower(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= 'A' and <= 'Z')
                chars[i] = (char)(chars[i] + 32);
        }

        return new string(chars);
    }

    static string AsciiUpper(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= 'a' and <= 'z')
                chars[i] = (char)(chars[i] - 32);
        }

        return new string(chars);
    }
}
