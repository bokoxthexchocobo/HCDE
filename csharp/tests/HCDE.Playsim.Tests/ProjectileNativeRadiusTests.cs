using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileNativeRadiusTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, 11)]
    [InlineData(ProjectileKind.CyberRocket, 11)]
    [InlineData(ProjectileKind.Bfg, 13)]
    public void NativeRadiusSurvivesAbsentSizeAndReconstruction(ProjectileKind kind, int radius)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, kind);
        Assert.Equal(radius, missile.Radius.ToDouble());
        var state = sim.CaptureState(); Assert.Null(state.Actors.Single(a => a.Id == missile.Id).Size);
        var bytes = SimSavegame.Write(state); var id = missile.Id;
        missile.Radius = Fixed.FromInt(99); SimSavegame.Apply(sim, bytes);
        Assert.Equal(radius, missile.Radius.ToDouble());
        missile.Destroy(); SimSavegame.Apply(sim, bytes);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        Assert.Equal(radius, missile.Radius.ToDouble());
        missile.Radius = Fixed.FromInt(6); bytes = SimSavegame.Write(sim);
        missile.Radius = Fixed.FromInt(radius); SimSavegame.Apply(sim, bytes);
        Assert.Equal(6, missile.Radius.ToDouble());
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket, 34)]
    [InlineData(ProjectileKind.CyberRocket, 34)]
    [InlineData(ProjectileKind.Bfg, 36)]
    public void WidthPatchOverridesNativeRadiusIncludingReconstruction(ProjectileKind kind, int thing)
    {
        var sim = Room(DehackedPatch.Apply($"Thing {thing}\nWidth = 491520\n"));
        var owner = sim.AddBot(-200, 0); var missile = sim.SpawnProjectile(owner, kind);
        Assert.Equal(7.5, missile.Radius.ToDouble());
        var bytes = SimSavegame.Write(sim); var id = missile.Id;
        missile.Destroy(); SimSavegame.Apply(sim, bytes);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        Assert.Equal(7.5, missile.Radius.ToDouble());
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket, false)]
    [InlineData(ProjectileKind.CyberRocket, false)]
    [InlineData(ProjectileKind.Bfg, false)]
    [InlineData(ProjectileKind.Rocket, true)]
    [InlineData(ProjectileKind.CyberRocket, true)]
    [InlineData(ProjectileKind.Bfg, true)]
    public void WiderNativeCylinderHitsOffsetActorAndExplicitSmallRadiusMisses(ProjectileKind kind, bool small)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); var target = sim.AddBot(15, 10, 3004);
        owner.Brain = target.Brain = null; target.Radius = Fixed.FromInt(1); target.PainChance = 0;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        if (small) missile.Radius = Fixed.FromInt(6);
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            sim.Tick(); Assert.Equal(!small, missile.Destroyed);
            SimSavegame.Apply(sim, bytes);
            missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == missile.Id);
        }
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, dehacked: patch);
}
