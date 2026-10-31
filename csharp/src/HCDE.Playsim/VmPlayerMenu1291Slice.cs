namespace HCDE.Playsim;

/// <summary>
/// Player menu color, name, class, skin, and cvar setters from <c>playermenu.cpp</c>
/// (1-246). Chunk 1291. The class display name and the skin name are host inputs.
/// </summary>
public sealed class VmPlayerMenu1291
{
    public const int TeamNone = 255;

    public bool InMenu = true;
    public uint ColorBits;
    public int ColorSet;
    public string PlayerClass = "";
    public int SkinNum;
    public float Autoaim;
    public int Team;
    public bool NeverSwitch;
    public bool AlwaysRun;
    public bool WroteColor;
    public bool WroteColorSet;
    public bool WroteClass;
    public bool WroteSkin;
    public bool WroteAutoaim;
    public bool WroteTeam;
    public bool WroteSwitch;
    public bool WroteRun;
    public List<string> Commands = new();

    public int ColorChanged(int r, int g, int b)
    {
        if (InMenu)
        {
            unchecked
            {
                ColorBits = (uint)((r << 16) | (g << 8) | b);
            }
            WroteColor = true;
            Commands.Add(Fit("color \"" + Hex(r) + " " + Hex(g) + " " + Hex(b) + "\"", 24));
        }
        return 0;
    }

    public int NameChanged(string name)
    {
        if (InMenu)
        {
            var command = new System.Text.StringBuilder("name \"");
            foreach (char c in name)
            {
                if (c == '"' || c == '\\')
                    command.Append('\\');
                command.Append(c);
            }
            command.Append('"');
            Commands.Add(command.ToString());
        }
        return 0;
    }

    public int ColorSetChanged(int sel)
    {
        if (InMenu)
        {
            ColorSet = sel;
            WroteColorSet = true;
            Commands.Add(Fit("colorset " + sel.ToString(), 24));
        }
        return 0;
    }

    public int ClassChanged(int sel, string displayName)
    {
        if (InMenu)
        {
            PlayerClass = sel == -1 ? "Random" : displayName;
            WroteClass = true;
            Commands.Add("cvar playerclass " + PlayerClass);
        }
        return 0;
    }

    public int SkinChanged(int sel, string skinName)
    {
        if (InMenu)
        {
            SkinNum = sel;
            WroteSkin = true;
            Commands.Add("cvar skin " + skinName);
        }
        return 0;
    }

    public int AutoaimChanged(float val)
    {
        if (InMenu)
        {
            Autoaim = val;
            WroteAutoaim = true;
        }
        return 0;
    }

    public int TeamChanged(int val)
    {
        if (InMenu)
        {
            Team = val == 0 ? TeamNone : val - 1;
            WroteTeam = true;
        }
        return 0;
    }

    public int GenderChanged(int v)
    {
        if (InMenu)
        {
            string? gender = v switch
            {
                0 => "male",
                1 => "female",
                2 => "neutral",
                3 => "other",
                _ => null,
            };
            if (gender != null)
                Commands.Add("cvar gender " + gender);
        }
        return 0;
    }

    public int SwitchOnPickupChanged(int v)
    {
        if (InMenu)
        {
            NeverSwitch = v != 0;
            WroteSwitch = true;
        }
        return 0;
    }

    public int AlwaysRunChanged(int v)
    {
        if (InMenu)
        {
            AlwaysRun = v != 0;
            WroteRun = true;
        }
        return 0;
    }

    static string Hex(int value)
    {
        return unchecked((uint)value).ToString("x2");
    }

    static string Fit(string text, int count)
    {
        if (count <= 1)
            return "";
        if (text.Length >= count)
            return text.Substring(0, count - 1);
        return text;
    }
}
