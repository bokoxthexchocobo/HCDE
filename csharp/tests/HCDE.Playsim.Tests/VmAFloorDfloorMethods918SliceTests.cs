namespace HCDE.Playsim.Tests;

public class VmAFloorDfloorMethods918SliceTests
{
    [Fact]
    public void SetFloorChangeType_Case3_CopiesSpecialAndType()
    {
        var model = new VmAFloorCreate.FloorSector { FloorTexture = "FLR2", Special = new VmAFloorTickElevator.SectorSpecial { Tag = 7 } };
        var floor = new VmAFloorCreate.FloorThinker { Sector = new VmAFloorCreate.FloorSector() };
        VmAFloorDfloorMethods918Slice.SetFloorChangeType(floor, model, 3);
        Assert.Equal("FLR2", floor.Texture);
        Assert.Equal(VmAFloorDfloorMethods918Slice.GenFloorChgT, floor.Type);
        Assert.Equal(7, floor.NewSpecial.Tag);
    }

    [Fact]
    public void DfloorStartFloorSound_ForwardsToStaticHelper()
    {
        var floor = new VmAFloorCreate.FloorThinker { Sector = new VmAFloorCreate.FloorSector() };
        var hit = false;
        VmAFloorDfloorMethods918Slice.DfloorStartFloorSound(floor, _ => hit = true);
        Assert.True(hit);
    }

    [Fact]
    public void ConstructThinker_InvokesSuperConstruct()
    {
        var sec = new VmAFloorCreate.FloorSector();
        var hit = false;
        VmAFloorDfloorMethods918Slice.ConstructThinker(sec, _ => hit = true);
        Assert.True(hit);
    }
}
