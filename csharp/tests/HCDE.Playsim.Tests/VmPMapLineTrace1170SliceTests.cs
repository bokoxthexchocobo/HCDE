namespace HCDE.Playsim.Tests;

public class VmPMapLineTrace1170SliceTests
{
    [Fact]
    public void Name_RejectsAnUnknownPuffAndForwardsAKnownOne()
    {
        var shooter = new VmPMapLineAttack1169Slice.Shooter();
        var victim = new VmPMapLineAttack1169Slice.LineTarget { Target = "stale", AttackAngle = 4, Unlinked = true };
        var damage = new VmPMapLineAttack1169Slice.DamageSlot { Value = 9 };
        var host = new VmPMapLineAttack1169Slice.AttackHost();
        var messages = new List<string>();
        Assert.Null(VmPMapLineTrace1170Slice.LineAttackByName(
            shooter, 0, 64, 0, 5, "None", "MissingPuff", 0, victim, damage, 4, 0, 0, host, _ => false, messages));
        Assert.Null(victim.Target);
        Assert.Equal(0, victim.AttackAngle);
        Assert.False(victim.Unlinked);
        Assert.Equal(9, damage.Value);
        Assert.Equal("Attempt to spawn unknown actor type 'MissingPuff'", Assert.Single(messages));
        Assert.Empty(host.Lag);

        victim.Target = "kept";
        messages.Clear();
        var known = new VmPMapLineAttack1169Slice.AttackHost { PreFired = true };
        Assert.Null(VmPMapLineTrace1170Slice.LineAttackByName(
            shooter, 0, 64, 0, 5, "Hitscan", "BulletPuff", 0, victim, damage, 4, 1, 2, known, _ => true, messages));
        Assert.Empty(messages);
        Assert.Equal("kept", victim.Target);
        Assert.Empty(known.Lag);
        Assert.Empty(known.Traces);
    }

    [Fact]
    public void Callback_SkipsPortalsAndSameSpecies()
    {
        var data = new VmPMapLineTrace1170Slice.LineData { CallerSpecies = "Imp", ThruSpecies = true };
        var portal = new VmPMapLineTrace1170Slice.TraceView { HitType = VmPMapLineTrace1170Slice.TraceCrossingPortal };
        Assert.Equal(VmPMapLineTrace1170Slice.TraceSkip, VmPMapLineTrace1170Slice.CheckLineTrace(portal, data));
        Assert.Equal(1, data.NumPortals);
        var wall = new VmPMapLineTrace1170Slice.TraceView { HitType = VmPMapLineTrace1170Slice.TraceHitWall };
        Assert.Equal(VmPMapLineTrace1170Slice.TraceStop, VmPMapLineTrace1170Slice.CheckLineTrace(wall, data));
        var same = new VmPMapLineTrace1170Slice.TraceView
        {
            HitType = VmPMapLineTrace1170Slice.TraceHitActor,
            Species = "Imp",
        };
        Assert.Equal(VmPMapLineTrace1170Slice.TraceSkip, VmPMapLineTrace1170Slice.CheckLineTrace(same, data));
        var other = new VmPMapLineTrace1170Slice.TraceView
        {
            HitType = VmPMapLineTrace1170Slice.TraceHitActor,
            Species = "Demon",
        };
        Assert.Equal(VmPMapLineTrace1170Slice.TraceStop, VmPMapLineTrace1170Slice.CheckLineTrace(other, data));
        data.ThruActors = true;
        Assert.Equal(VmPMapLineTrace1170Slice.TraceSkip, VmPMapLineTrace1170Slice.CheckLineTrace(other, data));
    }

    [Fact]
    public void Trace_PlacesTheShotAndFillsTheWall()
    {
        var shooter = new VmPMapLineTrace1170Slice.Shooter();
        var host = new VmPMapLineTrace1170Slice.TraceHost { TraceHit = true };
        var output = new VmPMapLineTrace1170Slice.TraceOut { LinePart = 9, HitTexture = 99, SectorPlane = -1 };
        var hit = VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 256, 0, 0, 4, 3, 1, output, host);
        Assert.True(hit);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(103, 199, 10), host.From);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(1, 0, 0), host.Direction);
        Assert.Equal(256d, host.Distance);
        Assert.Equal((uint)VmPMapLineTrace1170Slice.MfShootable, host.ActorMask);
        Assert.Equal((uint)(VmPMapLineTrace1170Slice.MlBlockEverything | VmPMapLineTrace1170Slice.MlBlockHitscan), host.WallMask);
        Assert.Equal(VmPMapLineTrace1170Slice.TraceReportPortals, host.TraceFlags);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(103, 199, 10), Assert.Single(host.SectorLooks));
        Assert.Equal(0, output.LinePart);
        Assert.Equal(-2, output.NumPortals);

        var absolute = new VmPMapLineTrace1170Slice.TraceHost();
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 256, 0, VmPMapLineTrace1170Slice.TrfAbsPosition, 4, 3, 1, null, absolute);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(3, 1, 4), absolute.From);
        var shifted = new VmPMapLineTrace1170Slice.TraceHost();
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 256, 0, VmPMapLineTrace1170Slice.TrfAbsOffset, 4, 3, 1, null, shifted);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(103, 201, 10), shifted.From);
        var origin = new VmPMapLineTrace1170Slice.TraceHost();
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 256, 0, 0, 4, 0, 0, null, origin);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(100, 200, 10), origin.From);

        var missile = new VmPMapLineTrace1170Slice.Shooter { Flags = VmPMapLineTrace1170Slice.MfMissile };
        var blocked = new VmPMapLineTrace1170Slice.TraceHost();
        var flags = VmPMapLineTrace1170Slice.TrfBlockSelf | VmPMapLineTrace1170Slice.TrfThruBlock | VmPMapLineTrace1170Slice.TrfThruHitscan;
        VmPMapLineTrace1170Slice.LineTrace(missile, 0, 10, 0, flags, 0, 0, 0, null, blocked);
        Assert.Equal((uint)(VmPMapLineTrace1170Slice.MlBlockProjectile | VmPMapLineTrace1170Slice.MlBlockMonsters), blocked.WallMask);

        var actor = new object();
        var line = new object();
        host.Steps.Add(new VmPMapLineTrace1170Slice.TraceView { HitType = VmPMapLineTrace1170Slice.TraceCrossingPortal });
        host.Steps.Add(new VmPMapLineTrace1170Slice.TraceView { HitType = VmPMapLineTrace1170Slice.TraceCrossingPortal });
        host.Steps.Add(new VmPMapLineTrace1170Slice.TraceView { HitType = VmPMapLineTrace1170Slice.TraceCrossingPortal });
        host.Final = new VmPMapLineTrace1170Slice.TraceView
        {
            HitType = VmPMapLineTrace1170Slice.TraceHitWall,
            Actor = actor,
            Line = line,
            Sector = "room",
            Side = 1,
            Tier = VmPMapLineTrace1170Slice.TierUpper,
            HitPos = new VmPMapLineTrace1170Slice.Vec3(1, 2, 3),
            HitVector = new VmPMapLineTrace1170Slice.Vec3(0, 1, 0),
            Distance = 40,
            SideTextures = new[] { 7, 8, 9 },
        };
        var wall = new VmPMapLineTrace1170Slice.TraceOut();
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 256, 0, VmPMapLineTrace1170Slice.TrfAllActors | VmPMapLineTrace1170Slice.TrfSolidActors | VmPMapLineTrace1170Slice.TrfBlockUse | VmPMapLineTrace1170Slice.TrfNoSky, 4, 0, 0, wall, host);
        Assert.Equal(0xFFFFFFFFu | (uint)VmPMapLineTrace1170Slice.MfSolid, host.ActorMask);
        Assert.Equal(
            (uint)(VmPMapLineTrace1170Slice.MlBlockEverything | VmPMapLineTrace1170Slice.MlBlockHitscan | VmPMapLineTrace1170Slice.MlBlockUse),
            host.WallMask);
        Assert.Equal(VmPMapLineTrace1170Slice.TraceReportPortals | VmPMapLineTrace1170Slice.TraceNoSky, host.TraceFlags);
        Assert.Same(actor, wall.HitActor);
        Assert.Same(line, wall.HitLine);
        Assert.Equal(1, wall.LineSide);
        Assert.Equal(0, wall.LinePart);
        Assert.Equal(7, wall.HitTexture);
        Assert.Equal(1, wall.NumPortals);
        Assert.Equal(0, wall.SectorPlane);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(1, 2, 3), wall.HitLocation);

        host.Final.Tier = VmPMapLineTrace1170Slice.TierFFloor;
        host.Final.FloorFlags = VmPMapLineTrace1170Slice.FfUpperTexture | VmPMapLineTrace1170Slice.FfLowerTexture;
        host.Final.MasterTextures = new[] { 3, 4, 5 };
        var floor = new VmPMapLineTrace1170Slice.TraceOut { LinePart = 4, HitTexture = 11 };
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 1, 0, 0, 0, 0, 0, floor, host);
        Assert.Equal(1, floor.LinePart);
        Assert.Equal(3, floor.HitTexture);
        host.Final.FloorFlags = VmPMapLineTrace1170Slice.FfLowerTexture;
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 1, 0, 0, 0, 0, 0, floor, host);
        Assert.Equal(5, floor.HitTexture);
        host.Final.FloorFlags = 0;
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 1, 0, 0, 0, 0, 0, floor, host);
        Assert.Equal(4, floor.HitTexture);

        host.Steps.Clear();
        host.Final.HitType = VmPMapLineTrace1170Slice.TraceHitCeiling;
        host.Final.HitTexture = 15;
        var ceiling = new VmPMapLineTrace1170Slice.TraceOut { LinePart = 6, LineSide = 2 };
        VmPMapLineTrace1170Slice.LineTrace(shooter, 0, 1, 0, 0, 0, 0, 0, ceiling, host);
        Assert.Equal(1, ceiling.SectorPlane);
        Assert.Equal(15, ceiling.HitTexture);
        Assert.Equal(0, ceiling.LinePart);
        Assert.Equal(0, ceiling.LineSide);
        Assert.Equal(-2, ceiling.NumPortals);
    }

    [Fact]
    public void Pick_ReturnsOnlyAnActorHit()
    {
        var shooter = new VmPMapLineTrace1170Slice.Shooter();
        var actor = new object();
        var host = new VmPMapLineTrace1170Slice.TraceHost
        {
            TraceHit = true,
            Final = new VmPMapLineTrace1170Slice.TraceView
            {
                HitType = VmPMapLineTrace1170Slice.TraceHitActor,
                Actor = actor,
            },
        };
        Assert.Same(actor, VmPMapLineTrace1170Slice.LinePickActor(shooter, 0, 128, 0, 4, 9, host));
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(100, 200, 29), host.From);
        Assert.Equal(new VmPMapLineTrace1170Slice.Vec3(1, 0, 0), host.Direction);
        Assert.Equal(4u, host.ActorMask);
        Assert.Equal(9u, host.WallMask);
        Assert.Equal(VmPMapLineTrace1170Slice.TraceNoSky | VmPMapLineTrace1170Slice.TracePortalRestrict, host.TraceFlags);
        Assert.True(host.PickGhosts);
        Assert.Equal("None", host.PickSpecies);

        host.Final.HitType = VmPMapLineTrace1170Slice.TraceHitWall;
        Assert.Null(VmPMapLineTrace1170Slice.LinePickActor(shooter, 0, 128, 0, 4, 9, host));
        host.TraceHit = false;
        host.Final.HitType = VmPMapLineTrace1170Slice.TraceHitActor;
        Assert.Null(VmPMapLineTrace1170Slice.LinePickActor(shooter, 0, 128, 0, 4, 9, host));
    }
}
