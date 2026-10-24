namespace HCDE.Playsim.Tests;

public class VmPMobjGrindTests
{
    [Fact]
    public void Grind_CorpseDontCrunch_Shrinks()
    {
        var actor = new VmPMobjGrind.GrindActor
        {
            Flags = VmPMobjGrind.MfCorpse,
            Health = 0,
            DontCrunchCorpses = true,
        };
        Assert.False(VmPMobjGrind.Grind(new(), actor));
        Assert.True(actor.VelZeroed);
    }

    [Fact]
    public void Grind_Nonsolid_ReturnsFalse()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = 0 };
        Assert.False(VmPMobjGrind.Grind(new(), actor));
    }

    [Fact]
    public void Grind_ItemsFlag_PassedThroughContext()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = 0 };
        Assert.False(VmPMobjGrind.Grind(new() { Items = true }, actor));
    }

    [Fact]
    public void CallGrind_UsesVirtualWhenPresent()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = VmPMobjGrind.MfSolid | VmPMobjGrind.MfShootable };
        Assert.True(VmPMobjGrind.CallGrind(new(), actor, hasVirtual: true, vmReturn: 1));
    }
}
