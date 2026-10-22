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

    [Fact]
    public void GetHealthGroupOrNew_CreatesWhenMissing()
    {
        var groups = new Dictionary<int, VmPDestructibleHealthGroups.HealthGroup>();
        var grp = VmPDestructibleSerializeScript.GetHealthGroupOrNew(groups, 7, 55);
        Assert.Equal(55, grp.Health);
        Assert.Same(grp, groups[7]);
    }
}
