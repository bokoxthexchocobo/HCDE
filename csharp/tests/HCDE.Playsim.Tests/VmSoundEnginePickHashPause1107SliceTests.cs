namespace HCDE.Playsim.Tests;

public class VmSoundEnginePickHashPause1107SliceTests
{
    [Fact]
    public void RunPickReplacement_FollowsRandomList()
    {
        var ctx = new VmSoundEnginePickHashPause1107Slice.PickReplacementContext
        {
            RefId = 1,
            IsRandomHeader = id => id == 1,
            RandomListIndex = _ => 0,
            ChoiceCount = _ => 2,
            ChoiceAt = (_, pick) => pick == 0 ? 9 : 10,
            RandomIndex = _ => 0
        };
        Assert.Equal(9, VmSoundEnginePickHashPause1107Slice.RunPickReplacement(ctx));
    }

    [Fact]
    public void RunHashSounds_InsertsResourceWhenPresent()
    {
        var resources = new List<int>();
        VmSoundEnginePickHashPause1107Slice.RunHashSounds(
            new VmSoundEnginePickHashPause1107Slice.HashSoundsContext
            {
                SfxCount = 3,
                ReadName = i => $"s{i}",
                ReadResourceId = i => i == 2 ? 42 : VmSoundEnginePickHashPause1107Slice.NoSound,
                InsertName = (_, _) => { },
                InsertResource = (_, resid) => resources.Add(resid)
            });
        Assert.Equal(new[] { 42 }, resources);
    }

    [Fact]
    public void RunCacheSoundCommand_SkipsUnknownNames()
    {
        var cached = new List<int>();
        VmSoundEnginePickHashPause1107Slice.RunCacheSoundCommand(
            new VmSoundEnginePickHashPause1107Slice.CacheSoundCommandContext
            {
                ArgCount = 2,
                ReadArg = _ => "missing",
                FindSound = _ => VmSoundEnginePickHashPause1107Slice.NoSound,
                CacheSound = cached.Add
            });
        Assert.Empty(cached);
    }

    [Fact]
    public void RunPauseSound_RespectsNotSfxFlag()
    {
        var paused = false;
        VmSoundEnginePickHashPause1107Slice.RunPauseSound(
            notMusic: true,
            notSfx: true,
            pauseMusic: () => Assert.Fail("music"),
            setPaused: () => paused = true,
            setSfxPaused: () => { });
        Assert.False(paused);
    }

    [Fact]
    public void RunResumeSound_InvokesMusicAlways()
    {
        var resumed = false;
        VmSoundEnginePickHashPause1107Slice.RunResumeSound(
            notSfx: true,
            resumeMusic: () => resumed = true,
            setPaused: () => Assert.Fail("sfx"),
            setSfxPaused: () => { });
        Assert.True(resumed);
    }
}
