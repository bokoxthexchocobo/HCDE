namespace HCDE.Client.Tests;

public class VmSbarinfoCommandFlowControl1096SliceTests
{
    [Fact]
    public void ResolveIfWaterLevelFlowTruth_RequiresMinimumLevel()
    {
        Assert.True(VmSbarinfoCommandIfWaterLevel1096Slice.ResolveIfWaterLevelFlowTruth(
            playerWaterLevel: 2,
            threshold: 1));
    }

    [Fact]
    public void ResolveIfWaterLevelParseNegatableThreshold_StoresToken()
    {
        var (parsed, threshold) = VmSbarinfoCommandIfWaterLevel1096Slice.ResolveIfWaterLevelParseNegatableThreshold(3);
        Assert.True(parsed);
        Assert.Equal(3, threshold);
    }

    [Fact]
    public void ClassifyIfCVarIntSupportedType_RejectsUnsupported()
    {
        Assert.Equal(
            VmSbarinfoCommandIfCVarInt1096Slice.IfCVarIntValueType.Unsupported,
            VmSbarinfoCommandIfCVarInt1096Slice.ClassifyIfCVarIntSupportedType(
                isBoolCVar: false,
                isIntCVar: false));
    }

    [Fact]
    public void ResolveIfCVarIntParseThresholdFields_EnablesEqualComparison()
    {
        var (equal, value) = VmSbarinfoCommandIfCVarInt1096Slice.ResolveIfCVarIntParseThresholdFields(
            parsedInt: 8,
            optionalSecondComma: true,
            equalIdentifier: true);
        Assert.True(equal);
        Assert.Equal(8, value);
    }

    [Fact]
    public void ResolveIfCVarIntFlowTruthFromLiveValue_UsesGreaterOrEqualByDefault()
    {
        Assert.True(VmSbarinfoCommandIfCVarInt1096Slice.ResolveIfCVarIntFlowTruthFromLiveValue(
            cvarPresent: true,
            liveValue: 10,
            compareValue: 5,
            equalComparison: false));
    }

    [Fact]
    public void ResolveIfCVarIntFlowTruthFromLiveValue_MissingCVarIsFalse()
    {
        Assert.False(VmSbarinfoCommandIfCVarInt1096Slice.ResolveIfCVarIntFlowTruthFromLiveValue(
            cvarPresent: false,
            liveValue: 0,
            compareValue: 0,
            equalComparison: true));
    }

    [Fact]
    public void TryResolveSBarInfoFlowCommandKind_MapsIfCVarIntOrdinal()
    {
        var (recognized, kind) = VmSbarinfoCommandFlowControlNextCommand1096Slice.TryResolveSBarInfoFlowCommandKind("ifcvarint");
        Assert.True(recognized);
        Assert.Equal(VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfCVarInt, kind);
    }

    [Fact]
    public void IsNextCommandDispatchCaseWithinBand3600_StopsAtInInventory()
    {
        Assert.True(VmSbarinfoCommandFlowControlNextCommand1096Slice.IsNextCommandDispatchCaseWithinBand3600(
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.InInventory));
        Assert.False(VmSbarinfoCommandFlowControlNextCommand1096Slice.IsNextCommandDispatchCaseWithinBand3600(
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.Alpha));
    }

    [Fact]
    public void ResolveNextCommandWhenIdentifierMissing_ClosesBlock()
    {
        var (close, allocate) = VmSbarinfoCommandFlowControlNextCommand1096Slice.ResolveNextCommandWhenIdentifierMissing(
            identifierPresent: false);
        Assert.True(close);
        Assert.False(allocate);
    }

    [Fact]
    public void TryResolveSBarInfoFlowCommandKind_MapsWeaponAmmo()
    {
        var (recognized, kind) = VmSbarinfoCommandFlowControlNextCommand1096Slice.TryResolveSBarInfoFlowCommandKind("weaponammo");
        Assert.True(recognized);
        Assert.Equal(VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.WeaponAmmo, kind);
    }
}
