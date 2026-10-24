namespace HCDE.Playsim.Tests;

public class VmAAnimatedDoorTests
{
    [Fact]
    public void StartClosing_BlockedWhenSectorTouching()
    {
        var sec = new VmAAnimatedDoor.AnimSector { TouchingThingList = new object() };
        var door = new VmAAnimatedDoor.AnimatedDoorThinker { Sector = sec, BotDist = 0 };
        Assert.False(VmAAnimatedDoor.StartClosing(door, new VmAAnimatedDoor.AnimTickContext()));
    }

    [Fact]
    public void Tick_OpeningFinishesWaitingWhenDelaySet()
    {
        var sec = new VmAAnimatedDoor.AnimSector();
        var line = new VmAAnimatedDoor.AnimLine { Side0 = new VmAAnimatedDoor.AnimSide(), Side1 = new VmAAnimatedDoor.AnimSide() };
        var anim = new VmAAnimatedDoor.DoorAnim { NumTextureFrames = 1, TextureFrames = { "A" } };
        var door = new VmAAnimatedDoor.AnimatedDoorThinker
        {
            Sector = sec,
            Line1 = line,
            Line2 = line,
            DoorAnim = anim,
            Status = VmAAnimatedDoor.StatusOpening,
            Timer = 1,
            Delay = 5,
            Speed = 2,
        };
        VmAAnimatedDoor.Tick(door, new VmAAnimatedDoor.AnimTickContext());
        Assert.Equal(VmAAnimatedDoor.StatusWaiting, door.Status);
        Assert.Equal(5, door.Timer);
    }

    [Fact]
    public void Serialize_CopiesThinkerFields()
    {
        var anim = new VmAAnimatedDoor.DoorAnim();
        var line = new VmAAnimatedDoor.AnimLine();
        var door = new VmAAnimatedDoor.AnimatedDoorThinker
        {
            Sector = new VmAAnimatedDoor.AnimSector(),
            Line1 = line,
            Line2 = line,
            DoorAnim = anim,
            Frame = 2,
            Timer = 3,
            BotDist = 4,
            Status = VmAAnimatedDoor.StatusWaiting,
            Speed = 5,
            Delay = 6,
            Type = VmAAnimatedDoor.AdClose,
            SetBlocking1 = true,
        };
        var arc = new VmAAnimatedDoor.SerializeState();
        VmAAnimatedDoor.Serialize(door, arc);
        Assert.Equal(2, arc.Frame);
        Assert.Same(anim, arc.DoorAnim);
        Assert.True(arc.SetBlocking1);
    }

    [Fact]
    public void Tick_ClosingFinishesClearsBlocking()
    {
        var sec = new VmAAnimatedDoor.AnimSector();
        var line = new VmAAnimatedDoor.AnimLine { Side0 = new VmAAnimatedDoor.AnimSide(), Side1 = new VmAAnimatedDoor.AnimSide() };
        var anim = new VmAAnimatedDoor.DoorAnim { NumTextureFrames = 1, TextureFrames = { "Z" } };
        var destroyed = false;
        var door = new VmAAnimatedDoor.AnimatedDoorThinker
        {
            Sector = sec,
            Line1 = line,
            Line2 = line,
            DoorAnim = anim,
            Status = VmAAnimatedDoor.StatusClosing,
            Timer = 1,
            Frame = 0,
            Speed = 1,
            BotDist = 10,
        };
        VmAAnimatedDoor.Tick(door, new VmAAnimatedDoor.AnimTickContext { Destroy = _ => destroyed = true });
        Assert.True(destroyed);
    }

    [Fact]
    public void EvSlidingDoor_Tag_IteratesSectors()
    {
        var sec = new VmAAnimatedDoor.AnimSector();
        var line = new VmAAnimatedDoor.AnimLine
        {
            BackSector = new VmAAnimatedDoor.AnimSector(),
            Side0 = new VmAAnimatedDoor.AnimSide { TopTexture = "d1" },
        };
        sec.Lines.Add(line);
        var hits = 0;
        VmAAnimatedDoor.EvSlidingDoor(new VmAAnimatedDoor.EvSlidingDoorContext
        {
            SectorTagIterator = _ => new[] { 0 },
            SectorByIndex = _ => sec,
            FindAnimatedDoor = _ => new VmAAnimatedDoor.DoorAnim(),
            CreateAnimatedDoor = (_, _, _, _, _, _) => { hits++; return true; },
        }, line, null, 5, 4, 0, VmAAnimatedDoor.AdOpenClose);
        Assert.Equal(1, hits);
    }

    [Fact]
    public void EvSlidingDoor_Manual_CreatesWhenAnimFound()
    {
        var sec = new VmAAnimatedDoor.AnimSector();
        var line = new VmAAnimatedDoor.AnimLine
        {
            BackSector = sec,
            Side0 = new VmAAnimatedDoor.AnimSide { TopTexture = "door1" },
        };
        var created = false;
        var ok = VmAAnimatedDoor.EvSlidingDoor(new VmAAnimatedDoor.EvSlidingDoorContext
        {
            FindAnimatedDoor = _ => new VmAAnimatedDoor.DoorAnim(),
            CreateAnimatedDoor = (s, l, sp, d, a, t) => created = ReferenceEquals(s, sec),
        }, line, null, 0, 6, 10, VmAAnimatedDoor.AdOpenClose);
        Assert.True(ok);
        Assert.True(created);
    }
}
