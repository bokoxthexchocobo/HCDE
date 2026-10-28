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

    [Fact]
    public void PendingWaveFromState_OnlyCountdownCarriesWave()
    {
        Assert.Equal(3, VmDNetInvasionSnapshotApply.PendingWaveFromState(2, 3));
        Assert.Equal(0, VmDNetInvasionSnapshotApply.PendingWaveFromState(1, 3));
    }

    [Fact]
    public void RunHeaderValidationPipeline_RejectsBadReserved()
    {
        var body = BuildMinimalHeader(version: 1, flags: 0, state: 2, reserved: 1);
        var ctx = NewContext(body);
        Assert.False(VmDNetInvasionSnapshotApply.RunHeaderValidationPipeline(ctx, out _));
    }

    [Fact]
    public void RunApplyInvasionSnapshotPipeline_AppliesV1Snapshot()
    {
        var body = BuildMinimalHeader(version: 1, flags: 0, state: 2, reserved: 0);
        var applied = false;
        var ctx = NewContext(body);
        ctx.ReadCoreFields = c =>
        {
            c.Payload = VmDNetInvasionSnapshotApply.BuildPayloadFromFields(
                1, 0, 2, 10, 1, 5, 20, 3, 2, 4, 0, 0, 0, 0, 0, 0);
            c.BodyCursor = c.BodyBytes;
            return true;
        };
        ctx.ApplyMirror = (_, _, _) => applied = true;
        Assert.True(VmDNetInvasionSnapshotApply.RunApplyInvasionSnapshotPipeline(ctx));
        Assert.True(applied);
    }

    [Fact]
    public void ValidateSpawnMetadataBlock_RejectsReservedBytes()
    {
        Assert.False(VmDNetInvasionSnapshotApply.ValidateSpawnMetadataBlock(0, 0, 1, 0));
        Assert.True(VmDNetInvasionSnapshotApply.ValidateSpawnMetadataBlock(0, 2, 0, 0));
    }

    static VmDNetInvasionSnapshotApply.ApplyInvasionSnapshotContext NewContext(byte[] body) =>
        new()
        {
            ValidClient = true,
            HasInvasionSnapshotCap = true,
            InvasionModeEnabled = true,
            Body = body,
            BodyBytes = body.Length,
            BodyCursor = 0,
            SnapshotMagic = VmDNetInvasionSnapshotApply.DefaultSnapshotMagic.ToArray(),
        };

    static byte[] BuildMinimalHeader(byte version, byte flags, byte state, byte reserved)
    {
        var body = new byte[48];
        "HCIV"u8.CopyTo(body);
        body[4] = version;
        body[5] = flags;
        body[6] = state;
        body[7] = reserved;
        return body;
    }
}
