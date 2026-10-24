namespace HCDE.Playsim.Tests;

public class VmAcsBehaviorInitImportsTests
{
    [Fact]
    public void ShouldResolveImportFunction_WhenAddressAndImportZero() =>
        Assert.True(VmAcsBehaviorInitImports.ShouldResolveImportFunction(0, 0));

    [Fact]
    public void ImportFunctionMatches_WhenLibDefinesBody() =>
        Assert.True(VmAcsBehaviorInitImports.ImportFunctionMatches(libAddress: 1, libImportNum: 0));

    [Fact]
    public void ResolveImportMapVarNative_OrLibraryIdWhenUnset() =>
        Assert.Equal(9, VmAcsBehaviorInitImports.ResolveImportMapVarNative(0, 1, libraryId: 8));

    [Fact]
    public void RegisterLoadedImportNative_PushesWhenNonNull()
    {
        var count = 0;
        VmAcsBehaviorInitImports.RegisterLoadedImportNative(new object(), _ => count++);
        Assert.Equal(1, count);
    }

    [Fact]
    public void ReleaseBehaviorNative_ReleasesInOrder()
    {
        var order = new List<string>();
        VmAcsBehaviorInitImports.ReleaseBehaviorNative(
            hasScripts: true,
            hasArrays: true,
            hasArrayStore: true,
            () => order.Add("scripts"),
            () => order.Add("arrays"),
            () => order.Add("store"));
        Assert.Equal(["scripts", "arrays", "store"], order);
    }
}
