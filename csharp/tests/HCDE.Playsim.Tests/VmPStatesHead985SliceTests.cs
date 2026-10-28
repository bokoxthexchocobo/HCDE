namespace HCDE.Playsim.Tests;

public class VmPStatesHead985SliceTests
{
    [Fact]
    public void GetSpriteTexture_SelectorsAndReturns()
    {
        Assert.Equal(5, VmPStatesHead985Slice.SelectEffectiveSprite(
            VmPStatesHead985Slice.UseSelfSprite,
            selfSprite: 5));
        Assert.Equal(2, VmPStatesHead985Slice.SelectEffectiveFrame(
            VmPStatesHead985Slice.UseSelfFrame,
            selfFrame: 2));
        Assert.True(VmPStatesHead985Slice.UsesAlternateSkinSprite(skinIndex: 1));
        Assert.Equal(3, VmPStatesHead985Slice.CapSpriteTextureReturnCount(numReturnValues: 9));
        Assert.True(VmPStatesHead985Slice.SpriteFlipForRotation(flipField: 4, rotation: 2));
        Assert.True(VmPStatesHead985Slice.ShouldWriteTextureReturn(1));
        Assert.False(VmPStatesHead985Slice.ShouldWriteScaleReturn(2));
    }

    [Fact]
    public void FindStateOwner_ScansClassesAndHierarchy()
    {
        var state = new object();
        var child = "Child";
        var parent = "Parent";
        bool Owns(object? actor, object? st) =>
            ReferenceEquals(actor, child) && ReferenceEquals(st, state);

        var fromScan = VmPStatesHead985Slice.ScanAllActorClassesForStateOwner(
            state,
            i => i == 0 ? parent : child,
            Owns,
            classCount: 2);
        Assert.Equal(child, fromScan);

        var fromHierarchy = VmPStatesHead985Slice.FindStateOwnerInHierarchy(
            state,
            child,
            actor => ReferenceEquals(actor, child) ? parent : null,
            Owns);
        Assert.Equal(child, fromHierarchy);
    }

    [Fact]
    public void StateNamingAndLabels()
    {
        Assert.Equal("DehExtraState.4", VmPStatesHead985Slice.FormatDehExtraStateName(4));
        Assert.Equal(
            "<unknown>",
            VmPStatesHead985Slice.ResolveStateDisplayName(
                new object(),
                globalOwner: null,
                hierarchyOwner: null,
                dehIndex: 0,
                stateIndexFromOwner: null));
        Assert.Equal(
            "Zombie.3",
            VmPStatesHead985Slice.ResolveStateDisplayName(
                new object(),
                globalOwner: "Zombie",
                hierarchyOwner: null,
                dehIndex: 0,
                stateIndexFromOwner: _ => 3));
        Assert.True(VmPStatesHead985Slice.StateLabelLookupEnabled(labelCount: 2));
    }
}
