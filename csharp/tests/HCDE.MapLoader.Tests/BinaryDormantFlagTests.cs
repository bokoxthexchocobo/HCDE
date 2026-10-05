using System.Buffers.Binary;
namespace HCDE.MapLoader.Tests;
public class BinaryDormantFlagTests
{
    [Theory]
    [InlineData(258, false, false)]
    [InlineData(274, true, false)]
    [InlineData(266, false, true)]
    [InlineData(282, true, true)]
    public void HexenDormantAndAmbushDecodeIndependently(int flags, bool dormant, bool ambush)
    {
        var wad = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 12), (ushort)flags);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(MapDataFormat.HexenBinary, level.Format);
        var actor = Assert.Single(level.Things);
        Assert.Equal(dormant, actor.Dormant); Assert.Equal(ambush, actor.Ambush);
        Assert.True(actor.Single); Assert.Equal(4, actor.SkillMask);
        Assert.Equal(42, actor.Id); Assert.Equal(80, actor.Special);
    }

    [Theory]
    [InlineData(2, true, false)]
    [InlineData(18, true, false)]
    [InlineData(10, true, true)]
    [InlineData(26, true, true)]
    public void DoomBit16LeavesNativeSinglePlayerInclusionSet(int flags, bool single, bool ambush)
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 8), (ushort)flags);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(MapDataFormat.DoomBinary, level.Format);
        var actor = Assert.Single(level.Things);
        Assert.False(actor.Dormant); Assert.Equal(single, actor.Single); Assert.Equal(ambush, actor.Ambush);
    }
}
