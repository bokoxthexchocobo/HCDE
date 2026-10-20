namespace HCDE.Client;

/// <summary><c>CalcFps</c> frame accounting from <c>d_main.cpp</c> (1041–1060).</summary>
public static class VmDMainCalcFps
{
    public readonly struct FpsState
    {
        public FpsState(ulong lastMs, uint lastSec, ulong frameCount)
        {
            LastMs = lastMs;
            LastSec = lastSec;
            FrameCount = frameCount;
        }

        public ulong LastMs { get; }
        public uint LastSec { get; }
        public ulong FrameCount { get; }
    }

    public static (FpsState state, ulong lastMsCount, ulong lastFps, bool updatedFrame) Tick(
        FpsState state,
        ulong frameMs,
        double timeScale)
    {
        var howLong = (ulong)((frameMs - state.LastMs) / timeScale);
        if (howLong == 0)
            return (state, 0, 0, false);

        var thisSec = (uint)(frameMs / 1000);
        var frameCount = state.FrameCount + 1;
        ulong lastFps = 0;
        var lastSec = state.LastSec;
        if (lastSec < thisSec)
        {
            lastFps = frameCount / (thisSec - lastSec) * (ulong)timeScale;
            lastSec = thisSec;
            frameCount = 0;
        }

        return (new FpsState(frameMs, lastSec, frameCount), howLong, lastFps, true);
    }
}
