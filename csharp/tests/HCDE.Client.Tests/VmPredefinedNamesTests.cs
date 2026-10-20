namespace HCDE.Client.Tests;

public class VmPredefinedNamesTests
{
    [Fact]
    public void GetIndex_NoneIsZero()
    {
        Assert.Equal(0, VmPredefinedNames.GetIndex("None"));
        Assert.Equal("Actor", VmPredefinedNames.GetName(VmPredefinedNames.GetIndex("Actor")));
    }
}
