namespace HCDE.Playsim.Tests;

public class VmPDestructibleSerializeScriptTests
{
    [Fact]
    public void ApplyReadGroup_UpdatesExistingGroupHealth()
    {
        var groups = new Dictionary<int, VmPDestructibleHealthGroups.HealthGroup>
        {
            [3] = new() { Id = 3, Health = 10 },
        };
        VmPDestructibleSerializeScript.ApplyReadGroup(groups, new VmPDestructibleSerializeScript.HealthGroupSnapshot { Id = 3, Health = 4 });
        Assert.Equal(4, groups[3].Health);
    }

    [Fact]
    public void ResolveLineHealth_UsesGroupWhenPresent()
    {
        Assert.Equal(99, VmPDestructibleSerializeScript.ResolveLineHealth(10, 2, id => 99));
        Assert.Equal(10, VmPDestructibleSerializeScript.ResolveLineHealth(10, 0, _ => 99));
    }
}
