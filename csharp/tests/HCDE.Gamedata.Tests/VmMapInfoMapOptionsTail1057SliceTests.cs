namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsTail1057SliceTests
{
    [Fact]
    public void ApplyHorizWallShade_HalvesAndClamps()
    {
        Assert.Equal(63, VmMapInfoMapOptionsTail1057Slice.ApplyHorizWallShade(126));
    }

    [Fact]
    public void ApplyNoGravity_UsesDblMaxMarker()
    {
        var draft = new VmMapInfoMapOptionsTail1057Slice.MapOptionsTailDraft();
        VmMapInfoMapOptionsTail1057Slice.ApplyNoGravity(draft);
        Assert.Equal(double.MaxValue, draft.Gravity);
    }

    [Fact]
    public void SpecialActionLookupFailed_RejectsZeroAction()
    {
        Assert.True(VmMapInfoMapOptionsTail1057Slice.SpecialActionLookupFailed(action: 0, minArg: 1));
    }

    [Fact]
    public void CountTrailingSpecialActionArgs_StopsAtFive()
    {
        Assert.Equal(5, VmMapInfoMapOptionsTail1057Slice.CountTrailingSpecialActionArgs(
            new[] { true, true, true, true, true, true }));
    }

    [Fact]
    public void ShouldPushPrecacheSound_SkipsNoSound()
    {
        Assert.False(VmMapInfoMapOptionsTail1057Slice.ShouldPushPrecacheSound(
            VmMapInfoMapOptionsTail1057Slice.NoSound));
    }

    [Fact]
    public void CountCommaSeparatedEntries_CountsCommas()
    {
        Assert.Equal(3, VmMapInfoMapOptionsTail1057Slice.CountCommaSeparatedEntries(
            new[] { true, true, false }));
    }

    [Fact]
    public void ClassifyTailOptionKeyword_RecognizesPrecacheClasses()
    {
        Assert.Equal(
            VmMapInfoMapOptionsTail1057Slice.MapOptionTailKeyword.PrecacheClasses,
            VmMapInfoMapOptionsTail1057Slice.ClassifyTailOptionKeyword("PrecacheClasses"));
    }

    [Fact]
    public void BeginRedirectOption_StoresTypeToken()
    {
        var draft = new VmMapInfoMapOptionsTail1057Slice.MapOptionsTailDraft();
        Assert.True(VmMapInfoMapOptionsTail1057Slice.BeginRedirectOption(draft, "KeyCard"));
        Assert.Equal("KeyCard", draft.RedirectTypeToken);
    }
}
