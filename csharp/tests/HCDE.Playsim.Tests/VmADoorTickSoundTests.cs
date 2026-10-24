namespace HCDE.Playsim.Tests;

public class VmADoorTickSoundTests
{
    [Fact]
    public void Tick_WaitingDoorRaise_CountsDownThenCloses()
    {
        var sec = new VmADoorTickSound.DoorSector();
        var door = new VmADoorTickSound.DoorThinker
        {
            Sector = sec,
            Type = VmADoorTickSound.DoorRaise,
            Direction = 0,
            TopCountdown = 1,
        };
        var sounded = false;
        VmADoorTickSound.Tick(door, new VmADoorTickSound.DoorTickContext
        {
            MoveCeiling = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.Ok,
        });
        Assert.Equal(-1, door.Direction);
        VmADoorTickSound.DoorSound(door, true, new VmADoorTickSound.DoorTickContext
        {
            StartSequenceSound = (_, _, snd, _) => sounded = snd == "DoorNormal",
            GameType = 0,
        });
        Assert.True(sounded);
    }

    [Fact]
    public void Tick_OpenDirectionPastDest_DestroyThinker()
    {
        var sec = new VmADoorTickSound.DoorSector { SectorNum = 3 };
        var door = new VmADoorTickSound.DoorThinker
        {
            Sector = sec,
            Type = VmADoorTickSound.DoorOpen,
            Direction = 1,
            TopDist = 128,
        };
        var traced = false;
        VmADoorTickSound.Tick(door, new VmADoorTickSound.DoorTickContext
        {
            MoveCeiling = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            DiagTraceDoorMove = (n, phase, _, _) => traced = n == 3 && phase == "finished-open",
        });
        Assert.True(door.Destroyed);
        Assert.Null(sec.CeilingData);
        Assert.True(traced);
    }

    [Fact]
    public void DoorSound_StrifeDoorTexture_PicksStone()
    {
        var sec = new VmADoorTickSound.DoorSector();
        sec.Lines.Add(new VmADoorTickSound.DoorLine
        {
            BackSector = new VmADoorTickSound.DoorSector(),
            Side0 = new VmADoorTickSound.DoorSide { TopTexture = "DORSTON1" },
        });
        var door = new VmADoorTickSound.DoorThinker { Sector = sec, Speed = 1 };
        string? picked = null;
        VmADoorTickSound.DoorSound(door, true, new VmADoorTickSound.DoorTickContext
        {
            GameType = VmADoorTickSound.GameStrife,
            StartSequenceSound = (_, _, snd, _) => picked = snd,
        });
        Assert.Equal("DoorStone", picked);
    }

    [Fact]
    public void DoorSound_FastDoor_AddsChoiceOffset()
    {
        var sec = new VmADoorTickSound.DoorSector { SeqType = 2 };
        var door = new VmADoorTickSound.DoorThinker { Sector = sec, Speed = 8 };
        int? choice = null;
        VmADoorTickSound.DoorSound(door, false, new VmADoorTickSound.DoorTickContext
        {
            SeqModesSameByType = (_, _, _, _) => false,
            StartSequenceType = (_, _, _, _, c) => choice = c,
        });
        Assert.Equal(3, choice);
    }
}
