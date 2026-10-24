namespace HCDE.Client.Tests;

public class VmTypedCvarTests
{
    [Fact]
    public void SetGenericRep_LatchesUntilUnlatch()
    {
        var latch = new VmCvarLatchBuffer();
        var cvar = new VmBoolCvar("coop", false, VmCvarFlags.Latch | VmCvarFlags.IsDefault);
        cvar.SetGenericRep(VmCvarValue.FromBool(true), VmCvarValueType.Bool, latch, () => true);
        Assert.False(cvar.Value);
        Assert.Equal(1, latch.Count);
        latch.Unlatch();
        Assert.True(cvar.Value);
        Assert.Equal(0, latch.Count);
    }

    [Fact]
    public void IntCvar_ParsesIntegerRep()
    {
        var cvar = new VmIntCvar("skill", 2, VmCvarFlags.IsDefault);
        cvar.ForceSet(VmCvarValue.FromInt(4), VmCvarValueType.Int);
        Assert.Equal(4, cvar.Value);
    }
}
