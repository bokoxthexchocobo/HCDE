namespace HCDE.Playsim.Tests;

public class VmModel1249SliceTests
{
    const string HeadGolden =
"""
gfx exact models/skin.png lump=4
gfx strip models/bare lump=9
skin lump=9 name=models/bare.tga flags=129 id=3
model kind=ue1 idx=0 path=models/ file=marine_d.3d
model kind=obj idx=1 path=models/ file=crate.obj
model kind=iqm idx=2 path= file=guy.iqm

""";

    const string TailGolden =
"""
---- edge
FindModel: 'models/missing.md2' not found
LoadModel: File to large: 'models/big.md2'
LoadModel: Unknown model format in 'models/nope.bin'
voxel own=1
model kind=voxel idx=3 path=models/ file=nope.bin
load fail md2
model kind=dmd idx=4 path=models/ file=lump0_d.3d
flush i=4
flush i=3
flush i=2
flush i=1
flush i=0
model kind=md3 idx=5 path=models/ file=box.md3
file null
skin null

""";

    [Fact]
    public void Find_DispatchesMagicAndKeepsTheHigherSkinLump()
    {
        string dump = Build(out int skin, out int obj, out int again);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("gfx exact models/skin.png lump=4\n", head, StringComparison.Ordinal);
        Assert.Contains("gfx strip models/bare lump=9\n", head, StringComparison.Ordinal);
        Assert.Contains("skin lump=9 name=models/bare.tga flags=129 id=3\n", head, StringComparison.Ordinal);
        Assert.Contains("model kind=ue1 idx=0 path=models/ file=marine_d.3d\n", head, StringComparison.Ordinal);
        Assert.Contains("model kind=obj idx=1 path=models/ file=crate.obj\n", head, StringComparison.Ordinal);
        Assert.Contains("model kind=iqm idx=2 path= file=guy.iqm\n", head, StringComparison.Ordinal);
        Assert.Equal(3, skin);
        Assert.Equal(1, obj);
        Assert.Equal(1, again);
        Assert.Equal(0xCBF43926u, VmModel1249.FrameHash("123456789"u8));
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Find_RejectsAMissingFileAndAFailedLoad()
    {
        string dump = Build(out _, out _, out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("FindModel: 'models/missing.md2' not found\n", tail, StringComparison.Ordinal);
        Assert.Contains("LoadModel: File to large: 'models/big.md2'\n", tail, StringComparison.Ordinal);
        Assert.Contains("LoadModel: Unknown model format in 'models/nope.bin'\n", tail, StringComparison.Ordinal);
        Assert.Contains("voxel own=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("load fail md2\n", tail, StringComparison.Ordinal);
        Assert.Contains("flush i=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("model kind=dmd idx=4 path=models/ file=lump0_d.3d\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out int skin, out int obj, out int again)
    {
        var vm = new VmModel1249();
        vm.Lumps["models/skin.png"] = 4;
        vm.Lumps["models/bare.png"] = 2;
        vm.Lumps["models/bare.jpg"] = 8;
        vm.Lumps["models/bare.tga"] = 9;
        vm.Lumps["models/bare.pcx"] = 1;
        vm.Names[9] = "models/bare.tga";
        vm.Textures["models/bare.tga"] = 3;
        string exact = "models/skin.png";
        Assert.Equal(4, vm.FindGfx(ref exact));
        skin = vm.LoadSkin("models/", "bare.xyz");

        vm.Lumps["models/marine_d.3d"] = 10;
        vm.Lumps["models/marine_a.3d"] = 11;
        vm.Bytes[10] = new byte[4];
        Assert.Equal(0, vm.Find("models/", "marine_d.3d"));

        vm.Lumps["models/crate.obj"] = 12;
        vm.Bytes[12] = new byte[] { 1 };
        obj = vm.Find("models/", "crate.obj");
        vm.Lumps["models/CRATE.obj"] = 12;
        again = vm.Find("models/", "CRATE.obj");

        vm.Lumps["guy.iqm"] = 13;
        var iqm = new byte[16];
        "INTERQUAKEMODEL"u8.CopyTo(iqm);
        vm.Bytes[13] = iqm;
        Assert.Equal(2, vm.Find(null, "guy.iqm", silent: true));

        vm.Log.Add("---- edge");
        Assert.Equal(-1, vm.Find("models/", "missing.md2"));
        vm.Lumps["models/big.md2"] = 14;
        vm.Bytes[14] = "IDP2"u8.ToArray();
        vm.ForcedLength = 0x80000000L;
        Assert.Equal(-1, vm.Find("models/", "big.md2"));
        vm.ForcedLength = null;

        vm.Lumps["models/nope.bin"] = 15;
        vm.Bytes[15] = new byte[] { 1, 2, 3, 4 };
        Assert.Equal(-1, vm.Find("models/", "nope.bin"));
        vm.VoxelOk = true;
        Assert.Equal(3, vm.Find("models/", "nope.bin"));
        vm.VoxelOk = false;

        vm.Lumps["models/bad.md2"] = 16;
        vm.Bytes[16] = "IDP2"u8.ToArray();
        vm.LoadOk = false;
        Assert.Equal(-1, vm.Find("models/", "bad.md2"));
        vm.LoadOk = true;

        vm.Lumps["models/lump0_d.3d"] = 17;
        vm.Lumps["models/lump0_a.3d"] = 0;
        vm.Bytes[17] = "DMDM"u8.ToArray();
        Assert.Equal(4, vm.Find("models/", "lump0_d.3d"));
        Assert.Equal("dmd", vm.Models[4].Kind);

        vm.Models[0].Buffers[0] = "buf";
        vm.Models[1].Buffers[2] = "buf";
        vm.Flush();
        Assert.Null(vm.Models[0].Buffers[0]);
        Assert.Null(vm.Models[1].Buffers[2]);
        Assert.Equal(5, vm.Models.Count);

        vm.Lumps["models/box.md3"] = 18;
        vm.Bytes[18] = "IDP3"u8.ToArray();
        Assert.Equal(5, vm.Find("models/", "box.md3"));
        Assert.Equal(-1, vm.Find(null, null));
        Assert.Equal(-1, vm.LoadSkin(null, "x"));

        string dump = string.Join('\n', vm.Log) + "\n";
        return dump;
    }
}
