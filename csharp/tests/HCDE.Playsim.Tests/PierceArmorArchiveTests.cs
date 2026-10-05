using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PierceArmorArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchivePreservesFlagsAndArmorBypass(bool otherFlags)
    {
        var sim = Room();
        var actor = sim.Players.Single();
        actor.PierceArmor = true;
        actor.FoilInvul = otherFlags;
        actor.SpecialFireDamage = otherFlags;
        actor.DeathType = "Fire";
        actor.DamageType = "Ice";
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(50, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.RestoreState(state);
        var inflictor = restored.Players.Single();
        Assert.True(inflictor.PierceArmor);
        Assert.Equal(otherFlags, inflictor.FoilInvul);
        Assert.Equal(otherFlags, inflictor.SpecialFireDamage);
        Assert.Equal("Fire", inflictor.DeathType);
        Assert.Equal("Ice", inflictor.DamageType);
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(20, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
        Assert.Equal(100, target.Armor);
    }

    [Fact]
    public void PoseRestoresFlagAndOldArchiveClearsIt()
    {
        var sim = Room();
        var oldBytes = SimSavegame.Write(sim);
        sim.Players.Single().PierceArmor = true;
        var pose = sim.CaptureState();
        sim.Players.Single().PierceArmor = false;
        sim.RestoreState(pose);
        Assert.True(sim.Players.Single().PierceArmor);
        Assert.True(SimSavegame.TryRead(oldBytes, out var old, out var error), error);
        sim.RestoreState(old);
        Assert.False(sim.Players.Single().PierceArmor);
        Assert.Equal(oldBytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void UnsupportedBitsAreRejected(int flags)
    {
        var sim = Room();
        sim.Players.Single().PierceArmor = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-special-fire-flags", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
