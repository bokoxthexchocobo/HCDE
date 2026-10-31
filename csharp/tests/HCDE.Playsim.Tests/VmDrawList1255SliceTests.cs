namespace HCDE.Playsim.Tests;

public class VmDrawList1255SliceTests
{
    const string WallGolden =
"""
wall list=6 type=3 maskedtex=1
wall list=4 type=3 maskedtex=0
wall list=2 type=3 maskedtex=1
wall list=0 type=1 maskedtex=1
wall list=0 type=7 maskedtex=0
normal 1.000000 0.000000 0.000000
makevertices 0
lights
decals
mirror list=7 vert=0 light=-1 u=1.000000 v=0.000000
normal 0.000000 0.000000 -1.000000
makevertices 0
decals
mirror list=7 vert=0 light=-1 u=0.000000 v=-1.000000
normal 1.000000 0.000000 0.000000
makevertices 0
decals
mirror list=7 vert=0 light=-1 u=1.000000 v=0.000000

""";

    const string FlatGolden =
"""
flat list=6 fog=0 alpha=1.000000
flat list=1 fog=0 alpha=1.000000
flat list=6 fog=0 alpha=1.000000
flat list=7 fog=1 alpha=1.000000
flat list=7 fog=0 alpha=1.000000
flat list=7 fog=0 alpha=1.000000
flat list=7 fog=0 alpha=1.000000
flat list=1 fog=0 alpha=1.000000
flat list=3 fog=0 alpha=1.000000
flat list=1 fog=0 alpha=1.000000
sprite list=5 model=0 flags=4096
sprite list=6 model=1 flags=4096
sprite list=6 model=1 flags=0
sprite list=5 model=1 flags=0
sprite list=6 model=0 flags=0

""";

    [Fact]
    public void Wall_RoutesSkyhackAndCopiesTheMirrorNormal()
    {
        var vm = new VmDrawList1255();
        var masked = new VmDrawList1255.Texture { Masked = true, Translucent = true };
        var solid = new VmDrawList1255.Texture { Masked = false };
        var trans = new VmDrawList1255.Wall { Flags = VmDrawList1255.TranslucentFlag | VmDrawList1255.SkyHack, Type = VmDrawList1255.RenderM2S, Texture = masked };
        vm.AddWall(trans);
        trans.Type = 99;
        vm.AddWall(new VmDrawList1255.Wall { Flags = VmDrawList1255.SkyHack, Type = VmDrawList1255.RenderM2S, Texture = solid });
        vm.AddWall(new VmDrawList1255.Wall { Type = VmDrawList1255.RenderM2S, Texture = masked });
        vm.AddWall(new VmDrawList1255.Wall { Type = VmDrawList1255.RenderTop, Texture = masked });
        vm.AddWall(new VmDrawList1255.Wall { Type = VmDrawList1255.RenderM2Snf });
        Assert.Equal(VmDrawList1255.RenderM2S, vm.Walls[VmDrawList1255.Translucent][0].Type);
        Assert.Single(vm.Walls[VmDrawList1255.MaskedWallsOfs]);
        Assert.Single(vm.Walls[VmDrawList1255.MaskedWalls]);
        Assert.Equal(2, vm.Walls[VmDrawList1255.PlainWalls].Count);
        var mirror = new VmDrawList1255.Wall { HasDecals = true, X1 = 0, Y1 = 0, X2 = 0, Y2 = 2, VertCount = 4 };
        vm.AddMirror(mirror);
        Assert.Equal(VmDrawList1255.RenderMirror, mirror.Type);
        Assert.Equal(4, mirror.VertCount);
        Assert.Equal(0, vm.Walls[VmDrawList1255.TranslucentBorder][0].VertCount);
        Assert.Equal(-1, vm.Walls[VmDrawList1255.TranslucentBorder][0].DynLightIndex);
        Assert.Equal(1f, vm.Walls[VmDrawList1255.TranslucentBorder][0].U[3]);
        Assert.Equal(0f, vm.Walls[VmDrawList1255.TranslucentBorder][0].V[2]);
        vm.FullBright = true;
        vm.AddMirror(new VmDrawList1255.Wall { HasDecals = true, X1 = 0, Y1 = 0, X2 = 3, Y2 = 0 });
        vm.FullBright = false;
        vm.HasDynamicLights = false;
        vm.AddMirror(new VmDrawList1255.Wall { HasDecals = false, X1 = 0, Y1 = 0, X2 = 0, Y2 = 2 });
        Assert.Equal(3, vm.Walls[VmDrawList1255.TranslucentBorder].Count);
        string dump = string.Join('\n', vm.Log) + "\n";
        Assert.Contains("lights\n", dump, StringComparison.Ordinal);
        Assert.Contains("normal 0.000000 0.000000 -1.000000\n", dump, StringComparison.Ordinal);
        Assert.Equal(WallGolden, dump);
    }

    [Fact]
    public void Flat_KeepsAFullAlphaAndSendsModelsToTheirOwnList()
    {
        var vm = new VmDrawList1255();
        var tex = new VmDrawList1255.Texture { Masked = true, Translucent = true };
        var opaque = new VmDrawList1255.Texture { Masked = true, Translucent = false };
        float edge = 1f - VmDrawList1255.FltEpsilon;
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = tex, RenderFlags = VmDrawList1255.Render3dPlanes }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = edge, Texture = tex }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = MathF.BitDecrement(edge), Texture = tex, RenderFlags = VmDrawList1255.Render3dPlanes }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = tex }, true);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = null }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = tex, RenderStyle = 1 }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = tex, Stack = true }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = tex, RenderFlags = VmDrawList1255.Render3dPlanes, Slope = true }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = opaque, Stack = true }, false);
        vm.AddFlat(new VmDrawList1255.Flat { Alpha = 1f, Texture = opaque }, false);
        vm.AddSprite(new VmDrawList1255.Sprite { RenderFlags = VmDrawList1255.WallSprite }, false);
        vm.AddSprite(new VmDrawList1255.Sprite { RenderFlags = VmDrawList1255.WallSprite, ModelFrame = true }, true);
        vm.AddSprite(new VmDrawList1255.Sprite { Actor = null, ModelFrame = true }, false);
        vm.AddSprite(new VmDrawList1255.Sprite { ModelFrame = true }, false);
        vm.AddSprite(new VmDrawList1255.Sprite(), false);
        Assert.Equal(edge, vm.Flats[VmDrawList1255.PlainFlats][0].Alpha);
        Assert.Equal(2, vm.Flats[VmDrawList1255.Translucent].Count);
        Assert.Equal(2, vm.Sprites[VmDrawList1255.Models].Count);
        Assert.Equal(3, vm.Sprites[VmDrawList1255.Translucent].Count);
        Assert.Equal(FlatGolden, string.Join('\n', vm.Log) + "\n");
    }
}
