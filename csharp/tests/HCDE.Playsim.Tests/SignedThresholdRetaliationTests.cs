using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SignedThresholdRetaliationTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(int.MinValue, false)]
    [InlineData(1, false)]
    [InlineData(-1, true)]
    [InlineData(int.MinValue, true)]
    [InlineData(0, false)]
    public void NonzeroSignedThresholdBlocksSwitchUnlessQuickRetaliation(int threshold, bool quick)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200, Id = 7 },
                new LevelThing { Type = 3002, X = 400 }],
        });
        var hunter = sim.Actors[0]; var current = sim.Actors[1]; var attacker = sim.Actors[2];
        hunter.Brain!.SetTargetThingId(sim, 7);
        hunter.Brain.RestoreChaseThreshold(new SimChaseThreshold(threshold, 100));
        hunter.QuickToRetaliate = quick;
        var bytes = SimSavegame.Write(sim);
        hunter.Brain.RestoreChaseThreshold(null);
        SimSavegame.Apply(sim, bytes);
        ActorDamage.Apply(hunter, 1, attacker, DamageFlags.NoPain);
        var switches = quick || threshold == 0;
        Assert.Equal(switches ? attacker.Id : current.Id, hunter.Brain.TargetId);
        Assert.Equal(switches ? 100 : threshold, hunter.Brain.Threshold);
    }
}
