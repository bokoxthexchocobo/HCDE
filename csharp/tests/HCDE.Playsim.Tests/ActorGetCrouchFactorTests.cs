using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetCrouchFactorTests
{
    [Theory]
    [InlineData(AcsActorPointer.Player1, true)]
    [InlineData(AcsActorPointer.Player1 << 1, false)]
    [InlineData(AcsActorPointer.Player1 << 2, false)]
    [InlineData(AcsActorPointer.Target, true)]
    [InlineData(AcsActorPointer.Master, true)]
    [InlineData(AcsActorPointer.Tracer, true)]
    [InlineData(AcsActorPointer.FriendPlayer, true)]
    [InlineData(AcsActorPointer.Default, false)]
    [InlineData(AcsActorPointer.Null, false)]
    public void SelectedPlayerFactorAndFallbackSurviveFreshSave(int selector, bool crouched)
    {
        var sim = Room(); var player = sim.Players.Single(p => p.PlayerNum == 0);
        for (var i = 0; i < 4; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        Assert.True(player.CrouchFactor < 1);
        var caller = sim.Actors.Single(a => a.DoomEdNum == 2007);
        caller.SetTargetPointer(player); caller.MasterId = caller.TracerId = player.Id; caller.FriendPlayer = 1;
        var expected = crouched ? player.CrouchFactor : 1;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(expected, caller.GetCrouchFactor(selector));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, fresh.Actors.Single(a => a.DoomEdNum == 2007).GetCrouchFactor(selector));
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void DefaultSelectsFirstPlayerRatherThanCaller()
    {
        var sim = Room();
        for (var i = 0; i < 4; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        var first = sim.Players.Single(p => p.PlayerNum == 0); var second = sim.Players.Single(p => p.PlayerNum == 1);
        Assert.Equal(first.CrouchFactor, second.GetCrouchFactor());
        Assert.Equal(1, second.GetCrouchFactor(AcsActorPointer.Default));
        Assert.Equal(first.CrouchFactor, first.GetCrouchFactor(AcsActorPointer.Default));
    }

    [Fact]
    public void MissingPointersAndNonPlayerTargetsReturnOne()
    {
        var sim = Room(); var caller = sim.Actors.Single(a => a.DoomEdNum == 2007);
        Assert.Equal(1, caller.GetCrouchFactor(AcsActorPointer.Target));
        caller.SetTargetPointer(caller);
        Assert.Equal(1, caller.GetCrouchFactor(AcsActorPointer.Target));
    }

    [Fact]
    public void DetachedSelfAndNullNeedNoWorldButPlayerSelectionRequiresOne()
    {
        var actor = new Actor();
        Assert.Equal(1, actor.GetCrouchFactor(AcsActorPointer.Default));
        Assert.Equal(1, actor.GetCrouchFactor(AcsActorPointer.Null));
        Assert.Throws<InvalidOperationException>(() => actor.GetCrouchFactor());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 256 }, new LevelThing { Type = 2007, X = -256 }]
    });
}
