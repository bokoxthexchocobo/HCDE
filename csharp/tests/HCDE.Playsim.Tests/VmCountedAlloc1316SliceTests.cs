namespace HCDE.Playsim.Tests;

public class VmCountedAlloc1316SliceTests
{
    [Fact]
    public void Release_ReportsMsizeAndLeavesTheBlockOnFailure()
    {
        var heap = new VmCountedAlloc1316(prefixed: false, sunMsize: false);
        heap.QuerySize = id => (ulong)(id * 10);
        heap.FailuresLeft = 1;
        InvalidOperationException missed = Assert.Throws<InvalidOperationException>(() => heap.Malloc(12));
        Assert.Equal("Could not malloc 12 bytes", missed.Message);
        Assert.Equal(new ulong[] { 12 }, heap.Attempts);
        Assert.Empty(heap.Reports);

        int raw = heap.Malloc(12);
        Assert.Equal(1, raw);
        Assert.Equal(new ulong[] { 12, 12 }, heap.Attempts);
        Assert.Equal(new CountReport("alloc", 10, 0), heap.Reports[0]);
        Assert.Equal(Enumerable.Repeat((byte)0xA5, 12).ToArray(), heap.Bytes(raw));

        int viaNull = heap.Realloc(0, 8);
        Assert.Equal(2, viaNull);
        Assert.Equal(new int[] { 0 }, heap.ReallocTargets);
        Assert.Equal(new CountReport("realloc", 0, 20), heap.Reports[1]);
        Assert.Equal(8ul, heap.Attempts[2]);

        int grown = heap.Realloc(raw, 9);
        Assert.Equal(3, grown);
        Assert.False(heap.Contains(raw));
        Assert.Equal(new int[] { 0, raw }, heap.ReallocTargets);
        Assert.Equal(new CountReport("realloc", 10, 30), heap.Reports[2]);

        heap.FailuresLeft = 1;
        int reports = heap.Reports.Count;
        InvalidOperationException failed = Assert.Throws<InvalidOperationException>(() => heap.Realloc(grown, 3));
        Assert.Equal("Could not realloc 3 bytes", failed.Message);
        Assert.True(heap.Contains(grown));
        Assert.Equal(reports, heap.Reports.Count);
        Assert.Equal(new int[] { 0, raw, grown }, heap.ReallocTargets);

        heap.Free(0);
        Assert.Equal(reports, heap.Reports.Count);
        heap.Free(grown);
        Assert.Equal(new CountReport("dealloc", 30, 0), heap.Reports[reports]);
        Assert.Equal(new int[] { grown }, heap.Freed);
        Assert.False(heap.Contains(grown));

        var fresh = new VmCountedAlloc1316(prefixed: false, sunMsize: false);
        int block = fresh.Calloc(3, 5);
        Assert.Equal(new byte[15], fresh.Bytes(block));
        Assert.Equal(15ul, fresh.LastCleared);
        Assert.Equal(15ul, fresh.Attempts[0]);
        Assert.Equal(new CountReport("alloc", 15, 0), fresh.Reports[0]);
        int wrapped = fresh.Calloc(1ul << 63, 4);
        Assert.Equal(0ul, fresh.Attempts[1]);
        Assert.Equal(0ul, fresh.LastCleared);
        Assert.Empty(fresh.Bytes(wrapped));
    }

    [Fact]
    public void Debug_StoresThePrefixAndNamesTheFile()
    {
        var heap = new VmCountedAlloc1316(prefixed: true, sunMsize: true);
        heap.FailuresLeft = 1;
        InvalidOperationException missed = Assert.Throws<InvalidOperationException>(
            () => heap.MallocDbg(7, "heap.cpp", 19));
        Assert.Equal("Could not malloc 7 bytes in heap.cpp, line 19", missed.Message);
        Assert.Equal(new ulong[] { 15 }, heap.Attempts);
        Assert.Empty(heap.Reports);

        int block = heap.MallocDbg(16, "heap.cpp", 19);
        Assert.Equal(24ul, heap.Attempts[1]);
        Assert.Equal(16ul, heap.Stored(block));
        Assert.NotEqual(block, heap.Base(block));
        Assert.Equal(Enumerable.Repeat((byte)0xA5, 16).ToArray(), heap.Bytes(block));
        Assert.Equal(new CountReport("alloc", 0, 0), heap.Reports[0]);

        heap.FailuresLeft = 1;
        InvalidOperationException nullMiss = Assert.Throws<InvalidOperationException>(
            () => heap.ReallocDbg(0, 4, "heap.cpp", 8));
        Assert.Equal("Could not malloc 4 bytes in heap.cpp, line 8", nullMiss.Message);
        Assert.Empty(heap.ReallocTargets);
        Assert.Equal(12ul, heap.Attempts[2]);

        int born = heap.ReallocDbg(0, 4, "heap.cpp", 8);
        Assert.Equal(4ul, heap.Stored(born));
        Assert.Equal(new CountReport("alloc", 0, 0), heap.Reports[1]);
        Assert.Empty(heap.ReallocTargets);

        int oldBase = heap.Base(block);
        int moved = heap.ReallocDbg(block, 30, "heap.cpp", 4);
        Assert.Equal(30ul, heap.Stored(moved));
        Assert.Equal(38ul, heap.Attempts[^1]);
        Assert.False(heap.Contains(block));
        Assert.Equal(new int[] { oldBase }, heap.ReallocTargets);
        Assert.Equal(new CountReport("realloc", 0, 0), heap.Reports[2]);
        Assert.DoesNotContain(oldBase, heap.Freed);

        heap.FailuresLeft = 1;
        int reports = heap.Reports.Count;
        InvalidOperationException failed = Assert.Throws<InvalidOperationException>(
            () => heap.ReallocDbg(moved, 3, "heap.cpp", 21));
        Assert.Equal("Could not realloc 3 bytes in heap.cpp, line 21", failed.Message);
        Assert.True(heap.Contains(moved));
        Assert.Equal(reports, heap.Reports.Count);
        Assert.Equal(new int[] { oldBase, heap.Base(moved) }, heap.ReallocTargets);

        int basis = heap.Base(moved);
        heap.Free(0);
        Assert.Equal(reports, heap.Reports.Count);
        heap.Free(moved);
        Assert.Equal(new CountReport("dealloc", 0, 0), heap.Reports[reports]);
        Assert.Equal(new int[] { basis }, heap.Freed);
        Assert.NotEqual(moved, basis);

        var bsd = new VmCountedAlloc1316(prefixed: true, sunMsize: false);
        bsd.QuerySize = _ => 100;
        int user = bsd.Malloc(16);
        int userBase = bsd.Base(user);
        Assert.Equal(24ul, bsd.Attempts[0]);
        Assert.Equal(16ul, bsd.Stored(user));
        Assert.Equal(new CountReport("alloc", 100, 0), bsd.Reports[0]);
        bsd.Free(user);
        Assert.Equal(new int[] { userBase }, bsd.Freed);
        Assert.Equal(new CountReport("dealloc", 100, 0), bsd.Reports[1]);

        var release = new VmCountedAlloc1316(prefixed: true, sunMsize: false);
        release.FailuresLeft = 1;
        InvalidOperationException plain = Assert.Throws<InvalidOperationException>(() => release.Malloc(7));
        Assert.Equal("Could not malloc 7 bytes", plain.Message);
        int zeroed = release.Calloc(2, 4);
        Assert.Equal(new ulong[] { 15, 16 }, release.Attempts);
        Assert.Equal(8ul, release.Stored(zeroed));
        Assert.Equal(8ul, release.LastCleared);
        Assert.Equal(new byte[8], release.Bytes(zeroed));
        Assert.Equal(new CountReport("alloc", 8, 0), release.Reports[0]);

        var normal = new VmCountedAlloc1316(prefixed: false, sunMsize: false);
        normal.FailuresLeft = 1;
        InvalidOperationException reallocNull = Assert.Throws<InvalidOperationException>(
            () => normal.ReallocDbg(0, 4, "heap.cpp", 8));
        Assert.Equal("Could not realloc 4 bytes in heap.cpp, line 8", reallocNull.Message);
        Assert.Equal(new ulong[] { 4 }, normal.Attempts);
        Assert.Equal(new int[] { 0 }, normal.ReallocTargets);
    }
}
