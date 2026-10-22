namespace HCDE.Playsim.Tests;

public class VmPDestructibleHealthGroupsTests
{
    [Fact]
    public void InitHealthGroups_MergesLineIntoGroup()
    {
        var ctx = new VmPDestructibleHealthGroups.InitHealthGroupsContext
        {
            Lines = new[] { new VmPDestructibleHealthGroups.LineHealth { Index = 1, Health = 50, HealthGroup = 2 } },
        };
        VmPDestructibleHealthGroups.InitHealthGroups(ctx);
        Assert.True(ctx.Groups.ContainsKey(2));
        Assert.Single(ctx.Groups[2].Lines);
    }

    [Fact]
    public void DamageLinedef_ReducesHealth()
    {
        var line = new VmPDestructibleHealthGroups.LineHealth { Health = 10 };
        VmPDestructibleHealthGroups.DamageLinedef(new VmPDestructibleHealthGroups.DamageLinedefContext
        {
            Line = line,
            Damage = 3,
        });
        Assert.Equal(7, line.Health);
    }

    [Fact]
    public void DamageSector_ReducesFloorHealthAndSyncsGroup()
    {
        var sector = new VmPDestructibleHealthGroups.SectorHealth
        {
            HealthFloor = 20,
            HealthFloorGroup = 5,
        };
        var group = new VmPDestructibleHealthGroups.HealthGroup { Id = 5, Health = 20 };
        VmPDestructibleHealthGroups.DamageSector(new VmPDestructibleHealthGroups.DamageSectorContext
        {
            Sector = sector,
            Part = VmPDestructibleHealthGroups.SecPartFloor,
            Damage = 8,
            DoGroups = true,
            GetHealthGroup = _ => group,
        });
        Assert.Equal(12, sector.HealthFloor);
        Assert.Equal(12, group.Health);
    }
}
