namespace HCDE.Net.Core.Tests;

public class VmDNetBlackboxTests
{
    [Fact]
    public void ClampRecordCvarNative_EnforcesBounds()
    {
        Assert.Equal(0, VmDNetBlackbox.ClampRecordCvarNative(-1));
        Assert.Equal(1, VmDNetBlackbox.ClampRecordCvarNative(2));
    }

    [Fact]
    public void ClampSizeMbCvarNative_EnforcesBounds()
    {
        Assert.Equal(4, VmDNetBlackbox.ClampSizeMbCvarNative(1));
        Assert.Equal(256, VmDNetBlackbox.ClampSizeMbCvarNative(999));
    }

    [Fact]
    public void NetBlackboxAutoSaveNative_RespectsCooldown()
    {
        ulong last = 1000;
        Assert.False(VmDNetBlackbox.NetBlackboxAutoSaveNative(1, 2000, ref last, _ => true, "t"));
        Assert.True(VmDNetBlackbox.NetBlackboxAutoSaveNative(1, 7000, ref last, _ => true, "t"));
    }

    [Fact]
    public void BlackboxStoreEntryNative_EvictsForBudget()
    {
        var ring = new BlackboxRingState
        {
            Capacity = 4,
            StorageCount = 0,
            RingStart = 0,
            RingCount = 1,
            BytesUsed = 100,
            BudgetBytes = 120,
            PayloadSizes = new int[4],
        };
        ring.PayloadSizes[0] = 80;
        VmDNetBlackbox.BlackboxStoreEntryNative(ref ring, 80);
        Assert.Equal(1, ring.RingCount);
    }
}
