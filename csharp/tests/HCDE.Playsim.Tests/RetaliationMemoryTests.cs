using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RetaliationMemoryTests
{
    [Theory]
    [InlineData(0, false, false, false)]
    [InlineData(42, false, false, true)]
    [InlineData(-42, false, false, true)]
    [InlineData(42, true, false, false)]
    [InlineData(0, false, true, true)]
    [InlineData(42, false, true, true)]
    [InlineData(42, true, true, false)]
    public void RetaliationPreservesLivingMemoryAccordingToHatePolicy(int hateTid, bool deadMemory, bool playerMemory, bool preserve)
    {
        var sim = Room();
        var owner = Monster(sim, 64, 3004);
        Actor first = playerMemory ? sim.Players.Single() : Monster(sim, 96);
        var second = Monster(sim, 128);
        var third = Monster(sim, 160);
        owner.TidToHate = hateTid;
        Hit(owner, first);
        Hit(owner, second);
        Assert.Equal(first.Id, owner.Brain!.LastEnemyId);
        if (deadMemory) first.Health = 0;
        Hit(owner, third);
        Assert.Equal(third.Id, owner.Brain.TargetId);
        Assert.Equal(preserve ? first.Id : second.Id, owner.Brain.LastEnemyId);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(42, true)]
    public void NewTargetAfterClearingCurrentTargetUpdatesMemory(int hateTid, bool preserve)
    {
        var sim = Room();
        var owner = Monster(sim, 64, 3004);
        var first = Monster(sim, 96);
        var second = Monster(sim, 128);
        var third = Monster(sim, 160);
        owner.TidToHate = hateTid;
        Hit(owner, first); Hit(owner, second);
        owner.Brain!.ClearTarget();
        Hit(owner, third);
        Assert.Equal(third.Id, owner.Brain.TargetId);
        Assert.Equal(preserve ? (uint?)first.Id : null, owner.Brain.LastEnemyId);
    }

    [Fact]
    public void LivingNonShootableRememberedEnemyIsPreservedWithHateTid()
    {
        var sim = Room();
        var owner = Monster(sim, 64, 3004);
        var first = Monster(sim, 96);
        var second = Monster(sim, 128);
        var third = Monster(sim, 160);
        owner.TidToHate = 42;
        Hit(owner, first); Hit(owner, second);
        first.Shootable = false;
        Hit(owner, third);
        Assert.Equal(first.Id, owner.Brain!.LastEnemyId);
    }

    private static void Hit(Actor owner, Actor source) => ActorDamage.Apply(owner, 1, source: source, inflictor: source);

    private static BotPawn Monster(AuthoritySimulation sim, double x, int type = 3001)
    {
        var actor = sim.AddBot(x, 64, doomEdNum: type);
        actor.Health = 100;
        actor.PainChance = 0;
        actor.Brain!.Enabled = false;
        actor.Brain.DefThreshold = 0;
        return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}

