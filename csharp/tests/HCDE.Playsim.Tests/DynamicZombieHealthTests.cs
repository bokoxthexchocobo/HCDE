using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicZombieHealthTests
{
    [Theory]
    [InlineData(20, false)]
    [InlineData(21, true)]
    public void DynamicZombieUsesNativeSpawnHealthForGibThreshold(int overkill, bool gibbed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var mapped = Assert.Single(sim.Actors); var dynamicActor = sim.AddBot(100, 0);
        Assert.Equal(20, mapped.Health); Assert.Equal(mapped.Health, dynamicActor.Health);
        Assert.Equal(20, dynamicActor.ResurrectionHealth); Assert.Equal(-20, dynamicActor.GibHealth);
        dynamicActor.ExtremeDeathState = 4;
        dynamicActor.States.Configure(dynamicActor, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(6, 3)], 0);
        ActorDamage.Apply(dynamicActor, 20 + overkill);
        Assert.Equal(-overkill, dynamicActor.Health); Assert.Equal(gibbed ? 4 : ActorStateMachine.Death, dynamicActor.States.Current);
    }
    [Theory]
    [InlineData(3004)]
    [InlineData(4000)]
    public void ExplicitHealthPatchControlsDynamicZombieDeathThreshold(int editorNumber)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nID # = {editorNumber}\nHit points = 35\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var actor = sim.AddBot(0, 0, editorNumber);
        ActorDamage.Apply(actor, 20); Assert.Equal(15, actor.Health); Assert.False(actor.IsDead);
        ActorDamage.Apply(actor, 15); Assert.True(actor.IsDead);
        Assert.Equal(35, actor.ResurrectionHealth); Assert.Equal(-35, actor.GibHealth);
    }
}