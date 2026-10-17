using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileMonsterDefaultsTests
{
    [Theory]
    [InlineData("COUNTKILL+SHOOTABLE+NOGRAVITY", true, true)]
    [InlineData("COUNTKILL+SHOOTABLE+NOGRAVITY+NOTELEPORT", true, true)]
    [InlineData("SHOOTABLE+NOGRAVITY+NOTELEPORT", false, false)]
    [InlineData("SHOOTABLE+NOGRAVITY+PASSMOBJ", false, true)]
    public void ExplicitCountkillControlsMonsterIdentityAndForcesPassmobj(string bits, bool monster, bool pass)
    {
        var (sim, missile) = Room(bits); Assert.Equal(monster, missile.IsMonster); Assert.Equal(pass, missile.CanPass);
        Assert.Null(missile.Brain);
        var bytes = SimSavegame.Write(sim, true); missile.IsMonster = !monster; missile.CanPass = !pass;
        SimSavegame.Apply(sim, bytes); Assert.Equal(monster, missile.IsMonster); Assert.Equal(pass, missile.CanPass);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData("COUNTKILL+SHOOTABLE+NOGRAVITY", true)]
    [InlineData("SHOOTABLE+NOGRAVITY", false)]
    public void PatchedProjectileCanBecomeRoughSearchMonsterCandidate(string bits, bool eligible)
    {
        var (sim, missile) = Room(bits); var observer = sim.AddBot(192, 192);
        missile.SetXYZ(210, 210, 0);
        Assert.Equal(eligible, observer.IsOkayToAttack(missile));
        Assert.Equal(eligible ? missile : null, ActorEnemySearchActions.RoughMonsterSearch(observer, 0));
    }

    [Fact]
    public void CountkillPassmobjSideEffectMatchesMapAndDynamicDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = COUNTKILL+SHOOTABLE+NOTELEPORT\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var map = sim.Actors.Single(); var dynamic = sim.AddBot(300, 0);
        Assert.True(map.IsMonster); Assert.True(map.CanPass); Assert.True(dynamic.IsMonster); Assert.True(dynamic.CanPass);
        var saved = SimSavegame.Write(sim, true); map.CanPass = dynamic.CanPass = false;
        SimSavegame.Apply(sim, saved); Assert.True(map.CanPass); Assert.True(dynamic.CanPass);
    }

    [Fact]
    public void PlayerCountkillPatchDoesNotTurnPlayerIntoMonster()
    {
        var patch = DehackedPatch.Apply("Thing 1\nBits = COUNTKILL+SHOOTABLE+NOTELEPORT\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        Assert.False(sim.Players.Single().IsMonster); Assert.True(sim.Players.Single().Friendly);
    }

    [Fact]
    public void NoncountkillPatchDoesNotClearAnExplicitPassmobjBit()
    {
        var (_, missile) = Room("SHOOTABLE+NOGRAVITY+PASSMOBJ");
        Assert.False(missile.IsMonster); Assert.True(missile.CanPass);
    }

    [Fact]
    public void UnpatchedPrimaryFlagRecordKeepsProjectileNonmonster()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 35\nSpeed = 12\n"));
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.False(missile.IsMonster); Assert.False(missile.CanPass); Assert.Null(missile.Brain);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = 600 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 6, 6), []) }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
