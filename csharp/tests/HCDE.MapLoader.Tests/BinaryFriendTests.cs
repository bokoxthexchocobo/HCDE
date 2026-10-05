namespace HCDE.MapLoader.Tests;

public class BinaryFriendTests
{
    [Theory]
    [InlineData(2, false)]
    [InlineData(0x82, true)]
    [InlineData(0xC2, true)]
    [InlineData(0x182, false)]
    [InlineData(0x102, false)]
    [InlineData(0x8002, false)]
    [InlineData(0x8082, true)]
    public void DoomFriendlyFlagHonorsBadEditorMask(int options, bool expected)
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(BinaryMapDecoder.TryReadMap(wad, "MAP01", out var map, out _, out var error), error);
        var core = new BinaryMapRecords([new MapThingRecord(0, 0, 0, 3004, unchecked((short)options))],
            map.Core.Linedefs, map.Core.Sectors);
        var level = LevelBuilder.FromBinary(new BinaryMap(core, map.Geometry, map.Surface, map.Collision, map.Behavior), "MAP01");
        Assert.Equal(expected, Assert.Single(level.Things).Friendly);
        Assert.Equal(unchecked((short)options), level.Things[0].Options);
    }
}
