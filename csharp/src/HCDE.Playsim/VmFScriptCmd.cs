namespace HCDE.Playsim;

/// <summary>Legacy FraggleScript console commands from <c>fragglescript/t_cmd.cpp</c> (1–186).</summary>
public static class VmFScriptCmd
{
    public const int ChangelevelResetInventory = 2;
    public const int ChangelevelNoMonsters = 4;
    public const int ChangelevelResetHealth = 32;
    public const int DfNoMonsters = 4;
    public const int DfNoJump = 1 << 16;

    public sealed class LevelPlayerState
    {
        public object? Mo { get; set; }
        public double ViewHeight { get; set; }
        public Action? Uncrouch { get; set; }
        public Action<double>? SetMoViewHeight { get; set; }
    }

    public sealed class LevelCmdHost
    {
        public int Dmflags { get; set; }
        public double Gravity { get; set; }
        public int FogDensity { get; set; }
        public int OutsideFogDensity { get; set; }
        public int SkyFog { get; set; }
        public uint FadeTo { get; set; }
        public int OutsideFogInfo { get; set; }
        public int MaxSkillIndex { get; init; } = 4;
        public List<LevelPlayerState> Players { get; } = new();
        public Action<string, int, int, int> ChangeLevel { get; init; } = (_, _, _, _) => { };
        public Action<string> AddCommandString { get; init; } = _ => { };
    }

    public static void Gimme(string what, Action<string> addCommandString)
    {
        if (what.StartsWith("health", StringComparison.OrdinalIgnoreCase)) what = "health";
        else if (what.StartsWith("ammo", StringComparison.OrdinalIgnoreCase)) what = "ammo";
        else if (what.StartsWith("armor", StringComparison.OrdinalIgnoreCase)) what = "greenarmor";
        else if (what.StartsWith("keys", StringComparison.OrdinalIgnoreCase)) what = "keys";
        else if (what.StartsWith("weapons", StringComparison.OrdinalIgnoreCase)) what = "weapons";
        else if (what.StartsWith("chainsaw", StringComparison.OrdinalIgnoreCase)) what = "chainsaw";
        else if (what.StartsWith("shotgun", StringComparison.OrdinalIgnoreCase)) what = "shotgun";
        else if (what.StartsWith("supershotgun", StringComparison.OrdinalIgnoreCase)) what = "supershotgun";
        else if (what.StartsWith("rocket", StringComparison.OrdinalIgnoreCase)) what = "rocketlauncher";
        else if (what.StartsWith("plasma", StringComparison.OrdinalIgnoreCase)) what = "plasmarifle";
        else if (what.StartsWith("bfg", StringComparison.OrdinalIgnoreCase)) what = "BFG9000";
        else if (what.StartsWith("chaingun", StringComparison.OrdinalIgnoreCase)) what = "chaingun";
        else if (what.StartsWith("berserk", StringComparison.OrdinalIgnoreCase)) what = "Berserk";
        else if (what.StartsWith("map", StringComparison.OrdinalIgnoreCase)) what = "Allmap";
        else if (what.StartsWith("fullmap", StringComparison.OrdinalIgnoreCase)) what = "Allmap";
        else return;

        addCommandString($"give {what[..Math.Min(what.Length, 72)]}");
    }

    public static void MapCmd(LevelCmdHost level, CmdScanner sc)
    {
        var flags = ChangelevelResetInventory | ChangelevelResetHealth;
        if ((level.Dmflags & DfNoMonsters) != 0)
            flags |= ChangelevelNoMonsters;
        sc.MustGetString();
        var nextmap = sc.String.Length > 8 ? sc.String[..8] : sc.String;
        var nextSkill = -1;
        while (sc.GetString())
        {
            if (sc.Compare("-skill"))
            {
                sc.MustGetNumber();
                nextSkill = Math.Clamp(sc.Number - 1, 0, level.MaxSkillIndex);
            }
            else if (sc.Compare("-monsters"))
            {
                sc.MustGetNumber();
                if (sc.Number != 0)
                    flags &= ~ChangelevelNoMonsters;
                else
                    flags |= ChangelevelNoMonsters;
            }
            else if (sc.Compare("-noresetplayers"))
                flags &= ~(ChangelevelResetInventory | ChangelevelResetHealth);
        }
        level.ChangeLevel(nextmap, 0, flags, nextSkill);
    }

    public static void EmulateCmd(LevelCmdHost level, string command)
    {
        var sc = new CmdScanner();
        sc.OpenMem("RUNCMD", command);
        while (sc.GetString())
        {
            if (sc.Compare("GIMME"))
            {
                while (sc.GetString())
                {
                    if (sc.Compare(";")) break;
                    Gimme(sc.String, level.AddCommandString);
                }
            }
            else if (sc.Compare("ALLOWJUMP"))
            {
                sc.MustGetNumber();
                level.Dmflags = sc.Number != 0
                    ? level.Dmflags & ~DfNoJump
                    : level.Dmflags | DfNoJump;
                while (sc.GetString())
                {
                    if (sc.Compare(";")) break;
                }
            }
            else if (sc.Compare("gravity"))
            {
                sc.MustGetFloat();
                level.Gravity = sc.Float * 800;
                while (sc.GetString())
                {
                    if (sc.Compare(";")) break;
                }
            }
            else if (sc.Compare("viewheight"))
            {
                sc.MustGetFloat();
                var playerviewheight = sc.Float;
                foreach (var p in level.Players)
                {
                    if (p.Mo != null)
                        p.SetMoViewHeight?.Invoke(playerviewheight);
                    p.ViewHeight = playerviewheight;
                    p.Uncrouch?.Invoke();
                }
                while (sc.GetString())
                {
                    if (sc.Compare(";")) break;
                }
            }
            else if (sc.Compare("map"))
                MapCmd(level, sc);
            else if (sc.Compare("gr_fogdensity"))
            {
                sc.MustGetNumber();
                level.FogDensity = sc.Number * 70 / 400;
                level.OutsideFogDensity = 0;
                level.SkyFog = 0;
                level.OutsideFogInfo = 0;
            }
            else if (sc.Compare("gr_fogcolor"))
            {
                sc.MustGetString();
                level.FadeTo = Convert.ToUInt32(sc.String, 16);
            }
            else
            {
                while (sc.GetString())
                {
                    if (sc.Compare(";")) break;
                }
            }
        }
    }

    public sealed class CmdScanner
    {
        string _buffer = "";
        int _pos;
        public string String { get; private set; } = "";
        public int Number { get; private set; }
        public double Float { get; private set; }

        public void OpenMem(string name, string text)
        {
            _ = name;
            _buffer = text;
            _pos = 0;
        }

        public bool GetString()
        {
            while (_pos < _buffer.Length && char.IsWhiteSpace(_buffer[_pos]))
                _pos++;
            if (_pos >= _buffer.Length)
                return false;
            var start = _pos;
            while (_pos < _buffer.Length && !char.IsWhiteSpace(_buffer[_pos]))
                _pos++;
            String = _buffer[start.._pos];
            return true;
        }

        public void MustGetString()
        {
            if (!GetString())
                throw new InvalidOperationException("Missing string");
        }

        public void MustGetNumber()
        {
            MustGetString();
            Number = int.Parse(String, System.Globalization.CultureInfo.InvariantCulture);
            Float = Number;
        }

        public void MustGetFloat()
        {
            MustGetString();
            Float = double.Parse(String, System.Globalization.CultureInfo.InvariantCulture);
            Number = (int)Float;
        }

        public bool Compare(string text) =>
            string.Equals(text, String, StringComparison.OrdinalIgnoreCase);
    }
}
