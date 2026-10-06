using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDirectMovementSaveTests
{
    [Theory]
    [InlineData(3004, 0)]
    [InlineData(3004, 1)]
    [InlineData(3004, 2)]
    [InlineData(3005, 0)]
    [InlineData(3005, 1)]
    [InlineData(3005, 2)]
    public void DirectOverridesOfGroundAndFloatingDefaultsRoundTrip(int type, int property)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, type);
        Toggle(actor, property); var solid = actor.Solid; var floating = actor.Floating; var noGravity = actor.NoGravity;
        Assert.False(actor.HasMovementActionOverride);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Toggle(actor, property); sim.RestoreState(state);
        Assert.Equal(solid, actor.Solid); Assert.Equal(floating, actor.Floating); Assert.Equal(noGravity, actor.NoGravity);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnchangedSpawnDefaultsDoNotAddMovementExtension()
    {
        var sim = Room(); sim.AddBot(0, 0, 3004); sim.AddBot(100, 0, 3005);
        Assert.All(sim.CaptureState().Actors, pose => Assert.Null(pose.MovementActionFlags));
    }

    [Theory]
    [InlineData(3004, 0)]
    [InlineData(3004, 1)]
    [InlineData(3004, 2)]
    [InlineData(3005, 0)]
    [InlineData(3005, 1)]
    [InlineData(3005, 2)]
    public void BaselineSaveRestoresGroundAndFlyingClassMovement(int type, int property)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, type);
        var solid = actor.Solid; var floating = actor.Floating; var noGravity = actor.NoGravity;
        var bytes = SimSavegame.Write(sim);
        Toggle(actor, property);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(solid, actor.Solid); Assert.Equal(floating, actor.Floating);
        Assert.Equal(noGravity, actor.NoGravity);
        Assert.False(actor.HasMovementActionOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LivingFlyingSaveUndoesDeathMovementFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3005);
        var bytes = SimSavegame.Write(sim);
        ActorDamage.Apply(actor, 1000);
        Assert.False(actor.Floating); Assert.False(actor.NoGravity);
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.IsDead); Assert.True(actor.Floating); Assert.True(actor.NoGravity);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static void Toggle(Actor actor, int property)
    {
        if (property == 0) actor.Solid = !actor.Solid;
        else if (property == 1) actor.Floating = !actor.Floating;
        else actor.NoGravity = !actor.NoGravity;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
