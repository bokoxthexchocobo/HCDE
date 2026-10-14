using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerSearchCursorTests
{
    [Theory]
    [InlineData(0, 53, 42, 62)]
    [InlineData(1, 43, 27, 4)]
    [InlineData(12345, 20, 38, 2)]
    public void NativeLookyLookyStartingSlotVectors(int seed, int a, int b, int c)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
        Assert.Equal(new[] { a, b, c }, Enumerable.Range(0, 3).Select(_ => sim.NextPlayerSearchStart()));
    }

    [Fact]
    public void SlotAfterCursorWinsOverNearestPlayer()
    {
        var (sim, hunter) = Room();
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(sim.Players.Single(p => p.PlayerNum == 1).Id, hunter.TargetId);
        Assert.Equal(1, hunter.LastLookPlayerNumber);
    }

    [Fact]
    public void SlotOrderDoesNotDependOnMapThingOrder()
    {
        var (sim, hunter) = Room(reverse: true); hunter.LastLookPlayerNumber = 63;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(sim.Players.Single(p => p.PlayerNum == 0).Id, hunter.TargetId);
        Assert.Equal(0, hunter.LastLookPlayerNumber);
    }

    [Fact]
    public void SuccessfulSearchDoesNotTestLaterInvisiblePlayers()
    {
        var (sim, hunter) = Room(); var later = sim.Players.Single(p => p.PlayerNum == 0);
        later.Shadow = true; later.VelocityX = Fixed.FromInt(5);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.NotEqual(later.Id, hunter.TargetId); Assert.Null(sim.CaptureState().PlayerLookRandomState);
    }

    [Fact]
    public void IneligiblePlayerStillAdvancesCursorBeforeWrap()
    {
        var (sim, hunter) = Room(); sim.Players.Single(p => p.PlayerNum == 1).NoTarget = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(sim.Players.Single(p => p.PlayerNum == 0).Id, hunter.TargetId);
        Assert.Equal(0, hunter.LastLookPlayerNumber);
    }

    [Fact]
    public void ExhaustedSearchVisitsEveryOccupiedSlotOnce()
    {
        var (sim, hunter) = Room(); hunter.LastLookPlayerNumber = 63;
        foreach (var player in sim.Players) player.NoTarget = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Null(hunter.TargetId); Assert.Equal(1, hunter.LastLookPlayerNumber);
    }

    [Fact]
    public void HateFallbackDrawsStartWithoutChangingOrdinaryCursor()
    {
        var (sim, hunter) = Room(); hunter.TidToHate = 99; hunter.HuntPlayers = true; hunter.LastLookPlayerNumber = 17;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.NotNull(hunter.TargetId); Assert.Equal(17, hunter.LastLookPlayerNumber);
        Assert.NotNull(sim.CaptureState().PlayerSearchRandomState);
    }

    [Fact]
    public void SaveRestoresCursorAndNextRandomStart()
    {
        var (sim, hunter) = Room(); hunter.LastLookPlayerNumber = 17; sim.NextPlayerSearchStart();
        var saved = SimSavegame.Write(sim, true); var expected = sim.NextPlayerSearchStart();
        hunter.LastLookPlayerNumber = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(17, hunter.LastLookPlayerNumber); Assert.Equal(saved, SimSavegame.Write(sim, true));
        Assert.Equal(expected, sim.NextPlayerSearchStart());
    }

    [Fact]
    public void OlderSnapshotAndFriendshipCopyHandleCursor()
    {
        var (sim, hunter) = Room(); var saved = SimSavegame.Write(sim, true);
        hunter.LastLookPlayerNumber = 12; var other = sim.AddBot(0, 200);
        ActorPropertyActions.CopySupportedFriendship(other, hunter); Assert.Equal(12, other.LastLookPlayerNumber);
        sim.NextPlayerSearchStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(0, hunter.LastLookPlayerNumber); Assert.Null(sim.CaptureState().PlayerSearchRandomState);
    }

    [Fact]
    public void OutOfRangeCursorRejectsStructuredState()
    {
        var (sim, _) = Room(); var state = sim.CaptureState(); state.Actors[0].LastLookPlayerNumber = 64;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Fact]
    public void RecursiveArchiveIsRejected()
    {
        var (sim, hunter) = Room(); hunter.LastLookPlayerNumber = 12; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 235);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-playersearch-header", error);
    }

    private static (AuthoritySimulation Sim, Actor Hunter) Room(bool reverse = false)
    {
        LevelThing[] players = [new() { Type = 1, X = 200 }, new() { Type = 2, X = 600 }];
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = reverse ? players.AsEnumerable().Reverse().ToArray() : players
        });
        return (sim, sim.AddBot(0, 0));
    }
}
