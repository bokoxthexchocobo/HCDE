using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSoundDefaultsTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, "weapons/rocklf", "weapons/rocklx")]
    [InlineData(ProjectileKind.CyberRocket, "weapons/rocklf", "weapons/rocklx")]
    [InlineData(ProjectileKind.Plasma, "weapons/plasmaf", "weapons/plasmax")]
    [InlineData(ProjectileKind.Bfg, null, "weapons/bfgx")]
    [InlineData(ProjectileKind.ImpBall, "imp/attack", "imp/shotx")]
    [InlineData(ProjectileKind.BaronBall, "baron/attack", "baron/shotx")]
    [InlineData(ProjectileKind.CacodemonBall, "caco/attack", "caco/shotx")]
    [InlineData(ProjectileKind.ArachnotronPlasma, "baby/attack", "baby/shotx")]
    [InlineData(ProjectileKind.MancubusBall, "fatso/attack", "fatso/shotx")]
    [InlineData(ProjectileKind.RevenantTracer, "skeleton/attack", "skeleton/tracex")]
    public void NativeSoundSlotsAndDeathActionReconstruct(ProjectileKind kind, string? see, string death)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var missile = sim.SpawnProjectile(owner, kind);
        Assert.Equal(see, missile.ActorSounds.GetValueOrDefault(ActorSoundType.See));
        Assert.Equal(death, missile.ActorSounds[ActorSoundType.Death]);
        Assert.Empty(sim.CaptureState().Actors.Single(a => a.Id == missile.Id).ActorSounds);
        var bytes = SimSavegame.Write(sim); var id = missile.Id; missile.Destroy();
        SimSavegame.Apply(sim, bytes); missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        ActorSoundActions.Scream(missile);
        Assert.Equal(death, Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Fact]
    public void PatchedNamesAndRuntimeSilenceSurviveSaves()
    {
        var sim = Room(DehackedPatch.Apply("Thing 35\nAlert sound = custom/launch\nDeath sound = custom/death\n"));
        var owner = sim.AddBot(0, 0); var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        Assert.Equal("custom/launch", missile.ActorSounds[ActorSoundType.See]);
        var baseline = SimSavegame.Write(sim); missile.ActorSounds = new Dictionary<ActorSoundType, string>();
        sim.Tick(); var silentHash = sim.Checksum;
        var silent = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline);
        Assert.Equal("custom/death", missile.ActorSounds[ActorSoundType.Death]);
        SimSavegame.Apply(sim, silent); Assert.Equal(silentHash, sim.Checksum); ActorSoundActions.Scream(missile); Assert.Empty(sim.DrainSoundRequests());
        Assert.Equal(silent, SimSavegame.Write(sim));
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = "runtime/death" };
        var bytes = SimSavegame.Write(sim); var id = missile.Id; missile.Destroy(); SimSavegame.Apply(sim, bytes);
        missile = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == id);
        ActorSoundActions.Scream(missile); Assert.Equal("runtime/death", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, dehacked: patch);
}
