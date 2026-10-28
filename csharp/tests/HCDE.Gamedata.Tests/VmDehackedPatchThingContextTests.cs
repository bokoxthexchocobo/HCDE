namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingContextTests
{
    [Fact]
    public void TryResolveThingType_RejectsOutOfRange()
    {
        Assert.False(VmDehackedPatchThingContext.TryResolveThingType(0, ["ZombieMan"], out _));
        Assert.True(VmDehackedPatchThingContext.TryResolveThingType(1, ["ZombieMan"], out var name));
        Assert.Equal("ZombieMan", name);
    }
}
