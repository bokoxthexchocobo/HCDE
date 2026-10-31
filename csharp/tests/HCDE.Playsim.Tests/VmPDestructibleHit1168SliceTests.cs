namespace HCDE.Playsim.Tests;

public class VmPDestructibleHit1168SliceTests
{
    [Fact]
    public void LinedefHit_DamagesTheNearPlaneAndTheWall()
    {
        var floor = new VmPDestructibleHit1168Slice.Sector { HealthFloor = 4, HealthCeiling = 2 };
        var solid = new VmPDestructibleHit1168Slice.Sector { Health3D = 6 };
        var line = new VmPDestructibleHit1168Slice.Line
        {
            Health = 3,
            V1 = new VmPDestructibleHit1168Slice.Vec2(0, 0),
            Delta = new VmPDestructibleHit1168Slice.Vec2(10, 0),
        };
        line.Sides[0] = new VmPDestructibleHit1168Slice.Side();
        line.Sides[1] = new VmPDestructibleHit1168Slice.Side { Sector = floor };
        VmPDestructibleHit1168Slice.Vec2? sampled = null;
        floor.FloorZ = point =>
        {
            sampled = point;
            return 10;
        };
        floor.CeilingZ = _ => 20;
        var factors = new List<int>();
        var missile = new VmPDestructibleHit1168Slice.Missile
        {
            Flags4 = VmPDestructibleHit1168Slice.Mf4StrifeDamage,
            DamageType = "Fire",
            X = 5,
            Y = 3,
            Z = 10,
            Height = 10,
            Blocking3D = solid,
            MissileDamage = (factor, scale) =>
            {
                factors.Add(factor);
                Assert.Equal(1, scale);
                return factor + 4;
            },
        };
        var hits = new List<VmPDestructibleHit1168Slice.Hit>();
        Assert.True(VmPDestructibleHit1168Slice.ProjectileHitLinedef(missile, line, (_, _) => 0, hits));
        Assert.Equal(new VmPDestructibleHit1168Slice.Vec2(5, 6), sampled);
        Assert.Equal(new[] { 3, 3, 3, 3 }, factors);
        Assert.Equal(VmPDestructibleHit1168Slice.SecPart3D, hits[0].Part);
        Assert.Same(solid, hits[0].Sector);
        Assert.Equal(VmPDestructibleHit1168Slice.SecPartFloor, hits[1].Part);
        Assert.Equal(VmPDestructibleHit1168Slice.SecPartCeiling, hits[2].Part);
        Assert.Equal("line", hits[3].Kind);
        Assert.Equal(0, hits[3].Side);
        Assert.False(hits[3].Radius);
        Assert.True(hits[3].Groups);
        Assert.Equal(7, hits[3].Damage);

        missile.Z = 10 + VmPDestructibleHit1168Slice.EqualEpsilon;
        missile.Blocking3D = null;
        missile.Flags4 = 0;
        hits.Clear();
        factors.Clear();
        Assert.True(VmPDestructibleHit1168Slice.ProjectileHitLinedef(missile, line, (_, _) => 0, hits));
        Assert.DoesNotContain(hits, hit => hit.Kind == "sector" && hit.Part == VmPDestructibleHit1168Slice.SecPartFloor);
        Assert.Contains(hits, hit => hit.Part == VmPDestructibleHit1168Slice.SecPartCeiling);
        Assert.Equal(7, factors[0]);

        line.Special = VmPDestructibleHit1168Slice.LineHorizon;
        hits.Clear();
        Assert.False(VmPDestructibleHit1168Slice.ProjectileHitLinedef(
            missile,
            line,
            (_, _) => 0,
            hits));
        Assert.Empty(hits);
    }

    [Fact]
    public void PlaneHit_PrefersABlockingCeilingAndStopsOnAnEmptyThreeDFloor()
    {
        var sector = new VmPDestructibleHit1168Slice.Sector
        {
            HealthFloor = 5,
            HealthCeiling = 8,
            FloorTexture = 2,
            CeilingTexture = 4,
        };
        var missile = new VmPDestructibleHit1168Slice.Missile
        {
            Sector = sector,
            BlockingCeiling = true,
            BlockingFloor = true,
            DamageType = "Hitscan",
            X = 1,
            Y = 2,
            Z = 3,
            MissileDamage = (factor, _) => factor,
        };
        var hits = new List<VmPDestructibleHit1168Slice.Hit>();
        Assert.True(VmPDestructibleHit1168Slice.ProjectileHitPlane(missile, -1, 9, hits));
        Assert.Equal(VmPDestructibleHit1168Slice.SecPartCeiling, hits[0].Part);
        Assert.Equal(VmPDestructibleHit1168Slice.SecPartCeiling, sector.LastPlanePart);

        sector.CeilingTexture = 9;
        hits.Clear();
        Assert.False(VmPDestructibleHit1168Slice.ProjectileHitPlane(missile, -1, 9, hits));
        Assert.Empty(hits);

        var empty = new VmPDestructibleHit1168Slice.Sector { Health3D = 0 };
        missile.Blocking3D = empty;
        Assert.False(VmPDestructibleHit1168Slice.ProjectileHitPlane(missile, VmPDestructibleHit1168Slice.SecPartFloor, 9, hits));
        Assert.Empty(hits);

        empty.Health3D = 2;
        Assert.True(VmPDestructibleHit1168Slice.ProjectileHitPlane(missile, VmPDestructibleHit1168Slice.SecPartFloor, 9, hits));
        Assert.Equal(VmPDestructibleHit1168Slice.SecPart3D, hits[0].Part);
        Assert.True(VmPDestructibleHit1168Slice.CheckSectorVulnerable(sector, VmPDestructibleHit1168Slice.SecPart3D, 9));
        Assert.False(VmPDestructibleHit1168Slice.CheckLinedefVulnerable(
            new VmPDestructibleHit1168Slice.Line { Special = VmPDestructibleHit1168Slice.LineHorizon },
            0));
        var open = new VmPDestructibleHit1168Slice.Line();
        open.Sides[1] = new VmPDestructibleHit1168Slice.Side();
        Assert.False(VmPDestructibleHit1168Slice.CheckLinedefVulnerable(open, 0, VmPDestructibleHit1168Slice.SecPartFloor));
        Assert.True(VmPDestructibleHit1168Slice.CheckLinedefVulnerable(open, 1));
    }

    [Fact]
    public void Groups_CopyHealthOntoAnExistingIdOnly()
    {
        var kept = new VmPDestructibleHit1168Slice.HealthGroup { Id = 4, Health = 10 };
        kept.Sectors.Add(new VmPDestructibleHit1168Slice.Sector());
        kept.Lines.Add(new VmPDestructibleHit1168Slice.Line());
        var other = new VmPDestructibleHit1168Slice.HealthGroup { Id = 8, Health = 1 };
        var closed = new VmPDestructibleHit1168Slice.GroupArchive();
        VmPDestructibleHit1168Slice.SerializeHealthGroups(new[] { kept }, closed, id => kept);
        Assert.Empty(closed.Written);
        Assert.Equal(10, kept.Health);

        var reading = new VmPDestructibleHit1168Slice.GroupArchive
        {
            Reading = true,
            OpenArray = true,
            Saved = { (4, 25), (99, 3) },
        };
        VmPDestructibleHit1168Slice.SerializeHealthGroups(new[] { kept, other }, reading, id => id == 4 ? kept : null);
        Assert.Equal(25, kept.Health);
        Assert.Equal(1, other.Health);
        Assert.Single(kept.Sectors);

        var writing = new VmPDestructibleHit1168Slice.GroupArchive { OpenArray = true };
        VmPDestructibleHit1168Slice.SerializeHealthGroups(new[] { other, kept }, writing, _ => null);
        Assert.Equal(new[] { (8, 1), (4, 25) }, writing.Written);

        var one = new VmPDestructibleHit1168Slice.GroupArchive { OpenObject = true };
        VmPDestructibleHit1168Slice.SerializeGroup(one, kept);
        Assert.Equal((4, 25), one.Written[0]);
        Assert.Null(VmPDestructibleHit1168Slice.FindHealthGroup(_ => null, 4));
        Assert.Same(kept, VmPDestructibleHit1168Slice.FindHealthGroup(id => id == 4 ? kept : null, 4));
    }

    [Fact]
    public void Actions_ApplyHealthAndDamageTheNamedSector()
    {
        var group = new VmPDestructibleHit1168Slice.HealthGroup { Id = 2, Health = 9 };
        var applied = 0;
        Assert.Equal(0, VmPDestructibleHit1168Slice.SetHealth(group, 14, (target, health) =>
        {
            Assert.Same(group, target);
            applied = health;
        }));
        Assert.Equal(14, applied);
        Assert.Equal(9, group.Health);

        var sector = new VmPDestructibleHit1168Slice.Sector();
        var source = new VmPDestructibleHit1168Slice.Missile();
        var hits = new List<VmPDestructibleHit1168Slice.Hit>();
        Assert.Equal(0, VmPDestructibleHit1168Slice.DamageSectorAction(
            sector,
            source,
            6,
            "Lava",
            VmPDestructibleHit1168Slice.SecPartFloor,
            1.5,
            -2,
            3.25,
            true,
            hits));
        Assert.Same(sector, hits[0].Sector);
        Assert.Same(source, hits[0].Source);
        Assert.Equal(6, hits[0].Damage);
        Assert.Equal("Lava", hits[0].DamageType);
        Assert.Equal(1.5, hits[0].X);
        Assert.Equal(-2, hits[0].Y);
        Assert.Equal(3.25, hits[0].Z);
        Assert.True(hits[0].Radius);
        Assert.True(hits[0].Groups);
    }
}
