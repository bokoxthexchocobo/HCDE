namespace HCDE.Client.Tests;

public class VmDNetCheckLevelStartTests
{
    [Fact]
    public void ParseGameticFromBuffer_BigEndian() =>
        Assert.Equal(258, VmDNetCheckLevelStart.ParseGameticFromBuffer(0, 0, 1, 2));

    [Fact]
    public void LateJoinSeat_WhenGameticAhead() =>
        Assert.True(VmDNetCheckLevelStart.LateJoinSeat(true, false, 100, 5));
}
