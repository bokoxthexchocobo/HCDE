namespace HCDE.Playsim.Tests;

public class VmPMapRailTalk1171SliceTests
{
    [Fact]
    public void Rail_SpawnsTheProbeAndBleedsAtTheReturnedDamage()
    {
        var probe = new VmPMapRailTalk1171Slice.Puff { Species = "Baron", ThruBits = 9, Flags8 = 0 };
        var state = new VmPMapRailTalk1171Slice.RailState { UseThruBits = true };
        var host = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = probe,
            DamageResult = 0,
            Hits =
            {
                new VmPMapRailTalk1171Slice.RailHit
                {
                    Actor = "imp",
                    Pos = new VmPMapRailTalk1171Slice.Vec3(4, 5, 6),
                    Angle = 20,
                },
            },
        };
        var defaults = new VmPMapRailTalk1171Slice.PuffDefaults
        {
            PoisonDamage = 3,
            PoisonDuration = 2,
            Flags7 = VmPMapRailTalk1171Slice.Mf7FoilBuddha,
        };
        var sector = new object();
        var portals = new object();
        Rail(host, state, "RailPuff", defaults, sector, portals, player: true, damage: 5);
        Assert.Equal(VmPMapRailTalk1171Slice.AllowReplace, host.ReplaceMode);
        Assert.Equal("Baron", state.PuffSpecies);
        Assert.Equal(9, state.ThruBits);
        Assert.False(state.UseThruBits);
        Assert.Equal((uint)VmPMapRailTalk1171Slice.MfShootable, host.ActorMask);
        Assert.Equal((uint)VmPMapRailTalk1171Slice.MlBlockEverything, host.WallMask);
        Assert.Same(sector, host.TracedSector);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(1, 0, 0), host.TracedDirection);
        Assert.Equal("imp", host.PoisonVictim);
        Assert.Same(probe, host.PoisonInflictor);
        Assert.Equal(3, host.PoisonDamage);
        Assert.Equal(5, host.BloodAmount);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(4, 5, 6), host.BleedAt);
        Assert.Equal(VmPMapRailTalk1171Slice.DmgInflictorIsPuff | VmPMapRailTalk1171Slice.DmgFoilBuddha | VmPMapRailTalk1171Slice.DmgPlayerAttack | VmPMapRailTalk1171Slice.DmgUseAngle | VmPMapRailTalk1171Slice.DmgRailgun, host.AppliedFlags);
        Assert.Equal(new[] { "probe:RailPuff@8", "trace:6", "poison", "damage", "blood", "geometry", "fired", "destroy", "trail" }, host.Order);
        Assert.Same(portals, host.TrailPortals);
        Assert.Equal(11, host.TrailColor1);
        Assert.Equal(90d, host.TrailAngle);
        Assert.True(probe.Destroyed);
        Assert.Same(probe, host.FiredPuff);

        var kept = new VmPMapRailTalk1171Slice.RailState { UseThruBits = true };
        var bits = new VmPMapRailTalk1171Slice.Puff { Species = "Imp", ThruBits = 4, Flags8 = VmPMapRailTalk1171Slice.Mf8AllowThruBits };
        var foil = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = bits,
            DamageResult = 9,
            Hits =
            {
                new VmPMapRailTalk1171Slice.RailHit
                {
                    Flags2 = VmPMapRailTalk1171Slice.Mf2Invulnerable,
                    Actor = "ghost",
                    Angle = 10,
                },
            },
        };
        Rail(foil, kept, "RailPuff", new VmPMapRailTalk1171Slice.PuffDefaults { Flags3 = VmPMapRailTalk1171Slice.Mf3FoilInvul }, sector, portals, player: false, damage: 5);
        Assert.True(kept.UseThruBits);
        Assert.Equal(9, foil.BloodAmount);
        Assert.Equal(0, foil.AppliedFlags & VmPMapRailTalk1171Slice.DmgPlayerAttack);
        Assert.Equal(VmPMapRailTalk1171Slice.DmgFoilInvul, foil.AppliedFlags & VmPMapRailTalk1171Slice.DmgFoilInvul);
        Assert.Empty(foil.Spawns);

        var dormant = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = bits,
            Hits = { new VmPMapRailTalk1171Slice.RailHit { Flags2 = VmPMapRailTalk1171Slice.Mf2Dormant, Angle = 10 } },
        };
        Rail(dormant, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", new VmPMapRailTalk1171Slice.PuffDefaults { Flags3 = VmPMapRailTalk1171Slice.Mf3FoilInvul | VmPMapRailTalk1171Slice.Mf3AlwaysPuff }, sector, portals, false, 5);
        var spawned = Assert.Single(dormant.Spawns);
        Assert.Equal(VmPMapRailTalk1171Slice.PfHitThing, spawned.Flags);
        Assert.Equal(1, spawned.UpDown);
        Assert.Equal(-80d, spawned.ParticleDir);
        Assert.Equal(-1, dormant.BloodAmount);

        var bloodless = new VmPMapRailTalk1171Slice.RailHost
        {
            Hits = { new VmPMapRailTalk1171Slice.RailHit { Angle = 15 } },
        };
        Rail(bloodless, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", new VmPMapRailTalk1171Slice.PuffDefaults { Flags3 = VmPMapRailTalk1171Slice.Mf3AlwaysPuff | VmPMapRailTalk1171Slice.Mf3BloodlessImpact, PoisonDuration = int.MinValue, PoisonDamage = 4 }, sector, portals, false, 5);
        Assert.Equal(VmPMapRailTalk1171Slice.PfHitThing | VmPMapRailTalk1171Slice.PfHitThingBleed, Assert.Single(bloodless.Spawns).Flags);
        Assert.Equal(-1, bloodless.BloodAmount);
        Assert.DoesNotContain("poison", bloodless.Order);

        var none = new VmPMapRailTalk1171Slice.RailState { UseThruBits = true };
        var quiet = new VmPMapRailTalk1171Slice.RailHost();
        Rail(quiet, none, null, new VmPMapRailTalk1171Slice.PuffDefaults(), sector, portals, false, 1);
        Assert.Equal("None", none.PuffSpecies);
        Assert.True(none.UseThruBits);
        Assert.Null(quiet.FiredPuff);
        Assert.Contains("trail", quiet.Order);
        Assert.DoesNotContain("destroy", quiet.Order);
    }

    [Fact]
    public void Rail_DecalsTheWallAndSplashesBeforeTheProbeDies()
    {
        var probe = new VmPMapRailTalk1171Slice.Puff();
        var host = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = probe,
            End = new VmPMapRailTalk1171Slice.TraceEnd
            {
                HitType = VmPMapRailTalk1171Slice.TraceHitWall,
                HitPos = new VmPMapRailTalk1171Slice.Vec3(2, 3, 4),
                SrcAngle = 40,
                LineSpecial = VmPMapRailTalk1171Slice.LineHorizon,
            },
        };
        var defaults = new VmPMapRailTalk1171Slice.PuffDefaults
        {
            Flags3 = VmPMapRailTalk1171Slice.Mf3AlwaysPuff,
            Flags7 = VmPMapRailTalk1171Slice.Mf7ForceDecal,
            HasDecal = true,
        };
        Rail(host, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", defaults, "room", "portals", false, 1);
        var wall = Assert.Single(host.Spawns);
        Assert.True(wall.Puff.Destroyed);
        Assert.Equal(40d, wall.HitDir);
        Assert.Equal(-50d, wall.ParticleDir);
        Assert.Null(wall.Victim);
        Assert.Equal(("defaults", "defaults"), Assert.Single(host.Decals));
        Assert.Equal(new[] { "probe:RailPuff@8", "trace:6", "geometry", "fired", "destroy", "trail" }, host.Order);

        defaults.HasDecal = false;
        var sky = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = new VmPMapRailTalk1171Slice.Puff(),
            SpawnFlags3 = VmPMapRailTalk1171Slice.Mf3SkyExplode,
            End = host.End,
        };
        Rail(sky, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", defaults, "room", "portals", false, 1);
        Assert.False(Assert.Single(sky.Spawns).Puff.Destroyed);
        Assert.Equal(("source", "source"), Assert.Single(sky.Decals));

        var floor = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = new VmPMapRailTalk1171Slice.Puff(),
            SpawnFloorPic = 77,
            End = new VmPMapRailTalk1171Slice.TraceEnd
            {
                HitType = VmPMapRailTalk1171Slice.TraceHitFloor,
                SrcAngle = 5,
            },
        };
        floor.SkyFlat = 77;
        Rail(floor, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", defaults, "room", "portals", false, 1);
        Assert.True(Assert.Single(floor.Spawns).Puff.Destroyed);
        Assert.Empty(floor.Decals);
        Assert.True(floor.Water);

        var wet = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = new VmPMapRailTalk1171Slice.Puff(),
            End = new VmPMapRailTalk1171Slice.TraceEnd
            {
                HitType = VmPMapRailTalk1171Slice.TraceHitFloor,
                Crossed3DWater = true,
                HeightSec = true,
            },
        };
        Rail(wet, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", new VmPMapRailTalk1171Slice.PuffDefaults(), "room", "portals", false, 1);
        Assert.True(wet.Splash);
        Assert.False(wet.Water);
        Assert.Equal(new[] { "probe:RailPuff@8", "trace:6", "geometry", "fired", "splash", "destroy", "trail" }, wet.Order);

        var pool = new VmPMapRailTalk1171Slice.RailHost
        {
            Probe = new VmPMapRailTalk1171Slice.Puff(),
            End = new VmPMapRailTalk1171Slice.TraceEnd { HitType = VmPMapRailTalk1171Slice.TraceHitFloor },
        };
        Rail(pool, new VmPMapRailTalk1171Slice.RailState(), "RailPuff", new VmPMapRailTalk1171Slice.PuffDefaults(), "room", "portals", false, 1);
        Assert.True(pool.Water);
        Assert.Contains("water", pool.Order);
    }

    [Fact]
    public void View_PullsTheCameraBackAndWalksDownThroughAFloor()
    {
        Assert.Equal(-8d, VmPMapRailTalk1171Slice.ChaseHeight);
        Assert.Equal(90d, VmPMapRailTalk1171Slice.ChaseDist);
        Assert.Equal(-180d, VmPMapRailTalk1171Slice.Normalize180(180d));

        var hardware = new VmPMapRailTalk1171Slice.ViewHost { Hardware = true };
        var view = new VmPMapRailTalk1171Slice.ViewPoint
        {
            AllowOutOfBounds = true,
            Pos = new VmPMapRailTalk1171Slice.Vec3(1, 2, -4),
        };
        VmPMapRailTalk1171Slice.OffsetView(view, new VmPMapRailTalk1171Slice.Vec3(1, 0, -1), 3, hardware);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(4, 2, -7), view.Pos);
        Assert.Equal("render", view.SectorFrom);
        Assert.Equal(0, hardware.Traces);
        Assert.Equal(0, view.PortalHops);

        var hit = new VmPMapRailTalk1171Slice.ViewHost
        {
            Trace = new VmPMapRailTalk1171Slice.ViewTrace
            {
                Hit = true,
                HitPos = new VmPMapRailTalk1171Slice.Vec3(10, 4, 5),
                HitVector = new VmPMapRailTalk1171Slice.Vec3(1, 0, 0),
                Distance = 9,
                SrcAngle = 0,
            },
        };
        var pulled = new VmPMapRailTalk1171Slice.ViewPoint();
        VmPMapRailTalk1171Slice.OffsetView(pulled, new VmPMapRailTalk1171Slice.Vec3(0, 1, 0), 9, hit);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(5, 4, 5), pulled.Pos);
        Assert.Equal(VmPMapRailTalk1171Slice.Normalize180(0d - VmPMapRailTalk1171Slice.AngleOf(new VmPMapRailTalk1171Slice.Vec3(0, 1, 0))), pulled.Yaw);
        Assert.Equal("render", pulled.SectorFrom);

        hit.Trace.Distance = 2;
        var close = new VmPMapRailTalk1171Slice.ViewPoint();
        VmPMapRailTalk1171Slice.OffsetView(close, new VmPMapRailTalk1171Slice.Vec3(1, 0, 0), 2, hit);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(8, 4, 5), close.Pos);

        var missed = new VmPMapRailTalk1171Slice.ViewHost
        {
            Trace = new VmPMapRailTalk1171Slice.ViewTrace
            {
                HitPos = new VmPMapRailTalk1171Slice.Vec3(7, 8, 9),
                Sector = "room",
                SrcAngle = 0,
            },
        };
        var miss = new VmPMapRailTalk1171Slice.ViewPoint { Yaw = 4 };
        VmPMapRailTalk1171Slice.OffsetView(miss, new VmPMapRailTalk1171Slice.Vec3(1, 0, 0), 2, missed);
        Assert.Equal(new VmPMapRailTalk1171Slice.Vec3(7, 8, 9), miss.Pos);
        Assert.Equal("trace", miss.SectorFrom);
        Assert.Equal("room", miss.Sector);
        Assert.Equal(4d, miss.Yaw);

        var lower = new VmPMapRailTalk1171Slice.SectorNode { Name = "lower", FloorBlocks = true };
        var mid = new VmPMapRailTalk1171Slice.SectorNode
        {
            Name = "mid",
            FloorPlane = 0,
            FloorShift = new VmPMapRailTalk1171Slice.Vec3(0, 0, 2),
            FloorDest = lower,
        };
        var top = new VmPMapRailTalk1171Slice.SectorNode
        {
            Name = "top",
            FloorPlane = 0,
            FloorShift = new VmPMapRailTalk1171Slice.Vec3(0, 1, 2),
            FloorDest = mid,
        };
        var drop = new VmPMapRailTalk1171Slice.ViewHost
        {
            Hardware = true,
            Trace = new VmPMapRailTalk1171Slice.ViewTrace
            {
                HitPos = new VmPMapRailTalk1171Slice.Vec3(1, 1, -5),
                Sector = top,
            },
        };
        var fallen = new VmPMapRailTalk1171Slice.ViewPoint();
        VmPMapRailTalk1171Slice.OffsetView(fallen, new VmPMapRailTalk1171Slice.Vec3(1, 0, -1), 1, drop);
        Assert.Equal(2, fallen.PortalHops);
        Assert.Same(lower, fallen.Sector);
        Assert.Equal(-1d, fallen.Pos.Z);
        Assert.Equal(2d, fallen.Pos.Y);

        var ceiling = new VmPMapRailTalk1171Slice.SectorNode { CeilingBlocks = true };
        var open = new VmPMapRailTalk1171Slice.SectorNode
        {
            CeilingPlane = 3,
            CeilingShift = new VmPMapRailTalk1171Slice.Vec3(0, 0, -4),
            CeilingDest = ceiling,
        };
        var rise = new VmPMapRailTalk1171Slice.ViewHost
        {
            Trace = new VmPMapRailTalk1171Slice.ViewTrace
            {
                HitPos = new VmPMapRailTalk1171Slice.Vec3(0, 0, 9),
                Sector = open,
            },
        };
        var risen = new VmPMapRailTalk1171Slice.ViewPoint();
        VmPMapRailTalk1171Slice.OffsetView(risen, new VmPMapRailTalk1171Slice.Vec3(0, 0, 1), 1, rise);
        Assert.Equal(1, risen.PortalHops);
        Assert.Equal(5d, risen.Pos.Z);
        Assert.Same(ceiling, risen.Sector);
    }

    [Fact]
    public void Talk_StopsAtTheFirstActorAndBlocksANetPlayer()
    {
        var dead = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat", Health = 0 };
        Assert.False(VmPMapRailTalk1171Slice.CanTalk(dead));
        Assert.True(VmPMapRailTalk1171Slice.HasConversation(dead));
        Assert.False(VmPMapRailTalk1171Slice.HasConversation(new VmPMapRailTalk1171Slice.Talker()));
        var fighting = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat", Flags4 = VmPMapRailTalk1171Slice.Mf4InCombat };
        Assert.False(VmPMapRailTalk1171Slice.CanTalkAction(fighting));
        var ready = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat", HasPlayer = true };
        var host = new VmPMapRailTalk1171Slice.TalkHost { Netgame = true, LimitConversations = true, ConsoleBody = ready, HapticsMenus = true };
        Assert.False(VmPMapRailTalk1171Slice.StartConversation(ready, "player", true, false, true, host));
        Assert.Equal(VmPMapRailTalk1171Slice.NetLimitMessage, Assert.Single(host.Messages));
        Assert.Empty(host.Starts);
        Assert.Empty(host.Rumbles);

        var other = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat", HasPlayer = true };
        Assert.False(VmPMapRailTalk1171Slice.StartConversation(other, "player", true, false, true, host));
        Assert.Single(host.Messages);

        ready.SettingsController = true;
        Assert.True(VmPMapRailTalk1171Slice.StartConversationAction(ready, "player", false, true, true, host));
        Assert.Equal("menu/activate", Assert.Single(host.Rumbles));
        Assert.Equal(0, Assert.Single(host.Animations));
        var started = Assert.Single(host.Starts);
        Assert.Same(ready, started.Self);
        Assert.Equal("player", started.Player);
        Assert.False(started.Face);
        Assert.True(started.Save);

        var facing = new VmPMapRailTalk1171Slice.TalkHost();
        var player = new VmPMapRailTalk1171Slice.Talker { Yaw = 10 };
        var quiet = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat", Health = 0 };
        facing.Targets.Enqueue(quiet);
        Assert.False(VmPMapRailTalk1171Slice.TalkFacing(player, facing));
        Assert.Equal((10d, 35d, VmPMapRailTalk1171Slice.TalkRange, VmPMapRailTalk1171Slice.AlfForceNoSmart | VmPMapRailTalk1171Slice.AlfCheckConversation | VmPMapRailTalk1171Slice.AlfPortalRestrict), Assert.Single(facing.Aims));

        var second = new VmPMapRailTalk1171Slice.TalkHost();
        second.Targets.Enqueue(null);
        var npc = new VmPMapRailTalk1171Slice.Talker { Conversation = "chat" };
        second.Targets.Enqueue(npc);
        Assert.True(VmPMapRailTalk1171Slice.TalkFacing(player, second));
        Assert.Equal(2, second.Aims.Count);
        Assert.Equal(10d + (90d / 16d), second.Aims[1].Angle);
        var faced = Assert.Single(second.Starts);
        Assert.Same(npc, faced.Self);
        Assert.Same(player, faced.Player);
        Assert.True(faced.Face);
        Assert.True(faced.Save);
    }

    private static void Rail(
        VmPMapRailTalk1171Slice.RailHost host,
        VmPMapRailTalk1171Slice.RailState state,
        string? puffClass,
        VmPMapRailTalk1171Slice.PuffDefaults defaults,
        object sector,
        object portals,
        bool player,
        int damage)
    {
        VmPMapRailTalk1171Slice.RailTail(
            "shooter",
            player,
            new VmPMapRailTalk1171Slice.Vec3(8, 0, 0),
            sector,
            puffClass,
            defaults,
            state,
            new VmPMapRailTalk1171Slice.Vec3(1, 2, 3),
            new VmPMapRailTalk1171Slice.Vec3(1, 0, 0),
            64,
            6,
            0,
            damage,
            "Rail",
            12,
            90,
            3,
            11,
            12,
            1,
            2,
            3,
            4,
            5,
            "Spark",
            portals,
            host);
    }
}
