using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFile7z1366SliceTests
{
    [Fact]
    public void StreamReadSeekAndArchive()
    {
        var host = Ready();
        var archive = new VmFile7z1366Slice.Archive(host, "reader");
        Assert.Equal(
            new[]
            {
                "CrcGenerateTable", "Seek", "LookToRead2_CreateVTable",
                "LookToRead2_INIT", "SzArEx_Init",
            },
            host.Trace);
        Assert.Equal(0u, host.CrcTable1);
        host.Trace.Clear();
        var stillEmpty = new VmFile7z1366Slice.Archive(host, "reader");
        Assert.Contains("CrcGenerateTable", host.Trace);
        Assert.Equal(0, stillEmpty.LookPos);
        host.Trace.Clear();
        host.CrcGenerateTable = () => host.CrcTable1 = 1;
        _ = new VmFile7z1366Slice.Archive(host, "reader");
        Assert.Equal(1u, host.CrcTable1);
        host.Trace.Clear();
        Assert.Same("reader", archive.Reader);
        Assert.Equal(16384, archive.StreamBuffer.Length);
        Assert.Same(archive.StreamBuffer, archive.LookBuf);
        Assert.Equal(0, archive.LookPos);
        Assert.Equal(0, archive.LookSize);
        Assert.True(archive.RealStreamAttached);
        Assert.Equal(uint.MaxValue, archive.BlockIndex);
        Assert.Null(archive.OutBuffer);
        Assert.Equal(0u, archive.OutBufferSize);
        Assert.Equal(1u, host.CrcTable1);

        host.Trace.Clear();
        var again = new VmFile7z1366Slice.Archive(host, "reader");
        Assert.DoesNotContain("CrcGenerateTable", host.Trace);
        Assert.Equal(0, again.LookPos);

        var missingCrc = Ready();
        missingCrc.CrcGenerateTable = null;
        Assert.Equal(
            "CrcGenerateTable",
            Assert.Throws<InvalidOperationException>(
                () => new VmFile7z1366Slice.Archive(missingCrc, "reader")).Message);
        Assert.Empty(missingCrc.Trace);

        byte[]? seen = null;
        long seenRequest = -1;
        host.FileRead = (buffer, request) =>
        {
            seen = buffer;
            seenRequest = request;
            return 4;
        };
        var dest = new byte[8];
        long size = 9;
        Assert.Equal(0, archive.Read(dest, ref size));
        Assert.Equal(4, size);
        Assert.Same(dest, seen);
        Assert.Equal(9, seenRequest);
        host.FileRead = (_, _) => 0;
        size = 3;
        Assert.Equal(0, archive.Read(dest, ref size));
        Assert.Equal(0, size);
        host.FileRead = (_, _) => -5;
        size = 3;
        Assert.Equal(8, archive.Read(dest, ref size));
        Assert.Equal(0, size);
        host.FileRead = null;
        Assert.Equal(
            "Read",
            Assert.Throws<InvalidOperationException>(() => archive.Read(dest, ref size)).Message);

        long pos = 40;
        var bad = archive.Seek(ref pos, 3);
        Assert.Equal(1, bad);
        Assert.Equal(VmFile7z1366Slice.SzErrorData, bad);
        Assert.Equal(40, pos);
        int? origin = null;
        long? at = null;
        host.FileSeek = (offset, mode) =>
        {
            at = offset;
            origin = mode;
            return 0x1_0000_0005;
        };
        host.FileTell = () => 99;
        pos = 40;
        Assert.Equal(5, archive.Seek(ref pos, 0));
        Assert.Equal(40, at);
        Assert.Equal(0, origin);
        Assert.Equal(99, pos);
        host.FileSeek = (_, mode) => mode;
        pos = 1;
        Assert.Equal(1, archive.Seek(ref pos, 1));
        Assert.Equal(2, archive.Seek(ref pos, 2));
        host.FileSeek = (_, _) => -3;
        Assert.Equal(-3, archive.Seek(ref pos, 0));
        host.FileTell = null;
        Assert.Equal(
            "Tell",
            Assert.Throws<InvalidOperationException>(() => archive.Seek(ref pos, 0)).Message);

        var packed = new byte[] { 9, 8, 7, 6, 5 };
        archive.OutBuffer = packed;
        host.SzArExExtract = index =>
        {
            Assert.Equal(2u, index);
            return new VmFile7z1366Slice.ExtractOutcome { Res = 0, Offset = 1, Size = 3 };
        };
        var outBytes = new byte[] { 1, 1, 1, 1 };
        Assert.Equal(0, archive.Extract(2, outBytes));
        Assert.Equal(new byte[] { 8, 7, 6, 1 }, outBytes);
        host.SzArExExtract = _ => new VmFile7z1366Slice.ExtractOutcome { Res = 4, Offset = 0, Size = 1 };
        var kept = new byte[] { 3, 3 };
        Assert.Equal(4, archive.Extract(2, kept));
        Assert.Equal(new byte[] { 3, 3 }, kept);
        host.SzArExExtract = _ => new VmFile7z1366Slice.ExtractOutcome { Res = 0, Offset = 0, Size = 1 };
        archive.OutBuffer = null;
        Assert.Equal(
            "OutBuffer",
            Assert.Throws<InvalidOperationException>(() => archive.Extract(2, kept)).Message);
        Assert.Equal(new byte[] { 3, 3 }, kept);
        archive.OutBuffer = packed;
        host.SzArExExtract = _ => new VmFile7z1366Slice.ExtractOutcome { Res = 0, Offset = 4, Size = 2 };
        Assert.Equal(
            "OutBuffer",
            Assert.Throws<IndexOutOfRangeException>(() => archive.Extract(2, kept)).Message);
        host.SzArExExtract = null;
        Assert.Equal(
            "SzArEx_Extract",
            Assert.Throws<InvalidOperationException>(() => archive.Extract(2, kept)).Message);

        byte[]? freed = null;
        host.AllocFree = buffer => freed = buffer;
        archive.OutBuffer = packed;
        archive.Destroy();
        Assert.Same(packed, freed);
        Assert.Equal(1, archive.FreeOut);
        Assert.Equal(1, archive.FreeDb);
        Assert.Equal(
            new[] { "IAlloc_Free", "SzArEx_Free" },
            host.Trace.TakeLast(2).ToArray());
        var bare = new VmFile7z1366Slice.Archive(Ready(), "reader");
        bare.Destroy();
        Assert.Equal(0, bare.FreeOut);
        Assert.Equal(1, bare.FreeDb);
        var explodeHost = Ready();
        var exploding = new VmFile7z1366Slice.Archive(explodeHost, "reader");
        exploding.OutBuffer = new byte[] { 1 };
        explodeHost.AllocFree = _ => throw new InvalidOperationException("IAlloc_Free");
        Assert.Equal(
            "IAlloc_Free",
            Assert.Throws<InvalidOperationException>(() => exploding.Destroy()).Message);
        Assert.Equal(0, exploding.FreeDb);
    }

    [Fact]
    public void OpenThroughFileCount()
    {
        var host = Ready();
        host.Construct("pack.7z", "reader", "pool");
        host.SzArExOpen = () =>
        {
            Assert.NotNull(host.Opened);
            host.Opened.NumFiles = 4;
            return 0;
        };
        uint allocated = 99;
        host.AllocateEntries = count => allocated = count;
        host.Open();
        Assert.Equal("pack.7z", host.FileName);
        Assert.Same("reader", host.Reader);
        Assert.Same("pool", host.Pool);
        Assert.False(host.Returned);
        Assert.Equal(0, host.Skipped);
        Assert.Equal(4u, allocated);
        Assert.Equal(4u, host.NumLumps);
        Assert.Equal(0u, host.J);
        Assert.NotNull(host.Opened);
        Assert.Equal(0, host.Opened.FreeDb);
        Assert.Empty(host.Messages);
        Assert.Contains("SzArEx_Open", host.Trace);
        Assert.Contains("AllocateEntries", host.Trace);
        Assert.DoesNotContain("Printf", host.Trace);

        var empty = Ready();
        empty.Construct("empty.7z", "reader", "pool");
        empty.SzArExOpen = () => 0;
        uint zeroAllocated = 7;
        empty.AllocateEntries = count => zeroAllocated = count;
        empty.Open();
        Assert.Equal(0u, zeroAllocated);
        Assert.Equal(0u, empty.NumLumps);
        Assert.Equal(0u, empty.J);

        var unsupported = Fail(4);
        Assert.True(unsupported.Returned);
        Assert.False(unsupported.Result);
        Assert.Null(unsupported.Opened);
        Assert.Null(unsupported.NumLumps);
        Assert.Null(unsupported.J);
        Assert.Equal(0, unsupported.Skipped);
        Assert.Equal(
            (1, "pack.7z: Decoder does not support this archive\n"),
            Assert.Single(unsupported.Messages));

        var memory = Fail(2);
        Assert.Equal((1, "Cannot allocate memory\n"), Assert.Single(memory.Messages));
        var crc = Fail(3);
        Assert.Equal((1, "CRC error\n"), Assert.Single(crc.Messages));
        var other = Fail(17);
        Assert.Equal((1, "error #17\n"), Assert.Single(other.Messages));
        var negative = Fail(-1);
        Assert.Equal((1, "error #-1\n"), Assert.Single(negative.Messages));
        var data = Fail(1);
        Assert.Equal((1, "error #1\n"), Assert.Single(data.Messages));

        var nameless = Ready();
        nameless.Construct(null, "reader", "pool");
        nameless.SzArExOpen = () => 4;
        var name = Assert.Throws<InvalidOperationException>(() => nameless.Open());
        Assert.Equal("FileName", name.Message);
        Assert.Null(nameless.Opened);
        Assert.Empty(nameless.Messages);
        Assert.Equal(1, nameless.Trace.Count(step => step == "SzArEx_Free"));

        var quiet = Ready();
        quiet.Construct("pack.7z", "reader", "pool");
        quiet.SzArExOpen = () => 2;
        quiet.Printf = null;
        Assert.Equal("Printf", Assert.Throws<InvalidOperationException>(() => quiet.Open()).Message);
        Assert.Null(quiet.Opened);
        Assert.Empty(quiet.Messages);

        var noEntries = Ready();
        noEntries.Construct("pack.7z", "reader", "pool");
        noEntries.SzArExOpen = () =>
        {
            noEntries.Opened!.NumFiles = 3;
            return 0;
        };
        noEntries.AllocateEntries = null;
        Assert.Equal(
            "AllocateEntries",
            Assert.Throws<InvalidOperationException>(() => noEntries.Open()).Message);
        Assert.NotNull(noEntries.Opened);
        Assert.Equal(0, noEntries.Opened.FreeDb);
        Assert.Null(noEntries.J);

        var first = Ready();
        first.Construct("pack.7z", "reader", "pool");
        first.SzArExOpen = () => 0;
        first.Open();
        var kept = first.Opened;
        Assert.NotNull(kept);
        first.SzArExOpen = () => 4;
        first.Open();
        Assert.Equal(0, kept.FreeDb);
        Assert.Null(first.Opened);
        Assert.True(first.Returned);
    }

    private static VmFile7z1366Slice Fail(int code)
    {
        var host = Ready();
        host.Construct("pack.7z", "reader", "pool");
        host.SzArExOpen = () => code;
        host.Open();
        return host;
    }

    private static VmFile7z1366Slice Ready()
    {
        return new VmFile7z1366Slice
        {
            CrcGenerateTable = () => { },
            FileSeek = (_, _) => 0,
            FileTell = () => 0,
            CreateLookVTable = _ => { },
            SzArExInit = () => { },
            SzArExOpen = () => 0,
            SzArExFree = () => { },
            AllocFree = _ => { },
            AllocateEntries = _ => { },
            Printf = (_, _) => { },
        };
    }
}
