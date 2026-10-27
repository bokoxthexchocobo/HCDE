namespace HCDE.Playsim.Tests;

public class VmAFloorElevatorHead921SliceTests
{
    [Fact]
    public void SerializeBundle_RoundTripsElevatorFields()
    {
        var el = new VmAFloorElevatorHead921Slice.ElevatorThinker
        {
            Sector = new VmAFloorElevatorHead921Slice.ElevatorSector(),
            Type = 2,
            Speed = 8,
            FloorDestDist = 64,
        };
        var bundle = VmAFloorElevatorHead921Slice.CaptureSerializeFields(el);
        el.Speed = 0;
        VmAFloorElevatorHead921Slice.ApplySerializeFields(el, bundle);
        Assert.Equal(8, el.Speed);
        Assert.Equal(64, el.FloorDestDist);
    }

    [Fact]
    public void ConstructElevator_WiresFloorAndCeilingData()
    {
        var sec = new VmAFloorElevatorHead921Slice.ElevatorSector
        {
            SetInterpolationFloor = _ => new object(),
            SetInterpolationCeiling = _ => new object(),
        };
        var el = new VmAFloorElevatorHead921Slice.ElevatorThinker { Sector = sec };
        VmAFloorElevatorHead921Slice.ConstructElevator(el, _ => { });
        Assert.Same(el, sec.FloorData);
        Assert.Same(el, sec.CeilingData);
    }

    [Fact]
    public void OnDestroyElevator_ReleasesInterpRefs()
    {
        var el = new VmAFloorElevatorHead921Slice.ElevatorThinker
        {
            Sector = new VmAFloorElevatorHead921Slice.ElevatorSector(),
            InterpFloor = new object(),
            InterpCeiling = new object(),
        };
        var refs = 0;
        VmAFloorElevatorHead921Slice.OnDestroyElevator(el, _ => refs++, _ => { });
        Assert.Equal(2, refs);
        Assert.True(el.SuperOnDestroyCalled);
    }
}
