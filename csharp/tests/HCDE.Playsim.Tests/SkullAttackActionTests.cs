using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullAttackActionTests
{
    [Theory]
    [InlineData(40, 40)]
    [InlineData(0, 20)]
    [InlineData(-5, 20)]
    public void StateActionUsesTargetAndSpeedAndClearsAmbush(double supplied, double expected)
    {
        var sim = Room(); var soul = Soul(sim);
        soul.Brain!.SetSpecialTarget(sim.Players.Single()); soul.Ambush = true;
        soul.PitchDegrees = 17;
        soul.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/skullattack" };
        soul.States.Configure(soul, [new ActorFrame(-1, 0, a => ActorSkullActions.SkullAttack(a, supplied))], 0);
        Assert.True(soul.Brain.Charging); Assert.False(soul.Ambush);
        Assert.Equal(expected, soul.VelocityX.ToDouble()); Assert.Equal(17, soul.PitchDegrees);
        var sound = Assert.Single(sim.DrainSoundRequests()); Assert.Equal(2, sound.Channel);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetLeavesActorUntouched(bool destroyTarget)
    {
        var sim = Room(); var soul = Soul(sim);
        if (destroyTarget)
        { soul.Brain!.SetSpecialTarget(sim.Players.Single()); sim.Players.Single().Destroy(); }
        soul.Ambush = true; soul.VelocityX = Fixed.FromInt(3);
        ActorSkullActions.SkullAttack(soul);
        Assert.False(soul.Brain!.Charging); Assert.True(soul.Ambush);
        Assert.Equal(Fixed.FromInt(3), soul.VelocityX); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void InvalidSpeedRejectsBeforeChargeMutation()
    {
        var sim = Room(); var soul = Soul(sim); soul.Brain!.SetSpecialTarget(sim.Players.Single());
        soul.Ambush = true;
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSkullActions.SkullAttack(soul, double.NaN));
        Assert.False(soul.Brain.Charging); Assert.True(soul.Ambush); Assert.Empty(sim.DrainSoundRequests());
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 100 }, new LevelThing { Type = 3006 }] });
}
