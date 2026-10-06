using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCheckTerrainTests
{
    [Theory]
    [InlineData(104, 0.25, 0)]
    [InlineData(124, 0, 0.25)]
    [InlineData(144, -0.25, 0)]
    [InlineData(164, 0, -0.25)]
    [InlineData(96, -0.25, 0)]
    public void CurrentUsesTagDirectionAndAddsVelocity(short tag, double x, double y)
    {
        var sim = Room(118, tag); var actor = sim.AddBot(0, 0); actor.VelocityX = Fixed.FromInt(1);
        ActorPropertyActions.CheckTerrain(actor);
        Assert.Equal(Fixed.FromDouble(1 + x).Raw, actor.VelocityX.Raw);
        Assert.Equal(Fixed.FromDouble(y).Raw, actor.VelocityY.Raw);
    }

    [Fact]
    public void AboveFloorAndUnrelatedSpecialDoNotApplyCurrent()
    {
        var actor = Room(118, 104).AddBot(0, 0); actor.Z = Fixed.FromInt(1);
        ActorPropertyActions.CheckTerrain(actor); Assert.Equal(0, actor.VelocityX.Raw);
        actor = Room(0, 104).AddBot(0, 0);
        ActorPropertyActions.CheckTerrain(actor); Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LethalTerrainUsesOrdinaryDamageAndOverridesCurrent(bool invulnerable)
    {
        var sim = Room(118, 104); sim.Level.Sectors[0].DamageAmount = 1_000_000;
        var actor = sim.AddBot(0, 0); actor.Invulnerable = invulnerable;
        ActorPropertyActions.CheckTerrain(actor);
        Assert.Equal(invulnerable, actor.Health > 0); Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void FrameCurrentAndVelocitySaveRoundTrip()
    {
        var sim = Room(118, 124); var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0, Action: ActorPropertyActions.CheckTerrain)], 0);
        Assert.Equal(0.25, actor.VelocityY.ToDouble());
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.VelocityY = default; sim.RestoreState(state);
        Assert.Equal(0.25, actor.VelocityY.ToDouble()); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DetachedActorFailsExplicitly() => Assert.Throws<InvalidOperationException>(() => ActorPropertyActions.CheckTerrain(new Actor()));

    private static AuthoritySimulation Room(short special, short tag) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Special = special, Tag = tag }] });
}
