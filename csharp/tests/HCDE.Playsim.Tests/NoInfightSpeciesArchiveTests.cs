using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoInfightSpeciesArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagRoundTripsWithPowerTrailerAndRetaliation(bool powers)
    {
        var sim = Room(); sim.Actors[1].NoInfightSpecies = true;
        sim.Actors[1].PierceArmor = true;
        if (powers) sim.Players.Single().GivePowerBuddha();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(powers ? 52 : 53, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        var target = restored.Actors[1];
        Assert.True(target.NoInfightSpecies); Assert.True(target.PierceArmor);
        // Actor damage eligibility is separate from the restored target-switch flag.
        target.DoHarmSpecies = true;
        Assert.Equal(1, ActorDamage.Apply(target, 1, restored.Actors[2]).HealthLost);
        Assert.Null(target.Brain!.TargetId);
        Assert.Equal(powers ? 2100 : 0, restored.Players.Single().PowerBuddhaTics);
    }

    [Fact]
    public void PoseRestoresAndLegacyClearsFlag()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim);
        sim.Actors[1].NoInfightSpecies = true; var pose = sim.CaptureState();
        sim.Actors[1].NoInfightSpecies = false; sim.RestoreState(pose);
        Assert.True(sim.Actors[1].NoInfightSpecies);
        Assert.True(SimSavegame.TryRead(legacy, out var old, out var error), error);
        sim.RestoreState(old); Assert.False(sim.Actors[1].NoInfightSpecies);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnknownBitsAreRejected()
    {
        var sim = Room(); sim.Actors[1].NoInfightSpecies = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 16);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-special-fire-flags", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = 3004, X = 100 }],
    });
}
