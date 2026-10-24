namespace HCDE.Client.Tests;

public class VmFindFileWildcardTests
{
    [Fact]
    public void ApplyWildcardExtensionFilter_AppendsExtension()
    {
        Assert.Equal("mod_*.wad", VmFindFileWildcard.ApplyWildcardExtensionFilter("mod_*", "*.wad"));
        Assert.Equal("*.wad", VmFindFileWildcard.ApplyWildcardExtensionFilter("*", "*.wad"));
    }

    [Fact]
    public void ContainsDuplicate_IsCaseInsensitive()
    {
        Assert.True(VmFindFileWildcard.ContainsDuplicate(["foo/bar.WAD"], "foo/bar.wad"));
    }
}
