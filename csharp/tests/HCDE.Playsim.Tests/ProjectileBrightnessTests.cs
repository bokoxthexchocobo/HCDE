using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileBrightnessTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.CyberRocket)]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Bfg)]
    [InlineData(ProjectileKind.ArachnotronPlasma)]
    [InlineData(ProjectileKind.ImpBall)]
    [InlineData(ProjectileKind.BaronBall)]
    [InlineData(ProjectileKind.CacodemonBall)]
    [InlineData(ProjectileKind.MancubusBall)]
    [InlineData(ProjectileKind.RevenantTracer)]
    public void NativeFlightBrightnessAndOverrideSurviveReconstruction(ProjectileKind kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(0, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, kind); var id = missile.Id;
        Assert.True(missile.FullBright); Assert.True(missile.States.CurrentFullBright);
        Assert.False(owner.FullBright);
        var state = sim.CaptureState();
        Assert.Null(state.Actors.Single(a => a.Id == id).FullBrightFlag);
        var defaults = SimSavegame.Write(state);
        missile.FullBright = false;
        state = sim.CaptureState();
        Assert.Equal(0, state.Actors.Single(a => a.Id == id).FullBrightFlag);
        var overridden = SimSavegame.Write(state);
        missile.Destroy(); SimSavegame.Apply(sim, overridden);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        Assert.False(missile.FullBright); Assert.True(missile.States.CurrentFullBright);
        SimSavegame.Apply(sim, defaults); Assert.True(missile.FullBright);
        sim.Tick(); Assert.True(missile.FullBright);
    }
}
