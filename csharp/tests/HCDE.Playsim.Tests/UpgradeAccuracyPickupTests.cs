using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UpgradeAccuracyPickupTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NamedGrantIgnoresQuantityAndReplays(int path)
    {
        var sim = Room(); var player = sim.Players.Single(); var saved = SimSavegame.Write(sim, true);
        Grant(player, path); Assert.Equal(10, player.Accuracy); Assert.Equal(100, player.Health);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "UpgradeAccuracy", false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "UpgradeAccuracy", true));
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Grant(player, path); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(0, 10, true)]
    [InlineData(95, 105, true)]
    [InlineData(100, 100, false)]
    [InlineData(105, 105, false)]
    public void NativeThresholdAllowsOvershootButRejectsAtCap(int before, int after, bool success)
    {
        var player = Room().Players.Single(); player.Accuracy = before;
        Assert.Equal(success, ActorPickupActions.UpgradeAccuracy(player)); Assert.Equal(after, player.Accuracy);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    public void AcsAccuracyPropertyRoundTripsSignedActorValues(int value)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a is not PlayerPawn);
        AcsActorProperties.Set(sim, actor, 0, 33, value);
        Assert.Equal(value, AcsActorProperties.Get(sim, actor, 0, 33));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 33, value));
        var saved = SimSavegame.Write(sim, true); sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.Accuracy = value == 1 ? 2 : 1; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.Equal(value, actor.Accuracy); Assert.Equal(checksum, sim.Checksum);
        Assert.False(ActorPickupActions.UpgradeAccuracy(actor));
    }

    [Fact]
    public void OldSaveClearsAccuracyAndMalformedArchiveRejects()
    {
        var sim = Room(); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        ActorPickupActions.UpgradeAccuracy(player); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(259, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        SimSavegame.Apply(sim, old); Assert.Equal(0, player.Accuracy);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void NonpositiveGrantsAndDestroyedPlayersKeepExistingContracts()
    {
        var player = Room().Players.Single();
        AcsPlayerInventory.Give(player, "UpgradeAccuracy", 0); Assert.Equal(0, player.Accuracy);
        Assert.True(ActorInventoryActions.SetInventory(player, "UpgradeAccuracy", 0)); Assert.Equal(0, player.Accuracy);
        Assert.True(ActorInventoryActions.GiveInventory(player, "upgradeaccuracy", 0)); Assert.Equal(10, player.Accuracy);
        player.Destroy(); Assert.False(ActorPickupActions.UpgradeAccuracy(player)); Assert.Equal(10, player.Accuracy);
    }

    private static void Grant(PlayerPawn player, int path)
    {
        if (path == 0) AcsPlayerInventory.Give(player, "UpgradeAccuracy", 77);
        else if (path == 1) Assert.True(ActorInventoryActions.GiveInventory(player, "UpgradeAccuracy", 77));
        else Assert.True(ActorInventoryActions.SetInventory(player, "UpgradeAccuracy", 77));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }]
    });
}
