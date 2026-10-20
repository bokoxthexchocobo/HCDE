namespace HCDE.Client.Tests;

public class VmDNetInvasionSnapshotApplyTests
{
    [Fact]
    public void VersionAllowed_AcceptsV1AndV2()
    {
        Assert.True(VmDNetInvasionSnapshotApply.VersionAllowed(1));
        Assert.True(VmDNetInvasionSnapshotApply.VersionAllowed(2));
    }

    [Fact]
    public void SpawnFlagsAllowed_RejectsUnknownBits() =>
        Assert.False(VmDNetInvasionSnapshotApply.SpawnFlagsAllowed(0x2));

    [Fact]
    public void ClampNonNegative_Floors() =>
        Assert.Equal(0, VmDNetInvasionSnapshotApply.ClampNonNegative(-3));

    [Fact]
    public void TraceGametypeOverride_WhenDisabledAndActiveState() =>
        Assert.True(VmDNetInvasionSnapshotApply.TraceGametypeOverride(2, false));
}
