using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRemovalActionTests
{
    [Theory]
    [InlineData(true, 0, null, true)]
    [InlineData(true, 2, null, false)]
    [InlineData(false, 0, null, false)]
    [InlineData(false, 4, null, true)]
    [InlineData(false, 8, null, true)]
    [InlineData(true, 0, "ZombieMan", false)]
    [InlineData(true, 16, "ZombieMan", true)]
    public void RemovalCategoryAndFiltersMatchNative(bool monster, int flags, string? filter, bool expected)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3001); actor.IsMonster = monster;
        ActorRemovalActions.Remove(actor, flags: flags, classFilter: filter);
        Assert.Equal(expected, actor.Destroyed);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void TargetPointerCanRemoveMissileWithoutExplosion(int flags, bool expected)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        missile.ThingId = 7; caller.Brain!.SetTargetThingId(sim, 7);
        ActorRemovalActions.Remove(caller, AcsActorPointer.Target, flags);
        Assert.Equal(expected, missile.Destroyed); Assert.Equal(100, sim.Players.Single().Health);
    }

    [Fact]
    public void EverythingProtectsPlayerAndNullPointerIsNoop()
    {
        var sim = Room(); var player = sim.Players.Single(); var actor = sim.AddBot(0, 0);
        ActorRemovalActions.Remove(player, flags: 8);
        ActorRemovalActions.Remove(actor, AcsActorPointer.Null, 8);
        Assert.False(player.Destroyed); Assert.False(actor.Destroyed);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void FamilyRemovalHonorsDeadOnlyAndRelationship(bool siblings, bool removeAll)
    {
        var sim = Room(); var parent = sim.AddBot(0, 0, 3001);
        var caller = siblings ? sim.AddBot(0, 0, 3001) : parent;
        if (siblings) caller.MasterId = parent.Id;
        var dead = sim.AddBot(0, 0, 3001); dead.MasterId = parent.Id;
        var live = sim.AddBot(0, 0, 3001); live.MasterId = parent.Id;
        var unrelated = sim.AddBot(0, 0, 3001);
        ActorHealthActions.Die(dead);
        Assert.True(dead.Health <= 0); Assert.False(dead.Destroyed);
        if (siblings) ActorRemovalActions.RemoveSiblings(caller, removeAll);
        else ActorRemovalActions.RemoveChildren(caller, removeAll);
        Assert.True(dead.Destroyed); Assert.Equal(removeAll, live.Destroyed);
        Assert.False(caller.Destroyed); Assert.False(parent.Destroyed); Assert.False(unrelated.Destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FamilyRemovalHonorsCategoryAndClassFilters(bool siblings)
    {
        var sim = Room(); var parent = sim.AddBot(0, 0, 3001); var caller = sim.AddBot(0, 0, 3001);
        caller.MasterId = parent.Id;
        var child = sim.AddBot(0, 0, 3001); child.MasterId = siblings ? parent.Id : caller.Id;
        if (siblings) ActorRemovalActions.RemoveSiblings(caller, true, classFilter: "ZombieMan");
        else ActorRemovalActions.RemoveChildren(caller, true, classFilter: "ZombieMan");
        Assert.False(child.Destroyed);
        if (siblings) ActorRemovalActions.RemoveSiblings(caller, true, flags: 2);
        else ActorRemovalActions.RemoveChildren(caller, true, flags: 2);
        Assert.False(child.Destroyed);
        if (siblings) ActorRemovalActions.RemoveSiblings(caller, true, flags: 16, classFilter: "ZombieMan");
        else ActorRemovalActions.RemoveChildren(caller, true, flags: 16, classFilter: "ZombieMan");
        Assert.True(child.Destroyed);
    }

    [Fact]
    public void NamedMasterRemovalAndMissingMasterSiblingSelection()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); var other = sim.AddBot(0, 0, 3001);
        ActorRemovalActions.RemoveSiblings(caller, true);
        Assert.False(other.Destroyed);
        caller.MasterId = other.Id;
        ActorRemovalActions.RemoveMaster(caller);
        Assert.True(other.Destroyed); Assert.False(caller.Destroyed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
