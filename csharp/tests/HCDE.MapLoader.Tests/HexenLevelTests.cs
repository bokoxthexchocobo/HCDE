using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class HexenLevelTests
{
    [Theory]
    [InlineData(0, true, false, false)]
    [InlineData(1, false, true, false)]
    [InlineData(2, false, false, false)]
    [InlineData(3, false, false, false)]
    [InlineData(4, false, false, false)]
    [InlineData(5, false, false, false)]
    [InlineData(6, false, true, true)]
    [InlineData(7, false, false, false)]
    public void ActivationMaskMatchesNativeSpac(int activation, bool cross, bool use, bool through)
    {
        var bytes = HexenWad();
        WadArchiveReader.TryReadDirectory(bytes, out var entries, out _);
        var line = entries.Single(entry => entry.Name == "LINEDEFS");
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((int)line.FilePosition + 4), (ushort)(activation << 10));
        Assert.True(LevelBuilder.TryFromWad(bytes, "MAP01", out var level, out var error), error);
        Assert.Equal((cross, use, through), (level.Lines[0].PlayerCross, level.Lines[0].PlayerUse, level.Lines[0].UseThrough));
    }
    public static byte[] HexenWad()
    {
        var original = TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true);
        WadArchiveReader.TryReadDirectory(original, out var entries, out _);
        var things = new byte[20];
        BinaryPrimitives.WriteUInt16LittleEndian(things, 42);
        BinaryPrimitives.WriteInt16LittleEndian(things.AsSpan(2), -100);
        BinaryPrimitives.WriteInt16LittleEndian(things.AsSpan(4), 200);
        BinaryPrimitives.WriteInt16LittleEndian(things.AsSpan(6), 16);
        BinaryPrimitives.WriteUInt16LittleEndian(things.AsSpan(8), 270);
        BinaryPrimitives.WriteUInt16LittleEndian(things.AsSpan(10), 3001);
        BinaryPrimitives.WriteUInt16LittleEndian(things.AsSpan(12), 2 | 512);
        things[14] = 80; things[15] = 7;
        var lines = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(lines.AsSpan(2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(lines.AsSpan(4), 1024 | 512);
        lines[6] = 12; lines[7] = 3; lines[8] = 16; lines[9] = 150;
        BinaryPrimitives.WriteUInt16LittleEndian(lines.AsSpan(14), ushort.MaxValue);
        var lumps = entries.Select(entry => (entry.Name, entry.Name == "THINGS" ? things
            : entry.Name == "LINEDEFS" ? lines : original.AsSpan((int)entry.FilePosition, (int)entry.Size).ToArray())).ToArray();
        return MapsModsTests.Wad(lumps);
    }

    [Fact]
    public void HexenRecordsKeepIdsHeightArgsFlagsAndBehavior()
    {
        Assert.True(LevelBuilder.TryFromWad(HexenWad(), "MAP01", out var level, out var error), error);
        Assert.Equal(MapDataFormat.HexenBinary, level.Format);
        var thing = Assert.Single(level.Things);
        Assert.Equal((-100d, 200d, 16d, 270d, 3001, 42, 4), (thing.X, thing.Y, thing.Z, thing.Angle, thing.Type, thing.Id, thing.SkillMask));
        Assert.True(thing.Coop); Assert.False(thing.Single); Assert.False(thing.Deathmatch);
        Assert.Equal(80, thing.Special); Assert.Equal(7, thing.Args[0]);
        var line = Assert.Single(level.Lines);
        Assert.Equal((12, 3, 16, 150, -1), (line.Special, line.Arg0, line.Arg1, line.Arg2, line.SideBack));
        Assert.True(line.PlayerUse); Assert.True(line.Repeat); Assert.False(line.PlayerCross);
        Assert.False(level.BehaviorData.IsEmpty);
    }

    [Theory]
    [InlineData("THINGS")]
    [InlineData("LINEDEFS")]
    [InlineData("BEHAVIOR")]
    public void BadHexenLumpBoundsFail(string name)
    {
        var bytes = HexenWad();
        WadArchiveReader.TryReadDirectory(bytes, out var entries, out _);
        var index = Array.FindIndex(entries, entry => entry.Name == name);
        var directory = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(8));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(directory + index * 16), uint.MaxValue);
        Assert.False(LevelBuilder.TryFromWad(bytes, "MAP01", out _, out _));
    }
}
