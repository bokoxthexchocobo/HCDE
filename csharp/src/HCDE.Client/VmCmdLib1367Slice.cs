using System.Text;

namespace HCDE.Client;

/// <summary>
/// <c>strbin1</c>, <c>ExpandEnvVars</c>, and <c>NicePath</c> from <c>cmdlib.cpp</c> lines 738-970.
/// <c>strbin</c> through line 737 and <c>IsAbsPath</c> at line 971 stay on chunk 122.
/// </summary>
public sealed class VmCmdLib1367Slice
{
    public const string EnvNameChars =
        "01234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

    public bool Windows { get; set; }
    public string ProgDir { get; set; } = "";
    public Func<string, string?>? GetEnv { get; set; }
    public Func<int>? GetUid { get; set; }
    public Func<int, Passwd?>? GetPwUid { get; set; }
    public Func<string, Passwd?>? GetPwNam { get; set; }
    public string? Who { get; private set; }
    public List<string> Trace { get; } = [];

    public sealed class Passwd
    {
        public string? Dir { get; init; }
    }

    public string StrBin1(string? start)
    {
        if (start == null)
            throw new InvalidOperationException("start");
        start = CutAtNul(start);
        var built = new StringBuilder();
        int p = 0;
        int n = start.Length;
        char At(int index) => index >= 0 && index < n ? start[index] : '\0';
        while (true)
        {
            char c = At(p);
            p++;
            if (c == '\0')
                break;
            if (c != '\\')
            {
                built.Append(c);
                continue;
            }

            if (At(p) == '\0')
                break;
            switch (At(p))
            {
                case 'a':
                    built.Append('\a');
                    break;
                case 'b':
                    built.Append('\b');
                    break;
                case 'c':
                    built.Append('\u001c');
                    break;
                case 'f':
                    built.Append('\f');
                    break;
                case 'n':
                    built.Append('\n');
                    break;
                case 't':
                    built.Append('\t');
                    break;
                case 'r':
                    built.Append('\r');
                    break;
                case 'v':
                    built.Append('\v');
                    break;
                case '?':
                    built.Append('?');
                    break;
                case '\n':
                    break;
                case 'x':
                case 'X':
                    built.Append(HexOrOctal(ref p, At, hex: true));
                    break;
                case >= '0' and <= '7':
                    built.Append(HexOrOctal(ref p, At, hex: false));
                    break;
                default:
                    built.Append(At(p));
                    break;
            }

            p++;
        }

        return built.ToString();
    }

    public string ExpandEnv(string? search)
    {
        if (search == null)
            return "";
        search = CutAtNul(search);
        int dollar = search.IndexOf('$');
        if (dollar < 0)
            return search;
        var built = new StringBuilder();
        built.Append(search, 0, dollar);
        int next = 0;
        while (dollar >= 0 && next < search.Length)
        {
            int length = 0;
            while (dollar + 1 + length < search.Length && IsEnvName(search[dollar + 1 + length]))
                length++;
            if (length != 0)
            {
                string name = search.Substring(dollar + 1, length);
                if (name.Equals("progdir", StringComparison.OrdinalIgnoreCase))
                    built.Append(ProgDir);
                else
                    AppendEnv(built, name);
            }
            else
            {
                built.Append('$');
            }

            next = dollar + length + 1;
            dollar = next <= search.Length ? search.IndexOf('$', next) : -1;
            if (dollar >= 0)
                built.Append(search, next, dollar - next);
        }

        if (next < search.Length)
            built.Append(search, next, search.Length - next);
        return built.ToString();
    }

    public string NicePath(string? path)
    {
        if (Windows)
        {
            if (path == null)
                throw new InvalidOperationException("path");
            path = CutAtNul(path);
            if (path.Length == 0)
                return ".";
            return ExpandEnv(path);
        }

        if (path == null)
            return "";
        path = CutAtNul(path);
        if (path.Length == 0)
            return "";
        if (path[0] != '~')
            return ExpandEnv(path);

        string slash;
        Passwd? pw;
        if (path.Length == 1 || path[1] == '/')
        {
            if (GetUid == null)
                throw new InvalidOperationException("getuid");
            int uid = GetUid();
            Trace.Add("getuid");
            if (GetPwUid == null)
                throw new InvalidOperationException("getpwuid");
            pw = GetPwUid(uid);
            Trace.Add("getpwuid");
            slash = path.Length == 1 ? "" : path[1..];
        }
        else
        {
            int slashAt = path.IndexOf('/');
            if (slashAt < 0)
                slashAt = path.Length;
            string who = path[..slashAt];
            Who = who;
            if (GetPwNam == null)
                throw new InvalidOperationException("getpwnam");
            pw = GetPwNam(who);
            Trace.Add("getpwnam");
            slash = path[slashAt..];
        }

        if (pw == null)
            return ExpandEnv(path);
        string where = pw.Dir ?? "";
        if (slash.Length != 0)
            where += ExpandEnv(slash);
        return where;
    }

    static string CutAtNul(string text)
    {
        int nul = text.IndexOf('\0');
        return nul < 0 ? text : text[..nul];
    }

    static bool IsEnvName(char c) => EnvNameChars.IndexOf(c) >= 0;

    void AppendEnv(StringBuilder built, string name)
    {
        if (GetEnv == null)
            throw new InvalidOperationException("getenv");
        Trace.Add("getenv");
        string? value = GetEnv(name);
        if (value == null)
            return;
        value = CutAtNul(value);
        if (value.Length != 0)
            built.Append(value);
    }

    static char HexOrOctal(ref int p, Func<int, char> at, bool hex)
    {
        sbyte acc = hex ? (sbyte)0 : (sbyte)(at(p) - '0');
        for (int i = 0; i < 2; i++)
        {
            p++;
            char d = at(p);
            int digit;
            if (hex)
            {
                if (d >= '0' && d <= '9')
                    digit = d - '0';
                else if (d >= 'a' && d <= 'f')
                    digit = 10 + d - 'a';
                else if (d >= 'A' && d <= 'F')
                    digit = 10 + d - 'A';
                else
                {
                    p--;
                    break;
                }
            }
            else if (d >= '0' && d <= '7')
            {
                digit = d - '0';
            }
            else
            {
                p--;
                break;
            }

            acc = unchecked((sbyte)(((int)acc << (hex ? 4 : 3)) + digit));
        }

        return (char)(byte)acc;
    }
}
