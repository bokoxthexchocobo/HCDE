namespace HCDE.Net.Core.Tests;

public class VmDNetLegacyEventPayloadTests
{
    [Fact]
    public void IsZeroPayloadEvent_Pause() =>
        Assert.True(VmDNetLegacyEventPayload.IsZeroPayloadEvent(VmDNetLegacyEventPayload.DemPause));

    [Fact]
    public void TryPlanLegacyRunArgs_NamedMasksCount()
    {
        Assert.True(VmDNetLegacyEventPayload.TryPlanLegacyRunArgs(true, out var leading, out var mask));
        Assert.Equal(0, leading);
        Assert.True(mask);
    }

    [Fact]
    public void MaskRunArgCount_NamedClearsHighBit() =>
        Assert.Equal(3, VmDNetLegacyEventPayload.MaskRunArgCount(131, named: true));

    [Fact]
    public void SetSlotNeedsPnumPrefix_OnlyPnumVariant() =>
        Assert.True(VmDNetLegacyEventPayload.SetSlotNeedsPnumPrefix(VmDNetLegacyEventPayload.DemSetslotpnum));

    [Fact]
    public void UsesLegacyCVarChange_SinfchangedFamily() =>
        Assert.True(VmDNetLegacyEventPayload.UsesLegacyCVarChange(VmDNetLegacyEventPayload.DemSinfchangedxor));

    [Fact]
    public void UsesRunArgs_RunscriptVariants() =>
        Assert.True(VmDNetLegacyEventPayload.UsesRunArgs(VmDNetLegacyEventPayload.DemRunscript2));

    [Fact]
    public void SetInvTrailingBytes_MatchesNative() =>
        Assert.Equal(5, VmDNetLegacyEventPayload.SetInvTrailingBytes);

    [Fact]
    public void ChangeMap2LeadingBytes_MatchesNative() =>
        Assert.Equal(1, VmDNetLegacyEventPayload.ChangeMap2LeadingBytes);

    [Fact]
    public void ConvReplyFieldBytes_MatchesNative() =>
        Assert.Equal(3, VmDNetLegacyEventPayload.ConvReplyFieldBytes);

    [Fact]
    public void GiveTakeCheatTrailingBytes_MatchesNative() =>
        Assert.Equal(4, VmDNetLegacyEventPayload.GiveTakeCheatTrailingBytes);

    [Fact]
    public void NetEventTrailingBytes_MatchesNative() =>
        Assert.Equal(14, VmDNetLegacyEventPayload.NetEventTrailingBytes);

    [Fact]
    public void Summon2TrailingBytes_MatchesNative() =>
        Assert.Equal(25, VmDNetLegacyEventPayload.Summon2TrailingBytes);

    [Fact]
    public void WarpCheatFieldBytes_MatchesNative() =>
        Assert.Equal(6, VmDNetLegacyEventPayload.WarpCheatFieldBytes);

    [Fact]
    public void TryReadSetSlotHeader_RoundTrip()
    {
        var input = new byte[] { 2, 3 };
        var cursor = 0;
        Assert.True(VmDNetLegacyEventPayload.TryReadSetSlotHeader(input, ref cursor, out var slot, out var count));
        Assert.Equal(2, slot);
        Assert.Equal(3, count);
    }
}
