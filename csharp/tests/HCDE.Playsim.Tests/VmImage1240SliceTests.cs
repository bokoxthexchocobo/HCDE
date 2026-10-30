using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmImage1240SliceTests
{
    const string HeadGolden =
"""
make id=1 w=0 h=0 frames=1
precache null
count id=1 tc=0 pal=1
count id=1 tc=0 pal=2
pal id=1 conv=0 frame=0
create id=1 conv=0 frame=0
pal frame unset asked 0 store 0 ref=1
view 
pal id=1 conv=0 frame=0
pal empty keep
own 
make id=2 w=2 h=2 frames=1
count id=2 tc=0 pal=1
count id=2 tc=0 pal=2
pal id=2 conv=0 frame=4
create id=2 conv=0 frame=4
pal frame unset asked 4 store 0 ref=1
view 01040304
pal id=2 conv=0 frame=0
move
own 01040304
pal id=2 conv=0 frame=0
create id=2 conv=0 frame=0
own 01000304
pal id=2 conv=1 frame=0
create id=2 conv=1 frame=0
own 09000304
make id=3 w=2 h=2 frames=1
count id=3 tc=0 pal=1
count id=3 tc=0 pal=2
pal id=3 conv=0 frame=0
create id=3 conv=0 frame=0
pal frame unset asked 0 store 0 ref=1
take copy 01000304
pal id=3 conv=0 frame=0
move
take own 01000304
begin
count id=2 tc=0 pal=1
end was pal=1 rgba=0

"""
;

    const string TailGolden =
"""
---- edge
make id=4 w=2 h=2 frames=1
count id=4 tc=1 pal=0
count id=4 tc=2 pal=0
bmp id=4 conv=0 frame=2 remap=0
create id=4 conv=0 frame=2
cache ref=1 trans=0 010203ff030405ff020304ff040506ff fill=01 n=1 rest=02 m=2/16
bmp id=4 conv=0 frame=2 remap=0
move trans=0 ab0203ff030405ff020304ff040506ff fill=ab n=1 rest=02 m=2/16
make id=5 w=2 h=2 frames=1
create id=5 conv=0 frame=0
ret 0
bmp 00000000000000000000000000000000 fill=00 n=16 rest=00 m=16/16
bmp id=2 conv=1 frame=0 remap=0
lum -> normal
create id=2 conv=0 frame=0
fresh trans=0 010203ff030405ff00000000040506ff fill=01 n=1 rest=02 m=1/16
copy lum -> normal
create id=2 conv=0 frame=0
ret 0
bmp 010203ff030405ff00000000040506ff fill=01 n=1 rest=02 m=1/16
bmp id=2 conv=2 frame=0 remap=0
create id=2 conv=2 frame=0
fresh trans=0 08090aff030405ff00000000040506ff fill=08 n=1 rest=09 m=1/16
make id=6 w=2 h=2 frames=1
bmp id=6 conv=0 frame=1 remap=0
frame 1 -> 0
create id=6 conv=0 frame=0
fresh trans=0 010203ff030405ff00000000040506ff fill=01 n=1 rest=02 m=1/16
make id=7 w=2 h=2 frames=3
bmp id=7 conv=0 frame=1 remap=0
create id=7 conv=0 frame=1
fresh trans=0 010203ff030405ff010203ff040506ff fill=01 n=2 rest=02 m=2/16
bmp id=6 conv=1 frame=1 remap=1
frame 1 -> 0
create id=6 conv=0 frame=0
remap trans=0 210000ff230000ff00000000240000ff fill=21 n=1 rest=00 m=10/16
make id=8 w=2 h=2 frames=1
hole id=8 frame=0 ref=2 trans=9
bmp id=8 conv=0 frame=0 remap=0
share null trans=9
bmp id=8 conv=0 frame=0 remap=0
create id=8 conv=0 frame=0
rgba hole keep n=1 trans=0 010203ff030405ff00000000040506ff fill=01 n=1 rest=02 m=1/16
bmp id=8 conv=0 frame=0 remap=0
create id=8 conv=0 frame=0
rgba hole keep n=1 trans=0 010203ff030405ff00000000040506ff fill=01 n=1 rest=02 m=1/16
lump -1
resize 4
closed 3
try IMGZ
try PNG
make id=9 w=1 h=1 frames=1
keep PNG id=9
hit 3 id=9
try IMGZ
try PNG
try DDS
try PCX
try STB
try QOI
try WEBP
try TGA
try ANM
try STARTUP
try RAW
skip FLAT
try PATCH
make id=10 w=2 h=2 frames=1
keep PATCH id=10
try IMGZ
try PNG
try DDS
try PCX
try STB
try QOI
try WEBP
try TGA
try ANM
try STARTUP
try RAW
try FLAT
make id=11 w=3 h=3 frames=1
keep FLAT id=11

"""
;

    [Fact]
    public void PalettedCache_LeavesFrameUnsetAndCopiesViews()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("pal frame unset asked 4 store 0 ref=1\n", head, StringComparison.Ordinal);
        Assert.Contains("view 01040304\n", head, StringComparison.Ordinal);
        Assert.Contains("move\nown 01040304\n", head, StringComparison.Ordinal);
        Assert.Contains("own 01000304\n", head, StringComparison.Ordinal);
        Assert.Contains("create id=2 conv=1 frame=0\n", head, StringComparison.Ordinal);
        Assert.Contains("own 09000304\n", head, StringComparison.Ordinal);
        Assert.Contains("pal empty keep\n", head, StringComparison.Ordinal);
        Assert.Contains("take copy 01000304\n", head, StringComparison.Ordinal);
        Assert.Contains("take own 01000304\n", head, StringComparison.Ordinal);
        Assert.Contains("begin\n", head, StringComparison.Ordinal);
        Assert.Contains("end was pal=1 rgba=0\n", head, StringComparison.Ordinal);
        Assert.Equal(0xEE, _copied[0]);
        Assert.Equal(1, _owned[0]);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void BitmapCache_BlitsColumnsAndProbesFormatsInOrder()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("cache ref=1 trans=0 010203ff030405ff020304ff040506ff fill=01 n=1 rest=02 m=2/16\n", tail, StringComparison.Ordinal);
        Assert.Contains("move trans=0 ab0203ff030405ff020304ff040506ff fill=ab n=1 rest=02 m=2/16\n", tail, StringComparison.Ordinal);
        Assert.Contains("lum -> normal\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy lum -> normal\n", tail, StringComparison.Ordinal);
        Assert.Contains("fresh trans=0 08090aff030405ff00000000040506ff fill=08 n=1 rest=09 m=1/16\n", tail, StringComparison.Ordinal);
        Assert.Contains("frame 1 -> 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("create id=7 conv=0 frame=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("010203ff030405ff010203ff040506ff fill=01 n=2 rest=02 m=2/16\n", tail, StringComparison.Ordinal);
        Assert.Contains("remap trans=0 210000ff230000ff00000000240000ff fill=21 n=1 rest=00 m=10/16\n", tail, StringComparison.Ordinal);
        Assert.Contains("share null trans=9\n", tail, StringComparison.Ordinal);
        Assert.Contains("rgba hole keep n=1 trans=0 ", tail, StringComparison.Ordinal);
        Assert.Contains("lump -1\n", tail, StringComparison.Ordinal);
        Assert.Contains("resize 4\n", tail, StringComparison.Ordinal);
        Assert.Contains("closed 3\n", tail, StringComparison.Ordinal);
        Assert.Contains("skip FLAT\n", tail, StringComparison.Ordinal);
        Assert.Contains("keep PATCH id=", tail, StringComparison.Ordinal);
        Assert.Contains("hit 3 id=", tail, StringComparison.Ordinal);
        Assert.Contains("keep FLAT id=", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("try PATCH", tail[tail.IndexOf("try FLAT", StringComparison.Ordinal)..], StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static byte[] _copied = new byte[1];
    static byte[] _owned = new byte[1];

    static string Build()
    {
        var vm = new VmImage1240();
        byte[] Paint(int conv, int frame)
        {
            byte[] p = new byte[4];
            p[0] = 1;
            p[1] = (byte)frame;
            p[2] = 3;
            p[3] = 4;
            if (conv == VmImage1240.Luminance)
                p[0] = 9;
            else if (conv == VmImage1240.NoRemap0)
                p[0] = 8;
            return p;
        }

        VmImage1240.Image empty = vm.Make(0, 0, 1, null);
        vm.Precache(null, false);
        vm.Precache(empty, false);
        vm.Precache(empty, false);
        vm.Cached(empty, VmImage1240.Normal, 0);
        vm.Cached(empty, VmImage1240.Normal, 0);

        VmImage1240.Image patterned = vm.Make(2, 2, 1, Paint);
        vm.Precache(patterned, false);
        vm.Precache(patterned, false);
        vm.Cached(patterned, VmImage1240.Normal, 4);
        vm.Cached(patterned, VmImage1240.Normal, 0);
        vm.Cached(patterned, VmImage1240.Normal, 0);
        vm.Cached(patterned, VmImage1240.Luminance, 0);

        VmImage1240.Image copied = vm.Make(2, 2, 1, Paint);
        vm.Precache(copied, false);
        vm.Precache(copied, false);
        _copied = vm.Paletted(copied, VmImage1240.Normal, 0);
        _copied[0] = 0xEE;
        _owned = vm.Paletted(copied, VmImage1240.Normal, 0);

        vm.Begin();
        vm.Precache(patterned, false);
        vm.End();

        vm.Log.Add("---- edge");

        VmImage1240.Image shared = vm.Make(2, 2, 1, Paint);
        vm.Precache(shared, true);
        vm.Precache(shared, true);
        byte[] first = vm.Bitmap(shared, null, VmImage1240.Normal, 2, out _);
        first[0] = 0xAB;
        vm.Bitmap(shared, null, VmImage1240.Normal, 2, out _);

        VmImage1240.Image plain = vm.Make(2, 2, 1, null);
        vm.Copy(plain, VmImage1240.Normal, 0);
        vm.Bitmap(patterned, null, VmImage1240.Luminance, 0, out _);
        vm.Copy(patterned, VmImage1240.Luminance, 0);
        vm.Bitmap(patterned, null, VmImage1240.NoRemap0, 0, out _);

        VmImage1240.Image one = vm.Make(2, 2, 1, Paint);
        vm.Bitmap(one, null, VmImage1240.Normal, 1, out _);
        VmImage1240.Image many = vm.Make(2, 2, 3, Paint);
        vm.Bitmap(many, null, VmImage1240.Normal, 1, out _);

        uint[] remap = new uint[256];
        for (int i = 0; i < 256; i++)
            remap[i] = VmImage1240.Pack((byte)(i ^ 0x20), 0, 0, i == 0 ? (byte)0 : (byte)255);
        vm.Bitmap(one, remap, VmImage1240.Luminance, 1, out _);

        VmImage1240.Image hole = vm.Make(2, 2, 1, Paint);
        vm.HoldRgbaHole(hole, 0, 2, 9);
        vm.Bitmap(hole, null, VmImage1240.Normal, 0, out _);
        vm.Bitmap(hole, null, VmImage1240.Normal, 0, out _);
        vm.Bitmap(hole, null, VmImage1240.Normal, 0, out _);

        Assert.Null(vm.GetImage(-1, false));
        Assert.Null(vm.GetImage(3, false));
        vm.Open[3] = new byte[] { 1 };
        vm.Try["PNG"] = (lump, flat) => lump == 3 ? vm.Make(1, 1, 1, null) : null;
        vm.Try["PATCH"] = (lump, flat) => vm.Make(2, 2, 1, null);
        vm.Try["FLAT"] = (lump, flat) => vm.Make(3, 3, 1, null);
        VmImage1240.Image? png = vm.GetImage(3, false);
        Assert.NotNull(png);
        Assert.Same(png, vm.GetImage(3, false));
        vm.Open[1] = new byte[] { 2 };
        VmImage1240.Image? patch = vm.GetImage(1, false);
        Assert.NotNull(patch);
        vm.Open[2] = new byte[] { 3 };
        VmImage1240.Image? flatImage = vm.GetImage(2, true);
        Assert.NotNull(flatImage);
        Assert.Equal("FLAT", LastKeep(vm));

        string dump = string.Join("\n", vm.Log) + "\n";
        return dump;
    }

    static string LastKeep(VmImage1240 vm)
    {
        for (int i = vm.Log.Count - 1; i >= 0; i--)
        {
            if (vm.Log[i].StartsWith("keep ", StringComparison.Ordinal))
                return vm.Log[i].Split(' ')[1];
        }

        return "";
    }
}
