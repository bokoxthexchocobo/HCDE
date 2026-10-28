namespace HCDE.Playsim.Tests;

public class VmAcsStringPoolTests
{
    [Fact]
    public void AddString_DeduplicatesIdentical()
    {
        var pool = new VmAcsStringPool();
        var a = pool.AddString("clip");
        var b = pool.AddString("clip");
        Assert.Equal(a, b);
        Assert.Equal("clip", pool.GetString(a));
    }

    [Fact]
    public void PurgeStrings_RemovesUnlockedUnmarked()
    {
        var pool = new VmAcsStringPool();
        var id = pool.AddString("temp");
        pool.PurgeStrings();
        Assert.Null(pool.GetString(id));
    }

    [Fact]
    public void LockString_PreventsPurge()
    {
        var pool = new VmAcsStringPool();
        var id = pool.AddString("keep");
        pool.LockString(1, id);
        pool.PurgeStrings();
        Assert.Equal("keep", pool.GetString(id));
    }

    [Fact]
    public void MarkString_PreventsPurgeUntilCleared()
    {
        var pool = new VmAcsStringPool();
        var id = pool.AddString("marked");
        pool.MarkString(id);
        pool.PurgeStrings();
        Assert.Equal("marked", pool.GetString(id));
    }

    [Fact]
    public void LockStringArray_LocksPoolIdsOnly()
    {
        var pool = new VmAcsStringPool();
        var id = pool.AddString("arr");
        pool.LockStringArray(2, [id, 0, id | 1]);
        pool.PurgeStrings();
        Assert.Equal("arr", pool.GetString(id));
    }
}
