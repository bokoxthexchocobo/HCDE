namespace HCDE.Playsim.Tests;

public class VmPDestructibleGeometryCombatTests
{
    [Fact]
    public void CheckLinedefVulnerable_RejectsHorizon()
    {
        Assert.False(VmPDestructibleVulnerability.CheckLinedefVulnerable(9, true));
        Assert.True(VmPDestructibleVulnerability.CheckLinedefVulnerable(0, true));
    }

    [Fact]
    public void CheckSectorVulnerable_RejectsSkyFlat()
    {
        Assert.False(VmPDestructibleVulnerability.CheckSectorVulnerable(
            VmPDestructibleHealthGroups.SecPartFloor, 1, 1, isSkyFlat: true));
        Assert.True(VmPDestructibleVulnerability.CheckSectorVulnerable(
            VmPDestructibleHealthGroups.SecPart3D, 0, 1, isSkyFlat: false));
    }

    [Fact]
    public void GeometryLineAttack_DamagesLineOnWallHit()
    {
        var lineDamage = 0;
        VmPDestructibleGeometryLineAttack.GeometryLineAttack(new VmPDestructibleGeometryLineAttack.GeometryLineAttackContext
        {
            HitType = VmPDestructibleGeometryLineAttack.TraceHitWall,
            Tier = VmPDestructibleGeometryLineAttack.TierLower,
            LineHealth = 40,
            LineVulnerable = true,
            Damage = 10,
            DamageLine = d => lineDamage = d,
        });
        Assert.Equal(10, lineDamage);
    }

    [Fact]
    public void GeometryLineAttack_SkipsWhenHit3DFloorsBlockSector()
    {
        var calls = 0;
        VmPDestructibleGeometryLineAttack.GeometryLineAttack(new VmPDestructibleGeometryLineAttack.GeometryLineAttackContext
        {
            HitType = VmPDestructibleGeometryLineAttack.TraceHitFloor,
            SectorFloorHealth = 50,
            SectorFloorVulnerable = true,
            Hit3DFloorsBlockSector = true,
            Damage = 10,
            DamageSector = (_, _) => calls++,
        });
        Assert.Equal(0, calls);
    }

    [Fact]
    public void ProjectileHitLinedef_DamagesLineWhenVulnerable()
    {
        var dmg = 0;
        Assert.True(VmPDestructibleProjectileHits.ProjectileHitLinedef(new VmPDestructibleProjectileHits.ProjectileHitLinedefContext
        {
            LineHealth = 50,
            LineVulnerable = true,
            MissileDamage = 7,
            DamageLine = d => dmg = d,
        }));
        Assert.Equal(7, dmg);
    }

    [Fact]
    public void ProjectileHitPlane_DamagesFloorWhenBlocking()
    {
        var dmg = 0;
        Assert.True(VmPDestructibleProjectileHits.ProjectileHitPlane(new VmPDestructibleProjectileHits.ProjectileHitPlaneContext
        {
            BlockingFloor = true,
            SectorFloorHealth = 30,
            FloorVulnerable = true,
            MissileDamage = 5,
            DamageSectorFloor = d => dmg = d,
        }));
        Assert.Equal(5, dmg);
    }
}
