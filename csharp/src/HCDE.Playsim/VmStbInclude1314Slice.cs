using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Include-line splice, the line directives, and the missing-file error from
/// <c>stb_include.cpp</c> (1-209). Chunk 1314.
/// </summary>
public sealed class VmStbInclude1314
{
    readonly Func<string, string?> _load;

    public VmStbInclude1314(Func<string, string?> load)
    {
        _load = load;
    }

    public string IncludeFile(string filename, List<string> filenames, ref string error)
    {
        string? text = _load(filename);
        if (text == null)
        {
            error += "Error: couldn't load '" + filename + "'";
            return "";
        }

        return IncludeString(text, filename, filenames, ref error);
    }

    public string IncludeString(string str, string filename, List<string> filenames, ref string error)
    {
        error = "";
        List<IncludeHit> includes = FindIncludes(str);
        filenames.Add(filename);
        int curIndex = filenames.Count;
        var text = new StringBuilder();
        text.Append("\n#line 1 ");
        text.Append(curIndex);
        text.Append(" // ");
        text.Append(filename);
        text.Append('\n');
        int last = 0;
        foreach (IncludeHit inc in includes)
        {
            text.Append(str, last, inc.Offset - last);
            string nested = IncludeFile(inc.Name, filenames, ref error);
            if (error.Length != 0)
                return "";
            text.Append(nested);
            text.Append("\n#line ");
            text.Append(inc.NextLine);
            text.Append(' ');
            text.Append(curIndex);
            text.Append(" // ");
            text.Append(filename);
            text.Append('\n');
            last = inc.End;
        }

        text.Append(str, last, str.Length - last);
        return text.ToString();
    }

    static List<IncludeHit> FindIncludes(string text)
    {
        var list = new List<IncludeHit>();
        long line = 1;
        int s = 0;
        int n = text.Length;
        while (s < n)
        {
            int start = s;
            s = SkipSpaces(text, s);
            if (s < n && text[s] == '#')
            {
                s++;
                s = SkipSpaces(text, s);
                if (IsInclude(text, s))
                {
                    s += 7;
                    s = SkipSpaces(text, s);
                    if (s < n && text[s] == '"')
                    {
                        int nameAt = s + 1;
                        int quote = FindEndQuote(text, nameAt);
                        if (quote < n && text[quote] == '"')
                        {
                            string name = text.Substring(nameAt, quote - nameAt);
                            int end = FindNewline(text, quote);
                            list.Add(new IncludeHit(start, end, name, line + 1));
                            s = end;
                        }
                    }
                }
            }

            s = SkipNewline(text, s);
            line++;
        }

        return list;
    }

    static bool IsInclude(string text, int s)
    {
        if (s + 8 > text.Length)
            return false;
        return text[s] == 'i'
            && text[s + 1] == 'n'
            && text[s + 2] == 'c'
            && text[s + 3] == 'l'
            && text[s + 4] == 'u'
            && text[s + 5] == 'd'
            && text[s + 6] == 'e'
            && IsSpace(text[s + 7]);
    }

    static bool IsSpace(char ch)
    {
        return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n';
    }

    static int SkipSpaces(string text, int s)
    {
        int n = text.Length;
        while (s < n && (text[s] == ' ' || text[s] == '\t'))
            s++;
        return s;
    }

    static int FindEndQuote(string text, int s)
    {
        int n = text.Length;
        while (s < n && text[s] != '"' && text[s] != '\n' && text[s] != '\r')
            s++;
        return s;
    }

    static int FindNewline(string text, int s)
    {
        int n = text.Length;
        while (s < n && text[s] != '\r' && text[s] != '\n')
            s++;
        return s;
    }

    static int SkipNewline(string text, int s)
    {
        int n = text.Length;
        while (s < n && text[s] != '\r' && text[s] != '\n')
            s++;
        if (s + 1 < n && text[s] == '\r' && text[s + 1] == '\n')
            return s + 2;
        if (s < n && (text[s] == '\r' || text[s] == '\n'))
            return s + 1;
        return s;
    }

    readonly record struct IncludeHit(int Offset, int End, string Name, long NextLine);
}
