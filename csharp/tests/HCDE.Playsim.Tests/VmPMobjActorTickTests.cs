namespace HCDE.Playsim.Tests;

public class VmPMobjActorTickTests
{
    [Fact]
    public void Tick_NullState_MarksDestroyed()
    {
        var mo = new VmPMobjActorTick.TickActor { State = null };
        VmPMobjActorTick.Tick(new VmPMobjActorTick.TickState { Actor = mo });
        Assert.True(mo.Destroyed);
    }

    [Fact]
    public void Tick_Freezetics_DecrementsAndReturns()
    {
        var mo = new VmPMobjActorTick.TickActor { Freezetics = 2, State = new object() };
        VmPMobjActorTick.Tick(new VmPMobjActorTick.TickState { Actor = mo });
        Assert.Equal(1, mo.Freezetics);
    }

    [Fact]
    public void Tick_NoInteraction_ClearsScrollFlag()
    {
        var mo = new VmPMobjActorTick.TickActor
        {
            State = new object(),
            Flags5 = VmPMobjActorTick.Mf5Nointeraction,
            Flags8 = VmPMobjActorTick.Mf8Inscrollsec,
        };
        VmPMobjActorTick.Tick(new VmPMobjActorTick.TickState { Actor = mo });
        Assert.Equal(0, mo.Flags8 & VmPMobjActorTick.Mf8Inscrollsec);
    }

    [Fact]
    public void AverageSectorScroll_DividesWhenMultipleSectors()
    {
        var (x, y) = VmPMobjActorTick.AverageSectorScroll(false, 0, 2, 2, 4, 6);
        Assert.Equal(2, x);
        Assert.Equal(3, y);
    }

    [Fact]
    public void AddPlayerScrollSpecial_HexenNorthSlow_AddsCarry()
    {
        var (x, y) = VmPMobjActorTick.AddPlayerScrollSpecial(0, VmPMobjActorTick.ScrollNorthSlow, 0, (0, 0));
        Assert.True(x < 0 || y > 0);
    }

    [Fact]
    public void AccumulateSectorCarry_AveragesTwoScrollContributions()
    {
        var mo = new VmPMobjActorTick.TickActor
        {
            State = new object(),
            Flags8 = VmPMobjActorTick.Mf8Inscrollsec,
        };
        var st = new VmPMobjActorTick.TickState
        {
            Actor = mo,
            LevelHasScrolls = true,
            TouchingSectors = _ => new[]
            {
                new VmPMobjActorTick.SectorCarryNode
                {
                    SectorIndex = 0,
                    Special = 0,
                    FloorAtActor = 0,
                    FloorAtPos = 0,
                },
                new VmPMobjActorTick.SectorCarryNode
                {
                    SectorIndex = 1,
                    Special = 0,
                    FloorAtActor = 0,
                    FloorAtPos = 0,
                },
            },
            LevelScrollAt = i => i == 0 ? (2, 0) : (2, 0),
        };
        VmPMobjActorTick.Tick(st);
        Assert.Equal(2, mo.CummScrollX);
        Assert.Equal(0, mo.CummScrollY);
    }

    [Fact]
    public void SteepSlopePush_AddsPlaneNormalToVelocity()
    {
        var mo = new VmPMobjActorTick.TickActor
        {
            State = new object(),
            Flags = VmPMobjActorTick.MfSolid,
            Z = 0,
            FloorZ = 0,
        };
        var st = new VmPMobjActorTick.TickState
        {
            Actor = mo,
            FindFloorPlane = _ => new VmPMobjActorTick.FloorPlaneSample
            {
                FC = 0.1,
                NormalX = 0.5,
                NormalY = -0.25,
                ZAtFloorSector = 0,
            },
            XYMovement = _ => 0,
        };
        VmPMobjActorTick.Tick(st);
        Assert.Equal(0.5, mo.VelX);
        Assert.Equal(-0.25, mo.VelY);
    }

    [Fact]
    public void OnMobj_StepUp_SetsActorZToBlockerTop()
    {
        var onmo = new VmPMobjActorTick.OnMobjActor { Top = 20 };
        var mo = new VmPMobjActorTick.TickActor
        {
            State = new object(),
            Z = 8,
            FloorZ = 0,
            VelZ = 0,
            Flags2 = VmPMobjActorTick.Mf2Passmobj,
        };
        VmPMobjActorTick.Tick(new VmPMobjActorTick.TickState
        {
            Actor = mo,
            CheckOnMobj = _ => onmo,
            ZMovement = (_, _) => { },
        });
        Assert.Equal(20, mo.Z);
        Assert.Equal(0, mo.VelZ);
    }
}
