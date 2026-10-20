namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppStateMapTests
{
    [Fact]
    public void AssignDehIndices_SumsSpans()
    {
        var entries = new[]
        {
            new VmDehackedStateMapEntry { StateSpan = 2 },
            new VmDehackedStateMapEntry { StateSpan = 3 }
        };
        Assert.Equal(5, VmDehackedDehSuppStateMap.AssignDehIndices(entries));
    }
}
