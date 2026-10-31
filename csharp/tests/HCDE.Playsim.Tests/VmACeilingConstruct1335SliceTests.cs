namespace HCDE.Playsim.Tests;

public class VmACeilingConstruct1335SliceTests
{
    [Fact]
    public void ConstructFieldsTypesAndTextureChange()
    {
        var linked = new VmACeilingConstruct1335.Sector();
        var interpKind = -1;
        linked.SetInterpolation = kind =>
        {
            interpKind = kind;
            return new VmACeilingConstruct1335.Interpolation();
        };
        var one = VmACeilingConstruct1335.Construct(linked);
        Assert.Same(one, linked.CeilingData);
        Assert.Equal(0, one.Crush);
        Assert.Equal(VmACeilingConstruct1335.CeilingMove, interpKind);
        Assert.NotNull(one.Interpolation);

        var four = VmACeilingConstruct1335.Construct(new VmACeilingConstruct1335.Sector(), 4, 9, 5);
        Assert.Equal(-1, four.Crush);
        Assert.Equal(VmACeilingConstruct1335.CrushDoom, four.CrushMode);
        Assert.Equal(4, four.Speed);
        Assert.Equal(4, four.Speed1);
        Assert.Equal(9, four.Speed2);
        Assert.Equal(5, four.Silent);
        Assert.Equal(0, four.Texture);
        Assert.Equal(0, four.Direction);
        Assert.Equal(0, four.OldDirection);
        Assert.Equal(0, four.Tag);

        var blocked = Sector(100, 64);
        blocked.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        var sounds = 0;
        var blockedOk = VmACeilingConstruct1335.CreateCeiling(
            Ctx(sounds: () => sounds++),
            blocked,
            (int)VmACeilingConstruct1335.ECeiling.LowerByValue,
            null,
            3,
            4,
            0,
            8,
            0,
            0,
            0,
            0);
        Assert.False(blockedOk);
        Assert.Null(blocked.CeilingData);
        Assert.Equal(0, sounds);

        var moving = Sector(100, 64);
        moving.CeilingData = new object();
        Assert.False(VmACeilingConstruct1335.CreateCeiling(
            Ctx(),
            moving,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            1,
            4,
            0,
            8,
            0,
            0,
            0,
            0));

        double seenX = -1;
        var raised = Sector(100, 64);
        raised.PointToDist = (v, h) =>
        {
            seenX = v.X;
            return h;
        };
        var raiseCtx = Ctx();
        raiseCtx.FindHighestCeilingSurrounding = (_, spot) =>
        {
            spot.Current = new VmACeilingConstruct1335.Vertex { X = 9, Y = 8 };
            return 200;
        };
        var raiseOk = VmACeilingConstruct1335.CreateCeiling(
            raiseCtx,
            raised,
            (int)VmACeilingConstruct1335.ECeiling.RaiseToHighest,
            null,
            11,
            4,
            1,
            0,
            0,
            0,
            0,
            VmACeilingConstruct1335.CrushHexen);
        var raiseThinker = (VmACeilingConstruct1335.Ceiling)raised.CeilingData!;
        Assert.True(raiseOk);
        Assert.Equal(9, seenX);
        Assert.Equal(1, raiseThinker.Direction);
        Assert.Equal(200, raiseThinker.TopHeight);
        Assert.Equal(11, raiseThinker.Tag);
        Assert.Equal(VmACeilingConstruct1335.CrushHexen, raiseThinker.CrushMode);
        Assert.NotNull(raiseThinker.Interpolation);

        var down = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerByValue,
            100,
            80,
            4,
            0,
            32,
            0,
            0,
            0);
        Assert.Equal(-1, down.Direction);
        Assert.Equal(48, down.BottomHeight);
        Assert.Equal(0, down.Silent);
        Assert.NotNull(down.Interpolation);

        var silent = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerByValue,
            100,
            80,
            52,
            0,
            32,
            0,
            5,
            0);
        Assert.Equal(2, silent.Silent);
        Assert.True(silent.Interpolation == null);

        var instant = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerInstant,
            100,
            90,
            4,
            8,
            50,
            0,
            0,
            0);
        Assert.Equal(50, instant.Speed);
        Assert.Equal(4, instant.Speed1);
        Assert.Equal(8, instant.Speed2);
        Assert.Equal(40, instant.BottomHeight);
        Assert.Equal(-1, instant.Direction);

        var equal = Make(
            (int)VmACeilingConstruct1335.ECeiling.MoveToValue,
            10,
            10,
            1,
            0,
            10,
            0,
            0,
            0);
        Assert.Equal(1, equal.Direction);
        Assert.Equal(10, equal.TopHeight);

        var below = Make(
            (int)VmACeilingConstruct1335.ECeiling.MoveToValue,
            40,
            40,
            1,
            0,
            10,
            0,
            0,
            0);
        Assert.Equal(-1, below.Direction);
        Assert.Equal(10, below.BottomHeight);

        var crush = Make(
            (int)VmACeilingConstruct1335.ECeiling.CrushAndRaise,
            100,
            64,
            2,
            3,
            8,
            12,
            0,
            0,
            floor: 40);
        Assert.Equal(100, crush.TopHeight);
        Assert.Equal(48, crush.BottomHeight);
        Assert.Equal(-1, crush.Direction);
        Assert.Equal(12, crush.Crush);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.CrushAndRaise, crush.Type);

        var stay = Make(
            (int)VmACeilingConstruct1335.ECeiling.CrushRaiseAndStay,
            70,
            20,
            1,
            1,
            5,
            0,
            0,
            0,
            floor: 15);
        Assert.Equal(70, stay.TopHeight);
        Assert.Equal(20, stay.BottomHeight);

        var lowerOnly = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerAndCrush,
            70,
            20,
            1,
            1,
            5,
            0,
            0,
            0,
            floor: 15);
        Assert.Equal(0, lowerOnly.TopHeight);
        Assert.Equal(20, lowerOnly.BottomHeight);

        var byTex = Make(
            (int)VmACeilingConstruct1335.ECeiling.RaiseByTexture,
            10,
            30,
            1,
            0,
            0,
            0,
            0,
            0,
            upper: 6);
        Assert.Equal(36, byTex.TopHeight);
        Assert.Equal(1, byTex.Direction);

        var lowTex = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerByTexture,
            10,
            30,
            1,
            0,
            0,
            0,
            0,
            0,
            upper: 6);
        Assert.Equal(24, lowTex.BottomHeight);

        var nearest = Make(
            (int)VmACeilingConstruct1335.ECeiling.RaiseToNearest,
            10,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            nextHigh: 77);
        Assert.Equal(77, nearest.TopHeight);

        var lowest = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerToLowest,
            10,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            lowest: 12);
        Assert.Equal(12, lowest.BottomHeight);
        Assert.Equal(-1, lowest.Direction);

        var upLowest = Make(
            (int)VmACeilingConstruct1335.ECeiling.RaiseToLowest,
            10,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            lowest: 12);
        Assert.Equal(12, upLowest.TopHeight);
        Assert.Equal(1, upLowest.Direction);

        var floor = Make(
            (int)VmACeilingConstruct1335.ECeiling.LowerToHighestFloor,
            10,
            0,
            1,
            0,
            3,
            0,
            0,
            0,
            aroundFloor: 20);
        Assert.Equal(23, floor.BottomHeight);

        var placeholder = Make(
            (int)VmACeilingConstruct1335.ECeiling.Placeholder,
            5,
            0,
            1,
            0,
            0,
            0,
            0,
            0);
        Assert.Equal(0, placeholder.Direction);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.Placeholder, placeholder.Type);
        Assert.Null(placeholder.Interpolation);

        var secSpecial = new VmACeilingConstruct1335.Special
        {
            DamageType = "Slime",
            DamageAmount = 9,
            SpecialId = 4,
            DamageInterval = 7,
            LeakyDamage = 2,
            Flags = VmACeilingConstruct1335.SpecialFlags | 1 | unchecked((int)0x80000000),
        };
        var frontSpecial = new VmACeilingConstruct1335.Special
        {
            DamageType = "Lava",
            DamageAmount = 3,
            SpecialId = 8,
            DamageInterval = 1,
            LeakyDamage = 1,
            Flags = VmACeilingConstruct1335.SecfPush | 1,
        };
        var modelFloor = new VmACeilingConstruct1335.Sector { CeilingTexture = 70 };
        var modelCeil = new VmACeilingConstruct1335.Sector { CeilingTexture = 80 };
        var host = Sector(100, 50);
        host.Special = secSpecial;
        host.CeilingTexture = 7;
        var front = new VmACeilingConstruct1335.Sector
        {
            CeilingTexture = 44,
            Special = frontSpecial,
        };
        var line = new VmACeilingConstruct1335.Line { Front = front };
        var numeric = VmACeilingConstruct1335.CreateCeiling(
            ModelCtx(modelFloor, modelCeil),
            host,
            (int)VmACeilingConstruct1335.ECeiling.LowerToFloor,
            line,
            2,
            1,
            0,
            4,
            0,
            0,
            6,
            0);
        var numericThinker = (VmACeilingConstruct1335.Ceiling)host.CeilingData!;
        Assert.True(numeric);
        Assert.Equal(70, numericThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.GenChgT, numericThinker.Type);
        Assert.Equal(9, numericThinker.NewSpecial.DamageAmount);
        Assert.Equal("Slime", numericThinker.NewSpecial.DamageType);
        Assert.Equal(4, numericThinker.NewSpecial.SpecialId);
        Assert.Equal((short)7, numericThinker.NewSpecial.DamageInterval);
        Assert.Equal((short)2, numericThinker.NewSpecial.LeakyDamage);
        Assert.Equal(VmACeilingConstruct1335.SpecialFlags, numericThinker.NewSpecial.Flags);

        var zeroed = Sector(100, 50);
        zeroed.Special = secSpecial;
        var zeroLine = new VmACeilingConstruct1335.Line { Front = front };
        VmACeilingConstruct1335.CreateCeiling(
            ModelCtx(modelFloor, modelCeil),
            zeroed,
            (int)VmACeilingConstruct1335.ECeiling.RaiseToHighest,
            zeroLine,
            1,
            1,
            0,
            0,
            0,
            0,
            5,
            0);
        var zeroThinker = (VmACeilingConstruct1335.Ceiling)zeroed.CeilingData!;
        Assert.Equal(80, zeroThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.GenChg0, zeroThinker.Type);
        Assert.Equal(0, zeroThinker.NewSpecial.DamageAmount);
        Assert.Equal("", zeroThinker.NewSpecial.DamageType);

        var left = Sector(100, 50);
        left.Special = secSpecial;
        VmACeilingConstruct1335.CreateCeiling(
            ModelCtx(modelFloor, null),
            left,
            (int)VmACeilingConstruct1335.ECeiling.RaiseToFloor,
            line,
            1,
            1,
            0,
            1,
            0,
            0,
            7,
            0);
        var leftThinker = (VmACeilingConstruct1335.Ceiling)left.CeilingData!;
        Assert.Equal(70, leftThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.GenChg, leftThinker.Type);
        Assert.Equal(0, leftThinker.NewSpecial.DamageAmount);

        var trigger = Sector(100, 50);
        trigger.Special = secSpecial;
        VmACeilingConstruct1335.CreateCeiling(
            Ctx(),
            trigger,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            line,
            1,
            1,
            0,
            1,
            0,
            0,
            2,
            0);
        var triggerThinker = (VmACeilingConstruct1335.Ceiling)trigger.CeilingData!;
        Assert.Equal(44, triggerThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.GenChgT, triggerThinker.Type);
        Assert.Equal(3, triggerThinker.NewSpecial.DamageAmount);
        Assert.Equal(VmACeilingConstruct1335.SecfPush, triggerThinker.NewSpecial.Flags);

        var missed = Sector(100, 50);
        var missedLine = new VmACeilingConstruct1335.Line { Front = front };
        VmACeilingConstruct1335.CreateCeiling(
            ModelCtx(null, null),
            missed,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            missedLine,
            1,
            1,
            0,
            1,
            0,
            0,
            7,
            0);
        var missedThinker = (VmACeilingConstruct1335.Ceiling)missed.CeilingData!;
        Assert.Equal(0, missedThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.RaiseByValue, missedThinker.Type);

        var noLine = Sector(100, 50);
        VmACeilingConstruct1335.CreateCeiling(
            Ctx(),
            noLine,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            1,
            1,
            0,
            1,
            0,
            0,
            2,
            0);
        var noLineThinker = (VmACeilingConstruct1335.Ceiling)noLine.CeilingData!;
        Assert.Equal(0, noLineThinker.Texture);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.RaiseByValue, noLineThinker.Type);
    }

    [Fact]
    public void EvManualCrushIteratorAndActionTag()
    {
        var calls = new List<string>();
        var back = Sector(100, 40);
        back.SectorNum = 7;
        var line = new VmACeilingConstruct1335.Line { Back = back };
        var manual = Ctx();
        manual.ActivateInStasisCeiling = tag =>
        {
            calls.Add("stasis:" + tag);
            return true;
        };
        manual.PlayCeilingSound = c => calls.Add("play:" + c.Tag);
        Assert.False(VmACeilingConstruct1335.EvDoCeiling(
            manual,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0));
        Assert.Empty(calls);

        back.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        Assert.False(VmACeilingConstruct1335.EvDoCeiling(
            manual,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            line,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0));
        Assert.Equal(new[] { "stasis:16777223" }, calls);
        Assert.Null(back.CeilingData);

        calls.Clear();
        back.CeilingPlaneFlags = 0;
        Assert.True(VmACeilingConstruct1335.EvDoCeiling(
            manual,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            line,
            0,
            4,
            2,
            8,
            0,
            0,
            0,
            0));
        var manualThinker = (VmACeilingConstruct1335.Ceiling)back.CeilingData!;
        Assert.Equal(16777223, manualThinker.Tag);
        Assert.Equal(1, manualThinker.Direction);
        Assert.Equal(new[] { "stasis:16777223", "play:16777223" }, calls);

        var a = Sector(20, 10);
        var b = Sector(20, 10);
        var c = Sector(20, 10);
        a.SectorNum = 0;
        b.SectorNum = 1;
        c.SectorNum = 2;
        b.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        var visited = new List<int>();
        var iter = Ctx();
        iter.Sectors = new[] { a, b, c };
        iter.TagSectors = new[] { 1, 0, -5, 2 };
        iter.ActivateInStasisCeiling = tag =>
        {
            visited.Add(tag);
            return true;
        };
        iter.PlayCeilingSound = thinker => visited.Add(100 + thinker.Sector.SectorNum);
        Assert.True(VmACeilingConstruct1335.EvDoCeiling(
            iter,
            (int)VmACeilingConstruct1335.ECeiling.CrushAndRaise,
            null,
            4,
            1,
            1,
            2,
            0,
            0,
            0,
            0));
        Assert.Equal(new[] { 4, 100 }, visited);
        Assert.Null(b.CeilingData);
        Assert.NotNull(a.CeilingData);
        Assert.Null(c.CeilingData);
        Assert.Equal(4, ((VmACeilingConstruct1335.Ceiling)a.CeilingData!).Tag);
        Assert.Equal((int)VmACeilingConstruct1335.ECeiling.CrushAndRaise, ((VmACeilingConstruct1335.Ceiling)a.CeilingData!).Type);

        var quiet = new List<int>();
        var plain = Ctx();
        var only = Sector(20, 10);
        plain.Sectors = new[] { only };
        plain.TagSectors = new[] { 0 };
        plain.ActivateInStasisCeiling = _ =>
        {
            quiet.Add(1);
            return true;
        };
        Assert.True(VmACeilingConstruct1335.EvDoCeiling(
            plain,
            (int)VmACeilingConstruct1335.ECeiling.Placeholder,
            null,
            9,
            1,
            0,
            0,
            0,
            0,
            0,
            0));
        Assert.Empty(quiet);

        var stalled = Ctx();
        var blockedA = Sector(20, 10);
        var blockedB = Sector(20, 10);
        blockedA.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        blockedB.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        stalled.Sectors = new[] { blockedA, blockedB };
        stalled.TagSectors = new[] { 0, 1 };
        stalled.ActivateInStasisCeiling = _ => true;
        Assert.True(VmACeilingConstruct1335.EvDoCeiling(
            stalled,
            (int)VmACeilingConstruct1335.ECeiling.CrushAndRaise,
            null,
            6,
            1,
            1,
            1,
            0,
            0,
            0,
            0));
        Assert.Null(blockedA.CeilingData);
        Assert.Null(blockedB.CeilingData);

        var none = Ctx();
        var idle = Sector(20, 10);
        idle.CeilingPlaneFlags = VmACeilingConstruct1335.PlanefBlocked;
        none.Sectors = new[] { idle };
        none.TagSectors = new[] { 0 };
        none.ActivateInStasisCeiling = _ => false;
        Assert.False(VmACeilingConstruct1335.EvDoCeiling(
            none,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            3,
            1,
            0,
            1,
            0,
            0,
            0,
            0));

        var acted = Sector(30, 12);
        var action = Ctx();
        Assert.Throws<ArgumentNullException>(() => VmACeilingConstruct1335.CallCreateCeiling(
            action,
            null,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            1,
            0,
            4,
            0,
            0,
            0,
            0));
        Assert.True(VmACeilingConstruct1335.CallCreateCeiling(
            action,
            acted,
            (int)VmACeilingConstruct1335.ECeiling.RaiseByValue,
            null,
            3,
            1,
            4,
            6,
            0,
            0,
            VmACeilingConstruct1335.CrushSlowdown));
        var actionThinker = (VmACeilingConstruct1335.Ceiling)acted.CeilingData!;
        Assert.Equal(0, actionThinker.Tag);
        Assert.Equal(16, actionThinker.TopHeight);
        Assert.Equal(6, actionThinker.Crush);
        Assert.Equal(VmACeilingConstruct1335.CrushSlowdown, actionThinker.CrushMode);
    }

    static VmACeilingConstruct1335.Sector Sector(double fd, double z)
    {
        var sec = new VmACeilingConstruct1335.Sector
        {
            CeilingPlaneD = fd,
            ZatPoint = _ => z,
            PointToDist = (_, h) => h,
            SetInterpolation = _ => new VmACeilingConstruct1335.Interpolation(),
        };
        sec.Lines.Add(new VmACeilingConstruct1335.Line
        {
            V1 = new VmACeilingConstruct1335.Vertex { X = 1, Y = 2 },
        });
        return sec;
    }

    static VmACeilingConstruct1335.Context Ctx(Action? sounds = null)
    {
        return new VmACeilingConstruct1335.Context
        {
            PlayCeilingSound = sounds == null ? null : _ => sounds(),
            FindHighestFloorPoint = (_, _) => 40,
            FindHighestCeilingSurrounding = (_, _) => 0,
            FindHighestFloorSurrounding = (_, _) => 0,
            FindNextLowestCeiling = (_, _) => 0,
            FindNextHighestCeiling = (_, _) => 0,
            FindLowestCeilingSurrounding = (_, _) => 0,
            FindShortestUpperAround = _ => 0,
        };
    }

    static VmACeilingConstruct1335.Context ModelCtx(
        VmACeilingConstruct1335.Sector? floor,
        VmACeilingConstruct1335.Sector? ceiling)
    {
        var ctx = Ctx();
        ctx.FindModelFloorSector = (_, _) => floor;
        ctx.FindModelCeilingSector = (_, _) => ceiling;
        ctx.FindHighestFloorPoint = (_, _) => 40;
        ctx.FindHighestCeilingSurrounding = (_, _) => 90;
        return ctx;
    }

    static VmACeilingConstruct1335.Ceiling Make(
        int type,
        double fd,
        double z,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        double floor = 0,
        double upper = 0,
        double nextHigh = 0,
        double lowest = 0,
        double aroundFloor = 0)
    {
        var sec = Sector(fd, z);
        var ctx = Ctx();
        ctx.FindHighestFloorPoint = (_, _) => floor;
        ctx.FindShortestUpperAround = _ => upper;
        ctx.FindNextHighestCeiling = (_, _) => nextHigh;
        ctx.FindLowestCeilingSurrounding = (_, _) => lowest;
        ctx.FindHighestFloorSurrounding = (_, _) => aroundFloor;
        var ok = VmACeilingConstruct1335.CreateCeiling(
            ctx, sec, type, null, 1, speed, speed2, height, crush, silent, change, 0);
        Assert.True(ok);
        return (VmACeilingConstruct1335.Ceiling)sec.CeilingData!;
    }
}
