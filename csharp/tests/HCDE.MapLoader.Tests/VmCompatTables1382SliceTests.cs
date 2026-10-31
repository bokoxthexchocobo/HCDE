namespace HCDE.MapLoader.Tests;

public class VmCompatTables1382SliceTests
{
    [Fact]
    public void OptionTableKeepsNativeFlagsAndTheNullSentinel()
    {
        var options = VmCompatTables1382Slice.Options;
        Assert.Equal(59, options.Length);
        Assert.Equal("setslopeoverflow", options[0].Name);
        Assert.Equal(1u, options[0].Flags);
        Assert.Equal(VmCompatTables1382Slice.SlotBcompat, options[0].Slot);
        var teleport = Assert.Single(options, option => option.Name == "ignoreteleporttags");
        Assert.Equal(1u << 3, teleport.Flags);
        Assert.Equal(VmCompatTables1382Slice.SlotBcompat, teleport.Slot);
        var midtex = Assert.Single(options, option => option.Name == "maskedmidtex");
        Assert.Equal(1u << 31, midtex.Flags);
        Assert.Equal(VmCompatTables1382Slice.SlotCompat, midtex.Slot);
        var mbf = Assert.Single(options, option => option.Name == "nombf21");
        Assert.Equal(1u << 14, mbf.Flags);
        Assert.Equal(VmCompatTables1382Slice.SlotCompat2, mbf.Slot);
        var id24 = Assert.Single(options, option => option.Name == "noid24");
        Assert.Equal(1u << 24, id24.Flags);
        Assert.Equal(3, VmCompatTables1382Slice.SlotCount);
        var tail = options[^1];
        Assert.Null(tail.Name);
        Assert.Equal(0u, tail.Flags);
        Assert.Equal(0, tail.Slot);

        Assert.Equal(new string?[] { "Front", "Back", null }, VmCompatTables1382Slice.LineSides);
        Assert.Equal(new string?[] { "Top", "Mid", "Bot", null }, VmCompatTables1382Slice.WallTiers);
        Assert.Equal(new string?[] { "floor", "ceil", null }, VmCompatTables1382Slice.SectorPlanes);
        Assert.Equal("corpsegibs", VmCompatTables1382Slice.ZandronumCompatCommands[^2]);
        Assert.Null(VmCompatTables1382Slice.ZandronumCompatCommands[^1]);
        Assert.Equal(10, VmCompatTables1382Slice.ZandronumCompatCommands.Length);
    }

    [Fact]
    public void HashUsesTheFirstWordAndCompareIsUnequal()
    {
        var digest = new byte[] { 0x11, 0x22, 0x33, 0x44, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 0, 0 };
        Assert.Equal(0x44332211u, VmCompatTables1382Slice.Hash(digest));
        var shortDigest = Assert.Throws<InvalidOperationException>(() => VmCompatTables1382Slice.Hash(digest.AsSpan(0, 4)));
        Assert.Equal("md5", shortDigest.Message);
        Assert.Equal(0, VmCompatTables1382Slice.Compare(1, 2, 3, 4, 1, 2, 3, 4));
        Assert.Equal(1, VmCompatTables1382Slice.Compare(1, 2, 3, 4, 9, 2, 3, 4));
        Assert.Equal(1, VmCompatTables1382Slice.Compare(1, 2, 3, 4, 1, 2, 3, 8));
    }
}
