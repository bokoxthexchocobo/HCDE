using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Hardware 2D command walk from <c>hw_draw2d.cpp</c> (1-255). Chunk 1276.
/// Upload, the viewpoint, and the screen buffer are host seams. The first pass
/// swaps BGRA once. An empty list unclocks before that swap.
/// </summary>
public sealed class VmDraw2dVert1276
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}

public sealed class VmDraw2dTex1276
{
    public int Id;
    public int UseType;
    public bool HardwareCanvas;
}

public sealed class VmDraw2dShape1276
{
    public readonly List<int> Buffers = new();
    public int BufIndex;
    public int LastCommand;
    public bool NeedsVertexUpload;
    public bool UploadedOnce = true;
}

public sealed class VmDraw2dCmd1276
{
    public int Special;
    public bool StencilOn;
    public int StencilOffs;
    public int StencilOp;
    public int StencilFlags;
    public int Type;
    public int VertIndex;
    public int VertCount;
    public int IndexIndex;
    public int IndexCount;
    public VmDraw2dTex1276? Texture;
    public int Translation;
    public uint Map0;
    public uint Map1;
    public int Sc0, Sc1, Sc2, Sc3;
    public int Desaturate;
    public int Style;
    public int StyleFlags;
    public uint Color1;
    public int DrawMode;
    public int LightLevel;
    public int Flags;
    public float ScreenFade = 1f;
    public bool UseTransform;
    public double C00, C01, C02, C10, C11, C12;
    public VmDraw2dShape1276? Shape;
    public int ShapeBufIndex;
    public int ShapeIndexCount;
    public int ShapeCounter;
}

public sealed class VmDraw2d1276
{
    public bool FirstPass = true;
    public bool ProfileActive;
    public long Cycle;
    public long SampleSeq;
    public bool Hw2dMip = true;
    public bool HasAnisoCallback;
    public bool AnisoDisables;
    public int AnisoReads;
    public bool SmoothLines;
    public int ViewportLeft;
    public int ViewportTop;
    public int ViewportWidth;
    public int ViewportHeight;
    public int GameWidth = 320;
    public int GameHeight = 200;
    public int DrawerWidth;
    public int DrawerHeight;
    public readonly List<VmDraw2dVert1276> Vertices = new();
    public readonly List<int> Indices = new();
    public readonly List<VmDraw2dCmd1276> Commands = new();
    public readonly List<string> Log = new();

    public void DrawViewport()
    {
        Draw(ViewportLeft, ViewportTop, ViewportWidth, ViewportHeight);
    }

    public void Draw(int x, int y, int width, int height)
    {
        Clock();
        Log.Add($"viewport {x} {y} {width} {height}");
        Log.Add($"set2d {DrawerWidth} {DrawerHeight}");
        Log.Add("stencil off");
        Log.Add("stencil 0 0 0");
        Log.Add("clear 2");
        Log.Add("depth off");
        Log.Add("msaa off");
        Log.Add(SmoothLines ? "smooth 1" : "smooth 0");
        bool cacheMip = CacheMip();
        Log.Add(cacheMip ? "mip 1" : "mip 0");
        if (Commands.Count == 0)
        {
            Unclock();
            return;
        }
        if (FirstPass)
        {
            foreach (var vert in Vertices)
            {
                byte red = vert.R;
                vert.R = vert.B;
                vert.B = red;
            }
            Log.Add("swap " + string.Join(" ", Vertices.Select(FormatVert)));
        }
        Log.Add($"upload {Vertices.Count} {Indices.Count}");
        Log.Add("vb upload");
        Log.Add("fog 0");
        foreach (var cmd in Commands)
        {
            if (cmd.Special != 0)
            {
                ApplySpecial(cmd);
                continue;
            }
            Walk(cmd, cacheMip);
            Log.Add("obj ffffffff");
            Log.Add("obj2 00000000");
            Log.Add("add 00000000");
            Log.Add("texmat off");
            Log.Add("model off");
            Log.Add("effect -1");
        }
        Log.Add("scissor -1 -1 -1 -1");
        Log.Add("style 6");
        Log.Add("vb screen");
        Log.Add("stencil off");
        Log.Add("stencil 0 0 0");
        Log.Add("tex on");
        Log.Add("bright on");
        Log.Add("mode 0");
        Log.Add("fog 0");
        Log.Add("fade " + Num(1f));
        Log.Add("soft 255 0");
        Log.Add("reset");
        FirstPass = false;
        Unclock();
    }

    void ApplySpecial(VmDraw2dCmd1276 cmd)
    {
        if (cmd.Special == 1)
            Log.Add(cmd.StencilOn ? "special stencil 1" : "special stencil 0");
        else if (cmd.Special == 2)
            Log.Add($"special set {cmd.StencilOffs} {cmd.StencilOp} {cmd.StencilFlags}");
        else if (cmd.Special == 3)
            Log.Add("special clear 2");
        else
            Log.Add("special other");
    }

    void Walk(VmDraw2dCmd1276 cmd, bool cacheMip)
    {
        Log.Add($"style {cmd.Style} {cmd.StyleFlags}");
        Log.Add((cmd.StyleFlags & 8) == 0 ? "bright 1" : "bright 0");
        Log.Add("fog 2");
        Log.Add("fade " + Num(cmd.ScreenFade));
        Log.Add($"mode {cmd.DrawMode}");
        int sciX, sciY, sciW, sciH;
        if ((cmd.Flags & 2) != 0)
        {
            sciX = ScreenToWindowX(cmd.Sc0);
            sciY = ScreenToWindowY(cmd.Sc3);
            sciW = ScreenToWindowX(cmd.Sc2) - sciX;
            sciH = ScreenToWindowY(cmd.Sc1) - sciY;
            if (sciX < 0)
            {
                sciW += sciX;
                sciX = 0;
            }
            if (sciY < 0)
            {
                sciH += sciY;
                sciY = 0;
            }
        }
        else
        {
            sciX = sciY = sciW = sciH = -1;
        }
        Log.Add($"scissor {sciX} {sciY} {sciW} {sciH}");
        if (((cmd.Map0 >> 24) & 0xff) != 0)
        {
            Log.Add("mode 7");
            Log.Add("obj " + Hex(cmd.Map0));
            Log.Add("add " + Hex(cmd.Map1));
        }
        Log.Add("fogc " + Hex(cmd.Color1) + " " + Num(FogFactor(0f)));
        Log.Add("color 1 1 1 1 " + Num(cmd.Desaturate * (1f / 255f)));
        if ((cmd.Flags & 8) != 0)
            Log.Add($"soft {cmd.LightLevel} 0");
        Log.Add("light 0 0");
        Log.Add("alpha 1 " + Num(AlphaThresh(1, 0f)));
        if (cmd.UseTransform)
        {
            Log.Add("matrix " + string.Join(" ", BuildModel(cmd).Select(Num)));
            Log.Add("model on");
        }
        var tex = cmd.Texture;
        if (tex != null && tex.UseType != 13)
        {
            int flags = tex.UseType >= 15 ? 0 : tex.UseType == 9 ? 4 : 1;
            int scaleFlags = (cmd.Flags & 8) != 0 ? 4 : 0;
            int clamp = (cmd.Flags & 1) != 0 ? 0 : cacheMip ? 3 : 4;
            Log.Add($"mat {tex.Id} {flags} {scaleFlags} {clamp} {cmd.Translation} -1");
            Log.Add("tex on");
            if (tex.HardwareCanvas)
            {
                Log.Add("texmat " + string.Join(" ", CanvasMatrix().Select(Num)));
                Log.Add("texmat on");
            }
            if ((cmd.Flags & 4) != 0)
                Log.Add("effect 2");
        }
        else
        {
            Log.Add("tex off");
        }
        if (cmd.Shape != null)
            DrawShape(cmd);
        else if (cmd.Type == 1)
            Log.Add($"draw 1 {cmd.VertIndex} {cmd.VertCount}");
        else if (cmd.Type == 2)
            Log.Add($"draw 0 {cmd.VertIndex} {cmd.VertCount}");
        else
            Log.Add($"drawidx 2 {cmd.IndexIndex} {cmd.IndexCount}");
    }

    void DrawShape(VmDraw2dCmd1276 cmd)
    {
        var info = cmd.Shape!;
        Log.Add($"vb shape {info.Buffers[cmd.ShapeBufIndex]}");
        Log.Add($"drawidx 2 0 {cmd.ShapeIndexCount}");
        Log.Add("vb upload");
        if (cmd.ShapeCounter == info.LastCommand)
        {
            info.LastCommand = -1;
            if (info.BufIndex > 0)
            {
                info.NeedsVertexUpload = true;
                info.Buffers.Clear();
                info.BufIndex = -1;
            }
        }
        info.UploadedOnce = false;
        Log.Add($"shape {info.LastCommand} {info.BufIndex} {(info.NeedsVertexUpload ? 1 : 0)} 0 {info.Buffers.Count}");
    }

    bool CacheMip()
    {
        if (!Hw2dMip)
            return false;
        if (!HasAnisoCallback)
            return true;
        AnisoReads++;
        return !AnisoDisables;
    }

    void Clock()
    {
        Log.Add("clock");
        if (ProfileActive)
            Cycle -= NextSample();
    }

    void Unclock()
    {
        Log.Add("unclock");
        if (ProfileActive)
            Cycle += NextSample();
    }

    long NextSample()
    {
        SampleSeq++;
        return SampleSeq;
    }

    int ScreenToWindowX(int x)
    {
        return ViewportLeft + RoundAway(x * ViewportWidth / (float)GameWidth);
    }

    int ScreenToWindowY(int y)
    {
        return ViewportTop + ViewportHeight - RoundAway(y * ViewportHeight / (float)GameHeight);
    }

    static int RoundAway(float value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    static float FogFactor(float distance)
    {
        const float log2E = 1.442692f;
        if (distance >= 0f)
            return distance * (-log2E / 64000f);
        return distance;
    }

    static float AlphaThresh(int func, float thresh)
    {
        if (func == 1)
            return thresh;
        return thresh - 0.001f;
    }

    static float[] BuildModel(VmDraw2dCmd1276 cmd)
    {
        var cells = new double[2, 3]
        {
            { cmd.C00, cmd.C01, cmd.C02 },
            { cmd.C10, cmd.C11, cmd.C12 },
        };
        var m = new float[16];
        m[10] = 1f;
        m[15] = 1f;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
                m[4 * j + i] = (float)cells[i, j];
        }
        for (int i = 0; i < 2; i++)
            m[12 + i] = (float)cells[i, 2];
        return m;
    }

    static float[] CanvasMatrix()
    {
        var m = new float[16];
        for (int i = 0; i < 4; i++)
            m[i * 5] = 1f;
        for (int i = 0; i < 4; i++)
            m[i] *= 1f;
        for (int i = 0; i < 4; i++)
            m[4 + i] *= -1f;
        for (int i = 0; i < 4; i++)
            m[8 + i] *= 1f;
        m[12] = m[0] * 0f + m[4] * 1f + m[8] * 0f + m[12];
        m[13] = m[1] * 0f + m[5] * 1f + m[9] * 0f + m[13];
        m[14] = m[2] * 0f + m[6] * 1f + m[10] * 0f + m[14];
        return m;
    }

    static string FormatVert(VmDraw2dVert1276 vert) => $"{vert.R:x2}{vert.G:x2}{vert.B:x2}{vert.A:x2}";

    static string Hex(uint value) => value.ToString("x8");

    static string Num(float value) => value.ToString("G9", CultureInfo.InvariantCulture);
}
