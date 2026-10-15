using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombatFloorClipTests
{
    [Theory]
    [InlineData(8, false, 8, 28, 24)]
    [InlineData(8, false, -4, 40, 36)]
    [InlineData(8, true, 8, 10, 8)]
    [InlineData(16, true, 8, 14, 12)]
    public void PlayerHitscanAndMissilesUseSignedClipWithCrouch(int offset, bool crouched, int clip, double hitZ, double missileZ)
    {
        var sim = Room(); var player = sim.Players.Single(); player.AttackZOffset = Fixed.FromInt(offset);
        if (crouched) for (var i = 0; i < 6; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        player.FloorClip = clip;
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 64);
        Assert.Equal(hitZ, hit.Z, 6); Assert.Equal(missileZ, sim.SpawnProjectile(player, ProjectileKind.Rocket).Z.ToDouble(), 4);
    }

    [Theory]
    [InlineData(ProjectileKind.ImpBall, 8, 24)]
    [InlineData(ProjectileKind.RevenantTracer, 8, 40)]
    [InlineData(ProjectileKind.ImpBall, -4, 36)]
    public void MonsterMissileOriginsIncludeFloorClip(ProjectileKind kind, int clip, int expected)
    {
        var sim = Room(); var actor = sim.AddBot(128, 0); actor.FloorClip = clip;
        Assert.Equal(expected, sim.SpawnProjectile(actor, kind).Z.ToDouble());
    }

    [Fact]
    public void PlayerMissileRetainsFloorClampAfterClipOffset()
    {
        var sim = Room(); var player = sim.Players.Single(); player.FloorClip = 64;
        Assert.Equal(0, sim.SpawnProjectile(player, ProjectileKind.Rocket).Z.ToDouble());
    }

    [Fact]
    public void ClipChangesWhichThinTargetCanBeHit()
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(128, 0);
        target.Z = Fixed.FromInt(27); target.Height = Fixed.FromInt(2);
        Assert.Null(CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256).Victim);
        source.FloorClip = 8;
        Assert.Same(target, CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256).Victim);
    }

    [Fact]
    public void FriendInterceptionUsesSameClippedOriginAsAttackTrace()
    {
        var sim = Room(); var enemy = sim.Players.Single(); enemy.X = Fixed.FromInt(128);
        var source = sim.AddBot(0, 0); source.Friendly = true; source.FloorClip = 16;
        var friend = sim.AddBot(64, 0); friend.Friendly = true; friend.Z = Fixed.FromInt(23); friend.Height = Fixed.FromInt(2);
        Assert.True(MonsterBrain.HitFriend(sim, source, enemy));
        source.FloorClip = 0; Assert.False(MonsterBrain.HitFriend(sim, source, enemy));
    }

    [Fact]
    public void SavedClipReplaysAttackHeightAndProjectileSpawn()
    {
        var sim = Room(); var player = sim.Players.Single(); player.FloorClip = 8;
        var saved = SimSavegame.Write(sim, true);
        var shot = sim.SpawnProjectile(player, ProjectileKind.Rocket); var z = shot.Z;
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 64);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(z, sim.SpawnProjectile(player, ProjectileKind.Rocket).Z);
        Assert.Equal(hit.Z, CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 64).Z);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
