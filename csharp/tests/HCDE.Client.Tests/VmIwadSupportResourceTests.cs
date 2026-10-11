namespace HCDE.Client.Tests;

public class VmIwadSupportResourceTests
{
    [Fact]
    public void RecursiveSearchChecksRootsAndNestedRelativeNamesAfterNormalDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-support-tree-" + Guid.NewGuid().ToString("N"));
        var normal = Directory.CreateDirectory(Path.Combine(root, "normal")).FullName;
        var tree = Directory.CreateDirectory(Path.Combine(root, "tree")).FullName;
        var nested = Directory.CreateDirectory(Path.Combine(tree, "one", "two", "resources")).FullName;
        try
        {
            var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test SupportWAD = \"resources/support.wad\" }").Entries);
            var support = Path.Combine(nested, "support.wad");
            File.WriteAllBytes(support, []);
            Assert.Null(VmIwadSupportResource.Resolve(entry, [tree]));
            Assert.Equal(support, VmIwadSupportResource.Resolve(entry, [], recursiveDirectories: [tree]));
            var rootResources = Directory.CreateDirectory(Path.Combine(tree, "resources")).FullName;
            var rootSupport = Path.Combine(rootResources, "support.wad");
            File.WriteAllBytes(rootSupport, []);
            Assert.Equal(rootSupport, VmIwadSupportResource.Resolve(entry, [], recursiveDirectories: [tree]));
            var normalResources = Directory.CreateDirectory(Path.Combine(normal, "resources")).FullName;
            var normalSupport = Path.Combine(normalResources, "support.wad");
            File.WriteAllBytes(normalSupport, []);
            Assert.Equal(normalSupport, VmIwadSupportResource.Resolve(entry, [normal], recursiveDirectories: [tree]));
            Assert.Null(VmIwadSupportResource.Resolve(entry, [normal], enabled: false, recursiveDirectories: [tree]));
            Assert.Null(VmIwadSupportResource.Resolve(entry, [normal], networkGame: true, recursiveDirectories: [tree]));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void SearchOrderMissingResourcesAndNativeNetworkGate()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-support-" + Guid.NewGuid().ToString("N"));
        var first = Path.Combine(root, "first");
        var second = Path.Combine(root, "second");
        Directory.CreateDirectory(first);
        Directory.CreateDirectory(second);
        try
        {
            var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test SupportWAD = support.wad }").Entries);
            Assert.Null(VmIwadSupportResource.Resolve(entry, [first, second]));
            File.WriteAllBytes(Path.Combine(second, "support.wad"), []);
            Assert.Equal(Path.Combine(second, "support.wad"), VmIwadSupportResource.Resolve(entry, [first, second]));
            File.WriteAllBytes(Path.Combine(first, "support.wad"), []);
            Assert.Equal(Path.Combine(first, "support.wad"), VmIwadSupportResource.Resolve(entry, [first, second]));
            Assert.Null(VmIwadSupportResource.Resolve(entry, [first], enabled: false));
            Assert.Null(VmIwadSupportResource.Resolve(entry, [first], networkGame: true));
            var noSupport = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test }").Entries);
            Assert.Null(VmIwadSupportResource.Resolve(noSupport, [first]));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
