namespace HCDE.ConversionInventory.Tests;

public class ConversionMatrixTests
{
    [Fact]
    public void GeneratedMatrixIncludesCoreCategories()
    {
        var root = FindRepositoryRoot();
        var matrix = HCDE.ConversionInventory.SupportMatrixBuilder.Build(root);
        var categories = matrix.Summaries.Select(summary => summary.Category).ToHashSet();
        Assert.Contains("acs_opcode", categories);
        Assert.Contains("line_special", categories);
        Assert.Contains("zscript_vm_opcode", categories);
        Assert.True(matrix.Entries.Count > 1000);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "playsim")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("HCDE repository root not found.");
    }
}
