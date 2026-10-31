using System.Buffers.Binary;
using System.IO.Compression;

namespace HCDE.Playsim.Tests;

public class VmPngSave1247SliceTests
{
    const string HeadGolden =
"""
init level=5 err=0
pack pal n=3 000102
deflate y=1 flush=0 err=0 in=0 out=32765
pack pal n=3 000304
deflate y=0 flush=4 err=1 in=0 out=32762
end
idat len=6 ok=1
init level=5 err=0
pack rgb n=4 00090807
deflate y=0 flush=4 err=1 in=0 out=32764
end
idat len=4 ok=1
init level=5 err=0
pack bgra n=7 00030201070605
deflate y=0 flush=4 err=1 in=0 out=32761
end
idat len=7 ok=1

""";

    const string TailGolden =
"""
---- edge
file null
init level=5 err=0
pack pal n=2 0001
deflate y=0 flush=4 err=0 in=0 out=0
idat len=4 ok=1
deflate tail flush=4 err=0 in=0 out=0
idat len=4 ok=1
deflate tail flush=4 err=1 in=0 out=0
end
idat len=4 ok=1
init level=5 err=0
pack pal n=2 0001
deflate y=0 flush=4 err=0 in=0 out=0
idat len=4 ok=0
idat abort
init level=5 err=0
pack pal n=3 000102
deflate y=0 flush=4 err=-3 in=3 out=32768
end
stream bad err=-3
init level=99 err=-2
init level=5 err=0
from null
init level=5 err=0
pack skip type=9
deflate y=0 flush=4 err=1 in=0 out=32768
end
idat len=0 ok=1
init level=5 err=0
deflate tail flush=4 err=1 in=0 out=32768
end
idat len=0 ok=1
init level=5 err=0
pack pal n=3 000102
deflate y=0 flush=4 err=1 in=0 out=32765
end
idat len=3 ok=0

""";

    [Fact]
    public void Save_PacksPaletteRgbAndBgraRows()
    {
        string dump = Build(out byte[] pal, out byte[] rgb, out byte[] bgra, out byte[] hostPal);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("pack pal n=3 000102\n", head, StringComparison.Ordinal);
        Assert.Contains("pack pal n=3 000304\n", head, StringComparison.Ordinal);
        Assert.Contains("pack rgb n=4 00090807\n", head, StringComparison.Ordinal);
        Assert.Contains("pack bgra n=7 00030201070605\n", head, StringComparison.Ordinal);
        Assert.Contains("deflate y=1 flush=0 err=0 in=0 out=32765\n", head, StringComparison.Ordinal);
        Assert.Contains("deflate y=0 flush=4 err=1 in=0 out=32762\n", head, StringComparison.Ordinal);
        Assert.Equal(Convert.FromHexString("00000006494441540001020003048bdf6f21"), pal);
        Assert.Equal(Convert.FromHexString("000000044944415400090807b34fc323"), rgb);
        Assert.Equal(Convert.FromHexString("0000000749444154000302010706052ec1d803"), bgra);
        Assert.Equal(new byte[] { 0, 1, 2, 0, 3, 4 }, Inflate(Payloads(hostPal)));
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Save_FlushesFullBuffersAndRejectsABadStream()
    {
        string dump = Build(out _, out _, out _, out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("idat len=4 ok=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("idat abort\n", tail, StringComparison.Ordinal);
        Assert.Contains("stream bad err=-3\n", tail, StringComparison.Ordinal);
        Assert.Contains("init level=99 err=-2\n", tail, StringComparison.Ordinal);
        Assert.Contains("from null\n", tail, StringComparison.Ordinal);
        Assert.Contains("file null\n", tail, StringComparison.Ordinal);
        Assert.Contains("pack skip type=9\n", tail, StringComparison.Ordinal);
        Assert.Contains("idat len=0 ok=1\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out byte[] pal, out byte[] rgb, out byte[] bgra, out byte[] hostPal)
    {
        var copy = new CopyDeflate();
        var vm = new VmPngSave1247 { Engine = () => copy };
        var palFile = new VmPngSave1247.OutFile();
        Assert.True(vm.Save(palFile, new byte[] { 1, 2, 0xFF, 0xFF, 3, 4, 0xFF, 0xFF }, VmPngSave1247.Pal, 2, 2, 4));
        pal = palFile.Bytes.ToArray();

        var rgbFile = new VmPngSave1247.OutFile();
        Assert.True(vm.Save(rgbFile, new byte[] { 9, 8, 7 }, VmPngSave1247.Rgb, 1, 1, 3));
        rgb = rgbFile.Bytes.ToArray();

        var bgraFile = new VmPngSave1247.OutFile();
        Assert.True(vm.Save(bgraFile, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, VmPngSave1247.Bgra, 2, 1, 8));
        bgra = bgraFile.Bytes.ToArray();

        var host = new VmPngSave1247();
        var hostFile = new VmPngSave1247.OutFile();
        Assert.True(host.Save(hostFile, new byte[] { 1, 2, 0xFF, 0xFF, 3, 4, 0xFF, 0xFF }, VmPngSave1247.Pal, 2, 2, 4));
        hostPal = hostFile.Bytes.ToArray();
        var hostRgb = new VmPngSave1247.OutFile();
        Assert.True(host.Save(hostRgb, new byte[] { 10, 20, 30, 40, 50, 60 }, VmPngSave1247.Rgb, 1, 2, 3));
        Assert.Equal(new byte[] { 0, 10, 20, 30, 0, 40, 50, 60 }, Inflate(Payloads(hostRgb.Bytes.ToArray())));
        var hostBgra = new VmPngSave1247.OutFile();
        Assert.True(host.Save(hostBgra, new byte[] { 1, 2, 3, 255, 5, 6, 7, 128 }, VmPngSave1247.Bgra, 2, 1, 8));
        Assert.Equal(new byte[] { 0, 3, 2, 1, 7, 6, 5 }, Inflate(Payloads(hostBgra.Bytes.ToArray())));
        var hostEmpty = new VmPngSave1247.OutFile();
        Assert.True(host.Save(hostEmpty, null, VmPngSave1247.Pal, 1, 0, 1));
        Assert.Empty(Inflate(Payloads(hostEmpty.Bytes.ToArray())));

        vm.Log.Add("---- edge");

        var fill = new FillDeflate();
        var full = new VmPngSave1247 { Engine = () => fill, ChunkBytes = 4 };
        var fullFile = new VmPngSave1247.OutFile();
        Assert.True(full.Save(fullFile, new byte[] { 1 }, VmPngSave1247.Pal, 1, 1, 1));
        Assert.True(fill.Ended);
        Assert.Equal(Convert.FromHexString("000000044944415411111111260b1d58000000044944415411111111260b1d58000000044944415411111111260b1d58"), fullFile.Bytes.ToArray());

        var blocked = new FillDeflate();
        var blockVm = new VmPngSave1247 { Engine = () => blocked, ChunkBytes = 4 };
        var blockFile = new VmPngSave1247.OutFile { Limit = 0 };
        Assert.False(blockVm.Save(blockFile, new byte[] { 1 }, VmPngSave1247.Pal, 1, 1, 1));
        Assert.False(blocked.Ended);
        Assert.Empty(blockFile.Bytes);

        var bad = new FailDeflate();
        var badVm = new VmPngSave1247 { Engine = () => bad };
        var badFile = new VmPngSave1247.OutFile();
        Assert.False(badVm.Save(badFile, new byte[] { 1, 2 }, VmPngSave1247.Pal, 2, 1, 2));
        Assert.True(bad.Ended);
        Assert.Empty(badFile.Bytes);

        var level = new VmPngSave1247 { Level = 99 };
        Assert.False(level.Save(new VmPngSave1247.OutFile(), new byte[] { 1 }, VmPngSave1247.Pal, 1, 1, 1));

        var missing = new CopyDeflate();
        var missVm = new VmPngSave1247 { Engine = () => missing };
        Assert.False(missVm.Save(new VmPngSave1247.OutFile(), null, VmPngSave1247.Rgb, 1, 1, 3));
        Assert.False(missing.Ended);

        Assert.False(vm.Save(null, new byte[] { 1 }, VmPngSave1247.Pal, 1, 1, 1));

        var skip = new CopyDeflate();
        var skipVm = new VmPngSave1247 { Engine = () => skip };
        var skipFile = new VmPngSave1247.OutFile();
        Assert.True(skipVm.Save(skipFile, new byte[] { 9, 9, 9 }, 9, 1, 1, 3));
        Assert.Equal(Convert.FromHexString("000000004944415435af061e"), skipFile.Bytes.ToArray());

        var empty = new CopyDeflate();
        var emptyVm = new VmPngSave1247 { Engine = () => empty };
        var emptyFile = new VmPngSave1247.OutFile();
        Assert.True(emptyVm.Save(emptyFile, null, VmPngSave1247.Pal, 4, 0, 4));
        Assert.Equal(Convert.FromHexString("000000004944415435af061e"), emptyFile.Bytes.ToArray());

        var shortFile = new VmPngSave1247.OutFile { Limit = 8 };
        var shortVm = new VmPngSave1247 { Engine = () => new CopyDeflate() };
        Assert.False(shortVm.Save(shortFile, new byte[] { 1, 2 }, VmPngSave1247.Pal, 2, 1, 2));
        Assert.Equal(8, shortFile.Bytes.Count);
        Assert.Equal(Convert.FromHexString("0000000349444154"), shortFile.Bytes.ToArray());

        vm.Log.AddRange(full.Log);
        vm.Log.AddRange(blockVm.Log);
        vm.Log.AddRange(badVm.Log);
        vm.Log.AddRange(level.Log);
        vm.Log.AddRange(missVm.Log);
        vm.Log.AddRange(skipVm.Log);
        vm.Log.AddRange(emptyVm.Log);
        vm.Log.AddRange(shortVm.Log);

        string dump = string.Join('\n', vm.Log) + "\n";
        return dump;
    }

    static byte[] Payloads(byte[] file)
    {
        var ms = new MemoryStream();
        int i = 0;
        while (i + 8 <= file.Length)
        {
            int len = BinaryPrimitives.ReadInt32BigEndian(file.AsSpan(i));
            ms.Write(file, i + 8, len);
            i += 12 + len;
        }
        return ms.ToArray();
    }

    static byte[] Inflate(byte[] zlib)
    {
        using var src = new MemoryStream(zlib);
        using var z = new ZLibStream(src, CompressionMode.Decompress);
        using var dst = new MemoryStream();
        z.CopyTo(dst);
        return dst.ToArray();
    }

    sealed class CopyDeflate : VmPngSave1247.Deflate
    {
        public bool Ended;

        public override int Init(int level) => VmPngSave1247.ZOk;

        public override int Process(ref VmPngSave1247.DeflateBuf io, int flush)
        {
            int n = Math.Min(io.AvailIn, io.AvailOut);
            if (n > 0 && io.Input != null)
                Buffer.BlockCopy(io.Input, io.InAt, io.Output, io.OutAt, n);
            else
                n = 0;
            io.InAt += n;
            io.AvailIn -= n;
            io.OutAt += n;
            io.AvailOut -= n;
            if (flush == VmPngSave1247.ZFinish && io.AvailIn == 0)
                return VmPngSave1247.ZStreamEnd;
            return VmPngSave1247.ZOk;
        }

        public override void End() => Ended = true;
    }

    sealed class FillDeflate : VmPngSave1247.Deflate
    {
        public bool Ended;
        int _calls;

        public override int Init(int level) => VmPngSave1247.ZOk;

        public override int Process(ref VmPngSave1247.DeflateBuf io, int flush)
        {
            _calls++;
            while (io.AvailOut > 0)
            {
                io.Output[io.OutAt++] = 0x11;
                io.AvailOut--;
            }
            io.AvailIn = 0;
            if (_calls >= 3)
                return VmPngSave1247.ZStreamEnd;
            return VmPngSave1247.ZOk;
        }

        public override void End() => Ended = true;
    }

    sealed class FailDeflate : VmPngSave1247.Deflate
    {
        public bool Ended;

        public override int Init(int level) => VmPngSave1247.ZOk;

        public override int Process(ref VmPngSave1247.DeflateBuf io, int flush) => -3;

        public override void End() => Ended = true;
    }
}
