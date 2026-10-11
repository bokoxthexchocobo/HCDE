using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSpawnSoundRoutingTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ActorRoutingUsesNativeSourceAndChannel(bool sourceFlag, bool hasSpawner)
    {
        var sim = Room(); var owner = sim.AddBot(100, 50);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket); sim.DrainSoundRequests();
        ActorSoundActions.PlaySpawnSound(missile, hasSpawner ? owner : null, sourceFlag);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("weapons/rocklf", request.Sound); Assert.Equal(sourceFlag ? 1 : 2, request.Channel);
        Assert.Equal(1, request.Attenuation); Assert.Equal(1, request.Volume); Assert.Equal(0, request.Flags);
        if (sourceFlag && !hasSpawner)
        {
            var position = missile.SoundPos();
            Assert.Equal(0u, request.ActorId);
            Assert.Equal(new SimulationSoundOrigin(position.X, position.Y, position.Z, 0), request.DetachedOrigin);
        }
        else
        {
            Assert.Equal(sourceFlag ? owner.Id : missile.Id, request.ActorId);
            Assert.Null(request.DetachedOrigin);
        }
    }

    [Fact]
    public void PositionFallbackSuppressesSilentSectorBeforeQueueing()
    {
        var sim = Room(silent: true);
        var missile = sim.SpawnProjectile(sim.AddBot(0, 0), ProjectileKind.Rocket); sim.DrainSoundRequests();
        ActorSoundActions.PlaySpawnSound(missile, spawnSoundSource: true);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void MissingSeeSoundAndDestroyedMissileStayQuiet()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Bfg);
        ActorSoundActions.PlaySpawnSound(missile, owner, true);
        Assert.Empty(sim.DrainSoundRequests());
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "custom/launch" };
        missile.Destroy(); ActorSoundActions.PlaySpawnSound(missile, owner, true);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void ForeignSpawnerIsRejectedWithoutQueueing()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.AddBot(0, 0), ProjectileKind.Rocket);
        sim.DrainSoundRequests(); var foreign = Room().AddBot(0, 0);
        Assert.Throws<InvalidOperationException>(() => ActorSoundActions.PlaySpawnSound(missile, foreign, true));
        Assert.Empty(sim.DrainSoundRequests());
    }

    private static AuthoritySimulation Room(bool silent = false) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Silent = silent }] });
}
