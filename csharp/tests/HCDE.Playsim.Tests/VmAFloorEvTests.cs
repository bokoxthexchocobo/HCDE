namespace HCDE.Playsim.Tests;

public class VmAFloorEvTests
{
    [Fact]
    public void EvDoFloor_CallsCreateFloorPerTag()
    {
        var ok = VmAFloorEv.EvDoFloor(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => new VmAFloorEv.StairSector { SectorNum = 0 },
                ToCreateSector = s => new VmAFloorCreate.FloorSector { PointToDist = (_, _, z) => z },
                CreateFloor = new VmAFloorCreate.CreateFloorContext
                {
                    CreateThinker = s => new VmAFloorCreate.FloorThinker { Sector = s },
                },
            },
            VmAFloorCreate.FloorRaiseByValue,
            null,
            1,
            4,
            16,
            0,
            0,
            false,
            false);
        Assert.True(ok);
    }

    [Fact]
    public void EvStopFloor_DestroyActiveThinker()
    {
        var sec = new VmAFloorEv.StairSector { FloorData = new object() };
        var destroyed = false;
        VmAFloorEv.EvStopFloor(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                StopSequence = _ => { },
                DestroyFloorData = _ => { sec.FloorData = null; destroyed = true; },
            },
            1,
            null);
        Assert.True(destroyed);
    }

    [Fact]
    public void EvDoChange_TrigChange_CopiesFrontTexture()
    {
        var sec = new VmAFloorEv.StairSector();
        var tex = "";
        VmAFloorEv.EvDoChange(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                SetFloorTexture = (s, t) => tex = t,
                TransferSpecial = (_, _) => { },
            },
            new VmAFloorEv.EvLine { FrontSector = new VmAFloorEv.StairSector { FloorTexture = "FLAT1" } },
            VmAFloorEv.TrigChangeOnly,
            1);
        Assert.Equal("FLAT1", tex);
    }

    [Fact]
    public void EvBuildStairs_ZeroSpeed_ReturnsFalse()
    {
        Assert.False(VmAFloorEv.EvBuildStairs(new VmAFloorEv.FloorEvContext(), 1, VmAFloorEv.BuildUp, null, 8, 0, 0, 0, 0, 0));
    }
}
