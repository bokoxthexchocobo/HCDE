using HCDE.MapLoader;

namespace HCDE.Playsim;

internal sealed class PlaneWaggle
{
    internal int SectorIndex { get; init; }
    internal bool Ceiling { get; init; }
    internal double OriginalHeight { get; init; }
    internal double Accumulator { get; set; }
    internal double AccDelta { get; init; }
    internal double TargetScale { get; init; }
    internal double Scale { get; set; }
    internal double ScaleDelta { get; init; }
    internal int Ticker { get; set; }
    internal int State { get; set; } = 1;
}

/// <summary>Native <c>DWaggleBase</c> / <c>EV_StartWaggle</c> from <c>a_floor.cpp</c>.</summary>
internal static class WaggleActions
{
    private const int StateExpand = 1;
    private const int StateStable = 2;
    private const int StateReduce = 3;
    private const int TicRate = 35;

    internal static bool Start(AuthoritySimulation sim, LevelLine? line, int tag, int height, int speed, int offset, int timer, bool ceiling)
    {
        var started = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.PlaneWaggles.Any(w => w.SectorIndex == sector && w.Ceiling == ceiling)) continue;
            if (sim.Motions.Any(m => m.SectorIndex == sector && (ceiling ? m.MovesCeiling : !m.MovesCeiling && !m.Completed))) continue;
            var targetScale = height / 64.0;
            sim.PlaneWaggles.Add(new PlaneWaggle
            {
                SectorIndex = sector,
                Ceiling = ceiling,
                OriginalHeight = ceiling ? sim.Ceilings[sector] : sim.Floors[sector],
                Accumulator = offset,
                AccDelta = speed / 64.0,
                TargetScale = targetScale,
                ScaleDelta = targetScale / (TicRate + (3 * TicRate * height) / 255.0),
                Ticker = timer > 0 ? timer * TicRate : -1,
            });
            started = true;
        }
        return started;
    }

    internal static void Tick(AuthoritySimulation sim)
    {
        for (var i = sim.PlaneWaggles.Count - 1; i >= 0; i--)
        {
            var waggle = sim.PlaneWaggles[i];
            var sector = waggle.SectorIndex;
            switch (waggle.State)
            {
                case StateExpand:
                    waggle.Scale += waggle.ScaleDelta;
                    if (waggle.Scale >= waggle.TargetScale)
                    {
                        waggle.Scale = waggle.TargetScale;
                        waggle.State = StateStable;
                    }
                    break;
                case StateReduce:
                    waggle.Scale -= waggle.ScaleDelta;
                    if (waggle.Scale <= 0)
                    {
                        if (waggle.Ceiling) sim.Ceilings[sector] = waggle.OriginalHeight;
                        else sim.Floors[sector] = waggle.OriginalHeight;
                        sim.PlaneWaggles.RemoveAt(i);
                        continue;
                    }
                    break;
                case StateStable:
                    if (waggle.Ticker != -1 && --waggle.Ticker == 0) waggle.State = StateReduce;
                    break;
            }
            waggle.Accumulator += waggle.AccDelta;
            var offset = BobSin(waggle.Accumulator) * waggle.Scale;
            if (waggle.Ceiling) sim.Ceilings[sector] = waggle.OriginalHeight + offset;
            else sim.Floors[sector] = waggle.OriginalHeight + offset;
        }
    }

    private static double BobSin(double fb) => Math.Sin(fb * (Math.PI / 32.0)) * 8.0;
}
