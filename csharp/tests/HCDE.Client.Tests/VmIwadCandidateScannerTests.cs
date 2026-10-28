namespace HCDE.Client.Tests;

public class VmIwadCandidateScannerTests
{
    [Fact]
    public void NativeNamesAndSpecialExtensionsSelectCandidatesWithoutReadingFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-candidates-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "folder.ipk3"));
        try
        {
            foreach (var name in new[] { "DOOM.WAD", "game.IPK3", "other.ipk7", "custom.iwad", "unknown.wad" })
                File.WriteAllBytes(Path.Combine(root, name), [255]);
            using var locked = new FileStream(Path.Combine(root, "DOOM.WAD"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var candidates = VmIwadCandidateScanner.Scan(root, ["", "doom.wad", "game.ipk3"]);
            Assert.Equal(5, candidates.Count);
            Assert.Contains(Path.Combine(root, "DOOM.WAD"), candidates);
            Assert.Equal(2, candidates.Count(path => Path.GetFileName(path) == "game.IPK3"));
            Assert.Contains(Path.Combine(root, "other.ipk7"), candidates);
            Assert.Contains(Path.Combine(root, "custom.iwad"), candidates);
            Assert.DoesNotContain(Path.Combine(root, "unknown.wad"), candidates);
            Assert.DoesNotContain(Path.Combine(root, "folder.ipk3"), candidates);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void RecursiveScanningAndHiddenFilesFollowDirectoryRules()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var root = Path.Combine(Path.GetTempPath(), "hcde-recursive-candidates-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        try
        {
            var nested = Path.Combine(root, "nested", "game.ipk3");
            var hidden = Path.Combine(root, "hidden.ipk7");
            File.WriteAllBytes(nested, []);
            File.WriteAllBytes(hidden, []);
            File.SetAttributes(hidden, FileAttributes.Hidden);
            Assert.Empty(VmIwadCandidateScanner.Scan(root, []));
            Assert.Equal(nested, Assert.Single(VmIwadCandidateScanner.Scan(root, [], recursive: true)));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void MissingDirectoriesAreReported()
    {
        Assert.Throws<DirectoryNotFoundException>(() => VmIwadCandidateScanner.Scan(
            Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid().ToString("N")), []));
    }
}
