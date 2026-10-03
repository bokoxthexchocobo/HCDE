using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RetaliationTickTests
{
    [Theory]
    [InlineData("threshold")]
    [InlineData("no-switch")]
    [InlineData("no-hate-players")]
    [InlineData("never-target")]
    [InlineData("no-target")]
    public void RejectedDamageTargetSwitchRemainsRejectedOnNextTick(string policy)
    {
        var (sim, owner, current) = Room();
        var attacker = sim.Players.Single();
        owner.Brain!.DefThreshold = policy == "threshold" ? 100 : 0;
        ActorDamage.Apply(owner, 1, source: current, inflictor: current);
        switch (policy)
        {
            case "no-switch": owner.NoTargetSwitch = true; break;
            case "no-hate-players": owner.NoHatePlayers = true; break;
            case "never-target": attacker.NeverTarget = true; break;
            case "no-target": attacker.NoTarget = true; break;
        }
        ActorDamage.Apply(owner, 1, source: attacker, inflictor: attacker);
        Assert.Equal(attacker.Id, owner.LastDamageSourceId);
        Assert.Equal(current.Id, owner.Brain.TargetId);
        owner.Brain.Tick(sim, owner);
        Assert.Equal(current.Id, owner.Brain.TargetId);
    }

    [Fact]
    public void FriendlyDamageDoesNotReplaceCurrentEnemyOnNextTick()
    {
        var (sim, owner, current) = Room();
        owner.Friendly = true;
        owner.HarmFriends = true;
        owner.Brain!.DefThreshold = 0;
        ActorDamage.Apply(owner, 1, source: current, inflictor: current);
        var ally = sim.AddBot(160, 64, doomEdNum: 3001);
        ally.Friendly = true;
        ally.HarmFriends = true;
        ActorDamage.Apply(owner, 1, source: ally, inflictor: ally);
        Assert.Equal(ally.Id, owner.LastDamageSourceId);
        Assert.Equal(current.Id, owner.Brain.TargetId);
        owner.Brain.Tick(sim, owner);
        Assert.Equal(current.Id, owner.Brain.TargetId);
    }

    [Fact]
    public void AcceptedRetaliationPersistsAndRemembersPreviousTarget()
    {
        var (sim, owner, current) = Room();
        owner.Brain!.DefThreshold = 0;
        ActorDamage.Apply(owner, 1, source: current, inflictor: current);
        var attacker = sim.Players.Single();
        ActorDamage.Apply(owner, 1, source: attacker, inflictor: attacker);
        Assert.Equal(attacker.Id, owner.Brain.TargetId);
        owner.Brain.Tick(sim, owner);
        Assert.Equal(attacker.Id, owner.Brain.TargetId);
        Assert.Equal(current.Id, owner.Brain.LastEnemyId);
    }

    private static (AuthoritySimulation Sim, BotPawn Owner, BotPawn Current) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 400 }],
        });
        var owner = sim.AddBot(64, 64, doomEdNum: 3004);
        var current = sim.AddBot(128, 64, doomEdNum: 3001);
        owner.Health = current.Health = 100;
        owner.PainChance = 0;
        current.Brain!.Enabled = false;
        return (sim, owner, current);
    }
}
