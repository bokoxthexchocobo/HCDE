using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerPickupFlashTests
{
    [Fact]
    public void ContactStartsSixTicFlashAndItCountsDown()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.Tick(); Assert.Equal(6, player.BonusCount);
        sim.Tick(); Assert.Equal(5, player.BonusCount);
        for (var i = 0; i < 5; i++) sim.Tick(); Assert.Equal(0, player.BonusCount);
    }

    [Fact]
    public void RejectedPickupDoesNotFlash()
    {
        var sim = Room(PickupCatalog.Medikit); sim.Tick(); Assert.Equal(0, sim.Players.Single().BonusCount);
        Assert.Contains(sim.Actors, a => a.DoomEdNum == PickupCatalog.Medikit);
    }

    [Fact]
    public void QuietInventoryGrantDoesNotStartContactFlash()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorInventoryActions.GiveInventory(player, "HealthBonus"); Assert.Equal(0, player.BonusCount);
    }

    [Fact]
    public void FlashArchiveReplaysAndOlderSnapshotClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        sim.Tick(); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(252, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        sim.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Assert.Equal(6, player.BonusCount); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old); Assert.Equal(0, player.BonusCount);
    }

    [Fact]
    public void FreezeDeathClearsPickupFlash()
    {
        var sim = Room(); sim.Tick(); var player = sim.Players.Single();
        ActorFreezeActions.FreezeDeath(player); Assert.Equal(0, player.BonusCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedFlashArchiveRejects(bool negativeCounter)
    {
        var sim = Room(); sim.Tick(); var saved = SimSavegame.Write(sim, true);
        if (negativeCounter)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
            BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), -1);
        }
        else BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room(int type = PickupCatalog.HealthBonus) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }]
    });
}
