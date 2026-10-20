namespace HCDE.Client.Tests;

public class VmDNetCanonicalEventPayloadTests
{
    [Fact]
    public void Summon2FamilyExtra_Is25() =>
        Assert.Equal(25, VmDNetCanonicalEventPayload.Summon2FamilyExtra());

    [Fact]
    public void ZscCmdChain_AllTrue() =>
        Assert.True(VmDNetCanonicalEventPayload.ZscCmdChain(true, true, true, true));
}
