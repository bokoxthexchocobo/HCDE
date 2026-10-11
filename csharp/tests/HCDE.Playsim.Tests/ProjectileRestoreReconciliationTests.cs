using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileRestoreReconciliationTests
{
    [Theory]
    [InlineData(ProjectileKind.ImpBall)]
    [InlineData(ProjectileKind.RevenantTracer)]
    [InlineData(ProjectileKind.CyberRocket)]
    public void RestoreRecreatesMissingProjectileWithSavedIdentityAndPointers(ProjectileKind kind)
    {
        var sim = Room(); var owner = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var missile = sim.SpawnProjectile(owner, kind, sim.Players.Single());
        var id = missile.Id; var x = missile.X; var velocity = missile.VelocityX;
        var tracer = missile.TracerTargetId; var bytes = SimSavegame.Write(sim);
        missile.Destroy(); sim.Thinkers.Run();
        SimSavegame.Apply(sim, bytes);
        var restored = Assert.Single(sim.Actors.OfType<ProjectileActor>());
        Assert.NotSame(missile, restored); Assert.False(restored.Destroyed);
        Assert.Equal(id, restored.Id); Assert.Same(owner, restored.Owner); Assert.Equal(kind, restored.Kind);
        Assert.Equal(x, restored.X); Assert.Equal(velocity, restored.VelocityX);
        Assert.Equal(tracer, restored.TracerTargetId); Assert.Equal(175, restored.RemainingTics);
        sim.Thinkers.Run(); Assert.Equal(174, restored.RemainingTics);
    }

    [Fact]
    public void RestoreRemovesLaterProjectilesAndRetainsSavedProjectile()
    {
        var sim = Room(); var owner = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var saved = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, sim.Players.Single());
        var bytes = SimSavegame.Write(sim);
        var later = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, sim.Players.Single());
        var notified = false; sim.ActorDestroyed += actor => { if (actor == later) notified = true; };
        SimSavegame.Apply(sim, bytes);
        Assert.Same(saved, Assert.Single(sim.Actors.OfType<ProjectileActor>()));
        Assert.False(saved.Destroyed); Assert.True(later.Destroyed); Assert.True(notified);
        var x = later.X; var y = later.Y;
        sim.Thinkers.Run();
        Assert.Equal(x, later.X); Assert.Equal(y, later.Y);
        var replacement = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, sim.Players.Single());
        Assert.NotEqual(saved.Id, replacement.Id);
    }

    [Fact]
    public void InvalidSnapshotDoesNotRemoveLaterProjectile()
    {
        var sim = Room(); var state = sim.CaptureState();
        var owner = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var later = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, sim.Players.Single());
        var index = state.Actors.FindIndex(a => a.Id == owner.Id);
        state.Actors[index] = new SimActorPose { Id = owner.Id, WeaponCooldown = -1 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Contains(later, sim.Actors); Assert.False(later.Destroyed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3001 }] });
}
