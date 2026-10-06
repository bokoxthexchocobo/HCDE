using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRaiseActionTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public void FamilySelectionAndFriendliness(bool siblings, int flags)
    {
        var sim = Room(); var parent = sim.AddBot(0, 0, 3004);
        var caller = siblings ? sim.AddBot(100, 0, 3004) : parent;
        caller.Friendly = true; caller.FriendPlayer = 2; caller.TidToHate = 77; caller.NoHatePlayers = true;
        if (siblings) caller.MasterId = parent.Id;
        var corpse = sim.AddBot(200, 0, 3004); corpse.MasterId = parent.Id; Kill(corpse);
        var unrelated = sim.AddBot(300, 0, 3004); Kill(unrelated);
        if (siblings) ActorRaiseActions.RaiseSiblings(caller, flags);
        else ActorRaiseActions.RaiseChildren(caller, flags);
        Assert.False(corpse.IsDead); Assert.True(unrelated.IsDead);
        Assert.Equal(flags == 1, corpse.Friendly);
        Assert.Equal(flags == 1 ? 2 : 0, corpse.FriendPlayer);
        Assert.Equal(flags == 1 ? 77 : 0, corpse.TidToHate);
        Assert.Equal(flags == 1, corpse.NoHatePlayers);
        Assert.Equal(default, corpse.VelocityX); Assert.Equal(Fixed.FromInt(3), corpse.VelocityZ);
        Assert.False(caller.Destroyed); Assert.False(parent.Destroyed);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public void PositionFailureStopsHorizontalMomentumAndBypassRaises(int flags, bool expected)
    {
        var sim = Room(); var corpse = sim.AddBot(200, 0, 3004); Kill(corpse);
        sim.AddBot(200, 0, 3004);
        Assert.Equal(expected, ActorRaiseActions.RaiseSelf(corpse, flags));
        Assert.Equal(!expected, corpse.IsDead);
        Assert.Equal(default, corpse.VelocityX); Assert.Equal(Fixed.FromInt(3), corpse.VelocityZ);
    }

    [Fact]
    public void MasterSelectionAndNullMaster()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3004); var master = sim.AddBot(200, 0, 3004); Kill(master);
        ActorRaiseActions.RaiseMaster(caller); Assert.True(master.IsDead);
        caller.MasterId = master.Id;
        ActorRaiseActions.RaiseMaster(caller); Assert.False(master.IsDead);
        Assert.False(ActorRaiseActions.RaiseActor(caller, null));
        Assert.False(ActorRaiseActions.RaiseSelf(caller));
    }

    [Fact]
    public void ThingRaiseTransfersActivatorFriendliness()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3004); caller.Friendly = true;
        var corpse = sim.AddBot(200, 0, 3004); corpse.ThingId = 7; Kill(corpse);
        Assert.True(ThingRaise.Execute(sim, 17, caller, 7, 1));
        Assert.True(corpse.Friendly);
    }

    private static void Kill(Actor actor)
    {
        actor.Health = 0; actor.Solid = false; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.VelocityX = Fixed.FromInt(2); actor.VelocityZ = Fixed.FromInt(3);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
