using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class StrifeBinaryFlagTests
{
    [Theory]
    [InlineData(0x42, true, false)]
    [InlineData(0x22, false, true)]
    [InlineData(0x62, true, true)]
    [InlineData(0x82, false, false)]
    [InlineData(0x142, true, false)]
    [InlineData(0xA, false, false)]
    [InlineData(0x8062, true, true)]
    public void ExplicitStrifeLoadingUsesNativeAllyAndAmbushFlags(int flags, bool friendly, bool ambush)
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 8), (ushort)flags);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error,
            BinaryThingFlagFormat.Strife), error);
        var thing = Assert.Single(level.Things);
        Assert.Equal(friendly, thing.Friendly);
        Assert.Equal(ambush, thing.Ambush);
        Assert.True(thing.Single);
        Assert.True(thing.Coop);
        Assert.True(thing.Deathmatch);
        Assert.Equal(4, thing.SkillMask);
    }
}
