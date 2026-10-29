namespace HCDE.Playsim;

/// <summary>
/// ENDOOM startup and console dump from <c>endoom.cpp</c> (<b>1–224</b>). Chunk <b>1231</b>.
/// showendoom clamps to 0..2. A missing lump, a length other than 4000, or
/// showendoom 2 on the IWAD returns before the screen exists. The blink flag
/// starts true and the attribute scan never clears it. The clock must move
/// more than 267 ms, as a uint64 add, before the phase flips. Key events and
/// the four dismiss GUI events leave the loop. The Linux build has no
/// FancyStdOut gate on the console dump.
/// </summary>
public sealed class VmEndoom1231
{
    public const int BlinkPeriod = 267;
    public const int MaxEvents = 128;
    public const byte EvKeyDown = 1;
    public const byte EvKeyUp = 2;
    public const byte EvGui = 4;
    public const byte GuiKeyDown = 1;
    public const byte GuiLButtonDown = 7;
    public const byte GuiMButtonDown = 10;
    public const byte GuiRButtonDown = 13;

    public readonly List<string> Log = new();
    public int ShowEndoom;
    public bool ConsoleEndoomEnabled;
    public string Name = "";
    public int Lump = -1;
    public int Length;
    public int Container;
    public int MaxIwad;
    public string QuitText = "";
    public readonly byte[] Screen = new byte[4000];
    public ulong Now;
    public int EventHead;
    public int EventTail;
    public readonly byte[] EvType = new byte[MaxEvents];
    public readonly byte[] EvSub = new byte[MaxEvents];
    public bool Blinking = true;
    public bool BlinkState;
    public ulong Last;
    public Action<VmEndoom1231>? GetEvent;

    public void SetShow(int value)
    {
        int clamped = value;
        if (clamped < 0)
            clamped = 0;
        else if (clamped > 2)
            clamped = 2;
        ShowEndoom = clamped;
        Log.Add("show in=" + value + " out=" + clamped);
    }

    public void Post(byte type, byte sub)
    {
        EvType[EventHead] = type;
        EvSub[EventHead] = sub;
        EventHead = (EventHead + 1) & (MaxEvents - 1);
        Log.Add("post type=" + type + " sub=" + sub + " head=" + EventHead);
    }

    public void StEndoom()
    {
        ConsoleEndoom();
        int code = Run();
        Log.Add("exit " + code);
    }

    public void ConsoleEndoom()
    {
        if (!ConsoleEndoomEnabled)
        {
            Log.Add("console flag=0");
            return;
        }

        if (Name.Length == 0)
        {
            Log.Add("console flag=1 namelen=0");
            return;
        }

        Log.Add("console flag=1 namelen=" + Name.Length);
        LogLookup();
        if (Lump < 0 || Length != 4000)
        {
            Log.Add("console return");
            return;
        }

        Log.Add("read lump=" + Lump);
        Log.Add("vga_to_ansi");
    }

    public int Run()
    {
        if (ShowEndoom == 0)
        {
            Log.Add("run show=0");
            Log.Add("run skip");
            return 0;
        }

        if (Name.Length == 0)
        {
            Log.Add("run show=" + ShowEndoom + " namelen=0");
            Log.Add("run skip");
            return 0;
        }

        Log.Add("run show=" + ShowEndoom + " namelen=" + Name.Length);
        LogLookup();
        if (Lump < 0 || Length != 4000)
        {
            Log.Add("run badlump");
            return 0;
        }

        Log.Add("container " + Container + " maxiwad " + MaxIwad);
        if (Container == MaxIwad && ShowEndoom == 2)
        {
            Log.Add("run pwadonly");
            return 0;
        }

        Log.Add("stopmusic 1");
        Construct();
        Log.Add("render force=1");
        while (true)
        {
            Log.Add("getevent");
            if (GetEvent == null)
                throw new InvalidOperationException("I_GetEvent");
            GetEvent(this);
            Update();
            while (EventTail != EventHead)
            {
                byte type = EvType[EventTail];
                byte sub = EvSub[EventTail];
                EventTail = (EventTail + 1) & (MaxEvents - 1);
                Log.Add("ev type=" + type + " sub=" + sub + " tail=" + EventTail);
                if (type == EvKeyDown || type == EvKeyUp)
                {
                    Log.Add("run key");
                    return 0;
                }

                if (type == EvGui && (sub == GuiKeyDown || sub == GuiLButtonDown || sub == GuiRButtonDown || sub == GuiMButtonDown))
                {
                    Log.Add("run gui");
                    return 0;
                }
            }
        }
    }

    public void Update()
    {
        if (Blinking && Now > Last + (ulong)BlinkPeriod)
        {
            Last = Now;
            Log.Add("blinkdraw on=" + (BlinkState ? 1 : 0) + " time=" + Last);
            BlinkState = !BlinkState;
            Log.Add("clean");
            Log.Add("render force=1");
        }
        else
        {
            Log.Add("blink hold now=" + Now + " last=" + Last);
        }
    }

    void Construct()
    {
        Blinking = true;
        BlinkState = false;
        Log.Add("read lump=" + Lump);
        Log.Add("bitmap " + (80 * 8) + "x" + (26 * 16));
        Log.Add("drawtext");
        Log.Add("clear x=0 y=" + (25 * 16) + " w=640 h=16 b=0 g=0 r=0 a=255");
        Log.Add("string TXT_QUITENDOOM [" + QuitText + "]");
        Log.Add("drawstring x=0 y=25 [" + QuitText + "] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255");
        Last = Now;
        Log.Add("time " + Last);
        int saw = 0;
        for (int i = 0; i < 80 * 25; i++)
        {
            if ((Screen[1 + i * 2] & 0x80) != 0)
            {
                Blinking = true;
                saw = 1;
                break;
            }
        }

        Log.Add("blink flag=" + (Blinking ? 1 : 0) + " saw=" + saw);
    }

    void LogLookup()
    {
        Log.Add("lookup \"" + Name + "\" trynormal=1 lump=" + Lump + " len=" + Length);
    }
}
