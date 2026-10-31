namespace HCDE.Playsim;

/// <summary>
/// Event ring, alt-enter, escape bypass, and the UI event view from <c>d_event.cpp</c>
/// (<b>1–273</b>). Chunk <b>1236</b>. The ring holds 128 events. A device-change post is
/// dropped when the slot at the head is already a device change. Alt+Enter toggles
/// fullscreen on this Linux build and does not reach the responders. A key-up whose
/// key was eaten by the game responder in the same pass is posted again at the end.
/// </summary>
public sealed class VmEvent1236
{
    public const int MaxEvents = 128;
    public const int KeyEscape = 0x01;
    public const int KeyEnter = 0x1c;
    public const int GkReturn = 13;
    public const int GkmShift = 1;
    public const int GkmCtrl = 2;
    public const int GkmAlt = 4;
    public const int GsIntro = 8;
    public const int EvNone = 0;
    public const int EvKeyDown = 1;
    public const int EvKeyUp = 2;
    public const int EvMouse = 3;
    public const int EvGui = 4;
    public const int EvDevice = 5;
    public const int GuiKeyDown = 1;
    public const int GuiKeyRepeat = 2;
    public const int GuiKeyUp = 3;
    public const int GuiChar = 4;

    public readonly List<string> Log = new();
    public readonly Ev[] Ring = new Ev[MaxEvents];
    public int Head;
    public int Tail;
    public int GameState;
    public bool Fullscreen;
    public float SensX = 2f;
    public float SensY = 2f;
    public bool InvertX;
    public bool InvertY;
    public bool? EscapeWanted;
    public int Devices;
    public readonly List<bool> DispatchEat = new();
    public readonly List<bool> ConsoleEat = new();
    public readonly List<bool> MenuEat = new();
    public readonly List<bool> GameEat = new();
    int _dispatch;
    int _console;
    int _menu;
    int _game;

    public struct Ev
    {
        public int Type;
        public int Sub;
        public short Data1;
        public short Data2;
        public short Data3;
        public float X;
        public float Y;
    }

    public VmEvent1236()
    {
        for (int i = 0; i < MaxEvents; i++)
            Ring[i] = new Ev();
    }

    public void Post(Ev ev)
    {
        if (ev.Type == EvDevice && Ring[Head].Type == EvDevice)
        {
            Log.Add("drop device head=" + Head);
            return;
        }

        if (Next(DispatchEat, ref _dispatch))
        {
            Log.Add("dispatch eat=1");
            return;
        }

        Ring[Head] = ev;
        Head = (Head + 1) & (MaxEvents - 1);
        Log.Add("post type=" + ev.Type + " head=" + Head);
    }

    public void PostMouse(int xx, int yy)
    {
        float x = xx * SensX;
        float y = -yy * SensY;
        if (InvertX)
            x = -x;
        if (InvertY)
            y = -y;
        Log.Add("move x=" + Fmt(x) + " y=" + Fmt(y));
        if (x != 0f || y != 0f)
        {
            var ev = new Ev { Type = EvMouse, X = x, Y = y };
            Post(ev);
        }
    }

    public void Process()
    {
        var down = new bool[0x1cb];
        var delayed = new List<Ev>();
        while (Tail != Head)
        {
            Ev ev = Ring[Tail];
            Tail = (Tail + 1) & (MaxEvents - 1);
            if (ev.Type == EvKeyUp && ev.Data1 >= 0 && ev.Data1 < down.Length && down[ev.Data1])
            {
                delayed.Add(ev);
                Log.Add("delay key=" + ev.Data1);
                continue;
            }

            if (ev.Type == EvNone)
            {
                Log.Add("skip none");
                continue;
            }

            if (ev.Type == EvDevice)
                Log.Add("device list=" + Devices);

            if ((ev.Type == EvKeyDown && ev.Data1 == KeyEnter && (ev.Data3 & GkmAlt) != 0)
                || (ev.Type == EvGui && ev.Sub == GuiKeyDown && ev.Data1 == GkReturn && (ev.Data3 & GkmAlt) != 0))
            {
                Fullscreen = !Fullscreen;
                Log.Add("fullscreen " + (Fullscreen ? 1 : 0));
                continue;
            }

            bool ui = ev.Type != EvKeyDown || ev.Data1 != KeyEscape || EscapeWanted == null || !EscapeWanted.Value;
            if (ev.Type == EvKeyDown && ev.Data1 == KeyEscape && EscapeWanted != null)
                Log.Add("escape want=" + (EscapeWanted.Value ? 1 : 0));
            if (ui)
            {
                if (GameState != GsIntro)
                {
                    if (Next(ConsoleEat, ref _console))
                    {
                        Log.Add("console eat=1");
                        continue;
                    }

                    Log.Add("console eat=0");
                    if (Next(MenuEat, ref _menu))
                    {
                        Log.Add("menu eat=1");
                        continue;
                    }

                    Log.Add("menu eat=0");
                }
                else
                {
                    Log.Add("intro");
                }
            }

            bool game = Next(GameEat, ref _game);
            Log.Add("game eat=" + (game ? 1 : 0));
            if (game && ev.Type == EvKeyDown && ev.Data1 >= 0 && ev.Data1 < down.Length)
            {
                down[ev.Data1] = true;
                Log.Add("mark key=" + ev.Data1);
            }
        }

        foreach (Ev ev in delayed)
            Post(ev);
    }

    public void RemoveNextChar()
    {
        Ev tail = Ring[Tail];
        if (tail.Type != EvGui || tail.Sub != GuiKeyDown)
        {
            Log.Add("assert fail");
            return;
        }

        for (int n = Tail; n != Head; n = (n + 1) & (MaxEvents - 1))
        {
            if (Ring[n].Type != EvGui)
                break;
            if (Ring[n].Sub == GuiKeyDown || Ring[n].Sub == GuiChar)
            {
                Ring[n].Type = EvNone;
                Log.Add("blank index=" + n + " sub=" + Ring[n].Sub);
                if (Ring[n].Sub == GuiChar)
                    break;
            }
            else
            {
                break;
            }
        }
    }

    public string Input(Ev ev)
    {
        int scan = 0;
        int key = 0;
        string text = "";
        int mx = 0;
        int my = 0;
        switch (ev.Type)
        {
            case EvKeyDown:
            case EvKeyUp:
                scan = ev.Data1;
                key = ev.Data2;
                text = OneChar(ev.Data1);
                break;
            case EvMouse:
                mx = (int)ev.X;
                my = (int)ev.Y;
                break;
        }

        return "input type=" + ev.Type + " scan=" + scan + " char=" + key + " str=" + text + " mouse=" + mx + "," + my;
    }

    public string Ui(Ev ev)
    {
        int key = 0;
        string text = "";
        int shift = 0;
        int alt = 0;
        int ctrl = 0;
        int mx = 0;
        int my = 0;
        switch (ev.Sub)
        {
            case GuiKeyDown:
            case GuiKeyRepeat:
            case GuiKeyUp:
                key = ev.Data1;
                text = OneChar(ev.Data1);
                shift = (ev.Data3 & GkmShift) != 0 ? 1 : 0;
                alt = (ev.Data3 & GkmAlt) != 0 ? 1 : 0;
                ctrl = (ev.Data3 & GkmCtrl) != 0 ? 1 : 0;
                break;
            case GuiChar:
                key = (ushort)ev.Data1;
                text = Utf8((ushort)ev.Data1);
                alt = ev.Data2 != 0 ? 1 : 0;
                break;
            case 0:
                break;
            default:
                mx = ev.Data1;
                my = ev.Data2;
                shift = (ev.Data3 & GkmShift) != 0 ? 1 : 0;
                alt = (ev.Data3 & GkmAlt) != 0 ? 1 : 0;
                ctrl = (ev.Data3 & GkmCtrl) != 0 ? 1 : 0;
                break;
        }

        return "ui type=" + ev.Sub + " char=" + key + " str=" + text + " shift=" + shift + " alt=" + alt + " ctrl=" + ctrl + " mouse=" + mx + "," + my;
    }

    public void ExportFields()
    {
        Log.Add("field UiEvent.Type");
        Log.Add("field UiEvent.KeyString");
        Log.Add("field UiEvent.KeyChar");
        Log.Add("field UiEvent.MouseX");
        Log.Add("field UiEvent.MouseY");
        Log.Add("field UiEvent.IsShift");
        Log.Add("field UiEvent.IsAlt");
        Log.Add("field UiEvent.IsCtrl");
        Log.Add("field InputEvent.Type");
        Log.Add("field InputEvent.KeyScan");
        Log.Add("field InputEvent.KeyString");
        Log.Add("field InputEvent.KeyChar");
        Log.Add("field InputEvent.MouseX");
        Log.Add("field InputEvent.MouseY");
    }

    static string OneChar(short data1)
    {
        int b = data1 & 0xff;
        if (b == 0)
            return "";
        return b.ToString("x2");
    }

    static string Utf8(int codepoint)
    {
        if (codepoint < 0)
            return "";
        if (codepoint < 0x80)
            return codepoint.ToString("x2");
        if (codepoint < 0x800)
        {
            int a = 0xC0 + ((codepoint & 0x7C0) >> 6);
            int b = 0x80 + (codepoint & 0x03F);
            return a.ToString("x2") + b.ToString("x2");
        }

        if (codepoint < 0x10000)
        {
            int a = 0xE0 + ((codepoint & 0xF000) >> 12);
            int b = 0x80 + ((codepoint & 0x0FC0) >> 6);
            int c = 0x80 + (codepoint & 0x003F);
            return a.ToString("x2") + b.ToString("x2") + c.ToString("x2");
        }

        return "";
    }

    static string Fmt(float v)
    {
        if (v == (int)v)
            return ((int)v).ToString();
        return v.ToString("G9");
    }

    static bool Next(List<bool> rows, ref int at)
    {
        if (at >= rows.Count)
            return false;
        return rows[at++];
    }
}
