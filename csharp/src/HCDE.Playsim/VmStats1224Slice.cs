namespace HCDE.Playsim;

/// <summary>
/// Stat registration, prefix lookup, and the bottom-up stat print from
/// <c>stats.cpp</c> (<b>1–150</b>). Chunk <b>1224</b>.
/// Exact lookup folds ASCII case. A prefix match does not.
/// Two prefix hits cancel. The destructor walks every other link,
/// so a node in the middle of a three-node list is not unlinked.
/// </summary>
public sealed class VmStats1224
{
    public readonly VmStatLink1224 Head = new();
    public int FontHeight = 7;
    public int Scale = 2;
    public int DrawerHeight = 200;
    public int TwodWidth = 640;
    public int TwodHeight = 480;
    public readonly System.Text.StringBuilder Log = new();

    public VmStats1224()
    {
        Scale = 2;
    }

    public VmStat1224 Add(string name, Func<string> text)
    {
        var stat = new VmStat1224
        {
            Name = name,
            Text = text,
        };
        stat.Next.Node = Head.Node;
        Head.Node = stat;
        return stat;
    }

    public VmStat1224? Find(string name)
    {
        VmStat1224? stat = Head.Node;
        VmStat1224? partial = null;
        bool badPartial = false;
        int len = name.Length;
        while (stat != null && Stricmp(name, stat.Name) != 0)
        {
            if (!badPartial && Strncmp(stat.Name, name, len) == 0)
            {
                if (partial != null)
                {
                    badPartial = true;
                    partial = null;
                }
                else
                {
                    partial = stat;
                }
            }

            stat = stat.Next.Node;
        }

        if (stat != null)
            return stat;
        if (badPartial)
            return null;
        return partial;
    }

    public void Toggle(string name)
    {
        VmStat1224? stat = Find(name);
        if (stat != null)
            stat.Active = !stat.Active;
        else
            Log.Append("Unknown stat: ").Append(name).Append('\n');
    }

    public void Enable(string name, bool on)
    {
        VmStat1224? stat = Find(name);
        if (stat != null)
            stat.Active = on;
        else
            Log.Append("Unknown stat: ").Append(name).Append('\n');
    }

    public void Command(string[] argv)
    {
        if (argv.Length != 2)
        {
            Log.Append("Usage: stat <statistics>\n");
            Dump();
        }
        else
        {
            Toggle(argv[1]);
        }
    }

    public void Dump()
    {
        Log.Append("Available stats:\n");
        for (VmStat1224? stat = Head.Node; stat != null; stat = stat.Next.Node)
        {
            Log.Append(' ');
            Log.Append(stat.Active ? '*' : ' ');
            Log.Append(stat.Name);
            Log.Append('\n');
        }
    }

    public void Print()
    {
        int fontheight = FontHeight + 1;
        int y = DrawerHeight / Scale;
        for (VmStat1224? stat = Head.Node; stat != null; stat = stat.Next.Node)
        {
            if (!stat.Active)
                continue;
            Log.Append("get ");
            Log.Append(stat.Name);
            Log.Append('\n');
            string stattext = stat.Text();
            if (stattext.Length > 0)
            {
                y -= fontheight;
                for (int i = 0; i < stattext.Length - 1; i++)
                {
                    if (stattext[i] == '\n')
                        y -= fontheight;
                }

                int x = (int)(5.0 / Scale * 1000);
                Log.Append("D 3 x=");
                Log.Append(x.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Log.Append(" y=");
                Log.Append(y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Log.Append(" vw=");
                Log.Append((TwodWidth / Scale).ToString(System.Globalization.CultureInfo.InvariantCulture));
                Log.Append(" vh=");
                Log.Append((TwodHeight / Scale).ToString(System.Globalization.CultureInfo.InvariantCulture));
                Log.Append(" k=1 t=");
                for (int i = 0; i < stattext.Length; i++)
                {
                    if (stattext[i] == '\n')
                        Log.Append("\\n");
                    else
                        Log.Append(stattext[i]);
                }

                Log.Append('\n');
            }
        }
    }

    public void Destroy(VmStat1224 stat)
    {
        VmStatLink1224 prev = Head;
        while (prev.Node != null && !ReferenceEquals(prev.Node, stat))
        {
            VmStat1224 next = prev.Node.Next.Node!;
            prev = next.Next;
        }

        if (ReferenceEquals(prev.Node, stat))
            prev.Node = stat.Next.Node;
    }

    static int Fold(int c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }

    static int Stricmp(string a, string b)
    {
        int n = a.Length < b.Length ? a.Length : b.Length;
        for (int i = 0; i < n; i++)
        {
            int ca = Fold(a[i]);
            int cb = Fold(b[i]);
            if (ca != cb)
                return ca - cb;
        }

        return a.Length - b.Length;
    }

    static int Strncmp(string a, string b, int len)
    {
        for (int i = 0; i < len; i++)
        {
            int ca = i < a.Length ? a[i] : 0;
            int cb = i < b.Length ? b[i] : 0;
            if (ca != cb)
                return ca - cb;
            if (ca == 0)
                return 0;
        }

        return 0;
    }
}

public sealed class VmStatLink1224
{
    public VmStat1224? Node;
}

public sealed class VmStat1224
{
    public string Name = "";
    public bool Active;
    public Func<string> Text = EmptyText;
    public readonly VmStatLink1224 Next = new();

    static string EmptyText()
    {
        return "";
    }
}
