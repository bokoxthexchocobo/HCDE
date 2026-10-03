using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DormantBeginPlayTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PatchedMonsterDormancyHoldsSpawnFrameWithOrWithoutMapFlag(bool mapDormant)
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = DORMANT\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 3004, Dormant = mapDormant }] }, dehacked: patch);
        var actor = Assert.Single(sim.Actors);
        Assert.True(actor.Dormant); Assert.Equal(-1, actor.States.RemainingTics);
        var current = actor.States.Current; sim.Tick(); Assert.Equal(current, actor.States.Current);
        Assert.True(ThingActivation.Execute(sim, actor, 0, true));
        Assert.False(actor.Dormant); Assert.Equal(1, actor.States.RemainingTics);
    }

    [Theory]
    [InlineData(false, 100, false)]
    [InlineData(true, 0, false)]
    [InlineData(true, 0, true)]
    public void BeginPlayClearsClassFlagBeforeBaseDeactivation(bool monster, int health, bool ice)
    {
        var actor = new Actor { IsMonster = monster, Health = health, IceCorpse = ice, Dormant = true };
        actor.States.Configure(actor, [new ActorFrame(5, 0)], 0);
        ThingActivation.InitializeSpawn(actor, false);
        Assert.Equal(ice, actor.Dormant);
        Assert.Equal(ice ? -1 : 5, actor.States.RemainingTics);
    }

    [Fact]
    public void CustomInactiveStateRunsOnceWhenClassAndMapBothRequestDormancy()
    {
        var actor = new Actor { IsMonster = true, Dormant = true, InactiveState = 1 };
        var actions = 0;
        actor.States.Configure(actor, [new ActorFrame(5, 0), new ActorFrame(3, 1, _ => actions++)], 0);
        ThingActivation.InitializeSpawn(actor, true);
        Assert.True(actor.Dormant); Assert.Equal(1, actor.States.Current);
        Assert.Equal(3, actor.States.RemainingTics); Assert.Equal(1, actions);
    }
}