using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1452SliceTests
{
    static VmNet1452Slice.Bytes View(params byte[] data)
    {
        return new VmNet1452Slice.Bytes { Data = data, End = data.Length };
    }

    static bool SkipPrint(int command, VmNet1452Slice.Bytes stream)
    {
        if (command != 5 || stream.Count < 2)
            return false;
        stream.Advance(2);
        return true;
    }

    [Fact]
    public void UserCmdAxesAndFailClosedSkip()
    {
        var full = View(1, 0x01, 0x80, 0x80, 0x80, 0x01, 0x00, 0x02);
        Assert.True(VmNet1452Slice.TrySkipUserCmd(full, false, SkipPrint));
        Assert.Equal(6, full.At);
        var pitched = View(1, 0x03, 0x80, 0x01, 0x11, 0x22);
        Assert.True(VmNet1452Slice.TrySkipUserCmd(pitched, false, SkipPrint));
        Assert.Equal(6, pitched.At);

        var empty = View(2, 9);
        Assert.True(VmNet1452Slice.TrySkipUserCmd(empty, false, SkipPrint));
        Assert.Equal(1, empty.At);

        var shortPitch = View(1, VmNet1452Slice.Pitch, 1);
        Assert.False(VmNet1452Slice.TrySkipUserCmd(shortPitch, false, SkipPrint));
        Assert.Equal(2, shortPitch.At);

        var events = View(5, 0x41, 0);
        Assert.False(VmNet1452Slice.TrySkipUserCmd(events, false, SkipPrint));
        Assert.Equal(3, events.At);
        events.At = 0;
        Assert.True(VmNet1452Slice.TrySkipUserCmd(events, true, SkipPrint));

        var bare = View();
        Assert.False(VmNet1452Slice.TrySkipUserCmd(bare, true, SkipPrint));

        var failed = View(5, 1);
        VmNet1452Slice.SkipCommand(5, failed, (command, stream) => false);
        Assert.Equal(0, failed.At);
        VmNet1452Slice.SkipCommand(5, failed, SkipPrint);
        Assert.Equal(2, failed.At);
    }

    [Fact]
    public void RemainingRecordsAndBoundary()
    {
        byte[] paired = { 0, 1, 0, 1, 2 };
        Assert.True(VmNet1452Slice.TrySkipRemaining(paired, 0, paired.Length, 4, 0, 2, SkipPrint));
        byte[] duplicate = { 0, 1, 0, 0, 2 };
        Assert.False(VmNet1452Slice.TrySkipRemaining(duplicate, 0, duplicate.Length, 4, 0, 2, SkipPrint));
        byte[] high = { 4, 1, 0 };
        Assert.False(VmNet1452Slice.TrySkipRemaining(high, 0, high.Length, 4, 0, 1, SkipPrint));

        byte[] split = { 5, 0, 0, 3, 1, 0 };
        var boundary = VmNet1452Slice.TrySkipWithBoundary(split, 0, split.Length, false, 8, 0, 1, SkipPrint);
        Assert.Equal((true, 3), boundary);

        byte[] trailing = { 0, 1, 0, 9 };
        Assert.False(VmNet1452Slice.TrySkipRemaining(trailing, 0, trailing.Length, 4, 0, 1, SkipPrint));
    }
}
