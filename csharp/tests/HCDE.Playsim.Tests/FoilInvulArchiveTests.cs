using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FoilInvulArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagRoundTripsWithOtherDamagePropertiesAndStillBypassesMonster(bool fire)
    {
        var original = Room();
        var actor = original.Players.Single();
        actor.FoilInvul = true; actor.SpecialFireDamage = fire;
        actor.DeathType = "Fire"; actor.DamageType = "Ice";
        var bytes = SimSavegame.Write(original);
        Assert.Equal(49, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.RestoreState(state);
        var inflictor = restored.Players.Single();
        Assert.True(inflictor.FoilInvul);
        Assert.Equal(fire, inflictor.SpecialFireDamage);
        Assert.Equal("Fire", inflictor.DeathType);
        Assert.Equal("Ice", inflictor.DamageType);
        Assert.Equal(20, ActorDamage.Apply(new Actor { Health = 100, Invulnerable = true }, 20, inflictor: inflictor).HealthLost);
    }

    [Fact]
    public void PoseRestoresFlagAndOldArchiveClearsIt()
    {
        var sim = Room();
        var legacy = SimSavegame.Write(sim);
        sim.Players.Single().FoilInvul = true;
        var pose = sim.CaptureState();
        sim.Players.Single().FoilInvul = false;
        sim.RestoreState(pose);
        Assert.True(sim.Players.Single().FoilInvul);
        Assert.True(SimSavegame.TryRead(legacy, out var old, out var error), error);
        sim.RestoreState(old);
        Assert.False(sim.Players.Single().FoilInvul);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void UnsupportedFlagBitsAreRejected(int flag)
    {
        var sim = Room(); sim.Players.Single().FoilInvul = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), flag);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-special-fire-flags", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
