namespace HCDE.Playsim.Tests;

public class VmPAcsStringArchive1023SliceTests
{
    [Fact]
    public void ShouldListPooledString_SkipsFreeEntries()
    {
        Assert.False(VmPAcsStringArchive1023Slice.ShouldListPooledString(VmAcsStringPool.FreeEntry));
        Assert.True(VmPAcsStringArchive1023Slice.ShouldListPooledString(1));
    }

    [Fact]
    public void FormatDumpLine_MatchesNativePrintfShape()
    {
        Assert.Equal("   3. ( 2) \"foo\"", VmPAcsStringArchive1023Slice.FormatDumpLine(3, 2, "foo"));
    }

    [Fact]
    public void UnlockLevelLocksNative_RemovesMatchingLock()
    {
        var removed = new List<(int, int)>();
        var pool = new[]
        {
            new VmAcsStringPoolUnlockForLevel.PoolEntry(VmAcsStringPool.FreeEntry, []),
            new VmAcsStringPoolUnlockForLevel.PoolEntry(1, [4, 9]),
        };
        VmPAcsStringArchive1023Slice.UnlockLevelLocksNative(pool, 9, (i, ndx) => removed.Add((i, ndx)));
        Assert.Equal([(1, 1)], removed);
    }

    [Fact]
    public void FormatScriptPresentation_QuotesNamedScripts()
    {
        var named = VmPAcsStringArchive1023Slice.FormatScriptPresentation(-42, _ => "intro");
        Assert.Equal("script \"intro\"", named);
        Assert.Equal("script 5", VmPAcsStringArchive1023Slice.FormatScriptPresentation(5, _ => null));
    }

    [Fact]
    public void CollectGlobalStringsNative_MarksStacksThenPurges()
    {
        var marks = new List<int>();
        VmPAcsStringArchive1023Slice.CollectGlobalStringsNative(
            [0, 2, -5000],
            (_, sp) => IntPtr.Zero,
            (_, sp) => marks.Add(sp),
            () => marks.Add(-1),
            () => marks.Add(-2),
            () => marks.Add(-3));
        Assert.Equal([2, -1, -2, -3], marks);
        Assert.Throws<InvalidOperationException>(() =>
            VmPAcsStringArchive1023Slice.CollectGlobalStringsNative(
                [-5000], (_, _) => IntPtr.Zero, (_, _) => { }, () => { }, () => { }, () => { }));
    }

    [Fact]
    public void ClearAcsVarsNative_BranchesLikeNative()
    {
        var log = new List<string>();
        VmPAcsStringArchive1023Slice.ClearAcsVarsNative(
            alsoGlobal: true,
            () => log.Add("world"),
            () => log.Add("global"),
            () => log.Add("purgeAll"),
            () => log.Add("mark"),
            () => log.Add("purgeRef"));
        Assert.Equal(["world", "global", "purgeAll"], log);

        log.Clear();
        VmPAcsStringArchive1023Slice.ClearAcsVarsNative(
            false,
            () => log.Add("world"),
            () => log.Add("global"),
            () => log.Add("purgeAll"),
            () => log.Add("mark"),
            () => log.Add("purgeRef"));
        Assert.Equal(["world", "mark", "purgeRef"], log);
    }
}
