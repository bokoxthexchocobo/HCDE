namespace HCDE.Playsim.Tests;

public class VmPDestructibleGeometry1176SliceTests
{
    [Fact]
    public void Init_KeepsTheHighestHealthAndSharesOneSector()
    {
        var level = new VmPDestructibleHealth1175Slice.Level();
        var host = new VmPDestructibleGeometry1176Slice.GeometryHost();
        host.Lines.Add(new VmPDestructibleHealth1175Slice.GeoLine { Health = 4, HealthGroup = 2 });
        host.Lines.Add(new VmPDestructibleHealth1175Slice.GeoLine { Health = 9, HealthGroup = 0 });
        host.Lines.Add(new VmPDestructibleHealth1175Slice.GeoLine { Health = 1, HealthGroup = 2 });
        host.Sectors.Add(new VmPDestructibleHealth1175Slice.GeoSector
        {
            HealthCeiling = 5,
            HealthCeilingGroup = 1,
            HealthFloor = 9,
            HealthFloorGroup = 1,
        });
        host.Sectors.Add(new VmPDestructibleHealth1175Slice.GeoSector
        {
            HealthCeiling = 2,
            HealthCeilingGroup = 3,
            HealthFloor = 2,
            HealthFloorGroup = 4,
        });
        VmPDestructibleGeometry1176Slice.Init(level, host);
        Assert.Equal(4, level.Groups[2].Health);
        Assert.Equal(2, level.Groups[2].Lines.Count);
        Assert.DoesNotContain(level.Groups[2].Lines, line => line.HealthGroup == 0);
        Assert.Equal(9, level.Groups[1].Health);
        Assert.Single(level.Groups[1].Sectors);
        Assert.Single(level.Groups[3].Sectors);
        Assert.Single(level.Groups[4].Sectors);
        Assert.Equal(
            new[]
            {
                VmPDestructibleGeometry1176Slice.TextRed + "Warning: line 2 in health group 2 has different starting health (line health = 1, group health = 4)\n",
                VmPDestructibleGeometry1176Slice.TextRed + "Warning: sector floor 0 in health group 1 has different starting health (floor health = 9, group health = 5)\n",
                VmPDestructibleGeometry1176Slice.TextGold + "Health group 2 is using the highest found health value of 4",
                VmPDestructibleGeometry1176Slice.TextGold + "Health group 1 is using the highest found health value of 9",
            },
            host.Warnings);
    }

    [Fact]
    public void Attack_DamagesThe3DFloorAndStopsAtThePlane()
    {
        var model = new VmPDestructibleHealth1175Slice.GeoSector { Health3D = 8 };
        var line = new VmPDestructibleHealth1175Slice.GeoLine { Health = 10 };
        var wall = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitWall,
            Tier = VmPDestructibleGeometry1176Slice.TierFFloor,
            Model = model,
            Line = line,
        };
        var host = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleGeometry1176Slice.LineAttack(wall, 3, host);
        Assert.Equal(5, model.Health3D);
        Assert.Equal(7, line.Health);
        Assert.Equal(new[] { "world-sector:3", "world-line:3" }, host.Events);

        var back = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10 };
        var lowerLine = new VmPDestructibleHealth1175Slice.GeoLine { Health = 10 };
        var lower = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitWall,
            Tier = VmPDestructibleGeometry1176Slice.TierLower,
            Back = back,
            Line = lowerLine,
            Side = 1,
        };
        var lowerHost = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleGeometry1176Slice.LineAttack(lower, 2, lowerHost);
        Assert.Equal(8, back.HealthFloor);
        Assert.Equal(8, lowerLine.Health);
        Assert.Equal(new[] { "world-sector:2", "world-line:2" }, lowerHost.Events);

        var blocked = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitWall,
            Tier = VmPDestructibleGeometry1176Slice.TierLower,
            LineVulnerable = false,
            Line = new VmPDestructibleHealth1175Slice.GeoLine { Health = 10 },
            Back = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10 },
        };
        var quiet = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleGeometry1176Slice.LineAttack(blocked, 4, quiet);
        Assert.Equal(10, blocked.Line.Health);
        Assert.Equal(10, blocked.Back.HealthFloor);
        Assert.Empty(quiet.Events);

        var plane = new VmPDestructibleHealth1175Slice.GeoSector { Health3D = 6 };
        var floor = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10 };
        var shot = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitFloor,
            Z = 4,
            Sector = floor,
        };
        shot.Floors.Add(new VmPDestructibleGeometry1176Slice.Floor3D
        {
            Flags = VmPDestructibleGeometry1176Slice.FfExists | VmPDestructibleGeometry1176Slice.FfSolid,
            Model = plane,
            TopZ = 4,
        });
        var shotHost = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleGeometry1176Slice.LineAttack(shot, 1, shotHost);
        Assert.Equal(5, plane.Health3D);
        Assert.Equal(10, floor.HealthFloor);
        Assert.Equal(new[] { "world-sector:1" }, shotHost.Events);

        var empty = new VmPDestructibleHealth1175Slice.GeoSector { Health3D = 0 };
        var under = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10 };
        var covered = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitFloor,
            Z = 4,
            Sector = under,
        };
        covered.Floors.Add(new VmPDestructibleGeometry1176Slice.Floor3D
        {
            Flags = VmPDestructibleGeometry1176Slice.FfExists | VmPDestructibleGeometry1176Slice.FfSolid,
            Model = empty,
            TopZ = 4,
        });
        var coveredHost = new VmPDestructibleHealth1175Slice.DamageHost();
        VmPDestructibleGeometry1176Slice.LineAttack(covered, 3, coveredHost);
        Assert.Equal(0, empty.Health3D);
        Assert.Equal(10, under.HealthFloor);
        Assert.Empty(coveredHost.Events);

        var open = new VmPDestructibleHealth1175Slice.GeoSector { HealthFloor = 10 };
        var missed = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitFloor,
            Z = 4,
            Sector = open,
        };
        missed.Floors.Add(new VmPDestructibleGeometry1176Slice.Floor3D
        {
            Flags = VmPDestructibleGeometry1176Slice.FfExists | VmPDestructibleGeometry1176Slice.FfSolid | VmPDestructibleGeometry1176Slice.FfShootThrough,
            Model = new VmPDestructibleHealth1175Slice.GeoSector { Health3D = 6 },
            TopZ = 4,
        });
        VmPDestructibleGeometry1176Slice.LineAttack(missed, 4, new VmPDestructibleHealth1175Slice.DamageHost());
        Assert.Equal(6, open.HealthFloor);

        var ceilModel = new VmPDestructibleHealth1175Slice.GeoSector { Health3D = 4 };
        var ceilSector = new VmPDestructibleHealth1175Slice.GeoSector { HealthCeiling = 10 };
        var ceiling = new VmPDestructibleGeometry1176Slice.Hit
        {
            HitType = VmPDestructibleGeometry1176Slice.TraceHitCeiling,
            Z = 8,
            Sector = ceilSector,
        };
        ceiling.Floors.Add(new VmPDestructibleGeometry1176Slice.Floor3D
        {
            Flags = VmPDestructibleGeometry1176Slice.FfExists | VmPDestructibleGeometry1176Slice.FfSolid,
            Model = ceilModel,
            BottomZ = 8,
        });
        VmPDestructibleGeometry1176Slice.LineAttack(ceiling, 1, new VmPDestructibleHealth1175Slice.DamageHost());
        Assert.Equal(3, ceilModel.Health3D);
        Assert.Equal(10, ceilSector.HealthCeiling);
    }

    [Fact]
    public void Helpers_ClampTheSegmentAndKeepTheNearerGroup()
    {
        var onLine = VmPDestructibleGeometry1176Slice.ClosestPoint(5, 4, 0, 0, 10, 0);
        Assert.Equal(5d, onLine.X);
        Assert.Equal(0d, onLine.Y);
        var before = VmPDestructibleGeometry1176Slice.ClosestPoint(-1, 0, 0, 0, 10, 0);
        Assert.Equal(0d, before.X);
        var after = VmPDestructibleGeometry1176Slice.ClosestPoint(12, 3, 0, 0, 10, 0);
        Assert.Equal(10d, after.X);
        Assert.Equal(0d, after.Y);

        var host = new VmPDestructibleGeometry1176Slice.GeometryHost();
        var line = new VmPDestructibleHealth1175Slice.GeoLine();
        VmPDestructibleGeometry1176Slice.InsertIfCloser(1, 3, 0, 0, 0, 0, 0, line, null, 0, host);
        VmPDestructibleGeometry1176Slice.InsertIfCloser(1, 1, 0, 0, 0, 0, 0, line, null, 2, host);
        VmPDestructibleGeometry1176Slice.InsertIfCloser(1, -1, 0, 0, 0, 0, 0, line, null, 9, host);
        VmPDestructibleGeometry1176Slice.InsertIfCloser(1, 4, 0, 0, 0, 0, 0, line, null, 1, host);
        Assert.Equal(1d, host.Points[1].X);
        Assert.Equal(2, host.Points[1].Part);
        Assert.Same(line, host.Points[1].Line);

        host.TraceHits = true;
        VmPDestructibleGeometry1176Slice.InsertIfCloser(2, 1, 0, 0, 0, 0, 0, line, null, 0, host);
        Assert.DoesNotContain(2, host.Points.Keys);

        var blocked = new VmPDestructibleGeometry1176Slice.GeometryHost { TraceHits = true };
        Assert.True(VmPDestructibleGeometry1176Slice.ExplosionBlocked(0, 0, 0, 0, 0, 0, blocked));
        Assert.NotNull(blocked.LastProbe);
        Assert.Equal(VmPDestructibleGeometry1176Slice.EqualEpsilon, blocked.LastProbe.Z);
        Assert.Equal(VmPDestructibleGeometry1176Slice.MlBlockEverything, blocked.LastProbe.Flags);
        Assert.True(blocked.LastProbe.Distance < 0);
        Assert.Equal(-1d, blocked.LastProbe.Dz);
    }
}
