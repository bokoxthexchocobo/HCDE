using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileClassGroupPatchTests
{
    [Theory]
    [InlineData(ProjectileKind.RevenantTracer, 7)]
    [InlineData(ProjectileKind.MancubusBall, 10)]
    [InlineData(ProjectileKind.BaronBall, 17)]
    [InlineData(ProjectileKind.ImpBall, 32)]
    [InlineData(ProjectileKind.CacodemonBall, 33)]
    [InlineData(ProjectileKind.Rocket, 34)]
    [InlineData(ProjectileKind.CyberRocket, 34)]
    [InlineData(ProjectileKind.Plasma, 35)]
    [InlineData(ProjectileKind.Bfg, 36)]
    [InlineData(ProjectileKind.ArachnotronPlasma, 37)]
    public void NativeThingIndicesApplyGroupsToSpawnedMissile(ProjectileKind kind, int index)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nInfighting group = 5\nProjectile group = 6\nSplash group = 7\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        Assert.Equal(5, missile.InfightingGroup); Assert.Equal(6, missile.ProjectileGroup); Assert.Equal(7, missile.SplashGroup);
    }
}
