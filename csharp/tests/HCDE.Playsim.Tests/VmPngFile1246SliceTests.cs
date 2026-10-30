namespace HCDE.Playsim.Tests;

public class VmPngFile1246SliceTests
{
    const string HeadGolden =
"""
gama 45454
png pal=0 bytes=49 type=2
save w=2 h=3 pitch=6 type=1 n=4 ok=1
gama 0
plte short 1
png pal=1 bytes=829 type=3
save w=1 h=1 pitch=1 type=0 n=0 ok=1
gama 90909
png pal=0 bytes=49 type=2
save w=2 h=3 pitch=6 type=1 n=0 ok=1

""";

    const string TailGolden =
"""
---- edge
gama 45454
png pal=0 bytes=49 type=2
save w=2 h=3 pitch=6 type=1 n=0 ok=1
text key=5 text=5 ok=1
chunk IDAT len=1 ok=1
iend ok
walk gAMA off=41 size=4
walk tEXt off=57 size=11
walk IDAT off=80 size=1
verify ok chunks=4 texts=1
found IHDR off=16 size=13 pt=1
found gAMA off=41 size=4 pt=2
found tEXt off=57 size=11 pt=3
found IDAT off=80 size=1 pt=4
miss IDAT pt=4
gettext Title n=5
gettext buf Title n=3 text=5
gettext buf Title n=8 text=5
dummy ok
walk IDAT off=41 size=10
verify end
verify fail
file null
verify sig bad
walk gAMA off=41 size=4
verify end
verify fail
gama 0
png pal=0 bytes=49 type=2
save w=1 h=1 pitch=1 type=1 n=0 ok=1
iend ok
walk gAMA off=41 size=4
verify iend bad
verify fail
gama 0
png pal=0 bytes=49 type=2
save w=1 h=1 pitch=1 type=1 n=0 ok=0
gama 0
write fail 49
free chunks=4 texts=1

""";

    [Fact]
    public void Create_WritesHeaderPaletteAndSaveCall()
    {
        string dump = Build(out byte[] rgb, out byte[] pal, out byte[] round);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("gama 45454\n", head, StringComparison.Ordinal);
        Assert.Contains("png pal=0 bytes=49 type=2\n", head, StringComparison.Ordinal);
        Assert.Contains("png pal=1 bytes=829 type=3\n", head, StringComparison.Ordinal);
        Assert.Contains("save w=2 h=3 pitch=6 type=1 n=4 ok=1\n", head, StringComparison.Ordinal);
        Assert.Contains("plte short 1\n", head, StringComparison.Ordinal);
        Assert.Equal(Convert.FromHexString("89504e470d0a1a0a0000000d4948445200000002000000030802000000368849d60000000467414d410000b18e7cfb5193"), rgb);
        Assert.Equal(829, pal.Length);
        Assert.Equal(Convert.FromHexString("89504e470d0a1a0a0000000d494844520000000100000001080300000028cb34bb0000000467414d41000000008b25604d"), pal.AsSpan(0, 49).ToArray());
        Assert.Equal(new byte[] { 255, 0, 0 }, pal.AsSpan(57, 3).ToArray());
        Assert.Equal(Convert.FromHexString("17899955"), pal.AsSpan(825, 4).ToArray());
        Assert.Equal(Convert.FromHexString("89504e470d0a1a0a0000000d4948445200000002000000030802000000368849d60000000467414d410001631d"), round.AsSpan(0, 45).ToArray());
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Verify_WalksChunksAndRejectsABadSignature()
    {
        string dump = Build(out _, out _, out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("dummy ok\n", tail, StringComparison.Ordinal);
        Assert.Contains("text key=5 text=5 ok=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("verify ok chunks=4 texts=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("found IHDR off=16 size=13 pt=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("gettext Title n=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("gettext buf Title n=3 text=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("verify sig bad\n", tail, StringComparison.Ordinal);
        Assert.Contains("verify iend bad\n", tail, StringComparison.Ordinal);
        Assert.Contains("write fail 49\n", tail, StringComparison.Ordinal);
        Assert.Contains("free chunks=4 texts=1\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out byte[] rgb, out byte[] pal, out byte[] round)
    {
        var vm = new VmPngFile1246();
        var file = new VmPngFile1246.OutFile();
        Assert.True(vm.Create(file, null, VmPngFile1246.Rgb, 2, 3, 6, 1f, new byte[] { 1, 2, 3, 4 }));
        rgb = file.Bytes.ToArray();

        var palFile = new VmPngFile1246.OutFile();
        var palette = new uint[1];
        palette[0] = 255u << 24 | 255u << 16;
        Assert.True(vm.Create(palFile, palette, VmPngFile1246.Pal, 1, 1, 1, 0f, null));
        pal = palFile.Bytes.ToArray();

        vm.PngGamma = 2f;
        var forced = new VmPngFile1246.OutFile();
        Assert.True(vm.Create(forced, null, VmPngFile1246.Rgb, 2, 3, 6, 1f, null));
        round = forced.Bytes.ToArray();
        vm.PngGamma = 0f;

        vm.Log.Add("---- edge");

        var built = new VmPngFile1246.OutFile();
        Assert.True(vm.Create(built, null, VmPngFile1246.Rgb, 2, 3, 6, 1f, null));
        Assert.True(vm.AppendText(built, "Title", "Hello"));
        Assert.True(vm.Append(built, "IDAT", new byte[] { 0xAB }));
        Assert.True(vm.Finish(built));
        var png = vm.Verify(built.Bytes.ToArray());
        Assert.NotNull(png);
        Assert.Equal(13u, vm.Find(png, "IHDR"));
        Assert.Equal(16, png!.Pos);
        Assert.Equal(4u, vm.Next(png, "gAMA"));
        Assert.Equal(11u, vm.Next(png, "tEXt"));
        Assert.Equal(1u, vm.Next(png, "IDAT"));
        Assert.Equal(0u, vm.Next(png, "IDAT"));
        Assert.Equal("Hello", vm.GetText(png, "Title"));
        var buf = new byte[3];
        Assert.True(vm.GetText(png, "Title", buf));
        Assert.Equal(new byte[] { (byte)'H', (byte)'e', (byte)'l' }, buf);
        var wide = new byte[8];
        wide[7] = 9;
        Assert.True(vm.GetText(png, "Title", wide));
        Assert.Equal(new byte[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0, 0, 0 }, wide);

        var dummy = new VmPngFile1246.OutFile();
        Assert.True(vm.CreateDummy(dummy));
        Assert.Equal(55, dummy.Bytes.Count);
        Assert.Null(vm.Verify(dummy.Bytes.ToArray()));

        Assert.False(vm.Create(null, null, VmPngFile1246.Rgb, 1, 1, 1, 0f, null));
        Assert.Null(vm.Verify(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
        Assert.Null(vm.Verify(rgb));
        var bare = new VmPngFile1246.OutFile();
        Assert.True(vm.Create(bare, null, VmPngFile1246.Rgb, 1, 1, 1, 0f, null));
        Assert.True(vm.Finish(bare));
        Assert.Null(vm.Verify(bare.Bytes.ToArray()));
        vm.SaveResult = false;
        int before = vm.SaveCalls;
        var saved = new VmPngFile1246.OutFile();
        Assert.False(vm.Create(saved, null, VmPngFile1246.Rgb, 1, 1, 1, 0f, null));
        Assert.Equal(before + 1, vm.SaveCalls);
        Assert.Equal(49, saved.Bytes.Count);
        vm.SaveResult = true;

        var blocked = new VmPngFile1246.OutFile { Limit = 0 };
        int calls = vm.SaveCalls;
        Assert.False(vm.Create(blocked, null, VmPngFile1246.Rgb, 1, 1, 1, 0f, null));
        Assert.Equal(calls, vm.SaveCalls);
        Assert.Empty(blocked.Bytes);

        vm.Free(png);
        Assert.Empty(png.Chunks);

        string dump = string.Join('\n', vm.Log) + "\n";
        return dump;
    }
}
