using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmHwMaterial1242SliceTests
{
    const string HeadGolden =
"""
brightmap skip
mat id=1 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=2 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
brightmap skip
mat id=2 shader=0 flags=65536 scale=0 n=4 slot=0
L i=0 tex=3 scale=0 clamp=-1
L i=1 tex=4 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
pick white=1
brightmap created id=6
mat id=3 shader=0 flags=65536 scale=0 n=4 slot=0
L i=0 tex=5 scale=0 clamp=-1
L i=1 tex=6 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
brightmap checked
mat id=4 shader=0 flags=65536 scale=0 n=4 slot=0
L i=0 tex=5 scale=0 clamp=-1
L i=1 tex=6 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
brightmap skip
mat id=5 shader=3 flags=0 scale=0 n=6 slot=0
L i=0 tex=7 scale=0 clamp=-1
L i=1 tex=8 scale=0 clamp=-1
L i=2 tex=9 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
L i=4 tex=1 scale=0 clamp=-1
L i=5 tex=1 scale=0 clamp=-1
brightmap skip
mat id=6 shader=4 flags=0 scale=0 n=8 slot=0
L i=0 tex=10 scale=0 clamp=-1
L i=1 tex=11 scale=0 clamp=-1
L i=2 tex=12 scale=0 clamp=-1
L i=3 tex=13 scale=0 clamp=-1
L i=4 tex=14 scale=0 clamp=-1
L i=5 tex=1 scale=0 clamp=-1
L i=6 tex=1 scale=0 clamp=-1
L i=7 tex=1 scale=0 clamp=-1
brightmap skip
mat id=7 shader=2 flags=0 scale=0 n=4 slot=0
L i=0 tex=15 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
mat id=8 shader=5 flags=0 scale=4 n=1 slot=4
L i=0 tex=18 scale=4 clamp=-1
mat id=9 shader=5 flags=0 scale=0 n=1 slot=0
L i=0 tex=19 scale=0 clamp=-1
mat id=10 shader=20 flags=0 scale=0 n=1 slot=0
L i=0 tex=20 scale=0 clamp=9
brightmap skip
user shader=15
mat id=11 shader=15 flags=0 scale=0 n=5 slot=0
L i=0 tex=22 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
L i=4 tex=23 scale=0 clamp=0
brightmap skip
user skip type=3 have=0
mat id=12 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=24 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
brightmap skip
mat id=13 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=25 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
brightmap skip
mat id=14 shader=0 flags=393216 scale=1 n=4 slot=1
L i=0 tex=26 scale=1 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=27 scale=0 clamp=0
L i=3 tex=28 scale=1 clamp=-1
brightmap skip
mat id=15 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=29 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
pick white=1
brightmap none
mat id=16 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=30 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1

"""
;

    const string TailGolden =
"""
---- edge
pick white=4
brightmap created id=32
mat id=17 shader=0 flags=65536 scale=0 n=4 slot=0
L i=0 tex=31 scale=0 clamp=-1
L i=1 tex=32 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
validate null
validate invalid
mat id=18 shader=5 flags=0 scale=4 n=1 slot=4
L i=0 tex=34 scale=4 clamp=-1
validate flags=4 id=18
validate flags=2 id=-1
validate flags=0 id=-1
validate oob flags=8
callback set
layer i=0 trans=-1 tex=34 scale=4 hw=34
layer i=1 callback trans=7 clamp=3 hw=70
layer i=3 indexed none
layer i=1 trans=9 tex=1 scale=0 hw=1
layer oob i=8
brightmap skip
placeholder missing
placeholder missing
placeholder missing
mat id=19 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=35 scale=0 clamp=-1
L i=1 tex=0 scale=0 clamp=-1
L i=2 tex=0 scale=0 clamp=-1
L i=3 tex=0 scale=0 clamp=-1
brightmap skip
shader oob 17
mat id=20 shader=0 flags=0 scale=0 n=4 slot=0
L i=0 tex=36 scale=0 clamp=-1
L i=1 tex=1 scale=0 clamp=-1
L i=2 tex=1 scale=0 clamp=-1
L i=3 tex=1 scale=0 clamp=-1
slot oob flags=5
mat id=21 shader=5 flags=0 scale=5 n=1 slot=-1
L i=0 tex=37 scale=5 clamp=-1
delete id=18 slot=4
still True

"""
;

    [Fact]
    public void Build_ChoosesShaderLayersAndBrightmap()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("brightmap skip\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=0 flags=0 scale=0 n=4 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("flags=65536 scale=0 n=4 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("pick white=1\n", head, StringComparison.Ordinal);
        Assert.Contains("brightmap created id=", head, StringComparison.Ordinal);
        Assert.Contains("brightmap checked\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=3 flags=0 scale=0 n=6 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=4 flags=0 scale=0 n=8 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=2 flags=0 scale=0 n=4 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=5 flags=0 scale=4 n=1 slot=4\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=5 flags=0 scale=0 n=1 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("shader=20 flags=0 scale=0 n=1 slot=0\n", head, StringComparison.Ordinal);
        Assert.Contains("clamp=9\n", head, StringComparison.Ordinal);
        Assert.Contains("user shader=15\n", head, StringComparison.Ordinal);
        Assert.Contains("clamp=0\n", head, StringComparison.Ordinal);
        Assert.Contains("user skip type=3 have=0\n", head, StringComparison.Ordinal);
        Assert.Contains("flags=393216 scale=1 n=4 slot=1\n", head, StringComparison.Ordinal);
        Assert.Contains("brightmap none\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Query_ValidatesFlagsAndRoutesIndexedLayers()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("pick white=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("validate null\n", tail, StringComparison.Ordinal);
        Assert.Contains("validate invalid\n", tail, StringComparison.Ordinal);
        Assert.Contains("validate flags=4 id=", tail, StringComparison.Ordinal);
        Assert.Contains("validate flags=2 id=-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("validate flags=0 id=", tail, StringComparison.Ordinal);
        Assert.Contains("validate oob flags=8\n", tail, StringComparison.Ordinal);
        Assert.Contains("layer i=0 trans=-1 tex=", tail, StringComparison.Ordinal);
        Assert.Contains("layer i=1 callback trans=7 clamp=3 hw=70\n", tail, StringComparison.Ordinal);
        Assert.Contains("layer i=3 indexed none\n", tail, StringComparison.Ordinal);
        Assert.Contains("layer i=1 trans=9 tex=1 scale=0 hw=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("layer oob i=8\n", tail, StringComparison.Ordinal);
        Assert.Contains("placeholder missing\n", tail, StringComparison.Ordinal);
        Assert.Contains("shader oob 17\n", tail, StringComparison.Ordinal);
        Assert.Contains("slot oob flags=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("delete id=", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmHwMaterial1242();
        VmHwMaterial1242.Tex hold = vm.NewTex();
        vm.Games[1] = hold;

        VmHwMaterial1242.Tex plain = vm.NewTex();
        vm.Build(plain, 0);

        VmHwMaterial1242.Tex marked = vm.NewTex();
        marked.Bright = vm.NewTex();
        vm.Build(marked, 0);

        VmHwMaterial1242.Tex auto = vm.NewTex();
        auto.HasGlobalBrightmap = true;
        auto.Pixels = new byte[] { 5 };
        auto.Remap = new byte[256];
        auto.Remap[5] = 1;
        vm.Build(auto, 0);
        vm.Build(auto, 0);

        VmHwMaterial1242.Tex spec = vm.NewTex();
        spec.HasLayers = true;
        spec.Normal = vm.NewTex();
        spec.Specular = vm.NewTex();
        vm.Build(spec, 0);

        VmHwMaterial1242.Tex pbr = vm.NewTex();
        pbr.HasLayers = true;
        pbr.Normal = vm.NewTex();
        pbr.Metallic = vm.NewTex();
        pbr.Roughness = vm.NewTex();
        pbr.Ao = vm.NewTex();
        vm.Build(pbr, 0);

        VmHwMaterial1242.Tex warp = vm.NewTex();
        warp.Warped = 2;
        warp.HasLayers = true;
        warp.Normal = vm.NewTex();
        warp.Specular = vm.NewTex();
        vm.Build(warp, 0);

        VmHwMaterial1242.Tex indexed = vm.NewTex();
        vm.Build(indexed, VmHwMaterial1242.Indexed);

        VmHwMaterial1242.Tex sw = vm.NewTex(VmHwMaterial1242.UseSwCanvas);
        sw.ColorFormat = 0;
        vm.Build(sw, 0);

        VmHwMaterial1242.Tex canvas = vm.NewTex();
        canvas.HardwareCanvas = true;
        canvas.ColorFormat = 1;
        canvas.ShaderIndex = 20;
        canvas.HasLayers = true;
        canvas.Normal = vm.NewTex();
        vm.Build(canvas, 0);

        vm.UserTypes.Add(VmHwMaterial1242.ShaderDefault);
        VmHwMaterial1242.Tex custom = vm.NewTex();
        custom.ShaderIndex = VmHwMaterial1242.FirstUserShader;
        custom.HasLayers = true;
        custom.Custom.Add(null);
        custom.Custom.Add(vm.NewTex());
        vm.Build(custom, 0);

        vm.UserTypes.Add(VmHwMaterial1242.ShaderSpecular);
        VmHwMaterial1242.Tex mismatch = vm.NewTex();
        mismatch.ShaderIndex = VmHwMaterial1242.FirstUserShader + 1;
        vm.Build(mismatch, 0);

        vm.CustomShader = false;
        VmHwMaterial1242.Tex blocked = vm.NewTex();
        blocked.ShaderIndex = VmHwMaterial1242.FirstUserShader;
        vm.Build(blocked, 0);
        vm.CustomShader = true;

        VmHwMaterial1242.Tex maps = vm.NewTex();
        maps.HasLayers = true;
        maps.Detail = vm.NewTex();
        maps.Glow = vm.NewTex();
        vm.Build(maps, VmHwMaterial1242.Expand);

        VmHwMaterial1242.Tex dark = vm.NewTex(VmHwMaterial1242.UseDecal);
        dark.HasGlobalBrightmap = true;
        dark.Pixels = new byte[] { 1 };
        dark.Remap = new byte[] { 0, 1 };
        vm.Build(dark, 0);

        VmHwMaterial1242.Tex empty = vm.NewTex();
        empty.HasGlobalBrightmap = true;
        empty.Pixels = new byte[] { 3 };
        empty.Remap = new byte[256];
        empty.Remap[3] = 2;
        vm.Build(empty, 0);

        vm.Log.Add("---- edge");

        vm.PalR = new byte[256];
        vm.PalG = new byte[256];
        vm.PalB = new byte[256];
        vm.PalR[4] = 255;
        vm.PalG[4] = 255;
        vm.PalB[4] = 255;
        VmHwMaterial1242.Tex lit = vm.NewTex();
        lit.HasGlobalBrightmap = true;
        lit.Pixels = new byte[] { 9 };
        lit.Remap = new byte[256];
        lit.Remap[9] = 4;
        vm.Build(lit, 0);

        vm.Validate(null, 0, true);
        VmHwMaterial1242.Tex dead = vm.NewTex(VmHwMaterial1242.UseNull);
        vm.Validate(dead, 0, true);

        VmHwMaterial1242.Tex made = vm.NewTex();
        vm.Validate(made, VmHwMaterial1242.Indexed | VmHwMaterial1242.Expand, true);
        made.ExpandSprites = false;
        vm.Validate(made, VmHwMaterial1242.Expand | 2, false);
        vm.Validate(made, 0, false);
        vm.Validate(made, 8, true);

        vm.SetCallback((layer, trans) => 70);
        VmHwMaterial1242.Mat indexedMat = made.Material[VmHwMaterial1242.Indexed]!;
        vm.GetLayer(indexedMat, 0, 3, out _);
        vm.GetLayer(indexedMat, 1, 7, out _);
        vm.GetLayer(indexedMat, 3, 7, out _);
        vm.GetLayer(plain.Material[0]!, 1, 9, out _);
        vm.GetLayer(plain.Material[0]!, 8, 0, out _);

        vm.Games.Remove(1);
        VmHwMaterial1242.Tex bare = vm.NewTex();
        vm.Build(bare, 0);
        vm.Games[1] = hold;

        VmHwMaterial1242.Tex missingShader = vm.NewTex();
        missingShader.ShaderIndex = 17;
        vm.Build(missingShader, 0);
        vm.Build(vm.NewTex(), 5);

        vm.Delete(indexedMat);
        vm.Log.Add("still " + (made.Material[VmHwMaterial1242.Indexed] == indexedMat));

        string dump = string.Join("\n", vm.Log) + "\n";
        return dump;
    }
}
