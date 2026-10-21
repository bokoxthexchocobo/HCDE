namespace HCDE.Playsim.Tests;

public class VmActionChangeFlagTests
{
    [Fact]
    public void CheckFlagOnOwner_FalseWhenOwnerNull() =>
        Assert.False(VmActionChangeFlag.CheckFlagOnOwner(null, true));

    [Fact]
    public void CheckFlagOnOwner_TrueWhenOwnerAndFlag() =>
        Assert.True(VmActionChangeFlag.CheckFlagOnOwner(new object(), true));
}
