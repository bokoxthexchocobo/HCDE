namespace HCDE.Playsim.Tests;

public class VmAcsStringPoolUnlockForLevelTests
{
    [Fact]
    public void RemoveLevelLock_FindsIndex()
    {
        Assert.Equal(1, VmAcsStringPoolUnlockForLevel.RemoveLevelLock([3, 7, 7], 7));
    }
}
