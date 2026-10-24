namespace HCDE.Scripting.Tests;

public class ZccScopeBarrierTests
{
    [Fact]
    public void TryValidateMemberCall_RejectsPrivateFromOtherClass()
    {
        Assert.False(ZccScopeBarrier.TryValidateMemberCall(
            ZccScopeBarrier.FlagPrivate,
            memberSymbolBelongsToDefiningClass: false,
            callerIsDescendantOfMemberClass: false,
            "Secret",
            out var error));
        Assert.Contains("private", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryValidateMemberCall_AllowsPrivateOnDefiningClass()
    {
        Assert.True(ZccScopeBarrier.TryValidateMemberCall(
            ZccScopeBarrier.FlagPrivate,
            memberSymbolBelongsToDefiningClass: true,
            callerIsDescendantOfMemberClass: false,
            "Secret",
            out _));
    }
}
