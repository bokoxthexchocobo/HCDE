namespace HCDE.Playsim;

/// <summary>
/// Hexen startup screen from <c>startscreen_hexen.cpp</c> (<b>1–203</b>). Chunk <b>1234</b>.
/// STARTUP, NETNOTCH, and NOTCH are looked up in namespace 15 and, only on lump -1,
/// in the global namespace. The background must be 640×480. Progress enters while
/// CurPos is less than or equal to MaxPos, and each new notch index is blitted at
/// x = 64 + width * index, y = 441, before the budget stretch. A backwards notch
/// still cleans and plays StartupTick. DoNetProgress calls base NetProgress, which
/// virtual-calls this method again, so the net blit after that call does not run.
/// </summary>
public sealed class VmHexenStart1234
{
    public const int NsGraphics = 15;

    public readonly List<string> Log = new();
    public int MaxPos;
    public int CurPos;
    public int NotchPos;
    public int NetCurPos;
    public int NetMaxPos;
    public int NotchW;
    public bool Failed;

    public static VmHexenStart1234 Create(
        int maxProgress,
        int startupNs, int startupAny,
        int netNs, int netAny,
        int notchNs, int notchAny,
        bool hasBg, bool hasNet, bool hasNotch,
        int bgW, int bgH, int netW, int netH, int notchW, int notchH,
        bool batch, string song)
    {
        var screen = new VmHexenStart1234();
        screen.MaxPos = maxProgress;
        int startup = screen.Resolve("STARTUP", startupNs, startupAny);
        int net = screen.Resolve("NETNOTCH", netNs, netAny);
        int notch = screen.Resolve("NOTCH", notchNs, notchAny);
        if (startup < 0 || net < 0 || notch < 0)
        {
            screen.Fail();
            return screen;
        }

        bool bgOk = screen.Image("STARTUP", startup, hasBg);
        bool netOk = screen.Image("NETNOTCH", net, hasNet);
        bool notchOk = screen.Image("NOTCH", notch, hasNotch);
        if (!bgOk || !netOk || !notchOk || bgW != 640 || bgH != 480)
        {
            screen.Fail();
            return screen;
        }

        screen.Log.Add("cache net style=0 w=" + netW + " h=" + netH);
        screen.NotchW = notchW;
        screen.Log.Add("cache notch style=0 w=" + notchW + " h=" + notchH);
        screen.Log.Add("cache bg style=0 w=" + bgW + " h=" + bgH);
        screen.Log.Add("bitmap 640x480");
        screen.Log.Add("blit bg x=0 y=0 w=640 h=480");
        if (!batch)
        {
            if (song.Length != 0)
                screen.Log.Add("music " + song + " order=1 looping=1 force=0");
            else
                screen.Log.Add("music orb order=1 looping=1 force=0");
        }
        else
        {
            screen.Log.Add("music skip");
        }

        screen.Log.Add("header");
        screen.Log.Add("countdown");
        return screen;
    }

    public bool DoProgress(int advance)
    {
        if (CurPos <= MaxPos)
        {
            int num = (16 * 32) / NotchW;
            int notch = ((CurPos + 1) * num) / MaxPos;
            Log.Add("notch calc=" + notch + " prev=" + NotchPos + " num=" + num);
            if (notch != NotchPos)
            {
                while (NotchPos < notch)
                {
                    int x = 64 + NotchW * NotchPos;
                    Log.Add("blit notch x=" + x + " y=441");
                    NotchPos++;
                }

                Log.Add("clean full=1");
                Log.Add("sound StartupTick");
            }
            else
            {
                Log.Add("notch hold");
            }
        }
        else
        {
            Log.Add("notch skip");
        }

        return BaseProgress(advance);
    }

    public void DoNetProgress(int count)
    {
        Log.Add("net old=" + NetCurPos + " count=" + count);
        // FStartScreen::NetProgress calls this override again, so the blit below it never runs.
        Log.Add("net recurse");
    }

    public void NetDone()
    {
        Log.Add("sound PickupWeapon");
        Log.Add("netdone");
    }

    int Resolve(string name, int nsLump, int anyLump)
    {
        Log.Add("lookup " + name + " ns=" + NsGraphics + " lump=" + nsLump);
        if (nsLump == -1)
        {
            Log.Add("lookup " + name + " ns=0 lump=" + anyLump);
            return anyLump;
        }

        return nsLump;
    }

    bool Image(string name, int lump, bool has)
    {
        Log.Add("image " + name + " lump=" + lump + " false");
        return has;
    }

    void Fail()
    {
        Failed = true;
        Log.Add("error Start screen assets missing");
    }

    bool BaseProgress(int advance)
    {
        if (advance > 0 && advance == 1 && MaxPos > 0)
        {
            int headroom = MaxPos / 8;
            if (headroom < 1024)
                headroom = 1024;
            if (CurPos + advance >= MaxPos - headroom)
            {
                int grow = MaxPos / 2;
                if (grow < 2048)
                    grow = 2048;
                MaxPos += grow;
                Log.Add("stretch max=" + MaxPos);
            }
        }

        int sum = CurPos + advance;
        CurPos = sum < MaxPos ? sum : MaxPos;
        Log.Add("base adv=" + advance + " cur=" + CurPos + " max=" + MaxPos + " done=1");
        return true;
    }
}
