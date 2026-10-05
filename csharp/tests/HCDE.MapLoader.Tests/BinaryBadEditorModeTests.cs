namespace HCDE.MapLoader.Tests;

public class BinaryBadEditorModeTests
{
    [Theory]
    [InlineData(0x62, false, false)]
    [InlineData(0x162, true, true)]
    [InlineData(0x42, false, true)]
    [InlineData(0x142, true, true)]
    [InlineData(0x22, true, false)]
    [InlineData(0x122, true, true)]
    [InlineData(0x816A, true, true)]
    public void BadEditorMarkerSuppressesMultiplayerExclusions(int options, bool coop, bool deathmatch)
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(BinaryMapDecoder.TryReadMap(wad, "MAP01", out var map, out _, out var error), error);
        var core = new BinaryMapRecords([new MapThingRecord(0, 0, 0, 3004, unchecked((short)options))],
            map.Core.Linedefs, map.Core.Sectors);
        var level = LevelBuilder.FromBinary(new BinaryMap(core, map.Geometry, map.Surface, map.Collision, map.Behavior), "MAP01");
        var thing = Assert.Single(level.Things);
        Assert.Equal(coop, thing.Coop);
        Assert.Equal(deathmatch, thing.Deathmatch);
        Assert.Equal(4, thing.SkillMask);
        Assert.Equal((options & 8) != 0, thing.Ambush);
        Assert.Equal(unchecked((short)options), thing.Options);
    }
}
