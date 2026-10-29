using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Console tab completion from <c>c_tabcomplete.cpp</c> (<b>1–312</b>). Chunk <b>1230</b>.
/// Names are sorted with a case-insensitive prefix compare. The first tab on a
/// multi-match prints the colored columns and may fill the common prefix.
/// Later tabs cycle. <c>None</c> can be added and cannot be removed.
/// </summary>
public sealed class VmTab1230
{
    public readonly List<string> Log = new();
    public readonly Dictionary<string, string> Predefined = new();
    public readonly Dictionary<string, string> Kind = new();
    public bool NoTabList;
    public bool TabbedLast;
    public bool TabbedList;
    public int ConCols = 80;
    public int Scale = 1;
    public string Line = "";

    readonly Dictionary<string, string> known = new();
    readonly List<Cmd> cmds = new();
    int tabPos;
    int tabStart;
    int tabSize;

    const string ColorBlue = "\u001cH";
    const string ColorGreen = "\u001cD";
    const string ColorRed = "\u001cG";
    const string ColorLightBlue = "\u001cN";

    sealed class Cmd
    {
        public int Count;
        public string Name = "";
    }

    public void Define(string spelling)
    {
        Predefined[FoldKey(spelling)] = spelling;
    }

    public void Add(string name)
    {
        if (Find(name, int.MaxValue, out int pos))
            cmds[pos].Count++;
        else
            cmds.Insert(pos, new Cmd { Count = 1, Name = Intern(name) });
        LogCmds("add");
    }

    public void Remove(string name)
    {
        if (cmds.Count == 0)
        {
            Log.Add("remove empty");
            return;
        }

        if (!TryIntern(name, false, out string canon, out bool isNone))
        {
            LogCmds("remove unknown");
            return;
        }

        if (isNone)
        {
            LogCmds("remove none");
            return;
        }

        for (int i = 0; i < cmds.Count; i++)
        {
            if (cmds[i].Name != canon)
                continue;
            cmds[i].Count--;
            if (cmds[i].Count == 0)
            {
                cmds.RemoveAt(i);
                LogCmds("remove drop " + Esc(canon));
            }
            else
            {
                LogCmds("remove " + Esc(canon) + " count=" + cmds[i].Count);
            }

            return;
        }

        LogCmds("remove miss " + Esc(canon));
    }

    public void Clear()
    {
        cmds.Clear();
        Log.Add("clear last=" + Bit(TabbedLast) + " listed=" + Bit(TabbedList));
    }

    public void Complete(bool goForward)
    {
        Log.Add(goForward ? "go forward" : "go back");
        string text = Line;
        if (!TabbedLast)
        {
            int i = 0;
            while (i < text.Length && text[i] == ' ')
                i++;
            if (i == text.Length)
            {
                Log.Add("blank");
                return;
            }

            tabStart = i;
            tabSize = text.Length - tabStart;
            if (!Find(text.Substring(tabStart), tabSize, out tabPos))
            {
                Log.Add("nomatch");
                return;
            }

            bool can = TabbedList || NoTabList || List();
            if (!TabbedList && !NoTabList)
                TabbedList = true;
            if (goForward)
                tabPos--;
            else
            {
                while (++tabPos < cmds.Count)
                {
                    if (Diff(cmds[tabPos].Name, text.Substring(tabStart)) < tabSize)
                        break;
                }
            }

            TabbedLast = true;
            if (!can)
            {
                LogState();
                return;
            }
        }

        if ((goForward && ++tabPos == cmds.Count) || (!goForward && --tabPos < 0))
        {
            TabbedLast = false;
            text = Cut(text, tabSize);
        }
        else
        {
            int diff = Diff(cmds[tabPos].Name, text.Substring(tabStart));
            if (diff < tabSize)
            {
                TabbedLast = false;
                text = Cut(text, tabSize - tabStart);
            }
            else
            {
                text = Cut(text, tabStart) + cmds[tabPos].Name + " ";
            }
        }

        Line = text;
        Log.Add("set [" + Esc(text) + "]");
        Log.Add("caret");
        LogState();
    }

    bool List()
    {
        string text = Line;
        int common = int.MaxValue;
        int matches = 0;
        int maxWidth = 0;
        string token = text.Substring(tabStart);
        for (int i = tabPos; i < cmds.Count; i++)
        {
            if (Diff(cmds[i].Name, token) < tabSize)
                break;
            if (i > tabPos)
            {
                int diff = Diff(cmds[i - 1].Name, cmds[i].Name);
                if (diff < common)
                    common = diff;
            }

            matches++;
            if (cmds[i].Name.Length > maxWidth)
                maxWidth = cmds[i].Name.Length;
        }

        if (matches <= 1)
            return true;

        int width = maxWidth + 3;
        Out(ColorBlue + "Completions for " + text + ":\n");
        ulong column = 0;
        int shown = matches;
        for (int i = tabPos; shown > 0; i++, shown--)
        {
            Out(ColorOf(cmds[i].Name) + Pad(cmds[i].Name, width));
            column += (ulong)width;
            ulong cols = (ulong)(ConCols / Scale);
            if (column > cols - (ulong)width)
            {
                column = 0;
                Out("\n");
            }
        }

        if (column != 0)
            Out("\n");
        if (tabSize != common)
        {
            tabSize = common;
            Line = Cut(text, tabStart) + cmds[tabPos].Name.Substring(0, common);
            Log.Add("set [" + Esc(Line) + "]");
        }

        return false;
    }

    bool Find(string name, int len, out int stop)
    {
        string canon = Intern(name);
        int cval = 1;
        int i;
        for (i = 0; i < cmds.Count; i++)
        {
            if (cmds[i].Name == canon)
            {
                stop = i;
                return true;
            }

            cval = Strnicmp(cmds[i].Name, name, len);
            if (cval >= 0)
                break;
        }

        stop = i;
        return cval == 0;
    }

    string Intern(string name)
    {
        TryIntern(name, true, out string canon, out _);
        return canon;
    }

    bool TryIntern(string name, bool create, out string canon, out bool isNone)
    {
        string key = FoldKey(name);
        isNone = key == "none";
        if (isNone)
        {
            canon = "None";
            return true;
        }

        if (Predefined.TryGetValue(key, out string? predefined))
        {
            canon = predefined;
            return true;
        }

        if (known.TryGetValue(key, out string? spelling))
        {
            canon = spelling;
            return true;
        }

        if (!create)
        {
            canon = "";
            return false;
        }

        known[key] = name;
        canon = name;
        return true;
    }

    string ColorOf(string canon)
    {
        if (!Kind.TryGetValue(canon, out string? kind))
            return "";
        if (kind == "cvar")
            return ColorGreen;
        if (kind == "alias")
            return ColorRed;
        if (kind == "command")
            return ColorLightBlue;
        return "";
    }

    void Out(string text)
    {
        Log.Add("out [" + Esc(text) + "]");
    }

    void LogState()
    {
        Log.Add(
            "state last=" + Bit(TabbedLast) + " listed=" + Bit(TabbedList)
            + " pos=" + tabPos + " start=" + tabStart + " size=" + tabSize
            + " line=[" + Esc(Line) + "]");
    }

    void LogCmds(string head)
    {
        var list = new StringBuilder();
        for (int i = 0; i < cmds.Count; i++)
        {
            if (i > 0)
                list.Append(',');
            list.Append(Esc(cmds[i].Name));
            list.Append(':');
            list.Append(cmds[i].Count);
        }

        Log.Add(head + " [" + list + "]");
    }

    static string Cut(string text, int newlen)
    {
        uint n = (uint)newlen;
        if (n == 0)
            return "";
        if (n < (uint)text.Length)
            return text.Substring(0, (int)n);
        return text;
    }

    static string Pad(string text, int width)
    {
        if (text.Length >= width)
            return text;
        return text + new string(' ', width - text.Length);
    }

    static int Diff(string left, string right)
    {
        int i = 0;
        while (FoldSigned(At(left, i)) == FoldSigned(At(right, i)))
        {
            if (At(left, i) == 0 || At(right, i) == 0)
                break;
            i++;
        }

        return i;
    }

    static int Strnicmp(string left, string right, int len)
    {
        for (int i = 0; i < len; i++)
        {
            int ca = At(left, i);
            int cb = At(right, i);
            int fa = FoldAscii(ca);
            int fb = FoldAscii(cb);
            if (fa != fb)
                return fa < fb ? -1 : 1;
            if (ca == 0 || cb == 0)
                return 0;
        }

        return 0;
    }

    static int At(string text, int index)
    {
        if (index < text.Length)
            return text[index];
        return 0;
    }

    static string FoldKey(string text)
    {
        var key = new StringBuilder(text.Length);
        foreach (char ch in text)
            key.Append((char)FoldAscii(ch));
        return key.ToString();
    }

    static int FoldAscii(int ch)
    {
        if (ch >= 'A' && ch <= 'Z')
            return ch + 32;
        return ch;
    }

    static int FoldSigned(int ch)
    {
        if (ch >= 'A' && ch <= 'Z')
            return ch + 32;
        if (ch == 0xFF)
            return -1;
        return ch;
    }

    static int Bit(bool value)
    {
        if (value)
            return 1;
        return 0;
    }

    public static string Esc(string text)
    {
        var escaped = new StringBuilder();
        foreach (char ch in text)
        {
            if (ch == '\\')
                escaped.Append("\\\\");
            else if (ch >= 32 && ch < 127)
                escaped.Append(ch);
            else
                escaped.Append("\\x").Append(((int)ch).ToString("x2"));
        }

        return escaped.ToString();
    }
}
