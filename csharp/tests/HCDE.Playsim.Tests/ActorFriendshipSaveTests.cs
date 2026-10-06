using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFriendshipSaveTests
{
    [Theory]
    [InlineData(2, 77)]
    [InlineData(0, 0)]
    [InlineData(-1, int.MinValue)]
    public void DirectValuesAndExplicitZeroRoundTrip(int friendPlayer, int hate)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004);
        actor.FriendPlayer = friendPlayer; actor.TidToHate = hate;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(86, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FriendPlayer = 9; actor.TidToHate = 99;
        sim.RestoreState(state);
        Assert.Equal(friendPlayer, actor.FriendPlayer); Assert.Equal(hate, actor.TidToHate);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacyAbsenceRestoresDefaultValues()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Actors.Single(item => item.Id == actor.Id).Friendship);
        actor.FriendPlayer = 3; actor.TidToHate = 88;
        sim.RestoreState(state);
        Assert.Equal(0, actor.FriendPlayer); Assert.Equal(0, actor.TidToHate);
    }

    [Fact]
    public void ResurrectionTransferSurvivesSaveRestore()
    {
        var sim = Room(); var raiser = sim.AddBot(0, 0, 3004); var corpse = sim.AddBot(200, 0, 3004);
        raiser.Friendly = true; raiser.FriendPlayer = 2; raiser.TidToHate = 77; raiser.NoHatePlayers = true;
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(ActorRaiseActions.RaiseActor(raiser, corpse, 1));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        corpse.FriendPlayer = 9; corpse.TidToHate = 99; corpse.Friendly = false; corpse.NoHatePlayers = false;
        sim.RestoreState(state);
        Assert.Equal(2, corpse.FriendPlayer); Assert.Equal(77, corpse.TidToHate);
        Assert.True(corpse.Friendly); Assert.True(corpse.NoHatePlayers);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MalformedTrailerIsRejected(int kind)
    {
        var sim = Room(); sim.AddBot(200, 0, 3004).FriendPlayer = 0;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        if (kind == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 86);
        if (kind == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), int.MaxValue);
        if (kind == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        if (kind == 3) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 12), 1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CopyActionPreservesHealthAndTargetAndCanClearFriendship(bool friendly)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004); var source = sim.AddBot(0, 0, 3004);
        source.Friendly = friendly; source.FriendPlayer = friendly ? 2 : 0;
        source.TidToHate = friendly ? 77 : 0; source.NoHatePlayers = friendly;
        actor.MasterId = source.Id; actor.Health = 7;
        sim.Players.Single().ThingId = 9; actor.Brain!.SetTargetThingId(sim, 9);
        var targetId = actor.Brain.TargetId;
        ActorPropertyActions.CopyFriendliness(actor);
        Assert.Equal(7, actor.Health); Assert.Equal(targetId, actor.Brain.TargetId);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(source.FriendPlayer, actor.FriendPlayer);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Friendly = !friendly; actor.FriendPlayer = 9; actor.TidToHate = 99;
        sim.RestoreState(state);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(source.FriendPlayer, actor.FriendPlayer);
        Assert.Equal(source.TidToHate, actor.TidToHate); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void CopyActionProtectsPlayersAndNullSelection()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004); var source = sim.AddBot(0, 0, 3004);
        source.FriendPlayer = 2; actor.MasterId = source.Id;
        ActorPropertyActions.CopyFriendliness(actor, AcsActorPointer.Null);
        Assert.Equal(0, actor.FriendPlayer);
        var player = sim.Players.Single(); player.MasterId = source.Id;
        ActorPropertyActions.CopyFriendliness(player);
        Assert.Equal(0, player.FriendPlayer);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
