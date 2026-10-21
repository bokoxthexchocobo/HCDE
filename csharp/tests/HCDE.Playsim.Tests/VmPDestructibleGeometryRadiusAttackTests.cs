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
}
