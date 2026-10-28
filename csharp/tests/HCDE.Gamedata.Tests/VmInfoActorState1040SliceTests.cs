namespace HCDE.Gamedata.Tests;

public class VmInfoActorState1040SliceTests
{
    [Fact]
    public void MatchesCallerType_AllowsWhenNoImplicitArgs()
    {
        var ctx = new VmInfoActorState1040Slice.StateCallerMatchContext(0, true, "AActor", false, null, 0);
        Assert.True(VmInfoActorState1040Slice.MatchesCallerType(ctx, null, null));
    }

    [Fact]
    public void MatchesCallerType_DehackedBypassesKindCheck()
    {
        var ctx = new VmInfoActorState1040Slice.StateCallerMatchContext(
            1, true, "AActor", false, null, VmInfoActorState1040Slice.StateDehackedFlag);
        Assert.True(VmInfoActorState1040Slice.MatchesCallerType(ctx, "Wrong", null));
    }

    [Fact]
    public void MatchesCallerType_RequiresKindOfSelf()
    {
        var ctx = new VmInfoActorState1040Slice.StateCallerMatchContext(1, true, "AActor", false, null, 0);
        Assert.True(VmInfoActorState1040Slice.MatchesCallerType(ctx, "AActor", null));
        Assert.False(VmInfoActorState1040Slice.MatchesCallerType(ctx, "Pistol", null));
    }

    [Fact]
    public void MatchesCallerType_SecondArgChecksStateOwner()
    {
        var ctx = new VmInfoActorState1040Slice.StateCallerMatchContext(
            2, true, "AActor", true, "AActor", 0);
        Assert.True(VmInfoActorState1040Slice.MatchesCallerType(ctx, "AActor", "AActor"));
        Assert.False(VmInfoActorState1040Slice.MatchesCallerType(ctx, "AActor", "Pistol"));
    }

    [Fact]
    public void ShouldAbortCheckCallerOnBadPrototype_WhenNotObjectPointer()
    {
        Assert.True(VmInfoActorState1040Slice.ShouldAbortCheckCallerOnBadPrototype(firstArgIsObjectPointer: false));
    }

    [Fact]
    public void ConstructActorInfoAtOffset_CopiesDefaultBytes()
    {
        var baseBytes = new byte[32];
        var def = new byte[] { 1, 2, 3, 4 };
        VmInfoActorState1040Slice.ConstructActorInfoAtOffset(baseBytes, 4, def);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, baseBytes.AsSpan(4, 4).ToArray());
    }

    [Fact]
    public void ShouldRegisterActorInfoMetaField_OnlyStarMeta()
    {
        Assert.True(VmInfoActorState1040Slice.ShouldRegisterActorInfoMetaField("*", isMeta: true));
        Assert.False(VmInfoActorState1040Slice.ShouldRegisterActorInfoMetaField("x", isMeta: true));
    }

    [Fact]
    public void CheckCallerTypePasses_DelegatesToMatches()
    {
        var ctx = new VmInfoActorState1040Slice.StateCallerMatchContext(1, true, "Foo", false, null, 0);
        Assert.True(VmInfoActorState1040Slice.CheckCallerTypePasses(ctx, "Foo", null));
    }

    [Fact]
    public void ShouldSkipCallerTypeMatch_WhenImplicitArgsZero()
    {
        Assert.True(VmInfoActorState1040Slice.ShouldSkipCallerTypeMatch(0));
    }
}
