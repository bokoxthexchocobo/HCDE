namespace HCDE.Playsim.Tests;

public class CorpseFlagTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void FlagControlsLedgeSlidingIndependentlyOfHealth(bool dead, bool corpse)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.AddBot(1, 80);
        actor.Brain = null; actor.AllowDropOff = true; actor.Z = default; actor.SectorIndex = 1;
        if (dead) actor.Health = 0;
        ActorPropertyActions.ChangeFlag(actor, "CORPSE", corpse);
        actor.VelocityX = Fixed.FromInt(1); ActorPhysics.Step(sim, actor);
        Assert.Equal(corpse ? 1 : ActorPhysics.GroundFriction, actor.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ExplicitOverrideSurvivesSave(bool dead, bool corpse)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.Players.Single();
        if (dead) actor.Health = 0;
        actor.Corpse = corpse; var saved = SimSavegame.Write(sim);
        actor.Corpse = !corpse; SimSavegame.Apply(sim, saved);
        Assert.Equal(corpse, actor.Corpse); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Fact]
    public void DeathAndRevivalUpdateTheDefaultFlag()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.AddBot(-40, 80);
        actor.Health = 0; Assert.True(actor.Corpse);
        actor.Health = 100; Assert.False(actor.Corpse);
    }
}
