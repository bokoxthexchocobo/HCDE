namespace HCDE.Gamedata.Tests;

public class VmDehackedGlobalsTests
{
    [Fact]
    public void InfightingDefault_IsZero() => Assert.Equal(0, VmDehackedGlobals.InfightingDefault);
}
