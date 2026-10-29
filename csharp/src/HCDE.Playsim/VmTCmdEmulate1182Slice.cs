namespace HCDE.Playsim;

/// <summary>
/// Legacy console commands from <c>t_cmd.cpp</c> (<b>1–186</b>). Chunk <b>1182</b>.
/// <c>OpenMem</c> leaves the scanner in classic mode, so a bare semicolon comments out the rest of the line.
/// A quoted semicolon is a real token and is what <c>Compare(";")</c> can match.
/// </summary>
public static class VmTCmdEmulate1182Slice
{
    public const int ChangeLevelResetInventory = 2;
    public const int ChangeLevelNoMonsters = 4;
    public const int ChangeLevelResetHealth = 32;
    public const int DfNoMonsters = 1 << 12;
    public const int DfNoJump = 1 << 16;

    public sealed class Player
    {
        public bool HasActor;
        public double ViewHeight;
        public double ActorViewHeight;
        public int UncrouchCount;
    }

    public sealed class Level
    {
        public int Dmflags;
        public float Gravity;
        public int FogDensity;
        public int OutsideFogDensity;
        public int SkyFog;
        public uint FadeTo;
        public int OutsideFog;
        public int SkillCount = 5;
        public List<Player> Players { get; } = new();
        public List<string> Commands { get; } = new();
        public List<string> Errors { get; } = new();
        public string? NextMap;
        public int NextPosition;
        public int NextFlags;
        public int NextSkill;
        public bool LevelChanged;
    }

    public static void Gimme(string what, Action<string> addCommand)
    {
        string? item = null;
        if (HasPrefix(what, "health")) item = "health";
        else if (HasPrefix(what, "ammo")) item = "ammo";
        else if (HasPrefix(what, "armor")) item = "greenarmor";
        else if (HasPrefix(what, "keys")) item = "keys";
        else if (HasPrefix(what, "weapons")) item = "weapons";
        else if (HasPrefix(what, "chainsaw")) item = "chainsaw";
        else if (HasPrefix(what, "shotgun")) item = "shotgun";
        else if (HasPrefix(what, "supershotgun")) item = "supershotgun";
        else if (HasPrefix(what, "rocket")) item = "rocketlauncher";
        else if (HasPrefix(what, "plasma")) item = "plasmarifle";
        else if (HasPrefix(what, "bfg")) item = "BFG9000";
        else if (HasPrefix(what, "chaingun")) item = "chaingun";
        else if (HasPrefix(what, "berserk")) item = "Berserk";
        else if (HasPrefix(what, "map")) item = "Allmap";
        else if (HasPrefix(what, "fullmap")) item = "Allmap";
        else return;

        if (item.Length > 72)
            item = item[..72];
        addCommand("give " + item);
    }

    public static void MapCmd(Level level, Scanner sc)
    {
        var flags = ChangeLevelResetInventory | ChangeLevelResetHealth;
        if ((level.Dmflags & DfNoMonsters) != 0)
            flags |= ChangeLevelNoMonsters;
        if (!sc.MustGetString())
            return;

        var raw = sc.Text;
        var nextMap = raw.Length > 8 ? raw[..8] : raw;
        var nextSkill = -1;
        while (sc.GetString())
        {
            if (sc.Compare("-skill"))
            {
                if (!sc.MustGetNumber())
                    return;
                var hi = level.SkillCount - 1;
                if (hi < 0)
                    hi = 0;
                nextSkill = Math.Clamp(sc.Number - 1, 0, hi);
            }
            else if (sc.Compare("-monsters"))
            {
                if (!sc.MustGetNumber())
                    return;
                if (sc.Number != 0)
                    flags &= ~ChangeLevelNoMonsters;
                else
                    flags |= ChangeLevelNoMonsters;
            }
            else if (sc.Compare("-noresetplayers"))
                flags &= ~(ChangeLevelResetInventory | ChangeLevelResetHealth);
        }

        level.NextMap = nextMap;
        level.NextPosition = 0;
        level.NextFlags = flags;
        level.NextSkill = nextSkill;
        level.LevelChanged = true;
    }

    public static void EmulateCmd(Level level, string command)
    {
        var sc = new Scanner(level);
        sc.OpenMem("RUNCMD", command);
        while (sc.GetString())
        {
            if (sc.Compare("GIMME"))
            {
                while (sc.GetString())
                {
                    if (sc.Compare(";"))
                        break;
                    Gimme(sc.Text, level.Commands.Add);
                }
            }
            else if (sc.Compare("ALLOWJUMP"))
            {
                if (!sc.MustGetNumber())
                    return;
                if (sc.Number != 0)
                    level.Dmflags &= ~DfNoJump;
                else
                    level.Dmflags |= DfNoJump;
                SkipUntilSemicolon(sc);
            }
            else if (sc.Compare("gravity"))
            {
                if (!sc.MustGetFloat())
                    return;
                level.Gravity = (float)(sc.Float * 800d);
                SkipUntilSemicolon(sc);
            }
            else if (sc.Compare("viewheight"))
            {
                if (!sc.MustGetFloat())
                    return;
                var playerviewheight = sc.Float;
                foreach (var player in level.Players)
                {
                    if (player.HasActor)
                        player.ActorViewHeight = playerviewheight;
                    player.ViewHeight = playerviewheight;
                    player.UncrouchCount++;
                }

                SkipUntilSemicolon(sc);
            }
            else if (sc.Compare("map"))
                MapCmd(level, sc);
            else if (sc.Compare("gr_fogdensity"))
            {
                if (!sc.MustGetNumber())
                    return;
                level.FogDensity = sc.Number * 70 / 400;
                level.OutsideFogDensity = 0;
                level.SkyFog = 0;
                level.OutsideFog = 0;
            }
            else if (sc.Compare("gr_fogcolor"))
            {
                if (!sc.MustGetString())
                    return;
                level.FadeTo = ParseHexUnsigned(sc.Text);
            }
            else
                SkipUntilSemicolon(sc);
        }
    }

    public static uint ParseHexUnsigned(string text)
    {
        var i = 0;
        while (i < text.Length && text[i] <= ' ')
            i++;
        var negative = false;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            negative = text[i] == '-';
            i++;
        }

        if (i + 1 < text.Length && text[i] == '0' && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            i += 2;
        ulong value = 0;
        var any = false;
        while (i < text.Length)
        {
            var c = text[i];
            int digit;
            if (c >= '0' && c <= '9')
                digit = c - '0';
            else if (c >= 'a' && c <= 'f')
                digit = c - 'a' + 10;
            else if (c >= 'A' && c <= 'F')
                digit = c - 'A' + 10;
            else
                break;
            any = true;
            value = unchecked(value * 16 + (ulong)digit);
            i++;
        }

        if (!any)
            return 0;
        if (negative)
            value = unchecked((ulong)-(long)value);
        return unchecked((uint)value);
    }

    static void SkipUntilSemicolon(Scanner sc)
    {
        while (sc.GetString())
        {
            if (sc.Compare(";"))
                break;
        }
    }

    static bool HasPrefix(string text, string prefix)
    {
        return text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public sealed class Scanner
    {
        readonly Level _level;
        string _text = "";
        int _at;
        int _line = 1;

        public Scanner(Level level)
        {
            _level = level;
        }

        public string ScriptName { get; private set; } = "";
        public string Text { get; private set; } = "";
        public int Number { get; private set; }
        public double Float { get; private set; }
        public bool Failed { get; private set; }

        public void OpenMem(string name, string body)
        {
            ScriptName = name;
            _text = body.Length == 0 || body[^1] != '\n' ? body + "\n" : body;
            _at = 0;
            _line = 1;
            Failed = false;
            Text = "";
        }

        public bool GetString()
        {
            if (Failed)
                return false;
            while (_at < _text.Length)
            {
                var c = _text[_at];
                if (c == '\n')
                {
                    _at++;
                    _line++;
                    continue;
                }

                if (c <= ' ')
                {
                    _at++;
                    continue;
                }

                if (c == ';' || StartsAt("//"))
                {
                    while (_at < _text.Length && _text[_at] != '\n')
                        _at++;
                    continue;
                }

                if (StartsAt("/*"))
                {
                    _at += 2;
                    while (_at + 1 < _text.Length && !(_text[_at] == '*' && _text[_at + 1] == '/'))
                    {
                        if (_text[_at] == '\n')
                            _line++;
                        _at++;
                    }

                    if (_at + 1 >= _text.Length)
                        return false;
                    _at += 2;
                    continue;
                }

                if (c == '"')
                    return ReadQuoted();

                var start = _at;
                while (_at < _text.Length)
                {
                    var d = _text[_at];
                    if (d == '\n' || d <= ' ' || d == '"' || d == ';')
                        break;
                    if (d == '/' && _at + 1 < _text.Length && (_text[_at + 1] == '/' || _text[_at + 1] == '*'))
                        break;
                    _at++;
                }

                Text = _text[start.._at];
                return true;
            }

            return false;
        }

        public bool Compare(string text)
        {
            return string.Equals(text, Text, StringComparison.OrdinalIgnoreCase);
        }

        public bool MustGetString()
        {
            if (GetString())
                return true;
            if (!Failed)
                Fail("Missing string (unexpected end of file).");
            return false;
        }

        public bool MustGetNumber()
        {
            if (GetNumber())
                return true;
            if (!Failed)
                Fail("Missing integer (unexpected end of file).");
            return false;
        }

        public bool MustGetFloat()
        {
            if (GetFloat())
                return true;
            if (!Failed)
                Fail("Missing floating-point number (unexpected end of file).");
            return false;
        }

        public bool GetNumber()
        {
            if (!GetString())
                return false;
            if (Text == "MAXINT")
            {
                Number = int.MaxValue;
                Float = Number;
                return true;
            }

            if (!TryParseInteger(Text, out var big))
            {
                Fail("SC_GetNumber: Bad numeric constant \"" + Text + "\".");
                return false;
            }

            Number = unchecked((int)big);
            Float = Number;
            return true;
        }

        public bool GetFloat()
        {
            if (!GetString())
                return false;
            if (!double.TryParse(Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                Fail("SC_GetFloat: Bad numeric constant \"" + Text + "\".");
                return false;
            }

            Float = parsed;
            Number = (int)parsed;
            return true;
        }

        bool ReadQuoted()
        {
            _at++;
            var built = new System.Text.StringBuilder();
            while (_at < _text.Length && _text[_at] != '"')
            {
                if (_text[_at] == '\\' && _at + 1 < _text.Length && _text[_at + 1] == '"')
                {
                    built.Append('"');
                    _at += 2;
                    continue;
                }

                if (_text[_at] == '\n')
                    _line++;
                built.Append(_text[_at]);
                _at++;
            }

            if (_at >= _text.Length)
            {
                Fail("Unterminated string constant");
                return false;
            }

            _at++;
            Text = built.ToString();
            return true;
        }

        bool StartsAt(string token)
        {
            if (_at + token.Length > _text.Length)
                return false;
            for (var i = 0; i < token.Length; i++)
            {
                if (_text[_at + i] != token[i])
                    return false;
            }

            return true;
        }

        void Fail(string message)
        {
            Failed = true;
            _level.Errors.Add("Script error, \"" + ScriptName + "\" line " + _line + ":\n" + message + "\n");
        }

        static bool TryParseInteger(string text, out long value)
        {
            value = 0;
            if (text.Length == 0)
                return false;
            var i = 0;
            var negative = false;
            if (text[0] == '+' || text[0] == '-')
            {
                negative = text[0] == '-';
                i++;
            }

            if (i >= text.Length)
                return false;
            var radix = 10;
            if (text[i] == '0' && i + 1 < text.Length && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            {
                radix = 16;
                i += 2;
            }
            else if (text[i] == '0')
                radix = 8;

            if (i >= text.Length)
                return false;
            ulong acc = 0;
            var any = false;
            for (; i < text.Length; i++)
            {
                var c = text[i];
                int digit;
                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'a' && c <= 'f')
                    digit = c - 'a' + 10;
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else
                    return false;
                if (digit >= radix)
                    return false;
                any = true;
                acc = unchecked(acc * (ulong)radix + (ulong)digit);
            }

            if (!any)
                return false;
            value = negative ? unchecked(-(long)acc) : unchecked((long)acc);
            return true;
        }
    }
}
