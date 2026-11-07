namespace HCDE.Playsim;

/// <summary>
/// Text responder through cast drawer from intermission.cpp 440-770.
/// Super init, ticker, and drawer are callees. The scroller starts at line 778.
/// </summary>
public static class VmIntermission1436Slice
{
    public const int BandStart = 440;
    public const int BandEnd = 770;
    public const int KeyDown = 1;
    public const int DoomStrifeChex = 25;
    public const int Voice = 2;
    public const int Weapon = 1;
    public const int ChanUi = 32;
    public const int NoSkin = 0x20;
    public const int Untranslated = 11;
    public const int Players = 1;

    public sealed class Anim
    {
        public Anim? Next;
        public int Sprite;
        public int Frame;
        public int Tics;
        public int TicRange;
        public int Index;
    }

    public sealed class Node
    {
        public Anim? State;
        public Dictionary<string, Node> Kids = new(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class Kind
    {
        public bool Player;
        public Node Labels = new();
    }

    public sealed class Body
    {
        public Anim? See;
        public Anim? Missile;
        public Anim? Melee;
        public Anim? Spawn;
        public int SeeSound;
        public int DeathSound;
        public int Translation;
        public int Flags4;
        public double ScaleX = 1;
        public double ScaleY = 1;
        public int Style;
        public double Alpha = 1;
    }

    public sealed class Snd
    {
        public int Sequence;
        public int Index;
        public string Name = "";
        public int Id;
    }

    public sealed class Frame
    {
        public string Pic = "pic";
        public int Flip;
        public int DispW = 1;
        public int DispH = 1;
    }

    public sealed class Sprite
    {
        public int First;
    }

    public sealed class Skin
    {
        public int Sprite;
        public double ScaleX = 1;
        public double ScaleY = 1;
    }

    public sealed class Face
    {
        public int Height = 8;
        public int Width = 8;
        public int Kerning;
        public int Measure;
        public Dictionary<int, int> Widths = new();
        public int CharWidth(int c) => Widths.TryGetValue(c, out int w) ? w : Width;
    }

    public sealed class World
    {
        public int Width = 320;
        public int Height = 200;
        public bool Generic;
        public int Game = 1;
        public int CleanX = 1;
        public int Console;
        public int SkinIndex;
        public bool HasActor = true;
        public int SkinnedDeath;
        public Dictionary<string, int> Sounds = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Strings = new(StringComparer.Ordinal);
        public Dictionary<string, Kind> Kinds = new(StringComparer.OrdinalIgnoreCase);
        public List<Kind> Classes = new();
        public List<Skin> Skins = new();
        public List<Sprite> Sprites = new();
        public List<Frame> Frames = new();
        public Queue<int> Rolls = new();
        public List<string> Audio = new();
        public bool Stopped;
        public List<string> Glyphs = new();
        public List<string> Titles = new();
        public List<string> Pics = new();
        public int SuperDraws;
    }

    public sealed class Page
    {
        public int Ticker;
        public int Duration;
        public int Speed = 1;
        public int Delay;
        public int Len;
        public int X;
        public int Y;
        public bool DefaultPlace;
        public byte[] Text = new byte[] { 0 };
        public int Color = Untranslated;
        public string? Name;
        public Kind? Class;
        public Body? Body;
        public List<Snd> Sounds = new();
        public Anim? State;
        public Anim? Advance;
        public Anim? Base;
        public int Translation;
        public bool Death;
        public int Frames;
        public int OnMelee;
        public bool Attacking;
        public int CastTics;
        public Face? Font;
    }

    public static int TextKey(World w, Page p, int type)
    {
        if (type == KeyDown && p.Ticker < p.Delay + p.Len * p.Speed)
        {
            p.Ticker = p.Delay + p.Len * p.Speed;
            return 1;
        }
        return type == KeyDown ? -1 : 0;
    }

    public static void DrawText(World w, Page p)
    {
        w.SuperDraws++;
        if (p.Ticker < p.Delay) return;
        if (p.Speed == 0) throw new DivideByZeroException();
        var font = p.Font ?? throw new NullReferenceException();
        int scale = Math.Max(w.Generic ? Math.Min(w.Width / 640, w.Height / 400) : Math.Min(w.Width / 400, w.Height / 250), 1);
        int cleanW = w.Width / scale;
        int cleanH = w.Height / scale;
        int refW = w.Generic ? 640 : 320;
        int refH = w.Generic ? 400 : 200;
        int rows = 1;
        for (int i = 0; i < p.Text.Length && p.Text[i] != 0; i++)
            if (p.Text[i] == (byte)'\n') rows++;
        int rowH = font.Height * scale;
        int pad = (w.Generic ? 2 : ((w.Game & DoomStrifeChex) != 0 ? 3 : -1)) * scale;
        int cx = (p.X - refW / 2) * scale + w.Width / 2;
        int cy = (p.Y - refH / 2) * scale + w.Height / 2;
        if (cx < 0) cx = 0;
        int start = cx;
        if (p.DefaultPlace)
        {
            int all;
            while ((all = rows * (rowH + pad)) > w.Height && pad > 0) pad--;
            all = rows * (rowH + pad);
            if (w.Height - cy - all < cy)
            {
                cy = (w.Height - all) / 2;
                if (cy < 0) cy = 0;
            }
        }
        else
        {
            while (pad > 0 && cy + rows * (rowH + pad) - pad > w.Height) pad--;
        }
        rowH += pad;
        int count = (p.Ticker - p.Delay) / p.Speed;
        int at = 0;
        for (; count > 0; count--)
        {
            int c = NextChar(p.Text, ref at);
            if (c == 0) break;
            if (c == '\n')
            {
                cx = start;
                cy += rowH;
                continue;
            }
            int width = (font.CharWidth(c) + font.Kerning) * scale;
            if (cx + width > w.Width) continue;
            w.Glyphs.Add(p.Color + "," + (cx / scale) + "," + (cy / scale) + "," + c + "," + cleanW + "," + cleanH);
            cx += width;
        }
    }

    public static void CastInit(World w, Page p, int duration, int pageSound, string? className, string? title, List<Snd> sounds, bool first)
    {
        p.Duration = duration;
        p.Ticker = 0;
        p.Name = title;
        w.Audio.Add(Voice + ":" + ChanUi + ":" + pageSound + ":1:0");
        if (className == null || !w.Kinds.TryGetValue(className, out Kind? kind))
        {
            p.Class = null;
            p.Body = null;
            p.State = null;
            return;
        }
        p.Class = kind;
        p.Sounds.Clear();
        foreach (Snd snd in sounds)
        {
            w.Sounds.TryGetValue(snd.Name, out int id);
            p.Sounds.Add(new Snd { Sequence = snd.Sequence, Index = snd.Index, Name = snd.Name, Id = id });
        }
        p.Body = p.Body ?? throw new NullReferenceException();
        p.State = p.Body.See;
        if (kind.Player)
        {
            p.Advance = p.Body.Missile;
            p.Translation = (Players << 16) | w.Console;
        }
        else
        {
            p.Advance = null;
            p.Translation = 0;
            if (p.Body.Translation != 0) p.Translation = p.Body.Translation;
        }
        p.Death = false;
        p.Frames = 0;
        p.OnMelee = 0;
        p.Attacking = false;
        if (p.Body.SeeSound > 0) w.Audio.Add(Voice + ":" + ChanUi + ":" + p.Body.SeeSound + ":1:0");
    }

    public static int CastKey(World w, Page p, int type)
    {
        if (type != KeyDown) return 0;
        if (p.Death) return 1;
        p.Death = true;
        if (p.Class != null)
        {
            p.State = Find(p.Class, "Death", "Cast");
            if (p.State == null) return -1;
            p.CastTics = Tics(w, p.State);
            p.Frames = 0;
            p.Attacking = false;
            if (p.Class.Player)
            {
                if (!w.HasActor) throw new NullReferenceException();
                if (w.SkinnedDeath != 0) w.Audio.Add(Voice + ":" + ChanUi + ":" + w.SkinnedDeath + ":1:0");
            }
            else if (p.Body != null && p.Body.DeathSound > 0)
                w.Audio.Add(Voice + ":" + ChanUi + ":" + p.Body.DeathSound + ":1:0");
        }
        return 1;
    }

    public static void AttackSound(World w, Page p)
    {
        if (p.State == null || !p.Attacking || p.Body == null) return;
        bool melee = p.Base != p.Body.Missile;
        foreach (Snd snd in p.Sounds)
        {
            bool seq = snd.Sequence != 0;
            if (seq == melee && p.Base != null && p.State.Index == p.Base.Index + snd.Index)
            {
                w.Stopped = true;
                w.Audio.Add(Weapon + ":" + ChanUi + ":" + snd.Id + ":1:0");
                return;
            }
        }
    }

    public static int CastTick(World w, Page p)
    {
        p.Ticker++;
        if (--p.CastTics > 0 && p.State != null) return 0;
        if (p.State == null || Tics(w, p.State) == -1 || p.State.Next == null || (p.State.Next == p.State && p.Death))
            return -1;
        if (p.State == p.Advance) return Stop(w, p);
        p.State = p.State.Next;
        AttackSound(w, p);
        p.Frames++;
        if (p.Frames == 12 && !p.Death)
        {
            p.Attacking = true;
            if (p.Class == null) throw new NullReferenceException();
            if (!p.Class.Player)
            {
                p.Base = p.State = p.OnMelee != 0 ? p.Body!.Melee : p.Body!.Missile;
                p.OnMelee ^= 1;
                if (p.State == null) p.Base = p.State = p.OnMelee != 0 ? p.Body!.Melee : p.Body!.Missile;
            }
            else p.Base = p.State = p.Body!.Missile;
            AttackSound(w, p);
        }
        if (p.Attacking && (p.Frames == 24 || p.State == p.Body!.See)) return Stop(w, p);
        if (p.State == null) throw new NullReferenceException();
        p.CastTics = Tics(w, p.State);
        if (p.CastTics == -1) p.CastTics = 15;
        return 0;
    }

    public static void DrawCast(World w, Page p)
    {
        w.SuperDraws++;
        var font = p.Font ?? throw new NullReferenceException();
        if (p.Name != null)
        {
            string name = p.Name;
            if (name.Length > 0 && name[0] == '$')
            {
                string key = name.Substring(1);
                name = w.Strings.TryGetValue(key, out string? text) ? text : key;
            }
            int x = (w.Width - font.Measure * w.CleanX) / 2;
            int y = w.Height * 180 / 200;
            w.Titles.Add(Untranslated + "," + x + "," + y + "," + name);
        }
        if (p.State == null) return;
        if (p.Body == null || p.Class == null) throw new NullReferenceException();
        double sx = p.Body.ScaleX;
        double sy = p.Body.ScaleY;
        int sprite = p.State.Sprite;
        if ((p.Body.Flags4 & NoSkin) == 0 && p.Body.Spawn != null && p.State.Sprite == p.Body.Spawn.Sprite && p.Class.Player && w.Skins.Count > 0)
        {
            foreach (Kind cls in w.Classes)
            {
                if (cls != p.Class) continue;
                Skin skin = w.Skins[w.SkinIndex];
                sprite = skin.Sprite;
                if ((p.Body.Flags4 & NoSkin) == 0)
                {
                    sx = skin.ScaleX;
                    sy = skin.ScaleY;
                }
            }
        }
        int slot = w.Sprites[sprite].First + p.State.Frame;
        Frame frame = w.Frames[slot];
        w.Pics.Add(frame.Pic + "," + (frame.Flip & 1) + "," + (frame.DispW * sx) + "," + (frame.DispH * sy) + "," + p.Body.Style + "," + p.Body.Alpha + "," + p.Translation);
    }

    static int Stop(World w, Page p)
    {
        p.Attacking = false;
        p.Frames = 0;
        p.State = p.Body!.See;
        if (p.State == null) throw new NullReferenceException();
        p.CastTics = Tics(w, p.State);
        if (p.CastTics == -1) p.CastTics = 15;
        return 0;
    }

    static int Tics(World w, Anim state)
    {
        if (state.TicRange == 0) return state.Tics;
        int roll = w.Rolls.Count > 0 ? w.Rolls.Dequeue() : 0;
        return state.Tics + roll % (state.TicRange + 1);
    }

    static Anim? Find(Kind kind, string a, string b)
    {
        Node labels = kind.Labels;
        Anim? best = null;
        foreach (string name in new[] { a, b })
        {
            if (!labels.Kids.TryGetValue(name, out Node? kid)) break;
            best = kid.State;
            labels = kid;
        }
        return best;
    }

    static int NextChar(byte[] text, ref int i)
    {
        if (i >= text.Length) throw new IndexOutOfRangeException();
        int z = text[i];
        if (z < 192)
        {
            i++;
            if (z >= 128 && z < 160) return Win1252[z - 128];
            return z;
        }
        int size;
        int chr = Decode(text, i, out size);
        if (chr >= 0)
        {
            i += size;
            return chr;
        }
        i++;
        return z;
    }

    static int Decode(byte[] src, int i, out int size)
    {
        size = 1;
        int c = src[i];
        if ((c & 0x80) == 0) return c;
        if (i + 1 >= src.Length) return -1;
        int c1 = src[i + 1];
        if (c1 < 0x80 || c1 >= 0xc0) return -1;
        c1 &= 0x3f;
        if ((c & 0xE0) == 0xC0)
        {
            int r = ((c & 0x1F) << 6) | c1;
            if (r >= 128)
            {
                size = 2;
                return r;
            }
            return -1;
        }
        if (i + 2 >= src.Length) return -1;
        int c2 = src[i + 2];
        if (c2 < 0x80 || c2 >= 0xc0) return -1;
        c2 &= 0x3f;
        if ((c & 0xF0) == 0xE0)
        {
            int r = ((c & 0x0F) << 12) | (c1 << 6) | c2;
            if (r >= 2048 && (r < 55296 || r > 57343))
            {
                size = 3;
                return r;
            }
            return -1;
        }
        if (i + 3 >= src.Length) return -1;
        int c3 = src[i + 3];
        if (c3 < 0x80 || c1 >= 0xc0) return -1;
        c3 &= 0x3f;
        if ((c & 0xF8) == 0xF0)
        {
            int r = ((c & 0x07) << 18) | (c1 << 12) | (c2 << 6) | c3;
            if (r >= 65536 && r <= 1114111)
            {
                size = 4;
                return r;
            }
        }
        return -1;
    }

    static readonly int[] Win1252 =
    {
        0x20AC, 0x81, 0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021, 0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x8d, 0x017D, 0x8f,
        0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014, 0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x9d, 0x017E, 0x0178,
    };
}
