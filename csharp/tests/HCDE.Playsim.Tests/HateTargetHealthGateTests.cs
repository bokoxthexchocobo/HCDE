using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HateTargetHealthGateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HateStickinessUsesHealthRegardlessOfShootability(bool dead, bool shootable)
    {
        var (sim, hunter, current, attacker) = Room();
        var (expected, _, _, _) = Room();
        var before = sim.SwitchTargetRandomState;
        var loyal = expected.NextSwitchTargetRandom() % 256 < 128;
        if (dead) current.Health = 0;
        current.Shootable = shootable;
        ActorDamage.Apply(hunter, 1, attacker, DamageFlags.NoPain);
        Assert.Equal(!dead && loyal ? current.Id : attacker.Id, hunter.Brain!.TargetId);
        Assert.Equal(dead ? before : expected.SwitchTargetRandomState, sim.SwitchTargetRandomState);
    }

    private static (AuthoritySimulation, Actor, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200, Id = 7 },
                new LevelThing { Type = 3002, X = 400 }],
        }, rngSeed: 42);
        var hunter = sim.Actors[0];
        hunter.TidToHate = 7;
        hunter.Brain!.SetTargetThingId(sim, 7);
        hunter.Brain.SetChaseThreshold(0, false);
        return (sim, hunter, sim.Actors[1], sim.Actors[2]);
    }
}
