using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileAutomaticSoundTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.CyberRocket)]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Bfg)]
    [InlineData(ProjectileKind.ImpBall)]
    [InlineData(ProjectileKind.BaronBall)]
    [InlineData(ProjectileKind.CacodemonBall)]
    [InlineData(ProjectileKind.ArachnotronPlasma)]
    [InlineData(ProjectileKind.MancubusBall)]
    [InlineData(ProjectileKind.RevenantTracer)]
    public void NewLaunchPlaysNativeVoiceSoundAndReconstructionDoesNotReplay(ProjectileKind kind)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); var missile = sim.SpawnProjectile(owner, kind);
        var requests = sim.DrainSoundRequests();
        if (kind == ProjectileKind.Bfg) Assert.Empty(requests);
        else
        {
            var request = Assert.Single(requests);
            Assert.Equal(missile.ActorSounds[ActorSoundType.See], request.Sound);
            Assert.Equal(missile.Id, request.ActorId); Assert.Equal(2, request.Channel);
            Assert.Equal(0, request.Flags); Assert.Equal(1, request.Attenuation);
        }
        var bytes = SimSavegame.Write(sim); missile.Destroy(); SimSavegame.Apply(sim, bytes);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ImpactPlaysDeathSoundWithNativeAttenuationAndReplays(bool fullVolume)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket); sim.DrainSoundRequests();
        missile.FullVolumeDeath = fullVolume; missile.Z = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(-10);
        var bytes = SimSavegame.Write(sim); var id = missile.Id;
        for (var pass = 0; pass < 2; pass++)
        {
            sim.Tick(); Assert.True(missile.Destroyed);
            var request = Assert.Single(sim.DrainSoundRequests());
            Assert.Equal("weapons/rocklx", request.Sound); Assert.Equal(id, request.ActorId);
            Assert.Equal(2, request.Channel); Assert.Equal(0, request.Flags);
            Assert.Equal(fullVolume ? 0 : 1, request.Attenuation);
            SimSavegame.Apply(sim, bytes); Assert.Empty(sim.DrainSoundRequests());
            missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SkyContactAndSilentOverrideSuppressImpactSound(bool sky)
    {
        var sim = Room(); if (sky) sim.Level.Sectors[0].FloorPic = "F_SKY1";
        var missile = sim.SpawnProjectile(sim.AddBot(-200, 0), ProjectileKind.Rocket); sim.DrainSoundRequests();
        if (!sky) missile.ActorSounds = new Dictionary<ActorSoundType, string>();
        missile.Z = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(-10);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void RestoringBeforeLaunchReusesProjectileIdAndSoundRequest()
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); var bytes = SimSavegame.Write(sim);
        var first = sim.SpawnProjectile(owner, ProjectileKind.Rocket); var expected = Assert.Single(sim.DrainSoundRequests());
        SimSavegame.Apply(sim, bytes);
        var second = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(first.Id, second.Id); Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()));
    }

    [Fact]
    public void NamedPatchLaunchAndDeathSoundsPlayAutomatically()
    {
        var sim = Room(DehackedPatch.Apply("Thing 34\nAlert sound = custom/launch\nDeath sound = custom/impact\n"));
        var missile = sim.SpawnProjectile(sim.AddBot(-200, 0), ProjectileKind.Rocket);
        Assert.Equal("custom/launch", Assert.Single(sim.DrainSoundRequests()).Sound);
        missile.Z = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(-10);
        sim.Tick(); Assert.Equal("custom/impact", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("None", 0)]
    [InlineData("nOnE", 0)]
    [InlineData("", 1)]
    [InlineData("None", 1)]
    [InlineData("nOnE", 1)]
    [InlineData("", 2)]
    [InlineData("None", 2)]
    [InlineData("nOnE", 2)]
    public void NoSoundSpawnIdentitySuppressesEveryOrigin(string sound, int origin)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); sim.DrainSoundRequests();
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = sound };
        ActorSoundActions.PlaySpawnSound(missile, origin == 1 ? owner : null, spawnSoundSource: origin != 0);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("nOnE")]
    public void NoSoundDeathIdentityRemainsSilentAfterSaveReplay(string sound)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); sim.DrainSoundRequests();
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = sound };
        missile.Z = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(-10);
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            missile.Tick(); Assert.True(missile.Destroyed); Assert.Empty(sim.DrainSoundRequests());
            SimSavegame.Apply(sim, bytes);
            missile = sim.Actors.OfType<ProjectileActor>().Single();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeathSoundIsQueuedAfterCleanupBeforeProjectileRemoval(bool fullVolume)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); sim.DrainSoundRequests();
        missile.FullVolumeDeath = fullVolume; missile.Shootable = true;
        missile.Z = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(-10);
        var calls = 0;
        sim.SoundRequestsReady += () =>
        {
            Assert.False(missile.Destroyed); Assert.False(missile.Shootable);
            Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
            var sound = Assert.Single(sim.PendingSoundRequests);
            Assert.Equal("weapons/plasmax", sound.Sound); Assert.Equal(fullVolume ? 0 : 1, sound.Attenuation);
            Assert.NotNull(sound.DetachedOrigin); calls++;
        };
        missile.Tick();
        Assert.Equal(1, calls); Assert.True(missile.Destroyed);
        var origin = Assert.IsType<SimulationSoundOrigin>(Assert.Single(sim.DrainSoundRequests()).DetachedOrigin);
        var position = missile.SoundPos();
        Assert.Equal(position.X, origin.X); Assert.Equal(position.Y, origin.Y); Assert.Equal(position.Z, origin.Z);
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, dehacked: patch);
}
