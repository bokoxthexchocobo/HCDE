using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileRenderDefaultsTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, false, 1)]
    [InlineData(ProjectileKind.CyberRocket, false, 1)]
    [InlineData(ProjectileKind.Plasma, true, 0.75)]
    [InlineData(ProjectileKind.Bfg, true, 0.75)]
    [InlineData(ProjectileKind.ArachnotronPlasma, true, 0.75)]
    [InlineData(ProjectileKind.ImpBall, true, 1)]
    [InlineData(ProjectileKind.BaronBall, true, 1)]
    [InlineData(ProjectileKind.CacodemonBall, true, 1)]
    [InlineData(ProjectileKind.MancubusBall, true, 1)]
    [InlineData(ProjectileKind.RevenantTracer, true, 1)]
    public void NativeClassDefaultsSurviveLegacyAndReconstruction(ProjectileKind kind, bool additive, double alpha)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0);
        var missile = sim.SpawnProjectile(owner, kind);
        var style = additive ? ActorRenderStyle.FromNative(7) : ActorRenderStyle.Normal;
        Assert.Equal(style, missile.RenderStyle); Assert.Equal(alpha, missile.Alpha);
        var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.RenderProperties = null;
        var bytes = SimSavegame.Write(state);
        missile.RenderStyle = ActorRenderStyle.Translucent; missile.Alpha = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(style, missile.RenderStyle); Assert.Equal(alpha, missile.Alpha);
        var id = missile.Id; missile.Destroy(); SimSavegame.Apply(sim, bytes);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        Assert.Equal(style, missile.RenderStyle); Assert.Equal(alpha, missile.Alpha);
        missile.RenderStyle = ActorRenderStyle.Normal; missile.Alpha = 1;
        bytes = SimSavegame.Write(sim);
        missile.RenderStyle = style; missile.Alpha = alpha;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(ActorRenderStyle.Normal, missile.RenderStyle); Assert.Equal(1, missile.Alpha);
    }

    [Theory]
    [InlineData("Translucency = 0\n", false, false)]
    [InlineData("Translucency = 0\nBits = SHADOW\n", true, false)]
    [InlineData("Translucency = 0\nBits = TRANSLUC75\n", false, true)]
    public void ProjectileSpawnAndReconstructionApplyPatchRenderProperties(string body, bool shadow, bool invisible)
    {
        var sim = Room(DehackedPatch.Apply("Thing 35\n" + body));
        var owner = sim.AddBot(0, 0); var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        Assert.Equal(ActorRenderStyle.Translucent, missile.RenderStyle); Assert.Equal(0, missile.Alpha);
        Assert.Equal(shadow, missile.Shadow); Assert.Equal(invisible, missile.Invisible);
        Assert.False(new Actor { DontSeekInvisible = true }.CanSeek(missile));
        var bytes = SimSavegame.Write(sim); var id = missile.Id;
        missile.Destroy(); SimSavegame.Apply(sim, bytes);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        Assert.Equal(ActorRenderStyle.Translucent, missile.RenderStyle); Assert.Equal(0, missile.Alpha);
        Assert.Equal(shadow, missile.Shadow); Assert.Equal(invisible, missile.Invisible);
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, dehacked: patch);
}
