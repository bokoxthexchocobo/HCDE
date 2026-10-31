using System.Text;

namespace HCDE.Playsim.Tests;

public class VmTextureAnim1250SliceTests
{
    const string HeadGolden =
"""
animated lump=4 len=116 flags=2
rec off=0 wall=1 decalbit=1 start=aaa end=ccc pic1=1 pic2=3
nodecals 0 3 1
speed 35
Defining animation 'AAA' (texture 1, lump 10, file 2) to 'CCC' (texture 3, lump 12, file 2)
simple check pic=1 count=3 end=3
anim add idx=0 base=1
simple stored count=3 nframe=1 speed=1000 range=0 pic=1 discrete=0
simple base=1 frames=3 type=0 speed=1000
rec off=23 wall=1 decalbit=0 start=ccc end=aaa pic1=3 pic2=1
nodecals 1 1 3
speed 8
Defining animation 'CCC' (texture 3, lump 12, file 2) to 'AAA' (texture 1, lump 10, file 2)
swap pic1=1 pic2=3 type=1
simple check pic=1 count=3 end=3
anim replace idx=0 base=1
simple stored count=3 nframe=1 speed=228 range=0 pic=1 discrete=0
simple base=1 frames=3 type=1 speed=228
rec off=46 wall=0 decalbit=0 start=f1 end=f2 pic1=4 pic2=5
speed 70
Defining animation 'F1' (texture 4, lump 40, file 3) to 'F2' (texture 5, lump 41, file 3)
simple check pic=4 count=2 end=5
anim add idx=1 base=4
simple stored count=2 nframe=1 speed=2000 range=0 pic=4 discrete=0
simple base=4 frames=2 type=1 speed=2000
rec off=69 wall=1 decalbit=1 start=WWW end=XXX pic1=6 pic2=7
nodecals 0 7 6
speed 65536
warp 6 style=2
rec off=92 wall=1 decalbit=0 start=aaa end=aaa pic1=1 pic2=1
nodecals 1 1 1
speed 35
Defining animation 'AAA' (texture 1, lump 10, file 2) to 'AAA' (texture 1, lump 10, file 2)
Animation aaa in ANIMATED has only one frame
animated end off=115

""";

    const string TailGolden =
"""
---- edge
animated lump=5 len=139 flags=2
rec off=0 wall=1 decalbit=0 start=NOPE end=AAA pic1=-1 pic2=na
rec skip missing
rec off=23 wall=1 decalbit=0 start=AAA end=NOPE pic1=1 pic2=-1
rec skip missing
rec off=46 wall=1 decalbit=0 start=OVR end=AAA pic1=0 pic2=1
nodecals 1 1 0
speed 35
rec skip type
rec off=69 wall=1 decalbit=0 start=BBB end=CCC pic1=2 pic2=3
nodecals 1 3 2
speed 65535
simple check pic=2 count=2 end=3
anim add idx=2 base=2
simple stored count=2 nframe=1 speed=1872428 range=0 pic=2 discrete=0
simple base=2 frames=2 type=0 speed=1872428
rec off=92 wall=1 decalbit=0 start=AAA end=WWW pic1=1 pic2=6
nodecals 1 6 1
speed 35
simple check pic=1 count=6 end=6
simple incompatible
simple null
rec off=115 wall=1 decalbit=0 start=XXX end=WWW pic1=7 pic2=6
nodecals 1 6 7
speed 70000
swap pic1=6 pic2=7 type=1
simple check pic=6 count=2 end=7
anim add idx=3 base=6
simple stored count=2 nframe=1 speed=2000000 range=0 pic=6 discrete=0
simple base=6 frames=2 type=1 speed=2000000
animated end off=138
clean 0 force=1
clean 1 force=1
free switch 0
delete anims=4 fires=2 switches=2 doors=1 indices=4
anim add idx=0 base=5
complex base=5 count=2 type=0 discrete=1 f0=4,5,9 f1=6,7,8
simple check pic=4 count=2 end=5
anim stale 1 1
simple check pic=1 count=2 end=2
anim replace idx=0 base=1
simple stored count=2 nframe=1 speed=5 range=1 pic=1 discrete=0
direct base=1 count=2 type=0 discrete=0 speed=5 range=1 pic=1 nframe=1
simple check pic=0 count=5 end=0
anim add idx=1 base=0
simple stored count=5 nframe=1 speed=10 range=3 pic=0 discrete=0
direct base=0 count=5 type=0 discrete=0 speed=10 range=3 pic=0 nframe=1
simple check pic=7 count=5 end=-1
simple incompatible
animated lump=6 len=3 flags=2
Tried to read past end of ANIMATED lump.
animated missing
animated lump=8 len=0 flags=2
Tried to read past end of ANIMATED lump.
animated lump=9 len=23 flags=2
rec off=0 wall=1 decalbit=0 start=aaa end=aaa pic1=1 pic2=1
nodecals 1 1 1
speed 35
Animation aaa in ANIMATED has only one frame
Tried to read past end of ANIMATED lump.
animated lump=11 len=1 flags=2
animated end off=0
animated lump=12 len=24 flags=2
rec off=0 wall=1 decalbit=0 start=- end=AAA pic1=0 pic2=1
nodecals 1 1 0
speed 35
rec skip type
animated end off=23
animated null

""";

    [Fact]
    public void Init_ReplacesOnTheSameBaseAndWarpsAbove16Bits()
    {
        string dump = Build(out VmTextureAnim1250 vm);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("simple base=1 frames=3 type=0 speed=1000\n", head, StringComparison.Ordinal);
        Assert.Contains("anim replace idx=0 base=1\n", head, StringComparison.Ordinal);
        Assert.Contains("simple base=1 frames=3 type=1 speed=228\n", head, StringComparison.Ordinal);
        Assert.Contains("simple base=4 frames=2 type=1 speed=2000\n", head, StringComparison.Ordinal);
        Assert.Contains("warp 6 style=2\n", head, StringComparison.Ordinal);
        Assert.Contains("Animation aaa in ANIMATED has only one frame\n", head, StringComparison.Ordinal);
        Assert.Contains("Defining animation 'AAA' (texture 1, lump 10, file 2) to 'CCC' (texture 3, lump 12, file 2)\n", head, StringComparison.Ordinal);
        Assert.Equal(1000, VmTextureAnim1250.Scale(35, 1000, 35));
        Assert.Equal(228, VmTextureAnim1250.Scale(8, 1000, 35));
        Assert.Equal(2, vm.Textures[6].Warp);
        Assert.Equal(1, vm.Textures[7].Warp);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Init_SkipsAShortLumpAndReusesAStaleIndex()
    {
        string dump = Build(out VmTextureAnim1250 vm);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("rec skip missing\n", tail, StringComparison.Ordinal);
        Assert.Contains("pic2=na\n", tail, StringComparison.Ordinal);
        Assert.Contains("rec skip type\n", tail, StringComparison.Ordinal);
        Assert.Contains("simple incompatible\n", tail, StringComparison.Ordinal);
        Assert.Contains("simple null\n", tail, StringComparison.Ordinal);
        Assert.Contains("speed=1872428\n", tail, StringComparison.Ordinal);
        Assert.Contains("speed=2000000\n", tail, StringComparison.Ordinal);
        Assert.Contains("anim stale 1 1\n", tail, StringComparison.Ordinal);
        Assert.Contains("Tried to read past end of ANIMATED lump.\n", tail, StringComparison.Ordinal);
        Assert.Contains("animated missing\n", tail, StringComparison.Ordinal);
        Assert.Contains("nframe=1\n", tail, StringComparison.Ordinal);
        Assert.Equal(2, vm.Anims.Count);
        Assert.Equal(1, vm.Anims[0].Base);
        Assert.False(vm.Anims[0].Discrete);
        Assert.Equal(2, vm.Anims[0].Count);
        Assert.Single(vm.Anims[0].Frames);
        Assert.Equal(5u, vm.Anims[0].Frames[0].SpeedMin);
        Assert.Equal(0, vm.Anims[1].Base);
        Assert.Equal(5, vm.Anims[1].Count);
        Assert.Single(vm.Anims[1].Frames);
        Assert.Equal(3u, vm.Anims[1].Frames[0].SpeedRange);
        Assert.Equal(0, vm.Indices[5]);
        Assert.Equal(0, vm.Indices[1]);
        Assert.Equal(1, vm.Indices[0]);
        Assert.True(vm.Textures[1].NoDecals);
        Assert.False(vm.Textures[4].NoDecals);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out VmTextureAnim1250 vm)
    {
        vm = new VmTextureAnim1250 { DebugAnimated = true };
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "OVR", Use = VmTextureAnim1250.Override, Lump = 1, File = 7 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "AAA", Use = VmTextureAnim1250.Wall, Lump = 10, File = 2 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "BBB", Use = VmTextureAnim1250.Wall, Lump = 11, File = 2 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "CCC", Use = VmTextureAnim1250.Wall, Lump = 12, File = 2 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "F1", Use = VmTextureAnim1250.Flat, Lump = 40, File = 3 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "F2", Use = VmTextureAnim1250.Flat, Lump = 41, File = 3 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "WWW", Use = VmTextureAnim1250.Wall, Lump = 50, File = 4 });
        vm.Textures.Add(new VmTextureAnim1250.Tex { Name = "XXX", Use = VmTextureAnim1250.Wall, Lump = 51, File = 4, Warp = 1 });
        vm.FileStarts.Add(0);
        vm.FileStarts.Add(4);
        vm.FileStarts.Add(8);

        var head = new List<byte>();
        head.AddRange(Rec(3, "ccc", "aaa", 35));
        head.AddRange(Rec(1, "aaa", "ccc", 8));
        head.AddRange(Rec(0, "f2", "f1", 70));
        head.AddRange(Rec(3, "XXX", "WWW", 65536));
        head.AddRange(Rec(1, "aaa", "aaa", 35));
        head.Add(0xFF);
        vm.Lumps["ANIMATED"] = 4;
        vm.Bytes[4] = head.ToArray();
        vm.InitAnimated();
        vm.Log.Add("---- edge");

        vm.DebugAnimated = false;
        var tail = new List<byte>();
        tail.AddRange(Rec(1, "AAA", "NOPE", 1));
        tail.AddRange(Rec(1, "NOPE", "AAA", 1));
        tail.AddRange(Rec(1, "AAA", "OVR", 35));
        tail.AddRange(Rec(1, "CCC", "BBB", 65535));
        tail.AddRange(Rec(1, "WWW", "AAA", 35));
        tail.AddRange(Rec(1, "WWW", "XXX", 70000));
        tail.Add(0xFF);
        vm.Lumps["ANIMATED"] = 5;
        vm.Bytes[5] = tail.ToArray();
        vm.InitAnimated();

        var kept = new VmTextureAnim1250.Fire();
        var other = new VmTextureAnim1250.Fire();
        vm.Fires.Add(kept);
        vm.Fires.Add(other);
        var slot = new VmTextureAnim1250.SwitchSlot();
        vm.Switches.Add(slot);
        vm.Switches.Add(null);
        vm.Doors.Add(9);
        vm.DeleteAll();
        Assert.True(kept.Cleaned);
        Assert.True(other.Cleaned);
        Assert.True(slot.Freed);
        Assert.Empty(vm.Anims);
        Assert.Equal(4, vm.Indices.Count);

        var frames = new List<VmTextureAnim1250.Frame>
        {
            new() { SpeedMin = 4, SpeedRange = 5, Pic = 9 },
            new() { SpeedMin = 6, SpeedRange = 7, Pic = 8 },
        };
        vm.AddComplex(5, frames);
        frames[0].SpeedMin = 99;
        Assert.Equal(4u, vm.Anims[0].Frames[0].SpeedMin);

        Assert.Null(vm.AddSimple(4, 2, 5, 1));
        AnimNote(vm, vm.AddSimple(1, 2, 5, 1));
        AnimNote(vm, vm.AddSimple(0, 5, 10, 3));
        Assert.Null(vm.AddSimple(7, 5, 1, 0));

        vm.Lumps["ANIMATED"] = 6;
        vm.Bytes[6] = new byte[] { 1, 2, 3 };
        vm.InitAnimated();

        vm.Lumps.Remove("ANIMATED");
        vm.InitAnimated();

        vm.Lumps["ANIMATED"] = 8;
        vm.Bytes[8] = Array.Empty<byte>();
        vm.InitAnimated();

        vm.Lumps["ANIMATED"] = 9;
        vm.Bytes[9] = Rec(1, "aaa", "aaa", 35);
        vm.InitAnimated();

        vm.Lumps["ANIMATED"] = 11;
        vm.Bytes[11] = new byte[] { 0xFF };
        vm.InitAnimated();

        var dash = new List<byte>();
        dash.AddRange(Rec(1, "AAA", "-", 35));
        dash.Add(0xFF);
        vm.Lumps["ANIMATED"] = 12;
        vm.Bytes[12] = dash.ToArray();
        vm.InitAnimated();

        vm.Lumps["ANIMATED"] = 13;
        vm.Bytes[13] = null!;
        vm.InitAnimated();

        return string.Join('\n', vm.Log) + "\n";
    }

    static void AnimNote(VmTextureAnim1250 vm, VmTextureAnim1250.Anim? anim)
    {
        if (anim == null)
        {
            vm.Log.Add("direct null");
            return;
        }
        VmTextureAnim1250.Frame frame = anim.Frames[0];
        vm.Log.Add($"direct base={anim.Base} count={anim.Count} type={anim.Type} discrete={(anim.Discrete ? 1 : 0)} speed={frame.SpeedMin} range={frame.SpeedRange} pic={frame.Pic} nframe={anim.Frames.Length}");
    }

    static byte[] Rec(byte flags, string endName, string startName, uint speed)
    {
        var raw = new byte[23];
        raw[0] = flags;
        WriteName(raw, 1, endName);
        WriteName(raw, 10, startName);
        raw[19] = (byte)speed;
        raw[20] = (byte)(speed >> 8);
        raw[21] = (byte)(speed >> 16);
        raw[22] = (byte)(speed >> 24);
        return raw;
    }

    static void WriteName(byte[] raw, int at, string name)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(name);
        int n = Math.Min(8, bytes.Length);
        Array.Copy(bytes, 0, raw, at, n);
    }
}
