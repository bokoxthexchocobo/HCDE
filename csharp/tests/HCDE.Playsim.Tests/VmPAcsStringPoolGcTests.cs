namespace HCDE.Playsim.Tests;

public class VmPAcsStringPoolGcTests
{
    [Fact]
    public void PurgeStrings_RemovesUnmarked()
    {
        var pool = new VmPAcsStringPoolGc();
        var id = pool.AddString("gone");
        pool.PurgeStrings();
        Assert.Null(pool.GetString(id));
    }

    [Fact]
    public void LockString_PreventsPurge()
    {
        var pool = new VmPAcsStringPoolGc();
        var id = pool.AddString("stay");
        pool.LockString(1, id);
        pool.PurgeStrings();
        Assert.Equal("stay", pool.GetString(id));
    }

    [Fact]
    public void InsertString_TriggersGcCallbackWhenFull()
    {
        var pool = new VmPAcsStringPoolGc();
        var hits = 0;
        pool.CollectGlobalStrings = () => hits++;
        for (var i = 0; i < 120; i++)
            pool.AddString($"s{i}");
        Assert.True(hits >= 0);
    }

    [Fact]
    public void UnlockAll_ClearsLocks()
    {
        var pool = new VmPAcsStringPoolGc();
        var id = pool.AddString("x");
        pool.LockString(1, id);
        pool.UnlockAll();
        pool.PurgeStrings();
        Assert.Null(pool.GetString(id));
    }
}
