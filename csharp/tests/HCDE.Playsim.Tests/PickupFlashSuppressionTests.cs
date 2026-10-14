using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupFlashSuppressionTests
{
    [Theory]
    [InlineData(false, false, 6)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 0)]
    [InlineData(true, true, 0)]
    public void FlagsSuppressOnlyTheFlash(bool quiet, bool noFlash, int expected)
    {
        var (sim, player, item) = Room(); item.PickupQuiet = quiet; item.NoScreenFlash = noFlash;
        sim.Tick(); Assert.True(item.Destroyed); Assert.Equal(101, player.Health); Assert.Equal(expected, player.BonusCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SuppressedPickupDoesNotClearOrRefreshExistingFlash(bool quiet)
    {
        var (sim, player, item) = Room(); player.BonusCount = 4;
        item.PickupQuiet = quiet; item.NoScreenFlash = !quiet;
        sim.Tick(); Assert.Equal(3, player.BonusCount); Assert.True(item.Destroyed);
    }

    [Fact]
    public void FlagsRoundTripAndOldSnapshotClearsThem()
    {
        var (sim, player, item) = Room(); var old = SimSavegame.Write(sim, true);
        item.PickupQuiet = item.NoScreenFlash = true;
        var saved = SimSavegame.Write(sim, true); Assert.Equal(253, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        sim.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); var restored = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus);
        Assert.True(restored.PickupQuiet); Assert.True(restored.NoScreenFlash);
        sim.Tick(); Assert.Equal(0, player.BonusCount); Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old); restored = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus);
        Assert.False(restored.PickupQuiet); Assert.False(restored.NoScreenFlash);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void UnsupportedSavedFlagsReject(int flags)
    {
        var (sim, _, item) = Room(); item.PickupQuiet = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Item) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus));
    }
}
