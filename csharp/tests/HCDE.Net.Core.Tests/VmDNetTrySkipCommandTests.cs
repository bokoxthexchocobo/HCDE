namespace HCDE.Net.Core.Tests;

public class VmDNetTrySkipCommandTests
{
    [Fact]
    public void TryReadCStringBytes_FindsTerminator()
    {
        var data = new byte[] { (byte)'a', 0, (byte)'b' };
        Assert.True(VmDNetTrySkipCommand.TryReadCStringBytes(3, 0, data, out var len));
        Assert.Equal(2, len);
    }

    [Fact]
    public void SkipSayOrTauntNative_AddsVariantByte() =>
        Assert.Equal(6ul, VmDNetTrySkipCommand.SkipSayOrTauntNative(5));

    [Fact]
    public void TrySkipZscCmdNative_ReadsSizePrefix()
    {
        var data = new byte[] { 0, 3, 1, 2, 3 };
        Assert.True(VmDNetTrySkipCommand.TrySkipZscCmdNative(5, 0, data, out var skip));
        Assert.Equal(5ul, skip);
    }

    [Fact]
    public void TrySkipSavegameNative_ReadsSecondString()
    {
        var data = new byte[] { (byte)'a', 0, (byte)'b', 0 };
        Assert.True(VmDNetTrySkipCommand.TrySkipSavegameNative(4, 2, data, out var skip));
        Assert.Equal(4ul, skip);
    }

    [Fact]
    public void PayloadlessSkipSizeNative_ReturnsZero() =>
        Assert.Equal(0ul, VmDNetTrySkipCommand.PayloadlessSkipSizeNative(0));

    [Fact]
    public void TrySkipSetSlotFamilyNative_SkipsWeapons()
    {
        var data = new byte[] { 0, 1, 0 };
        Assert.True(VmDNetTrySkipCommand.TrySkipSetSlotFamilyNative(0, 1, 3, data, out var skip));
        Assert.Equal(3ul, skip);
    }
}
