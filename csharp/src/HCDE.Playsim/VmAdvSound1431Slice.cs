namespace HCDE.Playsim;

/// <summary>
/// soundlinks through MarkSound from s_advsound.cpp 1761-2062.
/// Player lookup ends at line 1759. This is the rest of the translation unit.
/// </summary>
public static class VmAdvSound1431Slice
{
    public const int BandStart = 1761;
    public const int BandEnd = 2062;
    public const int Random = 1;
    public const int Continuous = 3;
    public const int Reserve = 1;
    public const int NoSound = 0;
    public const int NoLink = -1;
    public const int Missing = 0xffff;
    public const int Body = 4;
    public const int Loop = 256;
    public const int RandMax = 2147483647;
    public const int GenderMax = 4;
    public const string Red = "\u001cG";

    public sealed class Sfx
    {
        public string Name = "";
        public int User;
        public int Link = NoLink;
        public bool Random;
    }

    public sealed class Ambient
    {
        public int Type;
        public int Min;
        public int Max;
        public float Volume = 1f;
        public float Atten;
        public int Sound;
    }

    public sealed class Actor
    {
        public int[] Args = new int[5];
        public int Special1;
        public int Special2;
        public int Ticks;
        public int Activations;
        public int Deactivations;
        public bool Destroyed;
        public Actor? Last;
    }

    public sealed class Play
    {
        public string Kind = "";
        public int Channel;
        public int Flags;
        public int Sound;
        public float Volume;
        public float Atten;
        public float Min;
        public float Max;
    }

    public sealed class Class
    {
        public string Name = "";
        public int[] Lists = { Missing, Missing, Missing, Missing };
    }

    public struct Token
    {
        public string Text;
        public int Line;
    }

    public sealed class Lump
    {
        public string Path = "";
        public List<Token> Tokens = new();
    }

    public sealed class MapInfo
    {
        public string Name = "";
        public string Sky1 = "";
        public string Sky2 = "";
        public string Mist = "";
        public Dictionary<int, string> Music = new();
    }

    public sealed class State
    {
        public List<Sfx> Sounds = new();
        public List<Class> Classes = new();
        public List<Dictionary<int, int>> Tables = new();
        public Dictionary<int, Ambient> Ambients = new();
        public List<string> Lines = new();
        public List<int> Marks = new();
        public List<Play> Plays = new();
        public List<Lump> Lumps = new();
        public List<MapInfo> Maps = new();
        public MapInfo Default = new();
        public Dictionary<int, int> Lengths = new();
        public List<int> Rands = new();
        public int RandAt;
        public int Reserves;
    }

    public static void SoundLinks(State s)
    {
        for (int i = 0; i < s.Sounds.Count; i++)
        {
            Sfx sfx = s.Sounds[i];
            if (sfx.Link != NoLink && !sfx.Random && (sfx.User & Reserve) == 0)
            {
                Sfx target = At(s, sfx.Link) ?? throw new NullReferenceException();
                s.Lines.Add(sfx.Name + " -> " + target.Name + "\n");
            }
        }
    }

    public static void PlayerSounds(State s)
    {
        string?[] names = new string?[256];
        for (int i = 0, j = 0; j < s.Reserves && i < s.Sounds.Count; i++)
        {
            Sfx sfx = s.Sounds[i];
            if ((sfx.User & Reserve) == 0) continue;
            j++;
            if ((uint)sfx.Link >= 256u) throw new IndexOutOfRangeException();
            names[sfx.Link] = sfx.Name;
        }
        foreach (Class cls in s.Classes)
        {
            for (int gender = 0; gender < GenderMax; gender++)
            {
                int list = cls.Lists[gender];
                if (list == Missing) continue;
                s.Lines.Add("\n" + cls.Name + ", " + Gender(gender) + ":\n");
                for (int k = 0; k < s.Reserves; k++)
                {
                    string label = names[k] ?? throw new NullReferenceException();
                    int id = s.Tables[list].TryGetValue(k, out int found) ? found : NoSound;
                    Sfx snd = At(s, id) ?? throw new NullReferenceException();
                    s.Lines.Add(" " + label.PadRight(16) + snd.Name + "\n");
                }
            }
        }
    }

    public static int MarkAmbient(State s, Actor self)
    {
        if (s.Ambients.TryGetValue(self.Args[0], out Ambient? amb)) s.Marks.Add(amb.Sound);
        return 0;
    }

    public static int Ticker(State s, Ambient ambient)
    {
        if ((ambient.Type & Continuous) == Continuous) return 1;
        if ((ambient.Type & Random) != 0)
        {
            if (s.RandAt >= s.Rands.Count) throw new InvalidOperationException();
            int roll = s.Rands[s.RandAt++];
            return (int)(((float)roll / RandMax) * (float)(ambient.Max - ambient.Min)) + ambient.Min;
        }
        return ambient.Min;
    }

    public static int Tick(State s, Actor self)
    {
        self.Ticks++;
        if (self.Special1 > 0 && --self.Special1 > 0) return 0;
        if (self.Special2 == 0) return 0;
        if (!s.Ambients.TryGetValue(self.Args[0], out Ambient? ambient)) return 0;
        int loop = (ambient.Type & Continuous) == Continuous ? Loop : 0;
        if (ambient.Sound != NoSound)
        {
            float scale = self.Args[1] == 0 ? 1f : self.Args[1] / 100f;
            float use = ambient.Volume * scale;
            if (use < 0f) use = 0f;
            else if (use > 1f) use = 1f;
            if ((self.Args[2] | self.Args[3]) == 0 || self.Args[2] > self.Args[3])
                s.Plays.Add(new Play { Kind = "sound", Channel = Body, Flags = loop, Sound = ambient.Sound, Volume = use, Atten = ambient.Atten });
            else
            {
                float min = self.Args[2];
                float max = self.Args[3];
                if (self.Args[4] > 0) { min *= self.Args[4]; max *= self.Args[4]; }
                s.Plays.Add(new Play { Kind = "minmax", Channel = Body, Flags = loop, Sound = ambient.Sound, Volume = use, Min = min, Max = max });
            }
            if (loop == 0) self.Special1 += Ticker(s, ambient);
            else self.Special1 = int.MaxValue;
        }
        else self.Destroyed = true;
        return 0;
    }

    public static int Activate(State s, Actor self, Actor? activator)
    {
        self.Activations++;
        self.Last = activator;
        if (!s.Ambients.TryGetValue(self.Args[0], out Ambient? amb))
        {
            self.Destroyed = true;
            return 0;
        }
        if (self.Special2 == 0)
        {
            if ((amb.Type & 3) == 0 && amb.Min == 0)
            {
                if (amb.Sound <= 0)
                {
                    self.Destroyed = true;
                    return 0;
                }
                int ms = s.Lengths.TryGetValue(amb.Sound, out int got) ? got : 0;
                amb.Min = (int)((long)ms * 35 / 1000);
            }
            self.Special1 = 0;
            if ((amb.Type & 3) != 0) self.Special1 += Ticker(s, amb);
            self.Special2 = 1;
        }
        return 0;
    }

    public static int Deactivate(State s, Actor self, Actor? activator)
    {
        self.Deactivations++;
        self.Last = activator;
        if (self.Special2 != 0)
        {
            self.Special2 = 0;
            if (s.Ambients.TryGetValue(self.Args[0], out Ambient? ambient) && (ambient.Type & Continuous) == Continuous)
                s.Plays.Add(new Play { Kind = "stop", Channel = Body });
        }
        return 0;
    }

    public static void Parse(State s)
    {
        foreach (Lump lump in s.Lumps)
        {
            int at = 0;
            bool replay = false;
            Token held = default;
            bool Pull(out Token token)
            {
                if (replay) { replay = false; token = held; return true; }
                if (at >= lump.Tokens.Count) { token = default; return false; }
                held = lump.Tokens[at++];
                token = held;
                return true;
            }
            while (Pull(out Token map))
            {
                MapInfo? info = FindMap(s, map.Text);
                if (info == null)
                    s.Lines.Add(Red + "Script error, \"" + lump.Path + "\"" + Red + " line " + map.Line + ":\n" + Red + "Unknown map '" + map.Text + "'\n");
                while (Pull(out Token num))
                {
                    if (!TryNumber(num.Text, out int index)) { replay = true; break; }
                    if (!Pull(out Token music))
                        throw new InvalidOperationException("Script error, \"" + lump.Path + "\" line " + num.Line + ":\nMissing string (unexpected end of file).\n");
                    if (index > 0 && info != null) info.Music[index] = music.Text;
                }
            }
        }
    }

    public static int MarkSound(State s, int id)
    {
        s.Marks.Add(id);
        return 0;
    }

    static MapInfo FindMap(State s, string name)
    {
        foreach (MapInfo map in s.Maps)
            if (Compare(map.Name, name) == 0) return map;
        if (s.Default.Name.Length == 0)
        {
            s.Default.Sky1 = "SKY1";
            s.Default.Sky2 = "SKY1";
            s.Default.Mist = "SKYMIST1";
            s.Default.Name = "Unnamed";
        }
        return s.Default;
    }

    static bool TryNumber(string text, out int number)
    {
        number = 0;
        if (text == null || text.Length == 0) return false;
        if (text == "MAXINT") { number = int.MaxValue; return true; }
        int i = 0;
        while (i < text.Length && text[i] is ' ' or '\t' or '\n' or '\r' or '\v' or '\f') i++;
        if (i >= text.Length) return false;
        int sign = 1;
        if (text[i] is '+' or '-')
        {
            if (text[i] == '-') sign = -1;
            if (++i >= text.Length) return false;
        }
        int bas = 10;
        if (text[i] == '0' && i + 1 < text.Length)
        {
            if (text[i + 1] is 'x' or 'X')
            {
                bas = 16;
                i += 2;
                if (i >= text.Length) return false;
            }
            else bas = 8;
        }
        long value = 0;
        bool any = false;
        for (; i < text.Length; i++)
        {
            int digit = Digit(text[i]);
            if (digit < 0 || digit >= bas) return false;
            any = true;
            value = value * bas + digit;
        }
        if (!any) return false;
        number = (int)(sign < 0 ? -value : value);
        return true;
    }

    static Sfx? At(State s, int id) => (uint)id < (uint)s.Sounds.Count ? s.Sounds[id] : null;

    static string Gender(int gender) => gender switch
    {
        0 => "male",
        1 => "female",
        2 => "neutral",
        _ => "other",
    };

    static int Compare(string a, string b)
    {
        int n = a.Length < b.Length ? a.Length : b.Length;
        for (int i = 0; i < n; i++)
        {
            int d = Fold(a[i]) - Fold(b[i]);
            if (d != 0) return d;
        }
        return a.Length - b.Length;
    }

    static int Fold(char c) => (uint)(c - 'A') <= 25u ? c + 32 : c;

    static int Digit(char c)
    {
        if (c is >= '0' and <= '9') return c - '0';
        if (c is >= 'a' and <= 'f') return c - 'a' + 10;
        if (c is >= 'A' and <= 'F') return c - 'A' + 10;
        return -1;
    }
}
