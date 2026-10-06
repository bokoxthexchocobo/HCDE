using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKillFamilyTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    public void KillActionsSelectRelatedVictimAndHonorInvulnerability(int kind, bool foil)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var caller = sim.AddBot(0, 0, 3004); var target = sim.AddBot(100, 0, 3001); var other = sim.AddBot(200, 0, 3001);
        target.Health = other.Health = 50; target.Invulnerable = true;
        var flags = foil ? 1 : 0;
        switch (kind)
        {
            case 0:
                caller.MasterId = target.Id; ActorHealthActions.KillMaster(caller, flags: flags); break;
            case 1:
                target.MasterId = caller.Id; ActorHealthActions.KillChildren(caller, flags: flags); break;
            case 2:
                caller.MasterId = target.MasterId = sim.Players.Single().Id;
                ActorHealthActions.KillSiblings(caller, flags: flags); break;
            case 3:
                var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.RevenantTracer, target);
                ActorHealthActions.KillTracer(missile, flags: flags); break;
        }
        Assert.Equal(foil ? 0 : 50, target.Health); Assert.Equal(50, other.Health); Assert.Equal(20, caller.Health);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        target.Health = 1; sim.RestoreState(state);
        Assert.Equal(foil ? 0 : 50, target.Health);
    }
}
