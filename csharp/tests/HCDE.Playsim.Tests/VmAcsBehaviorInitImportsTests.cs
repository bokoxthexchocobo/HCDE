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
}
