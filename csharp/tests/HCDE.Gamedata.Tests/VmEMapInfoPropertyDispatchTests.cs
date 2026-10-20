namespace HCDE.Gamedata.Tests;

public class VmEMapInfoPropertyDispatchTests
{
    [Fact]
    public void ShouldTryShortLumpName_RejectsLongNames()
    {
        Assert.False(VmEMapInfoPropertyDispatch.ShouldTryShortLumpName("longlumpname"));
    }

    [Fact]
    public void TryApplyLevelName_SetsTitle()
    {
        var draft = new VmEMapInfoLevelDraft();
        Assert.True(VmEMapInfoPropertyDispatch.TryApplyLevelName("levelname", "My Map", draft));
        Assert.Equal("My Map", draft.LevelName);
    }

    [Fact]
    public void TryApplyGravity_ZeroIsMax()
    {
        var draft = new VmEMapInfoLevelDraft();
        var stats = new VmEMapInfoStats();
        Assert.True(VmEMapInfoPropertyDispatch.TryApplyGravity(stats, "MAP01", "gravity", "0", 1, draft));
        Assert.Equal(double.MaxValue, draft.Gravity);
    }

    [Fact]
    public void TryApplyDisableJump_SetsFlag()
    {
        var stats = new VmEMapInfoStats();
        var flags = 0;
        Assert.True(VmEMapInfoPropertyDispatch.TryApplyDisableJump(stats, "MAP01", "disable-jump", "true", 1, ref flags));
        Assert.NotEqual(0, flags);
    }

    [Fact]
    public void TryApplyMusic_PrefixesDoomTrack()
    {
        var draft = new VmEMapInfoLevelDraft();
        Assert.True(VmEMapInfoPropertyDispatch.TryApplyMusic(draft, "music", "intro", VmEMapInfoGameType.Doom));
        Assert.Equal("D_INTRO", draft.Music);
    }

    [Fact]
    public void TryApplySkyDelta_ConvertsTics()
    {
        var draft = new VmEMapInfoLevelDraft();
        var stats = new VmEMapInfoStats();
        Assert.True(VmEMapInfoPropertyDispatch.TryApplySkyDelta(stats, "MAP01", "skydelta", "1000", 1, draft));
        Assert.Equal(VmEMapInfoPropertyTail.SkySpeedFromDelta(1000), draft.SkySpeed1);
    }
}
