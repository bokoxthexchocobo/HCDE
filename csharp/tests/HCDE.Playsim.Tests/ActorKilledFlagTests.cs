using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKilledFlagTests
{
    [Fact]
    public void ZeroHealthWithoutDeathRemainsCountableAsUnkilled()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001); target.Health = 0;
        Assert.False(target.Killed);
        Assert.True(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100));
        Assert.False(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100, flags: 16));
    }

    [Fact]
    public void DamageDeathSetsKilledBeforeDeathFrameActionAndRaiseClearsIt()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001); var observed = false;
        target.States.Configure(target, [new(-1, 0), new(-1, 1),
            new(-1, 2, Action: self => observed = self.Killed)], 0);
        ActorDamage.Apply(target, 1000, sim.Players.Single());
        Assert.True(observed); Assert.True(target.Killed);
        Assert.False(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100));
        Assert.True(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100, flags: 16));
        ActorRaise.ReviveSupported(target);
        Assert.False(target.Killed);
        Assert.True(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100));
    }

    [Fact]
    public void DirectHealthRestorationDoesNotClearNativeKilledFlag()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001);
        ActorDamage.Apply(target, 1000); target.Health = 100;
        Assert.True(target.Killed);
        Assert.True(ActorProximity.Check(sim.Players.Single(), "DoomImp", 100, flags: 16));
    }

    [Fact]
    public void FlagMutationAndChecksumTrackKilledIndependentlyOfHealth()
    {
        var sim = Room(); sim.RestoreState(sim.CaptureState());
        var player = sim.Players.Single(); var before = sim.Checksum;
        Assert.True(AcsActorFlags.TrySet(player, "Actor.KILLED", true));
        Assert.True(AcsActorFlags.TryGet(player, "killed", out var killed)); Assert.True(killed);
        sim.RestoreState(sim.CaptureState());
        Assert.Equal(100, player.Health); Assert.NotEqual(before, sim.Checksum);
    }

    [Fact]
    public void SaveRoundTripPreservesKilledWithPositiveHealthAndArmorExtensions()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Killed = true;
        player.Inventory.Armor = 25; player.Inventory.ArmorType = "Custom";
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(67, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        player.Killed = false; sim.RestoreState(state);
        Assert.True(player.Killed); Assert.Equal(100, player.Health);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacySaveClearsKilledOverride()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim); sim.Players.Single().Killed = true;
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); Assert.False(sim.Players.Single().Killed);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 67, "save-killed-flag-header")]
    [InlineData(4, int.MaxValue, "save-killed-flag-header")]
    [InlineData(8, 2, "save-killed-flag-value")]
    [InlineData(8, -1, "save-killed-flag-value")]
    public void MalformedSaveTrailerIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); sim.Players.Single().Killed = true; var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
