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
}
