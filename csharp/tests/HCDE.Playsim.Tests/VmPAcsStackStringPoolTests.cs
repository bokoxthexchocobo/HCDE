namespace HCDE.Playsim.Tests;

public class VmPAcsStackStringPoolTests
{
    [Fact]
    public void AcsStack_PushesHeadLinkedList()
    {
        var a = new VmPAcsStackStringPool.AcsStack();
        var b = new VmPAcsStackStringPool.AcsStack();
        Assert.Same(b, VmPAcsStackStringPool.AcsStack.Head);
        b.Unlink();
        Assert.Same(a, VmPAcsStackStringPool.AcsStack.Head);
        a.Unlink();
        Assert.Null(VmPAcsStackStringPool.AcsStack.Head);
    }

    [Fact]
    public void AddString_Deduplicates()
    {
        var pool = new VmPAcsStackStringPool();
        var x = pool.AddString("ammo");
        var y = pool.AddString("ammo");
        Assert.Equal(x, y);
    }

    [Fact]
    public void LockStringArray_LocksValidPoolIds()
    {
        var pool = new VmPAcsStackStringPool();
        var id = pool.AddString("keep");
        pool.LockStringArray(3, [id, 0]);
        Assert.Equal("keep", pool.GetString(id));
    }

    [Fact]
    public void MarkStringMap_MarksValues()
    {
        var pool = new VmPAcsStackStringPool();
        var id = pool.AddString("map");
        pool.MarkStringMap(new Dictionary<int, int> { [0] = id });
        Assert.Equal("map", pool.GetString(id));
    }
}
