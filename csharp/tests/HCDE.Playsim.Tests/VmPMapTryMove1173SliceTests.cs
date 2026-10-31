namespace HCDE.Playsim.Tests;

public class VmPMapTryMove1173SliceTests
{
    [Fact]
    public void Block_StepsAPassableBodyAndRejectsATightFit()
    {
        var step = Body();
        step.Z = 20;
        step.Flags2 = VmPMapTryMove1173Slice.Mf2PassMobj;
        step.Flags3 = VmPMapTryMove1173Slice.Mf3FloorHugger;
        step.Flags = VmPMapTryMove1173Slice.MfNoClip;
        var host = BlockedMonster(top: 40, ceiling: 200);
        var tm = new VmPMapTryMove1173Slice.Trace { FloorZ = 8, CeilingZ = 200, FloorSector = "home" };
        host.CheckSetsPortalStep = true;
        var stepped = VmPMapTryMove1173Slice.CrossingChecks(step, 5, 0, 0, true, 20, tm, false, host);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, stepped.Kind);
        Assert.Equal(8d, step.Z);
        Assert.Equal(VmPMapTryMove1173Slice.Mf6InTryMove, step.Flags6);
        Assert.False(tm.FloatOk);
        Assert.True(tm.PortalStep);
        Assert.Equal(5d, tm.PosX);
        Assert.Contains("collide", host.Events);

        var predicting = Body();
        predicting.Cheats = VmPMapTryMove1173Slice.CfPredicting;
        var quiet = BlockedMonster(top: 100, ceiling: 400);
        var pushed = VmPMapTryMove1173Slice.CrossingChecks(predicting, 1, 1, 0, false, 0, Room(128), false, quiet);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, pushed.Kind);
        Assert.Equal(VmPMapTryMove1173Slice.Mf6InTryMove, predicting.Flags6);
        Assert.Empty(quiet.Events);

        var bumper = Body();
        bumper.Cheats = 1;
        var playerBlock = BlockedMonster(top: 10, ceiling: 400);
        playerBlock.BlockingIsPlayer = true;
        var bumped = VmPMapTryMove1173Slice.CrossingChecks(bumper, 1, 0, 0, false, 0, Room(128), false, playerBlock);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, bumped.Kind);
        Assert.Contains("collide", playerBlock.Events);

        var wall = new VmPMapTryMove1173Slice.CrossingHost { PositionOk = false };
        var walled = VmPMapTryMove1173Slice.CrossingChecks(Body(), 1, 0, 0, false, 0, Room(128), false, wall);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, walled.Kind);
        Assert.Empty(wall.Events);

        var compat = Body();
        compat.Z = 4;
        compat.Flags2 = VmPMapTryMove1173Slice.Mf2PassMobj;
        var noPass = BlockedMonster(top: 10, ceiling: 400);
        noPass.Compat = VmPMapTryMove1173Slice.CompatNoPassMobj;
        var refused = VmPMapTryMove1173Slice.CrossingChecks(compat, 2, 0, 0, true, 9, Room(128), false, noPass);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, refused.Kind);
        Assert.Equal(4d, compat.Z);
        Assert.Equal(0, compat.Flags6);

        var tight = Body();
        tight.Height = 56;
        var low = new VmPMapTryMove1173Slice.Trace { FloorZ = 0, CeilingZ = 40 };
        var squeezed = VmPMapTryMove1173Slice.CrossingChecks(tight, 0, 0, 1, false, 0, low, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, squeezed.Kind);
        Assert.False(low.FloatOk);

        var flyer = Body();
        flyer.Z = 40;
        flyer.Height = 16;
        flyer.VelZ = 3;
        flyer.Flags2 = VmPMapTryMove1173Slice.Mf2Fly;
        flyer.Flags = VmPMapTryMove1173Slice.MfNoGravity;
        var sky = new VmPMapTryMove1173Slice.Trace { FloorZ = 0, CeilingZ = 50 };
        var slid = VmPMapTryMove1173Slice.CrossingChecks(flyer, 0, 0, 0, false, 0, sky, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, slid.Kind);
        Assert.Equal(3d, flyer.VelZ);
        Assert.True(sky.FloatOk);

        var portalOnly = Body();
        portalOnly.Flags2 = VmPMapTryMove1173Slice.Mf2PassMobj;
        portalOnly.Flags = VmPMapTryMove1173Slice.MfNoClip;
        var gap = BlockedMonster(top: 40, ceiling: 50);
        gap.BlockingPortalBlocks = false;
        var open = new VmPMapTryMove1173Slice.Trace { CeilingZ = 200, FloorZ = 1, FloorSector = "other" };
        var passed = VmPMapTryMove1173Slice.CrossingChecks(portalOnly, 0, 0, 0, true, 20, open, false, gap);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, passed.Kind);
        Assert.Equal(20d, portalOnly.Z);

        var sealedGap = BlockedMonster(top: 40, ceiling: 50);
        sealedGap.BlockingPortalBlocks = true;
        var sealedBody = Body();
        sealedBody.Z = 20;
        sealedBody.Flags2 = VmPMapTryMove1173Slice.Mf2PassMobj;
        var sealedMove = VmPMapTryMove1173Slice.CrossingChecks(sealedBody, 0, 0, 0, false, 0, Room(200), false, sealedGap);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, sealedMove.Kind);
    }

    [Fact]
    public void Dropoff_KeepsTheFloorPictureAndStopsTheBot()
    {
        var missile = Body();
        missile.IsPlayer = false;
        missile.Flags = VmPMapTryMove1173Slice.MfMissile;
        missile.Z = 0;
        var climb = new VmPMapTryMove1173Slice.Trace { FloorZ = 8, CeilingZ = 128 };
        var blockedClimb = VmPMapTryMove1173Slice.CrossingChecks(missile, 1, 0, 0, false, 0, climb, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, blockedClimb.Kind);
        Assert.True(climb.FloatOk);
        Assert.Equal(VmPMapTryMove1173Slice.Mf6InTryMove, missile.Flags6);

        var walker = Body();
        walker.Z = 1;
        walker.Flags6 = VmPMapTryMove1173Slice.Mf6StepMissile;
        walker.VelZ = -4;
        var failedStep = new VmPMapTryMove1173Slice.Trace { FloorZ = 10, CeilingZ = 128, FloorSector = "other" };
        var stepHost = new VmPMapTryMove1173Slice.CrossingHost { StepTestOk = false };
        var rejected = VmPMapTryMove1173Slice.CrossingChecks(walker, 1, 0, 1, true, 4, failedStep, false, stepHost);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.PushLine, rejected.Kind);
        Assert.Equal(4d, walker.Z);
        Assert.Equal(-4d, walker.VelZ);

        var stepper = Body();
        stepper.IsPlayer = false;
        stepper.Z = 0;
        stepper.Flags = VmPMapTryMove1173Slice.MfMissile | VmPMapTryMove1173Slice.MfDropoff;
        stepper.Flags6 = VmPMapTryMove1173Slice.Mf6StepMissile;
        stepper.VelZ = -2;
        stepper.BounceFlags = VmPMapTryMove1173Slice.BounceFloors;
        var stair = new VmPMapTryMove1173Slice.Trace { FloorZ = 8, CeilingZ = 128 };
        var bounceHost = new VmPMapTryMove1173Slice.CrossingHost { Has3DFloor = true };
        var bounced = VmPMapTryMove1173Slice.CrossingChecks(stepper, 1, 0, 1, false, 0, stair, false, bounceHost);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, bounced.Kind);
        Assert.Equal(8d, stepper.Z);
        Assert.Equal(-2d, stepper.VelZ);
        Assert.Contains("bounce:3d", bounceHost.Events);

        var grounded = Body();
        grounded.IsPlayer = false;
        grounded.Flags = VmPMapTryMove1173Slice.MfMissile;
        grounded.Flags6 = VmPMapTryMove1173Slice.Mf6StepMissile;
        grounded.VelZ = -5;
        var flat = new VmPMapTryMove1173Slice.Trace { FloorZ = 6, CeilingZ = 80 };
        var stopped = VmPMapTryMove1173Slice.CrossingChecks(grounded, 0, 0, 1, false, 0, flat, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(0d, stopped.OldZ);
        Assert.Equal(6d, grounded.Z);
        Assert.Equal(0d, grounded.VelZ);

        var drop = Body();
        drop.Z = 10;
        var fell = VmPMapTryMove1173Slice.CrossingChecks(drop, 0, 0, 0, true, 12, new VmPMapTryMove1173Slice.Trace { FloorZ = 12, DropoffZ = -20, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, fell.Kind);
        Assert.Equal(10d, drop.Z);
        Assert.Equal(0, drop.Flags6);

        var rider = Body();
        rider.Z = 30;
        rider.Flags2 = VmPMapTryMove1173Slice.Mf2OnMobj;
        var ledge = new VmPMapTryMove1173Slice.Trace { FloorZ = 10, DropoffZ = 0, CeilingZ = 128 };
        var rode = VmPMapTryMove1173Slice.CrossingChecks(rider, 0, 0, 0, false, 0, ledge, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, rode.Kind);
        var standing = Body();
        standing.Z = 30;
        var stayed = VmPMapTryMove1173Slice.CrossingChecks(standing, 0, 0, 0, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 10, DropoffZ = 0, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, stayed.Kind);

        var blasted = Body();
        blasted.Flags2 = VmPMapTryMove1173Slice.Mf2Blasted;
        blasted.Z = 10;
        var blastedMove = VmPMapTryMove1173Slice.CrossingChecks(blasted, 0, 0, 0, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 10, DropoffZ = -20, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, blastedMove.Kind);

        var dog = Body();
        dog.HasTarget = false;
        var dogMove = VmPMapTryMove1173Slice.CrossingChecks(dog, 0, 0, 2, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 0, DropoffZ = -40, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, dogMove.Kind);
        var aimed = Body();
        aimed.HasTarget = true;
        aimed.TargetZ = -40;
        var aimedMove = VmPMapTryMove1173Slice.CrossingChecks(aimed, 0, 0, 2, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 0, DropoffZ = -40, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, aimedMove.Kind);

        var plain = Body();
        var forced = VmPMapTryMove1173Slice.CrossingChecks(plain, 0, 0, 1, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 0, DropoffZ = -40, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost { Compat = VmPMapTryMove1173Slice.CompatCrossDropoff });
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, forced.Kind);
        var scrolled = Body();
        scrolled.Flags4 = VmPMapTryMove1173Slice.Mf4ScrollMove;
        var kept = VmPMapTryMove1173Slice.CrossingChecks(
            scrolled,
            0,
            0,
            1,
            false,
            0,
            new VmPMapTryMove1173Slice.Trace { FloorZ = 0, DropoffZ = -40, CeilingZ = 128 },
            false,
            new VmPMapTryMove1173Slice.CrossingHost { Compat = VmPMapTryMove1173Slice.CompatCrossDropoff });
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, kept.Kind);

        var avoid = Body();
        avoid.Z = 3;
        avoid.FloorZ = 40;
        avoid.DropoffZ = 0;
        avoid.Flags5 = VmPMapTryMove1173Slice.Mf5AvoidingDropoff;
        var avoided = VmPMapTryMove1173Slice.CrossingChecks(avoid, 0, 0, 0, true, 7, new VmPMapTryMove1173Slice.Trace { FloorZ = 0, DropoffZ = 0, CeilingZ = 128, FloorSector = "other" }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, avoided.Kind);
        Assert.Equal(7d, avoid.Z);
        Assert.Equal(0, avoid.Flags6);

        var pic = Body();
        pic.Z = 5;
        pic.FloorPic = 2;
        pic.Flags2 = VmPMapTryMove1173Slice.Mf2CantLeaveFloorPic;
        pic.Flags = VmPMapTryMove1173Slice.MfDropoff;
        var picture = VmPMapTryMove1173Slice.CrossingChecks(pic, 0, 0, 1, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 5, FloorPic = 9, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, picture.Kind);
        Assert.Equal(5d, pic.Z);

        var bot = Body();
        bot.HasBot = true;
        bot.Flags = VmPMapTryMove1173Slice.MfShootable;
        bot.VelX = 4;
        bot.VelY = 5;
        bot.Sector = "home";
        var danger = new VmPMapTryMove1173Slice.CrossingHost { Dangerous = true };
        var stoppedBot = VmPMapTryMove1173Slice.CrossingChecks(bot, 0, 0, 1, true, 8, new VmPMapTryMove1173Slice.Trace { Sector = "lava", FloorZ = 0, CeilingZ = 128, FloorSector = "other" }, false, danger);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, stoppedBot.Kind);
        Assert.True(bot.BotPrevCopied);
        Assert.False(bot.BotHasDest);
        Assert.Equal(0d, bot.VelX);
        Assert.Equal(0d, bot.VelY);
        Assert.Equal(0d, bot.Z);

        var bouncer = Body();
        bouncer.IsPlayer = false;
        bouncer.Z = 0;
        bouncer.BounceFlags = VmPMapTryMove1173Slice.BounceMbf;
        bouncer.Height = 16;
        var mbf = VmPMapTryMove1173Slice.CrossingChecks(bouncer, 0, 0, 1, true, 4, new VmPMapTryMove1173Slice.Trace { FloorZ = 22, CeilingZ = 128, FloorSector = "other" }, false, new VmPMapTryMove1173Slice.CrossingHost { OldHasHeightSec = true, OldFakeFloorZ = 3, OldFakeCeilingZ = 40 });
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, mbf.Kind);
        Assert.Equal(4d, bouncer.Z);
        Assert.Equal(0, bouncer.Flags6);
        Assert.True(mbf.OldHasHeightSec);
        Assert.True(mbf.OldAboveFakeFloor);
        Assert.False(mbf.OldAboveFakeCeiling);
        Assert.Equal(8d, mbf.ViewHeight);

        var awake = Body();
        awake.Sentient = true;
        awake.BounceFlags = VmPMapTryMove1173Slice.BounceMbf;
        var walks = VmPMapTryMove1173Slice.CrossingChecks(awake, 0, 0, 1, false, 0, new VmPMapTryMove1173Slice.Trace { FloorZ = 22, CeilingZ = 128 }, false, new VmPMapTryMove1173Slice.CrossingHost());
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, walks.Kind);
    }

    [Fact]
    public void Portals_LinkInOrderAndRestoreABlockedExit()
    {
        var thing = Body();
        thing.IsPlayer = false;
        thing.Flags = VmPMapTryMove1173Slice.MfNoClip;
        thing.X = 0;
        thing.Y = 0;
        thing.Z = 10;
        var host = new VmPMapTryMove1173Slice.CrossingHost { CheckSetsPortalStep = true };
        host.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 2, Frac = 0.5, DispX = 1, DispY = 1, OldX = 0, OldY = 0, RefX = 10, RefY = 0 });
        host.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 1, Frac = 0.2, DispX = 20, DispY = 4, OldX = 0, OldY = 0, RefX = 10, RefY = 0 });
        host.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 9, Frac = 0.1, FrontGroup = 4 });
        host.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 8, Frac = 1.1, OldSide = 0, Side = 1 });
        var tm = new VmPMapTryMove1173Slice.Trace();
        var linked = VmPMapTryMove1173Slice.CrossingChecks(thing, 5, 0, 0, false, 0, tm, false, host);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Continue, linked.Kind);
        Assert.True(linked.PortalCrossed);
        Assert.Equal(26d, thing.X);
        Assert.Equal(5d, thing.Y);
        Assert.Equal(10d, thing.Z);
        Assert.Equal(21d, thing.PrevX);
        Assert.Equal(5d, thing.PrevY);
        Assert.Equal(26d, tm.PosX);
        Assert.Equal(5d, tm.PosY);
        Assert.False(tm.PortalStep);
        Assert.Equal(new[] { "portal:1", "unlink", "link", "floorceil", "portal:2", "unlink", "link", "floorceil" }, host.Events);
        Assert.Equal(2, host.Portals.Count);

        var equal = Body();
        equal.Flags = VmPMapTryMove1173Slice.MfNoClip;
        var ties = new VmPMapTryMove1173Slice.CrossingHost();
        ties.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 3, Frac = 0.4, DispX = 10, DispY = 0 });
        ties.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 4, Frac = 0.4, DispX = 0, DispY = 7, Type = VmPMapTryMove1173Slice.PorttVisual });
        var tie = VmPMapTryMove1173Slice.CrossingChecks(equal, 0, 0, 0, false, 0, new VmPMapTryMove1173Slice.Trace(), false, ties);
        Assert.True(tie.PortalCrossed);
        Assert.Equal(new[] { "portal:3", "unlink", "link", "floorceil", "portal:4" }, ties.Events);
        Assert.Equal(10d, equal.X);
        Assert.Equal(0d, equal.Y);

        var exit = Body();
        exit.IsPlayer = true;
        exit.Cheats = 0;
        exit.X = 0;
        exit.Y = 0;
        exit.Z = 10;
        exit.VelX = 1;
        exit.VelY = 2;
        exit.Yaw = 9;
        exit.Flags = VmPMapTryMove1173Slice.MfNoClip;
        var gate = new VmPMapTryMove1173Slice.CrossingHost { IsCamera = true, HasConsolePlayer = true, ViewZ = 4 };
        gate.Portals.Add(new VmPMapTryMove1173Slice.PortalHit
        {
            Id = 5,
            Type = VmPMapTryMove1173Slice.PorttInteractive,
            Frac = 0.5,
            ShiftX = 100,
            ShiftY = 0,
            ShiftZ = 3,
            VelShiftX = 7,
            VelShiftY = 8,
            AngleShift = 15,
            OldX = 0,
            OldY = 0,
            RefX = 10,
            RefY = 0,
        });
        var opened = VmPMapTryMove1173Slice.CrossingChecks(exit, 5, 0, 0, false, 0, new VmPMapTryMove1173Slice.Trace { PortalStep = true }, false, gate);
        Assert.True(opened.PortalCrossed);
        Assert.True(gate.Probed);
        Assert.Equal(100d, gate.ProbeX);
        Assert.Equal(13d, gate.ProbeZ);
        Assert.Equal(105d, gate.ProbeCheckX);
        Assert.Equal(105d, exit.X);
        Assert.Equal(0d, exit.Y);
        Assert.Equal(13d, exit.Z);
        Assert.Equal(8d, exit.VelX);
        Assert.Equal(10d, exit.VelY);
        Assert.Equal(24d, exit.Yaw);
        Assert.True(exit.CrossingPortal);
        Assert.Equal(7d, gate.ViewZ);
        Assert.Equal(2, gate.Points.Count);
        Assert.Equal(5d, gate.Points[0].X);
        Assert.Equal(0d, gate.Points[0].Z);
        Assert.Equal(105d, gate.Points[1].X);
        Assert.Equal(3d, gate.Points[1].Z);
        Assert.Equal(15d, gate.Points[1].Angle);
        Assert.Contains("clear-interp", gate.Events);

        var denied = Body();
        denied.IsPlayer = true;
        denied.Cheats = 1;
        denied.X = 0;
        denied.Y = 0;
        denied.Z = 10;
        denied.Flags = VmPMapTryMove1173Slice.MfNoClip;
        var stop = new VmPMapTryMove1173Slice.CrossingHost { PortalPositionOk = false, PortalHasBlocking = true };
        stop.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 6, Type = VmPMapTryMove1173Slice.PorttTeleport, Frac = 0.25, ShiftX = 100, ShiftZ = 3 });
        var shut = VmPMapTryMove1173Slice.CrossingChecks(denied, 5, 0, 0, true, 4, new VmPMapTryMove1173Slice.Trace { FloorSector = "other" }, false, stop);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, shut.Kind);
        Assert.Equal(0d, denied.X);
        Assert.Equal(4d, denied.Z);
        Assert.Equal(0, denied.Flags6);
        Assert.DoesNotContain("collide-portal", stop.Events);
        Assert.Empty(stop.Portals);

        var monster = Body();
        monster.IsPlayer = false;
        monster.Cheats = 1;
        monster.Flags = VmPMapTryMove1173Slice.MfNoClip;
        monster.Z = 2;
        var hit = new VmPMapTryMove1173Slice.CrossingHost { PortalPositionOk = false, PortalHasBlocking = true };
        hit.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 7, Type = VmPMapTryMove1173Slice.PorttVisual, Frac = 0.3, ShiftX = 4 });
        var blocked = VmPMapTryMove1173Slice.CrossingChecks(monster, 1, 0, 0, false, 0, new VmPMapTryMove1173Slice.Trace(), false, hit);
        Assert.Equal(VmPMapTryMove1173Slice.CrossingKind.Fail, blocked.Kind);
        Assert.Contains("collide-portal", hit.Events);
        Assert.Equal(2d, monster.Z);
    }

    static VmPMapTryMove1173Slice.Actor Body()
    {
        return new VmPMapTryMove1173Slice.Actor { IsPlayer = true, Sector = "home", FloorSector = "home" };
    }

    static VmPMapTryMove1173Slice.Trace Room(double ceiling)
    {
        return new VmPMapTryMove1173Slice.Trace { FloorZ = 0, CeilingZ = ceiling, FloorSector = "home" };
    }

    static VmPMapTryMove1173Slice.CrossingHost BlockedMonster(double top, double ceiling)
    {
        return new VmPMapTryMove1173Slice.CrossingHost
        {
            PositionOk = false,
            HasBlocking = true,
            BlockingTop = top,
            BlockingCeilingZ = ceiling,
        };
    }
}
