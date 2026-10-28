namespace HCDE.Playsim.Tests;

public class VmPOpenMapWadWalk993SliceTests
{
    [Fact]
    public void ClassicMapLumpAdvance_Uses992Scanner()
    {
        var last = 0;
        var result = VmPOpenMapWadWalk993Slice.TryAdvanceClassicMapLump(
            "THINGS",
            ref last,
            justCheck: true,
            out var index);
        Assert.Equal(VmPOpenMapWadWalk993Slice.ClassicMapLumpAdvanceResult.Assigned, result);
        Assert.Equal(1, index);
        Assert.Equal(1, last);

        result = VmPOpenMapWadWalk993Slice.TryAdvanceClassicMapLump(
            "LINEDEFS",
            ref last,
            justCheck: true,
            out index);
        Assert.Equal(VmPOpenMapWadWalk993Slice.ClassicMapLumpAdvanceResult.Assigned, result);
        Assert.Equal(2, index);

        last = 0;
        result = VmPOpenMapWadWalk993Slice.TryAdvanceClassicMapLump(
            "MISSING",
            ref last,
            justCheck: true,
            out _);
        Assert.Equal(VmPOpenMapWadWalk993Slice.ClassicMapLumpAdvanceResult.AbortOpen, result);
        Assert.True(VmPOpenMapWadWalk993Slice.ShouldAbortAfterMapIndex(
            VmPOpenMapHead992Slice.RequiredMissingIndex));
    }

    [Fact]
    public void TextMapSupplementLumps()
    {
        Assert.True(VmPOpenMapWadWalk993Slice.TryResolveTextMapSupplementLump(
            "BEHAVIOR",
            out var index,
            out var end,
            out var behavior));
        Assert.Equal(VmPOpenMapWadWalk993Slice.MapLumpBehavior, index);
        Assert.False(end);
        Assert.True(behavior);

        Assert.False(VmPOpenMapWadWalk993Slice.TryResolveTextMapSupplementLump(
            "ENDMAP",
            out _,
            out end,
            out _));
        Assert.True(end);

        Assert.Equal(
            VmPOpenMapWadWalk993Slice.TextMapSupplementLumpKind.GlZNodes,
            VmPOpenMapWadWalk993Slice.ClassifyTextMapSupplementLump("ZNODES"));
    }

    [Fact]
    public void EmbeddedWadAndMagic()
    {
        Assert.True(VmPOpenMapWadWalk993Slice.TrySelectEmbeddedMapsWadLump(
            lumpWad: 2,
            lumpMap: 5,
            out var selected));
        Assert.Equal(5, selected);
        Assert.False(VmPOpenMapWadWalk993Slice.TrySelectEmbeddedMapsWadLump(
            lumpWad: -1,
            lumpMap: -1,
            out _));

        Assert.True(VmPOpenMapWadWalk993Slice.IsRecognizedWadMagic(VmPOpenMapWadWalk993Slice.IwadMagic));
        Assert.True(VmPOpenMapWadWalk993Slice.NextLumpIsClassicDoomFormat("THINGS"));
        Assert.False(VmPOpenMapWadWalk993Slice.NextLumpIsClassicDoomFormat("TEXTMAP"));
        Assert.True(VmPOpenMapWadWalk993Slice.ResourceEntryIsTextMapHeader("TEXTMAP", entryIndex: 1));
    }
}
