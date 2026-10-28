using System.Text;

namespace HCDE.MapLoader.Tests;

public class WadArchiveNamespaceTests
{
    [Fact]
    public void IsMarker_MatchesNativeBoomRules()
    {
        Assert.True(WadArchiveNamespaceAssigner.IsMarker("s_start", "s_start"));
        Assert.True(WadArchiveNamespaceAssigner.IsMarker("f_end", "f_end"));
        Assert.True(WadArchiveNamespaceAssigner.IsMarker("xx_start", "x_start"));
    }

    [Fact]
    public void AssignNamespaces_MarksSpritesBetweenMarkers()
    {
        var entries = new[]
        {
            new WadLumpEntry(0, 0, "s_start"),
            new WadLumpEntry(0, 16, "sprf"),
            new WadLumpEntry(0, 0, "s_end"),
            new WadLumpEntry(0, 4, "other"),
        };
        var metadata = entries.Select(_ => new WadLumpMetadata()).ToArray();
        WadArchiveNamespaceAssigner.AssignNamespaces(entries, metadata);
        Assert.Equal(WadLumpNamespace.Sprites, metadata[1].Namespace);
        Assert.Equal(WadLumpNamespace.Global, metadata[3].Namespace);
    }

    [Fact]
    public void TryOpenResource_AssignsNamespacesAndLzssMetadata()
    {
        var wad = TestWadBuilder.BuildCustomWad(
            ("S_START", Array.Empty<byte>()),
            ("SPRITE01", new byte[8]),
            ("S_END", Array.Empty<byte>()));
        Assert.True(WadArchiveReader.TryOpenResource(wad, out var result, out var error), error);
        Assert.Equal(WadLumpNamespace.Sprites, result!.Metadata[1].Namespace);
        Assert.False(result.Metadata[1].IsLzssCompressed);
    }

    [Fact]
    public void FlatHack_Flags4096ByteLumpsBeforeEndMarker()
    {
        var entries = new[]
        {
            new WadLumpEntry(0, 4096, "flat1"),
            new WadLumpEntry(0, 0, "f_end"),
        };
        var metadata = entries.Select(_ => new WadLumpMetadata()).ToArray();
        WadArchiveNamespaceAssigner.AssignNamespaces(entries, metadata);
        Assert.True(metadata[0].MaybeFlat);
    }
}
