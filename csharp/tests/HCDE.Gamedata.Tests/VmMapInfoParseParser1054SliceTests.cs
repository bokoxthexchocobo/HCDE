namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseParser1054SliceTests
{
    [Fact]
    public void ApplyClusterInfoReset_ClearsClusterFields()
    {
        var cluster = new VmMapInfoClusterReset1054
        {
            Cluster = 9,
            ClusterName = "Test",
            CdTrack = 1
        };
        VmMapInfoParseParser1054Slice.ApplyClusterInfoReset(cluster);
        Assert.Equal(0, cluster.Cluster);
        Assert.Equal(string.Empty, cluster.ClusterName);
        Assert.Equal(0, cluster.CdTrack);
    }

    [Fact]
    public void ResolveFormatAfterOpenBrace_DetectsNewFromBraceToken()
    {
        var enableCMode = false;
        var fmt = VmMapInfoParseParser1054Slice.ResolveFormatAfterOpenBrace(
            currentFormat: -1,
            sawOpenBraceToken: true,
            enableCMode: ref enableCMode);
        Assert.Equal(VmMapInfoParseParser1054Slice.FmtNew, fmt);
        Assert.True(enableCMode);
    }

    [Fact]
    public void ResolveFormatAfterOpenBrace_KeepsOldWhenSet()
    {
        var enableCMode = false;
        var fmt = VmMapInfoParseParser1054Slice.ResolveFormatAfterOpenBrace(
            VmMapInfoParseParser1054Slice.FmtOld,
            sawOpenBraceToken: true,
            enableCMode: ref enableCMode);
        Assert.Equal(VmMapInfoParseParser1054Slice.FmtOld, fmt);
        Assert.False(enableCMode);
    }

    [Fact]
    public void TryParseCloseBrace_OldFormatUngetsAndReturnsTrue()
    {
        Assert.True(VmMapInfoParseParser1054Slice.TryParseCloseBrace(
            VmMapInfoParseParser1054Slice.FmtOld,
            tokenIsCloseBrace: false,
            out var unget));
        Assert.True(unget);
    }

    [Fact]
    public void TryParseCloseBrace_NewFormatRequiresBraceToken()
    {
        Assert.True(VmMapInfoParseParser1054Slice.TryParseCloseBrace(
            VmMapInfoParseParser1054Slice.FmtNew,
            tokenIsCloseBrace: true,
            out _));
        Assert.False(VmMapInfoParseParser1054Slice.TryParseCloseBrace(
            VmMapInfoParseParser1054Slice.FmtNew,
            tokenIsCloseBrace: false,
            out _));
    }

    [Fact]
    public void CheckAssign_OnlyNewFormatWithEquals()
    {
        Assert.True(VmMapInfoParseParser1054Slice.CheckAssign(
            VmMapInfoParseParser1054Slice.FmtNew, tokenIsEquals: true));
        Assert.False(VmMapInfoParseParser1054Slice.CheckAssign(
            VmMapInfoParseParser1054Slice.FmtOld, tokenIsEquals: true));
    }

    [Fact]
    public void TryCheckNumber_NewFormatNeedsCommaThenNumber()
    {
        Assert.True(VmMapInfoParseParser1054Slice.TryCheckNumber(
            VmMapInfoParseParser1054Slice.FmtNew,
            tokenIsComma: true,
            numberConsumed: false,
            out var needsGet));
        Assert.True(needsGet);
    }

    [Fact]
    public void TryCheckFloat_OldFormatUsesScannerFloat()
    {
        Assert.True(VmMapInfoParseParser1054Slice.TryCheckFloat(
            VmMapInfoParseParser1054Slice.FmtOld,
            tokenIsComma: false,
            floatConsumed: true,
            out var needsGet));
        Assert.False(needsGet);
    }
}
