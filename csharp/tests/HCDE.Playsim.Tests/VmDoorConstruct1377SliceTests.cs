namespace HCDE.Playsim.Tests;

public class VmDoorConstruct1377SliceTests
{
    [Fact]
    public void ConstructAppliesTypeLightAndBotDist()
    {
        var sec = Sector();
        var plain = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.ConstructSector(plain, sec);
        Assert.Equal(1, plain.SuperCalls);
        Assert.Same(sec, plain.Sector);
        Assert.Equal(0, plain.Type);

        var hooks = Hooks();
        var closed = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(closed, sec, VmDoorConstruct1377Slice.DoorClose, 2, 3, 7, 9, hooks);
        Assert.Equal(-1, closed.Direction);
        Assert.Equal(6d, closed.TopDist);
        Assert.Equal(10d, closed.BotDist);
        Assert.Equal(4, closed.BotSpot.X);
        Assert.False(hooks.Sounds[0].Raise);
        Assert.Null(hooks.Sounds[0].Sequence);
        Assert.Equal("construct", hooks.Moves[0].Kind);
        Assert.Equal(9d, hooks.Moves[0].Ceil);

        hooks = Hooks();
        var opened = new VmDoorConstruct1377Slice.Door();
        sec.CeilingPlaneD = 6;
        VmDoorConstruct1377Slice.Construct(opened, sec, VmDoorConstruct1377Slice.DoorOpen, 1, 0, 4, 0, hooks);
        Assert.Equal(1, opened.Direction);
        Assert.Empty(hooks.Sounds);
        sec.CeilingPlaneD = 1;

        hooks = Hooks();
        var raised = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(raised, sec, VmDoorConstruct1377Slice.DoorRaise, 1, 8, 4, 2, hooks);
        Assert.Equal(1, raised.Direction);
        Assert.True(hooks.Sounds[0].Raise);
        Assert.Equal(8, raised.TopWait);

        hooks = Hooks();
        var waiting = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(waiting, sec, VmDoorConstruct1377Slice.DoorCloseWaitOpen, 1, 0, 1, 0, hooks);
        Assert.Equal(sec.CeilingPlaneD, waiting.TopDist);
        Assert.Equal(-1, waiting.Direction);

        hooks = Hooks();
        var hold = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(hold, sec, VmDoorConstruct1377Slice.DoorWaitRaise, 1, 0, 1, 0, hooks);
        Assert.Equal(2, hold.Direction);
        Assert.Empty(hooks.Sounds);

        hooks = Hooks();
        sec.FloorData = "plat";
        var crush = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(crush, sec, VmDoorConstruct1377Slice.DoorWaitClose, 3, 5, 11, 1, hooks);
        Assert.Equal(VmDoorConstruct1377Slice.DoorRaise, crush.Type);
        Assert.Equal(0, crush.Direction);
        Assert.Equal(20d, crush.BotDist);
        Assert.Equal(8, crush.BotSpot.X);
        Assert.Equal(sec.FloorPlaneD, crush.OldFloorDist);
        Assert.Equal(sec.CeilingPlaneD, crush.TopDist);

        hooks = Hooks();
        hooks.CompatFlags = VmDoorConstruct1377Slice.CompatNoDoorLight;
        var dark = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(dark, sec, VmDoorConstruct1377Slice.DoorWaitRaise, 1, 0, 11, 0, hooks);
        Assert.Equal(0, dark.LightTag);

        hooks = Hooks();
        sec.FloorData = null;
        var unknown = new VmDoorConstruct1377Slice.Door();
        VmDoorConstruct1377Slice.Construct(unknown, sec, 99, 1, 0, 6, 0, hooks);
        Assert.Equal(0, unknown.Direction);
        Assert.Equal(6, unknown.LightTag);
        Assert.Equal(10, unknown.BotDist);
        Assert.Equal(VmDoorConstruct1377Slice.AnimatedDoorClassName, "DAnimatedDoor");
        Assert.Equal(1 << 9, VmDoorConstruct1377Slice.CompatNoDoorLight);
        Assert.Equal(1 << 4, VmDoorConstruct1377Slice.SpacPush);
        Assert.Equal(1 << 9, VmDoorConstruct1377Slice.SpacMPush);
    }

    [Fact]
    public void EvDoDoorChecksKeysReuseAndRemote()
    {
        var sec = Sector();
        var hooks = Hooks();
        hooks.CheckKeys = (_, _, _) => false;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), null, 0, 1, 0, 3, 0, false, 0));
        Assert.Empty(hooks.Events);

        hooks = Hooks();
        var seenRemote = false;
        hooks.CheckKeys = (_, lockNum, remote) =>
        {
            seenRemote = remote;
            Assert.Equal(4, lockNum);
            return true;
        };
        hooks.CreateThinker = (_, _, _, _, _, _) => true;
        hooks.TaggedSectors = _ => new[] { sec.SectorNum };
        hooks.SectorAt = index => index == sec.SectorNum ? sec : null;
        Assert.True(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorOpen, null, Player(true, false), 9, 2, 1, 4, 5, false, 6));
        Assert.True(seenRemote);
        Assert.Equal(9, hooks.Events[0].Tag);
        Assert.Equal(0, hooks.Events[0].Zero);
        Assert.Equal(2d, hooks.Events[0].Speed);
        Assert.Equal(7, hooks.Events[0].Player);

        hooks = Hooks();
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, null, null, 0, 1, 0, 0, 0, false, 0));
        var blocked = Line(null);
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, blocked, Player(false, false), 0, 1, 0, 0, 0, false, 0));
        Assert.Equal(VmDoorConstruct1377Slice.ChanVoice, hooks.Failures[0].Channel);
        Assert.Equal(0, hooks.Failures[0].Flags);
        Assert.Equal(VmDoorConstruct1377Slice.UseFailSoundName, hooks.Failures[0].Name);
        Assert.Equal(1d, hooks.Failures[0].Volume);
        Assert.Equal(VmDoorConstruct1377Slice.AttnNorm, hooks.Failures[0].Attenuation);

        var door = new VmDoorConstruct1377Slice.Door { Type = VmDoorConstruct1377Slice.DoorRaise, Direction = -1 };
        sec.CeilingDoor = door;
        hooks = Hooks();
        hooks.PlaneMoving = _ => true;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), Player(true, false), 0, 1, 0, 0, 0, false, 0));
        Assert.Equal(1, door.Direction);
        Assert.True(hooks.Sounds[0].Raise);
        Assert.Empty(hooks.Events);

        hooks = Hooks();
        hooks.PlaneMoving = _ => true;
        door.Direction = 1;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(
            hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec, VmDoorConstruct1377Slice.SpacPush), Player(true, false), 0, 1, 0, 0, 0, false, 0));
        Assert.Equal(1, door.Direction);

        hooks = Hooks();
        hooks.PlaneMoving = _ => true;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), null, 0, 1, 0, 0, 0, true, 0));
        Assert.Empty(hooks.Sounds);

        hooks = Hooks();
        hooks.PlaneMoving = _ => true;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), Player(true, true), 0, 1, 0, 0, 0, false, 0));
        hooks.Cooldown = true;
        hooks.Clients = 2;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), Player(true, false), 0, 1, 0, 0, 0, false, 0));
        hooks.Clients = 1;
        hooks.CheckSequence = (_, channel) =>
        {
            Assert.Equal(VmDoorConstruct1377Slice.ChanCeiling, channel);
            return "seq";
        };
        Assert.True(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), Player(true, false), 0, 1, 0, 0, 0, false, 0));
        Assert.Equal(-1, door.Direction);
        Assert.Equal("seq", hooks.Sounds[^1].Sequence);
        Assert.False(hooks.Sounds[^1].Raise);

        var moving = Sector();
        moving.SectorNum = 2;
        moving.CenterCeiling = 30;
        var quiet = Sector();
        quiet.SectorNum = 1;
        hooks = Hooks();
        hooks.PlaneMoving = target => target.SectorNum == 2;
        hooks.TaggedSectors = _ => new[] { 1, 2 };
        hooks.SectorAt = index => index == 1 ? quiet : moving;
        hooks.CreateThinker = (target, _, _, _, _, _) => target.SectorNum == 1;
        Assert.True(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorClose, null, null, 4, 1.5, 0, 0, 0, false, 0));
        Assert.Equal(2, hooks.Events[0].Sector);
        Assert.Equal(30d, hooks.Events[0].Ceil);
        Assert.Equal(-1, hooks.Events[0].Player);

        hooks = Hooks();
        hooks.CreateThinker = (_, _, _, _, _, _) => false;
        Assert.False(VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), null, 0, 1, 0, 0, 0, false, 0));
        Assert.Empty(hooks.Events);
        sec.CeilingDoor = new VmDoorConstruct1377Slice.Door { Type = VmDoorConstruct1377Slice.DoorRaise, Direction = 1 };
        hooks.PlaneMoving = _ => true;
        Assert.Throws<InvalidOperationException>(() =>
            VmDoorConstruct1377Slice.EvDoDoor(hooks, VmDoorConstruct1377Slice.DoorRaise, Line(sec), null, 0, 1, 0, 0, 0, false, 0));
    }

    static VmDoorConstruct1377Slice.Sector Sector() => new()
    {
        SectorNum = 4,
        FloorPlaneD = 3,
        CeilingPlaneD = 1,
        CenterCeiling = 9,
        CenterFloor = 2,
        PointToDist = (_, height) => height,
    };

    static VmDoorConstruct1377Slice.Hooks Hooks()
    {
        var hooks = new VmDoorConstruct1377Slice.Hooks
        {
            LowestCeilingSurrounding = (_, spot) =>
            {
                spot.X = 1;
                spot.Y = 2;
                return 10;
            },
            HighestFloorPoint = (_, spot) =>
            {
                spot.X = 4;
                spot.Y = 5;
                return 10;
            },
            LowestCeilingPoint = (_, spot) =>
            {
                spot.X = 8;
                spot.Y = 9;
                return 20;
            },
            IsPlat = data => data as string == "plat",
            IsLift = data => data as string == "plat",
            PlayerIndex = _ => 7,
        };
        return hooks;
    }

    static VmDoorConstruct1377Slice.Line Line(VmDoorConstruct1377Slice.Sector? back, int activation = 0) =>
        new() { Back = back, Activation = activation };

    static VmDoorConstruct1377Slice.Thing Player(bool hasPlayer, bool bot) =>
        new() { HasPlayer = hasPlayer, Bot = bot };
}
