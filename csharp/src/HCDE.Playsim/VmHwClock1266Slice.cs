using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Hardware profile lines and the bench command from <c>hw_clock.cpp</c> (<b>1–199</b>).
/// Chunk <b>1266</b>. Reset clears the cycles it names and then clocks <c>All</c> only when
/// profiling is active. Finish, the 2D pair, and the command-buffer count stay. The BSP
/// field is the BSP cycle minus the clip cycle. Render-time text is cached until more than
/// one second has passed. Bench turns the FPS counter on when it was off, hides the console,
/// and the save still prints when the benchmark file cannot be opened.
/// </summary>
public sealed class VmHwClock1266
{
    public readonly List<string> Log = new();
    public bool Active;
    public double ClockSample;
    public long NowMs;
    public bool ConsoleUp;
    public bool VidFps;
    public bool StatActive;
    public bool StatFound = true;
    public string Location = "";
    public bool FileOpens = true;
    public int PrintStats;
    public bool SwitchFps;
    public long WaitStart;

    public double All, Finish, PortalAll, Bsp, ProcessAll, PostProcess, RenderAll;
    public double RenderWall, SetupWall, ClipWall, RenderFlat, SetupFlat, RenderSprite, SetupSprite;
    public double DrawCalls, TwoD, Flush3D, MtWait, WtTotal;
    public int VertexCount, FlatVertices, FlatPrimitives;
    public int RenderedLines, RenderedFlats, RenderedSprites, VertexSplit, TexSplit, RenderedDecals, RenderedPortals, CommandBuffers;
    public int IterDLightF, IterDLight, DrawDLight, DrawDLightF;

    string _cachedTimes = "";
    long _lastTimesMs;

    public void ResetProfiling()
    {
        All = 0;
        if (Active)
        {
            All -= ClockSample;
            Log.Add("clock All");
        }

        Bsp = PortalAll = RenderAll = ProcessAll = PostProcess = 0;
        RenderWall = SetupWall = ClipWall = 0;
        RenderFlat = SetupFlat = 0;
        RenderSprite = SetupSprite = 0;
        DrawCalls = MtWait = WtTotal = 0;
        FlatVertices = FlatPrimitives = VertexCount = 0;
        TexSplit = VertexSplit = RenderedLines = RenderedFlats = RenderedSprites = RenderedDecals = RenderedPortals = 0;
        Log.Add("reset");
    }

    public string RenderTimes()
    {
        if (NowMs - _lastTimesMs > 1000)
        {
            _cachedTimes = FormatTimes();
            _lastTimesMs = NowMs;
            Log.Add("refresh");
        }
        else
        {
            Log.Add("cached");
        }

        return _cachedTimes;
    }

    public string RenderStats() => FormatStats();

    public string LightStats() => FormatLights();

    public void CheckBench()
    {
        if (PrintStats == 0 || !ConsoleUp)
        {
            Log.Add("bench idle");
            return;
        }

        if (WaitStart > 0 && NowMs - WaitStart < 5000)
        {
            Log.Add("bench wait");
            return;
        }

        string compose = Location;
        compose += FormatStats();
        compose += FormatTimes();
        compose += FormatLights();
        compose += "\n\n\n";
        if (FileOpens)
            Log.Add("write " + compose.Replace("\n", "\\n"));
        else
            Log.Add("write fail");
        Log.Add("saved");
        if (SwitchFps)
            VidFps = false;
        PrintStats = 0;
    }

    public void Bench()
    {
        PrintStats = 1;
        if (!VidFps)
        {
            VidFps = true;
            WaitStart = NowMs;
            SwitchFps = true;
            Log.Add($"bench on {WaitStart}");
        }
        else
        {
            if (ConsoleUp)
                WaitStart = NowMs;
            SwitchFps = false;
            Log.Add($"bench keep {WaitStart} {(ConsoleUp ? 1 : 0)}");
        }

        Log.Add("hide");
    }

    public void CheckActive()
    {
        Active = (StatFound && StatActive) || PrintStats != 0;
        Log.Add(Active ? "active" : "quiet");
    }

    string FormatTimes()
    {
        double setupWall = SetupWall;
        double clipWall = ClipWall;
        double bsp = Bsp - ClipWall;
        return
            $"BSP = {F3(bsp)}, Clip={F3(clipWall)}\n" +
            $"W: Render={F3(RenderWall)}, Setup={F3(setupWall)}\n" +
            $"F: Render={F3(RenderFlat)}, Setup={F3(SetupFlat)}\n" +
            $"S: Render={F3(RenderSprite)}, Setup={F3(SetupSprite)}\n" +
            $"2D: {F3(TwoD)} Finish3D: {F3(Flush3D - TwoD)}\n" +
            $"Main thread total={F3(MtWait + Bsp)}, Main thread waiting={F3(MtWait)} Worker thread total={F3(WtTotal)}, Worker thread waiting={F3(WtTotal - setupWall - SetupFlat - SetupSprite)}\n" +
            $"All={F3(All + Finish)}, Render={F3(RenderAll)}, Setup={F3(ProcessAll)}, Portal={F3(PortalAll)}, Drawcalls={F3(DrawCalls)}, Postprocess={F3(PostProcess)}, Finish={F3(Finish)}\n";
    }

    string FormatStats() =>
        $"Walls: {RenderedLines} ({VertexSplit} splits, {TexSplit} t-splits, {VertexCount} vertices)\n" +
        $"Flats: {RenderedFlats} ({FlatPrimitives} primitives, {FlatVertices} vertices)\n" +
        $"Sprites: {RenderedSprites}, Decals={RenderedDecals}, Portals: {RenderedPortals}, Command buffers: {CommandBuffers}\n";

    string FormatLights() =>
        $"DLight - Walls: {IterDLight} processed, {DrawDLight} rendered - Flats: {IterDLightF} processed, {DrawDLightF} rendered\n";

    static string F3(double value) => value.ToString("0.000", CultureInfo.InvariantCulture);
}
