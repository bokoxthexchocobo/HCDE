namespace HCDE.Playsim.Tests;

public class VmPSpecShowSecretCvarsTests
{
    [Fact]
    public void PrintSecretSector() =>
        Assert.True(VmPSpecShowSecretCvars.PrintSecretSector(true, 1));
}
