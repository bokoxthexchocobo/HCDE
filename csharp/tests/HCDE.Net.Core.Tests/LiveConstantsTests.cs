namespace HCDE.Net.Core.Tests;

public class LiveConstantsTests
{
    [Fact]
    public void KnownCapabilityMask_IncludesPredatorBit()
    {
        Assert.True((LiveConstants.KnownCapabilityMask & LiveConstants.CapPredatorSnapshotV1) != 0);
    }

    [Fact]
    public void LaneCount_MatchesCppEnum()
    {
        Assert.Equal(7, (int)LiveLane.Count);
    }
}
