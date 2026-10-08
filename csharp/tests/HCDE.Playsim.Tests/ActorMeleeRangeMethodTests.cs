using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorMeleeRangeMethodTests
{
    [Theory]
    [InlineData(-1, true)]
    [InlineData(-20, true)]
    [InlineData(0, false)]
    [InlineData(40, false)]
    [InlineData(41, true)]
    public void ActorMethodUsesCurrentTargetAndStrictRangeBoundary(double range, bool expected)
    {
        var (sim, actor, target) = Room();
        actor.MeleeRange = Fixed.FromInt(44); target.Radius = Fixed.FromInt(10);
        var random = sim.CombatRandomState;
        Assert.Equal(expected, actor.CheckMeleeRange(range));
        Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(44, actor.MeleeRange.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VerticalRangeFlagSurvivesSaveAndControlsActorMethod(bool ignoreVertical)
    {
        var (sim, actor, target) = Room(); target.Z = Fixed.FromInt(100);
        actor.NoVerticalMeleeRange = ignoreVertical;
        SimSavegame.Apply(sim, SimSavegame.Write(sim));
        Assert.Equal(ignoreVertical, actor.CheckMeleeRange());
    }

    [Fact]
    public void MissingTargetReturnsFalseAndFriendTargetIsRejected()
    {
        var (sim, actor, _) = Room();
        actor.Brain!.RestoreTargetMemory(default); Assert.False(actor.CheckMeleeRange());
        var friend = sim.AddBot(20, 0, 3001); actor.Friendly = friend.Friendly = true;
        actor.Brain.SetSpecialTarget(friend); Assert.False(actor.CheckMeleeRange());
    }

    [Fact]
    public void SightAndSectorAttackGatesApply()
    {
        Assert.False(Room(noAttack: true).Item2.CheckMeleeRange());
        Assert.True(Room().Item2.CheckMeleeRange());
        Assert.False(Room(blocked: true).Item2.CheckMeleeRange());
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room(bool noAttack = false, bool blocked = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1, X = 50 }],
            Sectors = [new LevelSector { CeilingHeight = 512, NoAttack = noAttack }],
            Lines = blocked ? [new LevelLine { X1 = 25, X2 = 25, Y1 = -100, Y2 = 100, SideBack = -1 }] : [] });
        var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.MeleeRange = Fixed.FromInt(44); actor.Brain!.SetSpecialTarget(target);
        return (sim, actor, target);
    }
}
