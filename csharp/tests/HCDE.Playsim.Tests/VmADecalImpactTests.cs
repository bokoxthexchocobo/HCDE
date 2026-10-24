namespace HCDE.Playsim.Tests;

public class VmADecalImpactTests
{
    [Fact]
    public void CheckMax_TrimsWhenAtCap()
    {
        var count = 999;
        var destroyed = false;
        VmADecalImpact.CheckMax(() => count, 1000, v => count = v, () => { destroyed = true; return new VmADecalBase.DecalState(); });
        Assert.True(destroyed);
        Assert.Equal(999, count);
    }

    [Fact]
    public void Expired_DecrementsCount()
    {
        var count = 5;
        VmADecalImpact.Expired(() => count, v => count = v);
        Assert.Equal(4, count);
    }

    [Fact]
    public void CloneSelf_CopiesRenderMask()
    {
        var src = new VmADecalBase.DecalState { AlphaColor = 7, RenderFlags = 0x00ff00aa };
        var ops = new VmADecalImpact.CloneSelfOps
        {
            CreateThinker = _ => new VmADecalBase.DecalState(),
            StickToWallValid = (_, _, _, _, _) => true,
            ApplyTemplate = (_, _, _) => { },
        };
        var clone = VmADecalImpact.CloneSelf(src, null, 1, 2, 3, new object(), null, ops);
        Assert.NotNull(clone);
        Assert.Equal(7u, clone!.AlphaColor);
    }

    [Fact]
    public void StaticCreate_SpreadDisabled_SkipsSpreadBranch()
    {
        var wall = new object();
        var decal = new VmADecalBase.DecalState { PicNum = 1, ScaleX = 1 };
        var ctx = new VmADecalImpact.StaticCreateContext
        {
            ClMaxDecals = 1,
            ClSpreadDecals = false,
            Wall = wall,
            CreateImpactThinker = _ => decal,
            StickValid = (_, _, _, _, _) => true,
            PicNumValid = _ => true,
        };
        Assert.Same(decal, VmADecalImpact.StaticCreateByTemplate(new object(), ctx));
    }

    [Fact]
    public void ImpactCloneSelf_RejectsNoAutodecalsWall()
    {
        var ops = new VmADecalImpact.ImpactCloneSelfOps { GetWallFlags = _ => VmADecalImpact.WallfNoautodecals };
        var result = VmADecalImpact.ImpactCloneSelf(new VmADecalBase.DecalState(), null, 0, 0, 0, new object(), null, ops);
        Assert.Null(result);
    }
}
