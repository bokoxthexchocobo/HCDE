namespace HCDE.Gamedata.Tests;

public class VmDecalLibCoreTests
{
    [Fact]
    public void DecalGroup_PickDecal_UsesWeights()
    {
        var group = new VmDecalGroup();
        var a = new VmDecalBase { Name = "A" };
        var b = new VmDecalBase { Name = "B" };
        group.AddDecal(a, 1);
        group.AddDecal(b, 9);
        Assert.Equal("B", group.PickDecal((_, _) => 8)?.Name);
    }

    [Fact]
    public void Clear_ResetsRoot()
    {
        var lib = new VmDecalLibCore { Root = new VmDecalTreeNode(new VmDecalDefinition { Name = "x" }) };
        lib.Clear();
        Assert.Null(lib.Root);
    }
}
