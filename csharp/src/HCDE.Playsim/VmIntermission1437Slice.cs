namespace HCDE.Playsim;

/// <summary>
/// Scroller and intermission NextPage from intermission.cpp 778-907 and 915-985.
/// Comment lines 908-914 stay uncredited. Super init is a callee.
/// </summary>
public static class VmIntermission1437Slice
{
    public const int BandStart = 778;
    public const int BandEnd = 985;
    public const int ScrollLeft = 0;
    public const int ScrollRight = 1;
    public const int ScrollUp = 2;
    public const int ScrollDown = 3;
    public const int Wiper = -1;
    public const int Title = -2;
    public const int KeyDown = 1;
    public const int Voice = 2;
    public const int ChanUi = 32;

    public sealed class Tex
    {
        public int W;
        public int H;
    }

    public sealed class Box
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;
    }

    public sealed class Pic
    {
        public int Id;
        public double X;
        public double Y;
        public double W;
        public double H;
        public int ClipL;
        public int ClipT;
        public int ClipW;
        public int ClipH;
    }

    public sealed class World
    {
        public int Width = 640;
        public int Height = 480;
        public bool AbortScroll;
        public bool Net;
        public bool Demo;
        public int Cutscenes;
        public int ClipClears;
        public Dictionary<int, Tex> Textures = new();
        public Dictionary<string, Desc> Links = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Audio = new();
        public List<Pic> Pics = new();
        public List<int> Deleted = new();
    }

    public sealed class Page
    {
        public int Ticker;
        public int Duration;
        public int Delay;
        public int Time = 1;
        public int Dir;
        public int Background;
        public int First;
        public int Second;
        public bool OpenedFirst;
    }

    public sealed class Act
    {
        public int Class = 1;
        public int Wipe;
    }

    public sealed class Desc
    {
        public int Id;
        public string? Link;
        public List<Act> Actions = new();
    }

    public sealed class Screen
    {
        public int Class;
        public int Background;
        public bool Flat;
        public bool Inited;
        public bool FirstArg;
        public bool Dead;
    }

    public sealed class Ctrl
    {
        public Desc? Desc;
        public bool Delete;
        public int Index;
        public bool Advance;
        public bool Sent;
        public bool First = true;
        public bool EndGame;
        public int Wipe;
        public Screen? Screen;
    }

    public static void Init(World w, Page p, int duration, int sound, int background, int second, int delay, int time, int dir, bool first)
    {
        p.Duration = duration;
        p.Ticker = 0;
        p.OpenedFirst = first;
        w.Audio.Add(Voice + ":" + ChanUi + ":" + sound + ":1:0");
        p.Background = background;
        p.First = background;
        p.Second = second;
        p.Delay = delay;
        p.Time = time;
        p.Dir = dir;
    }

    public static int Key(World w, Page p, int type)
    {
        int res = type == KeyDown ? -1 : 0;
        if (res == -1)
        {
            if (w.AbortScroll) return 0;
            p.Background = p.Second;
            p.Ticker = p.Delay + p.Time;
        }
        return res;
    }

    public static void Draw(World w, Page p)
    {
        Tex a = Need(w, p.First);
        Tex b = Need(w, p.Second);
        int ah = a.H == 200 ? 240 : a.H == 400 ? 480 : a.H;
        int tick = Math.Clamp(p.Ticker - p.Delay, 0, p.Time);
        Box box;
        if (p.Dir == ScrollLeft || p.Dir == ScrollRight)
        {
            int aw = ah * 4 / 3;
            double span = ah * ((a.W + b.W) / (double)ah - 4.0 / 3.0);
            box = Fit(w, span, ah);
            double scale = box.Height / ah;
            double x1;
            double x2;
            if (p.Dir == ScrollRight)
            {
                x1 = -(double)tick * aw / p.Time;
                x2 = x1 + a.W;
            }
            else
            {
                x2 = -aw + (double)tick * aw / p.Time;
                x1 = x2 + b.W;
            }
            Blit(w, box, p.First, (x1 * scale) + box.Left, box.Top, a.W * scale, ah * scale);
            Blit(w, box, p.Second, (x2 * scale) + box.Left, box.Top, b.W * scale, ah * scale);
        }
        else
        {
            box = Fit(w, a.W, ah);
            double scale = box.Height / ah;
            double y1;
            double y2;
            if (p.Dir == ScrollDown)
            {
                y1 = -(double)tick * ah / p.Time;
                y2 = y1 + ah;
            }
            else
            {
                y1 = (double)tick * ah / p.Time;
                y2 = y1 - ah;
            }
            Blit(w, box, p.First, box.Left, box.Top + (y1 * scale), a.W * scale, ah * scale);
            Blit(w, box, p.Second, box.Left, box.Top + (y2 * scale), b.W * scale, ah * scale);
        }
        w.ClipClears++;
        p.Background = p.Second;
    }

    public static Ctrl Ctor(Desc desc, bool delete, bool ending)
    {
        return new Ctrl { Desc = desc, Delete = delete, EndGame = ending };
    }

    public static bool Next(World w, Ctrl c)
    {
        Desc desc = c.Desc ?? throw new NullReferenceException();
        if (c.Index == desc.Actions.Count && desc.Link == null) return c.EndGame;
        int bg = 0;
        bool fill = false;
        if (c.Screen != null)
        {
            bg = c.Screen.Background;
            fill = c.Screen.Flat;
            c.Screen.Dead = true;
            c.Screen = null;
        }
        while (true)
        {
            while (c.Index < desc.Actions.Count)
            {
                Act act = desc.Actions[c.Index++];
                if (act.Class == Wiper) c.Wipe = act.Wipe;
                else if (act.Class == Title) return false;
                else
                {
                    c.Screen = new Screen { Class = act.Class, Background = bg, Flat = fill, Inited = true, FirstArg = c.First };
                    c.First = false;
                    if (w.Net && !w.Demo) w.Cutscenes++;
                    return true;
                }
            }
            if (desc.Link != null && w.Links.TryGetValue(desc.Link, out Desc? linked))
            {
                if (c.Delete) w.Deleted.Add(desc.Id);
                c.Delete = false;
                c.Index = 0;
                c.Desc = desc = linked;
                continue;
            }
            return false;
        }
    }

    public static Box Fit(World w, double srcW, double srcH)
    {
        double h = srcH == 200 ? 240 : srcH == 400 ? 480 : srcH;
        double aspect = srcW / h;
        double screen = w.Width / (double)w.Height;
        int mode = 3;
        if (screen >= aspect || aspect < 1.4) mode = 1;
        else if (screen > 1.32)
            mode = 2;
        else
        {
            double w43 = h * (4.0 / 3.0);
            return new Box
            {
                Width = w.Width * srcW / w43,
                Height = w.Height * screen * (3.0 / 4.0),
                Left = w.Width * (-(srcW - w43) / 2.0) / w43,
                Top = (w.Height - (w.Height * screen * (3.0 / 4.0))) / 2.0,
            };
        }
        bool tall = mode == 7 || ((screen > aspect) ^ (mode == 2));
        if (tall)
        {
            double width = w.Width * aspect / screen;
            return new Box { Height = w.Height, Width = width, Left = (w.Width - width) / 2.0 };
        }
        double height = w.Height * screen / aspect;
        return new Box { Width = w.Width, Height = height, Top = (w.Height - height) / 2.0 };
    }

    static Tex Need(World w, int id)
    {
        if (!w.Textures.TryGetValue(id, out Tex? tex) || tex == null) throw new NullReferenceException();
        return tex;
    }

    static void Blit(World w, Box box, int id, double x, double y, double pw, double ph)
    {
        w.Pics.Add(new Pic
        {
            Id = id,
            X = x,
            Y = y,
            W = pw,
            H = ph,
            ClipL = (int)box.Left,
            ClipT = (int)box.Top,
            ClipW = (int)box.Width,
            ClipH = (int)box.Height,
        });
    }
}
