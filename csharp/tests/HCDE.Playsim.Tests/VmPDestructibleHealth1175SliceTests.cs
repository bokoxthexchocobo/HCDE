namespace HCDE.Playsim.Tests;

public class VmPDestructibleHealth1175SliceTests
{
    [Fact]
    public void Group_CopiesPlaneHealthAndSpreadsLineDamage()
    {
        var level = new VmPDestructibleHealth1175Slice.Level();
        var group = new VmPDestructibleHealth1175Slice.HealthGroup { Id = 7, Health = 10 };
        var lineA = new VmPDestructibleHealth1175Slice.GeoLine
        {
            Health = 10,
            HealthGroup = 7,
            Activation = VmPDestructibleHealth1175Slice.SpacDamage | VmPDestructibleHealth1175Slice.SpacDeath,
            Level = level,
        };
        var lineB = new VmPDestructibleHealth1175Slice.GeoLine
        {
            Health = 3,
            HealthGroup = 7,
            Activation = VmPDestructibleHealth1175Slice.SpacDamage,
            Level = level,
        };
        var sector = new VmPDestructibleHealth1175Slice.GeoSector
        {
            HealthCeiling = 1,
            HealthCeilingGroup = 7,
            HealthFloor = 2,
            HealthFloorGroup = 9,
            Health3D = 4,
            Health3DGroup = 7,
            HasAction = true,
            Level = level,
        };
        group.Lines.Add(lineA);
        group.Lines.Add(lineB);
        group.Sectors.Add(sector);
        level.Groups[7] = group;

        VmPDestructibleHealth1175Slice.SetHealth(group, 12);
        Assert.Equal(12, lineA.Health);
        Assert.Equal(12, lineB.Health);
        Assert.Equal(12, sector.HealthCeiling);
        Assert.Equal(2, sector.HealthFloor);
        Assert.Equal(12, sector.Health3D);

        VmPDestructibleHealth1175Slice.SetHealth((VmPDestructibleHealth1175Slice.HealthGroup?)null, 1);
        VmPDestructibleHealth1175Slice.SetHealth(level, 99, 1);
        Assert.Equal(12, group.Health);

        var host = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleHealth1175Slice.DamageLine(lineA, 4, 1, false, true, host);
        Assert.Equal(8, lineA.Health);
        Assert.Equal(8, lineB.Health);
        Assert.Equal(8, group.Health);
        Assert.Equal(8, sector.HealthCeiling);
        Assert.Equal(2, sector.HealthFloor);
        Assert.Equal(8, sector.Health3D);
        Assert.Equal(
            new[]
            {
                "world-line:4",
                "activate:" + VmPDestructibleHealth1175Slice.SpacDamage + ":1",
                "activate:" + VmPDestructibleHealth1175Slice.SpacDamage + ":1",
                "trigger:" + VmPDestructibleHealth1175Slice.SecDamageCeiling,
                "trigger:" + VmPDestructibleHealth1175Slice.SecDamage3D,
            },
            host.Events);

        var dying = new VmPDestructibleHealth1175Slice.GeoLine
        {
            Health = 5,
            Activation = VmPDestructibleHealth1175Slice.SpacDeath,
            Level = level,
        };
        var death = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleHealth1175Slice.DamageLine(dying, 5, 0, false, false, death);
        Assert.Equal(0, dying.Health);
        Assert.Equal(new[] { "activate:" + VmPDestructibleHealth1175Slice.SpacDeath + ":0" }, death.Events);

        var raised = new VmPDestructibleHealth1175Slice.GeoLine { Health = 10, HealthGroup = 7, Level = level };
        var world = new VmPDestructibleHealth1175Slice.DamageHost { HasLineWorld = true, LineWorldDamage = 4 };
        VmPDestructibleHealth1175Slice.DamageLine(raised, -5, 2, true, true, world);
        Assert.Equal(6, raised.Health);
        Assert.Equal("world-line:0", world.Events[0]);
        Assert.Contains("activate:" + VmPDestructibleHealth1175Slice.SpacDamage + ":2", world.Events);
    }

    [Fact]
    public void Sector_ClampsDamageAndSkipsThePlaneThatWasHit()
    {
        var level = new VmPDestructibleHealth1175Slice.Level();
        var group = new VmPDestructibleHealth1175Slice.HealthGroup { Id = 7, Health = 10 };
        var sector = new VmPDestructibleHealth1175Slice.GeoSector
        {
            HealthCeiling = 10,
            HealthFloor = 10,
            HealthCeilingGroup = 7,
            HealthFloorGroup = 7,
            HasAction = true,
            Level = level,
        };
        group.Sectors.Add(sector);
        level.Groups[7] = group;
        var host = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleHealth1175Slice.DamageSector(sector, 4, VmPDestructibleHealth1175Slice.SecPartCeiling, false, true, host);
        Assert.Equal(6, sector.HealthCeiling);
        Assert.Equal(6, sector.HealthFloor);
        Assert.Equal(6, group.Health);
        Assert.Equal(
            new[]
            {
                "world-sector:4",
                "trigger:" + VmPDestructibleHealth1175Slice.SecDamageCeiling,
                "trigger:" + VmPDestructibleHealth1175Slice.SecDamageFloor,
            },
            host.Events);

        var broken = new VmPDestructibleHealth1175Slice.GeoSector
        {
            HealthCeiling = 10,
            HealthCeilingGroup = 7,
            HasAction = true,
            Level = level,
        };
        group.Health = 10;
        group.Sectors.Clear();
        group.Sectors.Add(broken);
        var killed = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleHealth1175Slice.DamageSector(broken, 10, VmPDestructibleHealth1175Slice.SecPartCeiling, true, true, killed);
        Assert.Equal(0, broken.HealthCeiling);
        Assert.Contains("trigger:" + VmPDestructibleHealth1175Slice.SecDeathCeiling, killed.Events);

        var ignored = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10, Level = level };
        var badPart = new VmPDestructibleHealth1175Slice.DamageHost { HasSectorWorld = true, SectorWorldDamage = 4 };
        VmPDestructibleHealth1175Slice.DamageSector(ignored, 9, 99, false, true, badPart);
        Assert.Equal(10, ignored.HealthFloor);
        Assert.Equal(new[] { "world-sector:9" }, badPart.Events);

        var held = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 8, HealthFloorGroup = 3, HasAction = true, Level = level };
        var zero = new VmPDestructibleHealth1175Slice.DamageHost { HasSectorWorld = true, SectorWorldDamage = 0 };
        VmPDestructibleHealth1175Slice.DamageSector(held, 5, VmPDestructibleHealth1175Slice.SecPartFloor, false, true, zero);
        Assert.Equal(8, held.HealthFloor);
        Assert.Equal(new[] { "world-sector:5" }, zero.Events);

        var created = VmPDestructibleHealth1175Slice.GetOrNew(level, 3, 15);
        var again = VmPDestructibleHealth1175Slice.GetOrNew(level, 3, 1);
        Assert.Same(created, again);
        Assert.Equal(15, again.Health);
        Assert.Null(VmPDestructibleHealth1175Slice.GetGroup(level, 40));
    }
}
