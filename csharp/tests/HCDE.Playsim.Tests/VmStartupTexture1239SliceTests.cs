using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmStartupTexture1239SliceTests
{
    [Fact]
    public void Probe_RejectsAPatchAndDecodesStrifeAndTheBackgroundWindow()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("missing 9\n", head, StringComparison.Ordinal);
        Assert.Contains("raw len 10 need 153648\n", head, StringComparison.Ordinal);
        Assert.Contains("raw no\n", head, StringComparison.Ordinal);
        Assert.Contains("reject NOTCH\n", head, StringComparison.Ordinal);
        Assert.Contains("notch pal steps 0\n", head, StringComparison.Ordinal);
        Assert.Contains("notch work zero\n", head, StringComparison.Ordinal);
        Assert.Contains("copy none\n", head, StringComparison.Ordinal);
        Assert.Contains("back seek=18240 read=30400 at=13120 r0=f0 r41=11 rend=22 r136=f0\n", head, StringComparison.Ordinal);
        Assert.Contains("map=gray\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Startup_SharesThePaletteWithNotchesAndSkipsTheFlipOnCopy()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("kind=startup w=640 h=480 pal=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal8 ", tail, StringComparison.Ordinal);
        Assert.Contains("notch pal steps 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("ret 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("m=307198/307200\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("copy none", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmStartupTexture1239();
        var log = vm.Log;
        vm.TryCreate(9);

        vm.Files[1] = ("STARTUP", new byte[10]);
        vm.TryCreate(1);

        var patch = new byte[184];
        patch[0] = 1;
        patch[2] = 1;
        patch[8] = 12;
        vm.Files[2] = ("NOTCH", patch);
        vm.TryCreate(2);

        vm.Files[3] = ("NOTCH", new byte[184]);
        var notch = vm.TryCreate(3);
        vm.Pixels(notch!, VmStartupTexture1239.Normal, 3);
        vm.Copy(notch!, VmStartupTexture1239.Luminance, 4, out _);

        var strife = new byte[256];
        strife[0] = 1;
        strife[1] = 2;
        strife[16] = 3;
        vm.Files[4] = ("STRTLZ1", strife);
        var pic = vm.TryCreate(4);
        vm.Pixels(pic!, VmStartupTexture1239.Normal, 0);
        vm.Pixels(pic!, VmStartupTexture1239.Luminance, 1);
        vm.Copy(pic!, VmStartupTexture1239.Normal, 2, out _);

        vm.Files[5] = ("STRTLZ1", new byte[255]);
        vm.TryCreate(5);

        var back = new byte[64000];
        back[18240] = 0x11;
        back[18240 + 95 * 320 - 1] = 0x22;
        vm.Files[6] = ("STARTUP0", back);
        var bg = vm.TryCreate(6);
        vm.Pixels(bg!, VmStartupTexture1239.Normal, 0);
        vm.Copy(bg!, VmStartupTexture1239.Normal, 0, out _);

        vm.Files[7] = ("NOPE", Array.Empty<byte>());
        vm.TryCreate(7);

        log.Add("---- edge");

        var startup = new byte[153648];
        startup[3] = 1;
        startup[24] = 0x15;
        startup[25] = 0x2a;
        startup[26] = 0x03;
        startup[30] = 2;
        int plane = 640 / 8 * 480;
        startup[48] = 0x40;
        startup[48 + 3 * plane] = 0x80;
        vm.Files[8] = ("STARTUP", startup);
        var page = vm.TryCreate(8);
        vm.Pixels(page!, VmStartupTexture1239.Normal, 8);
        vm.Pixels(page!, VmStartupTexture1239.Luminance, 8);
        vm.Copy(page!, VmStartupTexture1239.Normal, 9, out _);

        vm.Files[9] = ("NETNOTCH", Enumerable.Repeat((byte)0x1a, 32).ToArray());
        var net = vm.TryCreate(9);
        vm.Pixels(net!, VmStartupTexture1239.Normal, 0);
        vm.Copy(net!, VmStartupTexture1239.Normal, 0, out _);

        return string.Join('\n', log) + "\n";
    }

    const string HeadGolden =
"""
missing 9
raw len 10 need 153648
reject STARTUP
raw no
reject NOTCH
make id=1 kind=notch w=16 h=23 pal=0
pixels id=1 kind=notch conv=0 frame=3 map=game
notch pal steps 0
notch work zero
pix 07070707070707070707070707070707 fill=07 n=368 rest=07 m=368/368
copy id=1 kind=notch conv=1 frame=4
ret 0
bmp 00000000000000000000000000000000 fill=00 n=1472 rest=00 m=1472/1472
make id=2 kind=strife w=16 h=16 pal=0
pixels id=2 kind=strife conv=0 frame=0 map=game
pix 01030707070707070707070707070707 fill=01 n=1 rest=03 m=1/256
pixels id=2 kind=strife conv=1 frame=1 map=gray
pix 23212222222222222222222222222222 fill=23 n=1 rest=21 m=1/256
copy id=2 kind=strife conv=0 frame=2
copy none
raw len 255 need 256
reject STRTLZ1
make id=3 kind=back w=320 h=200 pal=0
pixels id=3 kind=back conv=0 frame=0 map=game
back seek=18240 read=30400 at=13120 r0=f0 r41=11 rend=22 r136=f0
pix f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0 fill=f0 n=33600 rest=f0 m=33600/64000
copy id=3 kind=back conv=0 frame=0
copy none
unknown NOPE

""";

    const string TailGolden =
"""
---- edge
pal8 000c000000000000a500180000000000
pal32 000000ff000004ff000000ff000000ff000000ff000000ff000000ff000000ff0caa55ff000000ff000008ff000000ff000000ff000000ff000000ff000000ff
make id=4 kind=startup w=640 h=480 pal=0
pixels id=4 kind=startup conv=0 frame=8 map=game
pix a5070707070707070707070707070707 fill=a5 n=1 rest=07 m=307198/307200
pixels id=4 kind=startup conv=1 frame=8 map=gray
pix 87222222222222222222222222222222 fill=87 n=1 rest=22 m=307198/307200
copy id=4 kind=startup conv=0 frame=9
ret 0
bmp 0caa55ff000004ff000000ff000000ff fill=0c n=1 rest=aa m=1/1228800
make id=5 kind=notch w=4 h=16 pal=0
pixels id=5 kind=notch conv=0 frame=0 map=game
notch pal steps 0
notch work zero
pix 07070707070707070707070707070707 fill=07 n=64 rest=07 m=64/64
copy id=5 kind=notch conv=0 frame=0
ret 0
bmp 000004ff000008ff000004ff000008ff fill=00 n=128 rest=00 m=128/256

""";
}
