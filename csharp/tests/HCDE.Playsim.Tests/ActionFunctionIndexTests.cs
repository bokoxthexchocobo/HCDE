using System.Text.Json;

namespace HCDE.Playsim.Tests;

public class ActionFunctionIndexTests
{
    [Fact]
    public void ActionFunctionIndexListsCoreWeaponActions()
    {
        var path = Path.Combine(RepoRoot(), "csharp/docs/data/action_function_index.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var names = document.RootElement.GetProperty("names").EnumerateArray().Select(element => element.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.True(document.RootElement.GetProperty("count").GetInt32() > 100);
        Assert.Contains("GetCrouchFactor", names);
        Assert.Contains("GetCVar", names);
    }

    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "csharp")))
            dir = Directory.GetParent(dir)?.FullName;
        return dir ?? throw new InvalidOperationException("Repository root not found.");
    }
}
