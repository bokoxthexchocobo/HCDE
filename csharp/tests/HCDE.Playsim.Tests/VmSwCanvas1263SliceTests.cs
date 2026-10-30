namespace HCDE.Playsim.Tests;

public class VmSwCanvas1263SliceTests
{
    const string TrueGolden =
"""
link 7 1
miss 8
hit 7 1
cache miss 2
lock bgra
need
canvas bgra 1 4 2 4
fill b 00000000ffffffffffffffffffffffffffffffff000000000000000000000000
update tc
flip
cache miss 0
lock pal 0
need
canvas pal 2 4 2 4
fill p 00000000ffffffff
cache hit 2
remap 060f141a2a00d100
updated 0 0 0
update tc
flip
cache miss 0
lock pal 0
need
cache miss 2
lock bgra
need
remap 060f161a2a00d100
updated 0 0 0

""";

    const string PalGolden =
"""
link 4 1
hit 4 1
lock pal 3
need
canvas pal 1 3 3 3
fill p 00000000ffffffff00
update pal
remap fffcf9fefbf8fdfaf7
updated 0 0 0
lock bgra
need
canvas bgra 2 3 3 3
fill b 00000000ffffffffffffffffffffffffffffffffffff0000000000000000000000000000
hit 4 1
drop pal 1
drop bgra 2
unload
hit 4 1
cache miss 0
lock pal 0
need
canvas pal 3 3 3 3
fill p 00000000ffffffff00
relink 4 2
hit 4 2
drop pal 3
dtor 1
hit 4 2
dtor 2
hit 4 2
link 9 3
update tc

""";

    [Fact]
    public void Truecolor_FlipsColumnsAndKeepsTheFillWhenTheCacheMisses()
    {
        var host = new VmSwCanvasHost1263();
        FillRgb(host.Rgb32k);
        var vm = VmSwCanvas1263.Create(host, 7, 4, 2);
        Assert.Null(VmSwCanvas1263.Find(host, 8));
        Assert.Same(vm, VmSwCanvas1263.Find(host, 7));

        byte[] bgra = vm.GetPixels(2);
        Assert.Equal(32, bgra.Length);
        Assert.Equal(255, bgra[4]);
        Assert.Equal(0, bgra[20]);
        Put(vm.BgraCanvas!, 0, 0, 16, 0, 0);
        Put(vm.BgraCanvas!, 1, 0, 0, 32, 0);
        Put(vm.BgraCanvas!, 2, 0, 0, 0, 48);
        Put(vm.BgraCanvas!, 3, 0, 255, 255, 255);
        Put(vm.BgraCanvas!, 0, 1, 8, 8, 8);
        Put(vm.BgraCanvas!, 1, 1, 24, 16, 8);
        Put(vm.BgraCanvas!, 2, 1, 7, 0, 0);
        Put(vm.BgraCanvas!, 3, 1, 0, 7, 0);
        vm.UpdatePixels(true);
        Assert.False(vm.NeedsUpdate);
        Assert.False(vm.FirstUpdate);
        Assert.False(vm.LastUpdateType);
        Assert.Equal(16, vm.Bgra![2]);
        Assert.Equal(48, vm.Bgra[16]);

        host.CurrentUpdate = 1;
        Put(vm.BgraCanvas!, 1, 0, 40, 0, 8);
        vm.UpdatePixels(true);
        Assert.Equal(2, vm.PalCanvas!.Id);
        Assert.Equal(1, vm.BgraCanvas!.Id);
        Assert.Equal(0x16, vm.Pal![2]);
        Assert.Equal(TrueGolden, Dump(host));
    }

    [Fact]
    public void Paletted_RemapsOddFillAndKeepsTheMapAfterDestroy()
    {
        var host = new VmSwCanvasHost1263();
        for (int i = 0; i < 256; i++)
            host.Remap[i] = (byte)(255 - i);
        var vm = VmSwCanvas1263.Create(host, 4, 3, 3);
        Assert.Same(vm, VmSwCanvas1263.Find(host, 4));
        byte[] pal = vm.GetPixelsLocked(3);
        Assert.Equal(9, pal.Length);
        Assert.Equal(0, pal[0]);
        Assert.Equal(0, pal[3]);
        Assert.Equal(255, pal[4]);
        Assert.Equal(255, pal[7]);
        Assert.Equal(0, pal[8]);
        for (int i = 0; i < 9; i++)
            vm.PalCanvas!.Pixels[i] = (byte)i;
        vm.UpdatePixels(false);
        Assert.Equal(247, vm.Pal![8]);
        Assert.False(vm.FirstUpdate);

        byte[] bgra = vm.GetPixelsBgraLocked();
        Assert.Equal(36, bgra.Length);
        Assert.Equal(0, bgra[3]);
        Assert.Equal(255, bgra[4]);
        Assert.Equal(255, bgra[21]);
        Assert.Equal(0, bgra[22]);
        Assert.Same(vm, VmSwCanvas1263.Find(host, 4));
        vm.Unload();
        Assert.Null(vm.PalCanvas);
        Assert.Null(vm.Bgra);
        Assert.Same(vm, VmSwCanvas1263.Find(host, 4));
        Assert.Equal(0, host.CurrentUpdate);
        byte[] again = vm.GetPixels(0);
        Assert.Equal(255, again[4]);
        Assert.Equal(0, again[8]);

        var replaced = VmSwCanvas1263.Create(host, 4, 3, 3);
        Assert.Same(replaced, VmSwCanvas1263.Find(host, 4));
        vm.Destroy();
        Assert.Same(replaced, VmSwCanvas1263.Find(host, 4));
        replaced.Destroy();
        Assert.Same(replaced, VmSwCanvas1263.Find(host, 4));
        Assert.Null(replaced.PalCanvas);

        var fresh = VmSwCanvas1263.Create(host, 9, 2, 2);
        Assert.True(fresh.NeedsUpdate);
        Assert.True(fresh.FirstUpdate);
        Assert.Throws<NullReferenceException>(() => fresh.UpdatePixels(true));
        Assert.True(fresh.NeedsUpdate);
        Assert.True(fresh.FirstUpdate);
        Assert.Equal(PalGolden, Dump(host));
    }

    static void FillRgb(byte[] rgb)
    {
        for (int ri = 0; ri < 32; ri++)
        {
            for (int gi = 0; gi < 32; gi++)
            {
                for (int bi = 0; bi < 32; bi++)
                    rgb[(ri * 32 + gi) * 32 + bi] = (byte)((ri * 3 + gi * 5 + bi * 7) & 255);
            }
        }
    }

    static void Put(VmSwCanvas1263.Canvas canvas, int x, int y, int r, int g, int b)
    {
        int i = (x + y * canvas.Pitch) * 4;
        canvas.Pixels[i] = (byte)b;
        canvas.Pixels[i + 1] = (byte)g;
        canvas.Pixels[i + 2] = (byte)r;
        canvas.Pixels[i + 3] = 0;
    }

    static string Dump(VmSwCanvasHost1263 host) => string.Join('\n', host.Log) + "\n";
}
