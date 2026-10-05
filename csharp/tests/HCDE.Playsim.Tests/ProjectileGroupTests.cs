using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileGroupTests
{
    [Theory]
    [InlineData(0, 0, true, false, true)]
    [InlineData(0, 0, true, true, false)]
    [InlineData(0, 0, false, false, false)]
    [InlineData(7, 7, false, true, true)]
    [InlineData(7, 8, true, false, false)]
    [InlineData(-1, -1, true, false, false)]
    public void NativeImmunityRules(int targetGroup, int sourceGroup, bool sameSpecies, bool harmSpecies, bool expected)
    {
        var target = new Actor { DoomEdNum = 3004, ProjectileGroup = targetGroup, DoHarmSpecies = harmSpecies };
        var source = new Actor { DoomEdNum = sameSpecies ? 3004 : 3001, ProjectileGroup = sourceGroup };
        Assert.Equal(expected, target.ProjectileImmune(source));
    }

    [Fact]
    public void GrouplessSelfComparisonRetainsImmunity()
    {
        var actor = new Actor { ProjectileGroup = -1 };
        Assert.True(actor.ProjectileImmune(actor));
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(-7, false)]
    public void ParsedGroupsControlDamageEligibility(int group, bool immune)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 100 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nProjectile group = {group}\nThing 12\nProjectile group = {group}\n"));
        var target = sim.Actors[1]; var source = sim.Actors[2];
        Assert.Equal(group < 0 ? -1 : group, target.ProjectileGroup);
        Assert.Equal(immune ? 0 : 1, ActorDamage.Apply(target, 1, source).HealthLost);
    }
}
