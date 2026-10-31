using System.Text;

namespace HCDE.Playsim.Tests;

public class VmZipFile1325SliceTests
{
    [Fact]
    public void Directory_SkipsFoldersAndReadsPastTheLocalHeader()
    {
        bool zip64 = false;
        var tailEnd = new byte[30];
        tailEnd[26] = (byte)'P';
        tailEnd[27] = (byte)'K';
        tailEnd[28] = 5;
        tailEnd[29] = 6;
        Assert.Equal(26u, VmZipFile1325.FindCentralDir(new VmZipFile1325.Reader(tailEnd), ref zip64));

        var tail = new byte[70000];
        tail[10] = (byte)'P';
        tail[11] = (byte)'K';
        tail[12] = 5;
        tail[13] = 6;
        int at = 70000 - 22;
        tail[at] = (byte)'P';
        tail[at + 1] = (byte)'K';
        tail[at + 2] = 5;
        tail[at + 3] = 6;
        Assert.Equal((uint)at, VmZipFile1325.FindCentralDir(new VmZipFile1325.Reader(tail), ref zip64));
        Assert.False(zip64);

        int before = VmZipFile1325.DestroyCount;
        Assert.Null(VmZipFile1325.CheckZip("plain.bin", new VmZipFile1325.Reader(new byte[40]), null));
        Assert.Equal(before, VmZipFile1325.DestroyCount);

        byte[] corrupt = LocalOnly();
        Assert.Null(VmZipFile1325.CheckZip("bad.zip", new VmZipFile1325.Reader(corrupt), null));
        Assert.Equal(before + 1, VmZipFile1325.DestroyCount);

        var filter = new object();
        byte[] zip = MakeZip(new[]
        {
            Item("DIR/", Array.Empty<byte>()),
            Item("Maps\\A.TXT", new byte[] { (byte)'a', (byte)'b' }, crc: 0x04030201),
            Item("secret.dat", new byte[] { 1 }, method: 99),
            Item("lock.dat", new byte[] { 2 }, flags: VmZipFile1325.Encrypted),
            Item("boom.dat", new byte[] { 9, 8 }, method: VmZipFile1325.MethodImplode, flags: 6),
            Item("empty.dat", Array.Empty<byte>()),
        });
        var opened = VmZipFile1325.CheckZip("pack.zip", new VmZipFile1325.Reader(zip), filter);
        Assert.NotNull(opened);
        Assert.True(opened!.HashGenerated);
        Assert.True(opened.PostProcessed);
        Assert.Same(filter, opened.PostedFilter);
        Assert.Equal(3, opened.NumLumps);
        Assert.Equal("maps/a.txt", opened.Entries[0].FileName);
        Assert.Equal(2u, opened.Entries[0].Length);
        Assert.Equal(0x04030201u, opened.Entries[0].Crc32);
        Assert.Equal((ushort)(VmZipFile1325.ResFullPath | VmZipFile1325.ResNeedFileStart), opened.Entries[0].Flags);
        Assert.Equal(VmZipFile1325.MethodImplode6, opened.Entries[1].Method);
        Assert.Equal((ushort)(VmZipFile1325.ResFullPath | VmZipFile1325.ResNeedFileStart | VmZipFile1325.ResCompressed), opened.Entries[1].Flags);
        Assert.Equal("empty.dat", opened.Entries[2].FileName);

        VmZipFile1325.Compressed raw = opened.GetRawData(0);
        Assert.Equal(new byte[] { (byte)'a', (byte)'b' }, raw.Buffer);
        Assert.Equal((ushort)VmZipFile1325.ResFullPath, opened.Entries[0].Flags);
        VmZipFile1325.Compressed again = opened.GetRawData(0);
        Assert.Equal(new byte[] { (byte)'a', (byte)'b' }, again.Buffer);

        VmZipFile1325.Compressed empty = opened.GetRawData(2);
        Assert.Null(empty.Buffer);
        Assert.Equal(0, empty.Method);
        Assert.Equal((ushort)(VmZipFile1325.ResFullPath | VmZipFile1325.ResNeedFileStart), opened.Entries[2].Flags);

        Assert.Contains(opened.Messages, m => m.Level == VmZipFile1325.LevelError && m.Text == "pack.zip: 'secret.dat' uses an unsupported compression algorithm (#99).\n");
        Assert.Contains(opened.Messages, m => m.Text == "pack.zip: 'lock.dat' is encrypted. Encryption is not supported.\n");

        byte[] high = MakeZip(new[] { ItemRaw(new byte[] { 0x80, (byte)'A' }, new byte[] { 7 }) });
        var highZip = VmZipFile1325.CheckZip("h.zip", new VmZipFile1325.Reader(high), null);
        Assert.Equal("@a", highZip!.Entries[0].FileName);
    }

    [Fact]
    public void Zip64_AppliesSizesAndKeepsAnOversizeSlot()
    {
        byte[] multi = MakeZip(new[] { Item("a.txt", new byte[] { 1 }) }, disk: 1);
        var denied = new VmZipFile1325.Archive("multi.zip", new VmZipFile1325.Reader(multi));
        Assert.False(denied.Open(null));
        Assert.False(denied.HashGenerated);
        Assert.Contains(denied.Messages, m => m.Text == "multi.zip: Multipart Zip files are not supported.\n");

        byte[] overrun = MakeZip(new[] { Item("only.txt", new byte[] { 1 }) }, claimed: 2);
        var broken = new VmZipFile1325.Archive("over.zip", new VmZipFile1325.Reader(overrun));
        Assert.False(broken.Open(null));
        Assert.Contains(broken.Messages, m => m.Text == "over.zip: Central directory corrupted.");

        var specs = new List<Spec>
        {
            Item("wide.dat", new byte[] { (byte)'Z', (byte)'Z' }, zip64Unc: 0x80000000UL, zip64Local: 1),
            Item("ok.txt", new byte[] { (byte)'O', (byte)'K' }),
        };
        byte[] huge = MakeZip(specs);
        var hugeZip = VmZipFile1325.CheckZip("huge.zip", new VmZipFile1325.Reader(huge), null);
        Assert.NotNull(hugeZip);
        Assert.Equal(1, hugeZip!.NumLumps);
        Assert.Equal(2, hugeZip.Entries.Count);
        Assert.Equal("wide.dat", hugeZip.Entries[0].FileName);
        Assert.Equal(2u, hugeZip.Entries[0].Length);
        Assert.Equal("ok.txt", hugeZip.Entries[1].FileName);
        Assert.Equal(new byte[] { (byte)'Z', (byte)'Z' }, hugeZip.GetRawData(0).Buffer);
        Assert.Null(hugeZip.GetRawData(1).Buffer);
        Assert.Contains(hugeZip.Messages, m => m.Level == VmZipFile1325.LevelWarning && m.Text == "huge.zip: 'wide.dat' is too large.\n");

        byte[] zip64 = MakeZip(new[]
        {
            Item("real.bin", new byte[] { (byte)'W', (byte)'X', (byte)'Y', (byte)'Z' }, uncSize: 1, zip64Unc: 4, zip64UseRealLocal: true, force32LocalZero: true),
        }, zip64Record: true, pad: true);
        bool flag = false;
        Assert.NotEqual(0u, VmZipFile1325.FindCentralDir(new VmZipFile1325.Reader(zip64), ref flag));
        Assert.True(flag);
        var z = VmZipFile1325.CheckZip("z64.zip", new VmZipFile1325.Reader(zip64), null);
        Assert.NotNull(z);
        Assert.Equal(4u, z!.Entries[0].Length);
        Assert.Equal(4u, z.Entries[0].CompressedSize);
        Assert.Equal(new byte[] { (byte)'W', (byte)'X', (byte)'Y', (byte)'Z' }, z.GetRawData(0).Buffer);
    }

    sealed class Spec
    {
        public string Name = "";
        public byte[]? RawName;
        public byte[] Data = Array.Empty<byte>();
        public ushort Method;
        public ushort Flags;
        public uint Crc;
        public uint? UncSize;
        public ulong? Zip64Unc;
        public ulong Zip64Local;
        public bool Zip64UseRealLocal;
        public bool Force32LocalZero;
    }

    static Spec Item(string name, byte[] data, ushort method = 0, ushort flags = 0, uint crc = 0, uint? uncSize = null, ulong? zip64Unc = null, ulong zip64Local = 0, bool zip64UseRealLocal = false, bool force32LocalZero = false)
    {
        return new Spec
        {
            Name = name,
            Data = data,
            Method = method,
            Flags = flags,
            Crc = crc,
            UncSize = uncSize,
            Zip64Unc = zip64Unc,
            Zip64Local = zip64Local,
            Zip64UseRealLocal = zip64UseRealLocal,
            Force32LocalZero = force32LocalZero,
        };
    }

    static Spec ItemRaw(byte[] name, byte[] data)
    {
        return new Spec { RawName = name, Data = data };
    }

    static byte[] LocalOnly()
    {
        var d = new List<byte>();
        Le32(d, 0x04034b50);
        while (d.Count < 40)
        {
            d.Add(0);
        }

        return d.ToArray();
    }

    static byte[] MakeZip(IReadOnlyList<Spec> specs, ushort disk = 0, int? claimed = null, bool zip64Record = false, bool pad = false)
    {
        var blob = new List<byte>();
        var central = new List<byte>();
        if (pad)
        {
            byte[] padName = Encoding.ASCII.GetBytes("pad");
            Le32(blob, 0x04034b50);
            blob.AddRange(new byte[22]);
            Le16(blob, padName.Length);
            Le16(blob, 0);
            blob.AddRange(padName);
            blob.Add(0xEE);
        }

        foreach (Spec s in specs)
        {
            uint at = (uint)blob.Count;
            byte[] name = s.RawName ?? Encoding.ASCII.GetBytes(s.Name);
            uint comp = (uint)s.Data.Length;
            uint unc = s.UncSize ?? comp;
            byte[] extra = Array.Empty<byte>();
            if (s.Zip64Unc != null)
            {
                extra = Zip64(s.Zip64Unc.Value, comp, s.Zip64UseRealLocal ? at : s.Zip64Local);
            }

            uint localField = s.Force32LocalZero ? 0 : at;
            Le32(blob, 0x04034b50);
            blob.AddRange(new byte[22]);
            Le16(blob, name.Length);
            Le16(blob, 0);
            blob.AddRange(name);
            blob.AddRange(s.Data);

            Le32(central, 0x02014b50);
            central.AddRange(new byte[4]);
            Le16(central, s.Flags);
            Le16(central, s.Method);
            Le16(central, 0);
            Le16(central, 0);
            Le32(central, s.Crc);
            Le32(central, comp);
            Le32(central, unc);
            Le16(central, name.Length);
            Le16(central, extra.Length);
            Le16(central, 0);
            Le16(central, 0);
            Le16(central, 0);
            Le32(central, 0);
            Le32(central, localField);
            central.AddRange(name);
            central.AddRange(extra);
        }

        uint dirOff = (uint)blob.Count;
        blob.AddRange(central);
        int count = claimed ?? specs.Count;
        if (!zip64Record)
        {
            Le32(blob, 0x06054b50);
            Le16(blob, disk);
            Le16(blob, 0);
            Le16(blob, count);
            Le16(blob, count);
            Le32(blob, (uint)central.Count);
            Le32(blob, dirOff);
            Le16(blob, 0);
        }
        else
        {
            Le32(blob, 0x06064b50);
            Le64(blob, 44);
            Le16(blob, 45);
            Le16(blob, 45);
            Le32(blob, disk);
            Le32(blob, 0);
            Le64(blob, (ulong)count);
            Le64(blob, (ulong)count);
            Le64(blob, (ulong)central.Count);
            Le64(blob, dirOff);
            Le16(blob, 0);
        }

        blob.AddRange(new byte[] { 9, 9, 9, 9 });
        return blob.ToArray();
    }

    static byte[] Zip64(ulong unc, ulong comp, ulong local)
    {
        var e = new List<byte>();
        Le16(e, 1);
        Le16(e, 24);
        Le64(e, unc);
        Le64(e, comp);
        Le64(e, local);
        return e.ToArray();
    }

    static void Le16(List<byte> d, int v)
    {
        d.Add((byte)v);
        d.Add((byte)(v >> 8));
    }

    static void Le32(List<byte> d, uint v)
    {
        d.Add((byte)v);
        d.Add((byte)(v >> 8));
        d.Add((byte)(v >> 16));
        d.Add((byte)(v >> 24));
    }

    static void Le64(List<byte> d, ulong v)
    {
        for (int i = 0; i < 8; i++)
        {
            d.Add((byte)(v >> (8 * i)));
        }
    }
}
