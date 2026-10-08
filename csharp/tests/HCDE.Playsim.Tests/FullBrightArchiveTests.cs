using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FullBrightArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RuntimeOverrideSurvivesSaveAndFreshMatchingRestore(bool frameBright)
    {
        var sim = Room(frameBright); var actor = sim.Actors[0];
        actor.FullBright = !frameBright;
        var saved = SimSavegame.Write(sim);
        Assert.Equal(107, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(frameBright); SimSavegame.Apply(restored, saved);
        Assert.Equal(!frameBright, restored.Actors[0].FullBright);
        Assert.Equal(saved, SimSavegame.Write(restored));
        restored.Actors[0].States.Enter(restored.Actors[0], 0);
        Assert.Equal(frameBright, restored.Actors[0].FullBright);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OlderSaveResetsOverrideToFrameBrightness(bool frameBright)
    {
        var sim = Room(frameBright); var saved = SimSavegame.Write(sim);
        Assert.NotEqual(107, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        sim.Actors[0].FullBright = !frameBright;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(frameBright, sim.Actors[0].FullBright);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MalformedTrailerRejectsBeforeApplying(int corruption)
    {
        var sim = Room(false); sim.Actors[0].FullBright = true;
        var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        var start = saved.Length - size;
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), 11);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start), 107);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 12), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved));
        Assert.True(sim.Actors[0].FullBright);
    }

    private static AuthoritySimulation Room(bool frameBright)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        sim.Actors[0].Brain = null;
        sim.Actors[0].States.Configure(sim.Actors[0], [new(-1, 0, FullBright: frameBright)], 0);
        return sim;
    }
}
