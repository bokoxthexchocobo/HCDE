namespace HCDE.Playsim;

/// <summary>
/// Heretic startup screen from <c>startscreen_heretic.cpp</c> (<b>1–172</b>). Chunk <b>1232</b>.
/// LOADING must be 4000 bytes. Column 49 of text row 2 is forced to the character 3.
/// The thermometer is 404 pixels wide at (112, 224) and paints light green only when
/// the new notch differs and is a multiple of 4. That notch uses CurPos + 1, then
/// the base progress budget runs. Status text is byte-wise and always cleans.
/// </summary>
public sealed class VmHereticStart1232
{
    public const int ThermX = 14 * 8;
    public const int ThermY = 14 * 16;
    public const int ThermWidth = 51 * 8 - 4;
    public const int ThermHeight = 16;
    public const byte MinorVersion = (byte)'3';

    public readonly List<string> Log = new();
    public readonly byte[] Screen = new byte[4000];
    public int MaxPos;
    public int CurPos;
    public int NotchPos;
    public int HMsgY = 7;
    public int SMsgX = 1;
    public bool Failed;

    public static VmHereticStart1232 Create(int maxProgress, int lump, int length, byte[]? file)
    {
        var screen = new VmHereticStart1232();
        screen.MaxPos = maxProgress;
        screen.Log.Add("lookup LOADING lump=" + lump + " len=" + length);
        if (lump < 0 || length != 4000)
        {
            screen.Failed = true;
            screen.Log.Add("error 'LOADING' not found");
            return screen;
        }

        if (file != null)
        {
            int n = file.Length < 4000 ? file.Length : 4000;
            for (int i = 0; i < n; i++)
                screen.Screen[i] = file[i];
        }

        screen.Log.Add("read lump=" + lump);
        int index = 2 * 160 + 49 * 2;
        screen.Screen[index] = MinorVersion;
        screen.Log.Add("patch index=" + index + " byte=" + screen.Screen[index]);
        screen.Log.Add("bitmap " + (80 * 8) + "x" + (25 * 16));
        screen.Log.Add("drawtext");
        screen.Log.Add("therm x=" + ThermX + " y=" + ThermY + " w=" + ThermWidth + " h=" + ThermHeight + " msgY=" + screen.HMsgY + " msgX=" + screen.SMsgX + " notch=" + screen.NotchPos);
        screen.Log.Add("header");
        screen.Log.Add("countdown");
        return screen;
    }

    public bool DoProgress(int advance)
    {
        if (CurPos < MaxPos)
        {
            int notch = ((CurPos + 1) * ThermWidth) / MaxPos;
            Log.Add("notch calc=" + notch + " prev=" + NotchPos);
            if (notch != NotchPos && (notch & 3) == 0)
            {
                int left = NotchPos + ThermX;
                int top = ThermY;
                int right = notch + ThermX;
                int bottom = top + ThermHeight;
                Log.Add("clear x=" + left + " y=" + top + " w=" + (right - left) + " h=" + (bottom - top) + " b=85 g=255 r=85 a=255");
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

    public void Loading(string message, int colors)
    {
        byte color = (byte)colors;
        int y = HMsgY;
        for (int x = 0; x < message.Length; x++)
        {
            int cell = message[x] & 0xFF;
            Log.Add("char x=" + (17 + x) + " y=" + y + " byte=" + cell + " color=" + color);
        }

        Log.Add("load y=" + y + " next=" + (y + 1));
        HMsgY = y + 1;
        Log.Add("clean full=1");
        Log.Add("render force=0");
    }

    public void Append(string status)
    {
        int x0 = SMsgX;
        for (int x = 0; x < status.Length; x++)
        {
            int cell = status[x] & 0xFF;
            Log.Add("char x=" + (x0 + x) + " y=24 byte=" + cell + " color=31");
        }

        Log.Add("append x=" + x0 + " next=" + (x0 + status.Length));
        SMsgX = x0 + status.Length;
        Log.Add("clean full=1");
        Log.Add("render force=0");
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
