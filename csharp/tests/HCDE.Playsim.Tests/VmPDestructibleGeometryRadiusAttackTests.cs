namespace HCDE.Playsim.Tests;

public class VmPDestructibleGeometryRadiusAttackTests
{
    [Fact]
    public void GeometryRadiusAttack_InsertsCeilingGroup()
    {
        var ctx = new VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttackContext
        {
            BombSpot = new object(),
            BombX = 100,
            BombY = 200,
            BombZ = 50,
            SourceSector = new VmPDestructibleGeometryRadiusAttack.SectorGeometry(
                1, HealthCeiling: 10, HealthFloor: 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0),
        };
        VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttack(ctx);
        Assert.NotEmpty(ctx.DamageGroups);
    }

    [Fact]
    public void GeometryRadiusAttack_SkipsNullSpot()
    {
        var ctx = new VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttackContext();
        VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttack(ctx);
        Assert.Empty(ctx.DamageGroups);
    }

    [Fact]
    public void GeometryRadiusAttack_InsertsFloorGroup()
    {
        var ctx = new VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttackContext
        {
            BombSpot = new object(),
            BombX = 0,
            BombY = 0,
            BombZ = 0,
            SourceSector = new VmPDestructibleGeometryRadiusAttack.SectorGeometry(
                2, 0, 5, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0),
        };
        VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttack(ctx);
        Assert.Contains(ctx.DamageGroups.Keys, k => (k & VmPDestructibleGeometryRadiusAttack.GroupMaskFloor) != 0);
    }

    [Fact]
    public void ComputeGroupFalloffDamage_ZeroBeyondRadius()
    {
        var ctx = new VmPDestructibleGeometryRadiusAttack.GeometryRadiusAttackContext
        {
            BombSpot = new object(),
            BombDamage = 100,
            BombDistance = 64,
            FullDamageDistance = 0,
            BombX = 0,
            BombY = 0,
            BombZ = 0,
        };
        var inv = VmPDestructibleGeometryRadiusAttack.ComputeInverseDamageRange(64, 0);
        Assert.Equal(0, VmPDestructibleGeometryRadiusAttack.ComputeGroupFalloffDamage(ctx, 100, 0, 0, inv));
    }

    [Fact]
    public void FfloorEligible_RejectsInvertedGeometry()
    {
        var f = new VmPDestructibleGeometryRadiusAttack.FFloorGeometry(
            true, true, 10, 0, 1, 5, 10, 0, 0, 1, 0, 0, 0, 1, 0);
        Assert.False(VmPDestructibleGeometryRadiusAttack.FfloorEligible(f));
    }
}
