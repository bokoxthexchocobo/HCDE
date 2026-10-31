namespace HCDE.Playsim.Tests;

public class VmALightsThinkers1164SliceTests
{
    [Fact]
    public void FlickerAndFlash_ClampArgsAndSwapWhenTheCountHitsZero()
    {
        var flicker = VmALightsThinkers1164Slice.ConstructFlicker(40000, -40000, 64);
        Assert.Equal(short.MaxValue, flicker.Max);
        Assert.Equal(short.MinValue, flicker.Min);
        Assert.Equal(short.MaxValue, flicker.Light);
        Assert.Equal(65, flicker.Count);
        Assert.Equal(1, VmALightsThinkers1164Slice.ConstructFlicker(10, 2, 0).Count);

        var fromSector = VmALightsThinkers1164Slice.ConstructFlashFromSector(128, 40, 0);
        Assert.Equal(128, fromSector.Max);
        Assert.Equal(40, fromSector.Min);
        Assert.Equal(64, fromSector.MaxTime);
        Assert.Equal(7, fromSector.MinTime);
        Assert.Equal(1, fromSector.Count);

        var fromArgs = VmALightsThinkers1164Slice.ConstructFlashFromArgs(-1, 300, 64);
        Assert.Equal(300, fromArgs.Max);
        Assert.Equal(-1, fromArgs.Min);
        Assert.Equal(65, fromArgs.Count);

        fromArgs.Light = 300;
        fromArgs.Count = 3;
        VmALightsThinkers1164Slice.TickFlash(fromArgs, 255);
        Assert.Equal(2, fromArgs.Count);
        Assert.Equal(300, fromArgs.Light);

        fromArgs.Count = 1;
        VmALightsThinkers1164Slice.TickFlash(fromArgs, 0x7F);
        Assert.Equal(-1, fromArgs.Light);
        Assert.Equal(8, fromArgs.Count);

        fromArgs.Count = 1;
        fromArgs.Light = 10;
        VmALightsThinkers1164Slice.TickFlash(fromArgs, 255);
        Assert.Equal(300, fromArgs.Light);
        Assert.Equal(65, fromArgs.Count);

        var wide = VmALightsThinkers1164Slice.ConstructFlashFromArgs(0, 200, 0);
        wide.Min = 40000;
        wide.Light = 200;
        wide.Count = 1;
        VmALightsThinkers1164Slice.TickFlash(wide, 0);
        Assert.Equal(short.MaxValue, wide.Light);
    }

    [Fact]
    public void Strobe_StaysInSyncForHexenAndZerosAFlatDoomLight()
    {
        var hexen = VmALightsThinkers1164Slice.ConstructStrobeHexen(40000, -5, 8, 15);
        Assert.Equal(short.MaxValue, hexen.Max);
        Assert.Equal(-5, hexen.Min);
        Assert.Equal(8, hexen.BrightTime);
        Assert.Equal(15, hexen.DarkTime);
        Assert.Equal(1, hexen.Count);

        var flat = VmALightsThinkers1164Slice.ConstructStrobeDoom(80, 80, 5, 9, true, 9);
        Assert.Equal(80, flat.Max);
        Assert.Equal(0, flat.Min);
        Assert.Equal(1, flat.Count);

        var delayed = VmALightsThinkers1164Slice.ConstructStrobeDoom(80, 10, 5, 9, false, 9);
        Assert.Equal(10, delayed.Min);
        Assert.Equal(2, delayed.Count);
        Assert.Equal(80, delayed.Light);

        delayed.Count = 1;
        delayed.Light = 80;
        VmALightsThinkers1164Slice.TickStrobe(delayed);
        Assert.Equal(10, delayed.Light);
        Assert.Equal(9, delayed.Count);

        delayed.Count = 1;
        VmALightsThinkers1164Slice.TickStrobe(delayed);
        Assert.Equal(80, delayed.Light);
        Assert.Equal(5, delayed.Count);

        delayed.Count = 4;
        VmALightsThinkers1164Slice.TickStrobe(delayed);
        Assert.Equal(3, delayed.Count);
        Assert.Equal(80, delayed.Light);
    }

    [Fact]
    public void Glow_TurnsAtTheEdgeAndInterpolatesAfterThePostIncrement()
    {
        var glow = VmALightsThinkers1164Slice.ConstructGlow(80, 10);
        Assert.Equal(10, glow.Min);
        Assert.Equal(80, glow.Max);
        Assert.Equal(-1, glow.Direction);

        VmALightsThinkers1164Slice.TickGlow(glow);
        Assert.Equal(72, glow.Light);
        Assert.Equal(-1, glow.Direction);

        glow.Light = 10;
        VmALightsThinkers1164Slice.TickGlow(glow);
        Assert.Equal(10, glow.Light);
        Assert.Equal(1, glow.Direction);

        glow.Light = 200;
        glow.Max = 210;
        VmALightsThinkers1164Slice.TickGlow(glow);
        Assert.Equal(208, glow.Light);
        Assert.Equal(1, glow.Direction);

        glow.Light = 206;
        VmALightsThinkers1164Slice.TickGlow(glow);
        Assert.Equal(206, glow.Light);
        Assert.Equal(-1, glow.Direction);

        glow.Direction = 0;
        glow.Light = 40000;
        VmALightsThinkers1164Slice.TickGlow(glow);
        Assert.Equal(short.MaxValue, glow.Light);
        Assert.Equal(0, glow.Direction);

        var fade = VmALightsThinkers1164Slice.ConstructGlow2(0, 10, 4, true);
        Assert.Equal(-1, fade.Tics);
        int[] lights = new int[5];
        for (int i = 0; i < 5; i++)
        {
            VmALightsThinkers1164Slice.TickGlow2(fade);
            lights[i] = fade.Light;
            Assert.False(fade.Destroyed);
        }

        Assert.Equal(new[] { 0, 2, 5, 7, 10 }, lights);
        Assert.Equal(4, fade.Tics);
        VmALightsThinkers1164Slice.TickGlow2(fade);
        Assert.True(fade.Destroyed);
        Assert.Equal(10, fade.Light);
        Assert.Equal(5, fade.Tics);
        Assert.Equal(0, fade.Start);

        var loop = VmALightsThinkers1164Slice.ConstructGlow2(0, 10, 4, false);
        for (int i = 0; i < 5; i++)
            VmALightsThinkers1164Slice.TickGlow2(loop);
        VmALightsThinkers1164Slice.TickGlow2(loop);
        Assert.False(loop.Destroyed);
        Assert.Equal(10, loop.Start);
        Assert.Equal(0, loop.End);
        Assert.Equal(1, loop.Tics);
        Assert.Equal(8, loop.Light);
    }

    [Fact]
    public void Phase_ChainsSpecialsAndWrapsFromZero()
    {
        var sector = new VmALightsThinkers1164Slice.PhaseSector();
        var thinker = new VmALightsThinkers1164Slice.PhaseThinker { Sector = sector, Base = 15, Phase = 0 };
        VmALightsThinkers1164Slice.TickPhase(thinker);
        Assert.Equal(15, sector.Light);
        Assert.Equal(63, thinker.Phase);

        thinker.Phase = 11;
        VmALightsThinkers1164Slice.TickPhase(thinker);
        Assert.Equal(235, sector.Light);
        Assert.Equal(10, thinker.Phase);

        thinker.Phase = 12;
        VmALightsThinkers1164Slice.TickPhase(thinker);
        Assert.Equal(235, sector.Light);
        Assert.Equal(11, thinker.Phase);

        thinker.Phase = 23;
        VmALightsThinkers1164Slice.TickPhase(thinker);
        Assert.Equal(15, sector.Light);
        Assert.Equal(22, thinker.Phase);

        thinker.Phase = 24;
        VmALightsThinkers1164Slice.TickPhase(thinker);
        Assert.Equal(15, sector.Light);
        Assert.Equal(23, thinker.Phase);

        var built = new VmALightsThinkers1164Slice.PhaseThinker { Sector = sector };
        VmALightsThinkers1164Slice.ConstructPhase(built, 300, 300);
        Assert.Equal(44, built.Base);
        Assert.Equal(44, built.Phase);

        var first = new VmALightsThinkers1164Slice.PhaseSector { Light = -1, Special = 3 };
        var second = new VmALightsThinkers1164Slice.PhaseSector { Light = 0, Special = 4 };
        var third = new VmALightsThinkers1164Slice.PhaseSector { Light = 10, Special = 3 };
        var origin = new VmALightsThinkers1164Slice.PhaseThinker { Sector = first, Base = 1, Phase = 99 };
        var world = new VmALightsThinkers1164Slice.PhaseWorld { ValidCount = 4, Origin = origin };
        var seen = new List<int>();
        int steps = VmALightsThinkers1164Slice.Propagate(world, (current, wanted) =>
        {
            seen.Add(wanted);
            if (current == first)
                return second;
            if (current == second)
                return third;
            return null;
        });

        Assert.Equal(5, world.ValidCount);
        Assert.Equal(3, steps);
        Assert.Equal(new[] { 4, 3, 4 }, seen);
        Assert.Equal(255, origin.Base);
        Assert.Equal(42, origin.Phase);
        Assert.False(origin.Spawned);
        Assert.Equal(0, first.Special);
        Assert.Equal(5, first.ValidCount);
        Assert.Equal(2, world.Spawned.Count);
        Assert.Equal(255, world.Spawned[0].Base);
        Assert.Equal(21, world.Spawned[0].Phase);
        Assert.Equal(second, world.Spawned[0].Sector);
        Assert.Equal(0, second.Special);
        Assert.Equal(10, world.Spawned[1].Base);
        Assert.Equal(0, world.Spawned[1].Phase);
        Assert.Equal(0, third.Special);

        var marked = new VmALightsThinkers1164Slice.PhaseSector { Light = 8, Special = 4, ValidCount = 9 };
        var again = new VmALightsThinkers1164Slice.PhaseWorld
        {
            ValidCount = 8,
            Origin = new VmALightsThinkers1164Slice.PhaseThinker { Sector = marked, Phase = 7 },
        };
        int stopped = VmALightsThinkers1164Slice.Propagate(again, (_, _) => throw new InvalidOperationException());
        Assert.Equal(0, stopped);
        Assert.Equal(4, marked.Special);
        Assert.Equal(7, again.Origin.Phase);
        Assert.Empty(again.Spawned);
    }
}
