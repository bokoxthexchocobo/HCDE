using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDamageTests
{
    [Theory]
    [InlineData(7)]
    [InlineData(0)]
    [InlineData(-5)]
    public void ExplicitMissileDamageInitializesActor(int damage)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMissile damage = {damage}\n");
        Assert.True(patch.Actors.Single(actor => actor.Index == 2).MissileDamagePatched);
        Assert.Equal(damage, Spawn(patch).Damage);
    }

    [Fact]
    public void ChainedUnrelatedPatchPreservesExplicitDamageWithoutMutatingBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nMissile damage = 7\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        var third = DehackedPatch.Apply("Thing 2\nMissile damage = 0\n", second);
        Assert.Equal(7, Spawn(first).Damage);
        Assert.Equal(7, Spawn(second).Damage);
        Assert.Equal(0, Spawn(third).Damage);
        Assert.Equal(88, Spawn(second).Health);
    }

    [Fact]
    public void UnrelatedPatchDoesNotMarkDamageAsExplicit()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 88\n");
        Assert.False(patch.Actors.Single(actor => actor.Index == 2).MissileDamagePatched);
        Assert.Equal(0, Spawn(patch).Damage);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
