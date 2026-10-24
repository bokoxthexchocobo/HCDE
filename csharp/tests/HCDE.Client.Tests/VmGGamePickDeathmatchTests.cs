namespace HCDE.Client.Tests;

public class VmGGamePickDeathmatchTests
{
    [Fact]
    public void PickDeathmatch_EmptyStartsReturnsMaxAngle()
    {
        Assert.Equal(int.MaxValue, VmGGamePickDeathmatch.EmptyStart().Angle);
    }
}
