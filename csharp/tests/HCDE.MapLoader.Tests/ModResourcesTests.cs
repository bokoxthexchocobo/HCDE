using System.IO.Compression;
using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class ModResourcesTests
{
    [Fact]
    public void ExcessWadEntriesAreRejectedBeforeMaterializingDirectory()
    {
        var bytes = new byte[12 + 65537 * 16];
        "PWAD"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), 65537);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), 12);
        Assert.False(ModResources.TryLoad([bytes], out _, out var error));
        Assert.Equal("mod-entry-limit", error);
    }
    public static byte[] Zip(params (string Path, byte[] Data)[] entries)
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var entry in entries)
            {
                using var output = zip.CreateEntry(entry.Path).Open();
                output.Write(entry.Data);
            }
        return memory.ToArray();
    }

    [Fact]
    public void Pk3LoadsNamedMapAndPreservesPathsAndLastResourceWins()
    {
        var zip = Zip(("maps/MAP02.wad", TestWadBuilder.BuildMinimalMapWad("MAP01")),
            ("textures/wall.png", [1, 2]), ("sounds/wall.png", [3]));
        Assert.True(ModResources.TryLoad([zip, Zip(("textures/WALL.png", [4]))], out var resources, out var error), error);
        Assert.True(LevelBuilder.TryFromWad(resources.MapWad, "MAP02", out var level, out error), error);
        Assert.Equal("MAP02", level.MapName);
        Assert.Equal(new byte[] {4}, resources.Find("TEXTURES/wall.PNG")!.Data.ToArray());
        Assert.Equal(new byte[] {3}, resources.Find("sounds/wall.png")!.Data.ToArray());
        Assert.Null(resources.Find("wall.png"));
    }

    [Theory]
    [InlineData("../bad")]
    [InlineData("/bad")]
    [InlineData("C:/bad")]
    [InlineData("a/../bad")]
    [InlineData("a\\..\\bad")]
    public void InvalidArchivePathsFailWithoutExtraction(string path) =>
        Assert.False(ModResources.TryLoad([Zip((path, [1]))], out _, out _));

    [Fact]
    public void ExpandedEntryLimitIsCheckedBeforeAllocation()
    {
        var zip = Zip(("huge", [1]));
        var directory = zip.AsSpan().IndexOf(new byte[] { 0x50, 0x4b, 0x01, 0x02 });
        Assert.True(directory >= 0);
        BinaryPrimitives.WriteUInt32LittleEndian(zip.AsSpan(directory + 24), uint.MaxValue);
        Assert.False(ModResources.TryLoad([zip], out _, out var error));
        Assert.Equal("mod-expanded-size-or-entry-limit", error);
    }

    [Fact]
    public void CorruptZipAndNestedWadFailCleanly()
    {
        Assert.False(ModResources.TryLoad([[1, 2, 3]], out _, out _));
        Assert.False(ModResources.TryLoad([Zip(("maps/MAP01.wad", [1, 2, 3]))], out _, out _));
    }

    [Fact]
    public void CallerCannotChangeLoadedWadByChangingInput()
    {
        var bytes = MapsModsTests.Wad(("DEHACKED", [1, 2]));
        Assert.True(ModResources.TryLoad([bytes], out var resources, out _));
        Array.Clear(bytes);
        Assert.Equal(new byte[] {1, 2}, resources.Find("dehacked")!.Data.ToArray());
    }
}
