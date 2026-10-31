namespace HCDE.Playsim;

/// <summary>
/// Strife startup screen from <c>startscreen_strife.cpp</c> (<b>1–186</b>). Chunk <b>1233</b>.
/// Eight startup lumps are looked up in the graphics namespace, then the global
/// namespace. STARTUP0 must be 320×200. The laser notch uses CurPos + 1 times 184
/// and paints only on a new even value. DrawStuff ignores the old laser: the
/// background blit covers it, the laser frame is the low bit, and the bot hop is
/// max(0, (laser &gt;&gt; 1) % 5 - 2).
/// </summary>
public sealed class VmStrifeStart1233
{
    public const int NsGraphics = 15;
    public static readonly string[] Names =
    {
        "STRTPA1", "STRTPB1", "STRTPC1", "STRTPD1",
        "STRTLZ1", "STRTLZ2", "STRTBOT", "STARTUP0",
    };

    public readonly List<string> Log = new();
    public readonly int[] Width = new int[8];
    public readonly int[] Height = new int[8];
    public int MaxPos;
    public int CurPos;
    public int NotchPos;
    public int Scale;
    public bool Failed;

    public static VmStrifeStart1233 Create(int maxProgress, int[] nsLump, int[] anyLump, bool[] hasImage, int[] width, int[] height)
    {
        var screen = new VmStrifeStart1233();
        screen.MaxPos = maxProgress;
        for (int i = 0; i < Names.Length; i++)
        {
            screen.Log.Add("lookup " + Names[i] + " ns=" + NsGraphics + " lump=" + nsLump[i]);
            int lump = nsLump[i];
            if (lump < 0)
            {
                screen.Log.Add("lookup " + Names[i] + " ns=0 lump=" + anyLump[i]);
                lump = anyLump[i];
            }

            if (lump >= 0)
            {
                screen.Log.Add("image lump=" + lump + " false");
                if (hasImage[i])
                {
                    screen.Width[i] = width[i];
                    screen.Height[i] = height[i];
                    screen.Log.Add("cache style=0 w=" + width[i] + " h=" + height[i]);
                }
            }
        }

        screen.Log.Add("assets w=" + screen.Width[7] + " h=" + screen.Height[7]);
        if (screen.Width[7] != 320 || screen.Height[7] != 200)
        {
            screen.Failed = true;
            screen.Log.Add("error bad startscreen assets");
            return screen;
        }

        screen.DrawStuff(0, 0);
        screen.Scale = 2;
        screen.Log.Add("scale 2");
        screen.Log.Add("header");
        screen.Log.Add("countdown");
        return screen;
    }

    public bool DoProgress(int advance)
    {
        if (CurPos < MaxPos)
        {
            int notch = ((CurPos + 1) * (200 - 16)) / MaxPos;
            Log.Add("notch calc=" + notch + " prev=" + NotchPos);
            if (notch != NotchPos && (notch & 1) == 0)
            {
                DrawStuff(NotchPos, notch);
                NotchPos = notch;
                Log.Add("clean full=1");
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

    public void DrawStuff(int oldLaser, int newLaser)
    {
        Log.Add("draw old=" + oldLaser + " new=" + newLaser);
        Log.Add("blit x=0 y=0 pic=7");
        int laser = 4 + (newLaser & 1);
        Log.Add("blit x=" + (60 + newLaser) + " y=156 pic=" + laser);
        int hop = (newLaser >> 1) % 5 - 2;
        if (hop < 0)
            hop = 0;
        Log.Add("blit x=14 y=" + (138 + hop) + " pic=6");
        int peasant = (newLaser >> 1) & 3;
        Log.Add("blit x=262 y=136 pic=" + peasant);
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
