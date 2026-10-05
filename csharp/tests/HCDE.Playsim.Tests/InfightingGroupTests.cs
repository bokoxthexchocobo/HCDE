using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfightingGroupTests
{
    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(7, 7, true)]
    [InlineData(7, 8, false)]
    public void NonzeroMatchingGroupsBlockRetaliation(int targetGroup, int sourceGroup, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 100 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nInfighting group = {targetGroup}\nThing 12\nInfighting group = {sourceGroup}\n"));
        var target = sim.Actors[1]; var source = sim.Actors[2];
        Assert.Equal(targetGroup, target.InfightingGroup); Assert.Equal(sourceGroup, source.InfightingGroup);
        Assert.Equal(1, ActorDamage.Apply(target, 1, source).HealthLost);
        Assert.Equal(blocked ? null : (uint?)source.Id, target.Brain!.TargetId);
    }

    [Fact]
    public void NegativePatchedGroupNormalizesToZero()
    {
        var patch = DehackedPatch.Apply("Thing 2\nInfighting group = -7\n");
        var actor = patch.Actors.Single(a => a.Index == 2);
        Assert.True(actor.InfightingGroupPatched); Assert.Equal(0, actor.InfightingGroup);
    }
}
