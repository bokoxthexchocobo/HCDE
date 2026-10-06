using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HearingThresholdResetTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(-1)]
    [InlineData(0)]
    public void IdleSoundWakeClearsThresholdAndAllowsDamageRetaliation(int threshold)
    {
        var (sim, hunter, heard, attacker) = Room();
        hunter.Brain!.SetChaseThreshold(threshold, false);
        hunter.Brain.Hear(sim, hunter, heard);
        Assert.Equal(0, hunter.Brain.Threshold);
        Assert.Equal(heard.Id, hunter.Brain.TargetId);
        ActorDamage.Apply(hunter, 1, attacker, DamageFlags.NoPain);
        Assert.Equal(attacker.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.Brain.DefThreshold, hunter.Brain.Threshold);
    }

    [Fact]
    public void ExistingTargetSoundDoesNotResetPursuitThreshold()
    {
        var (sim, hunter, heard, attacker) = Room(); heard.ThingId = 7;
        hunter.Brain!.SetTargetThingId(sim, 7);
        hunter.Brain.SetChaseThreshold(100, false);
        hunter.Brain.Hear(sim, hunter, attacker);
        Assert.Equal(100, hunter.Brain.Threshold);
        Assert.Equal(heard.Id, hunter.Brain.TargetId);
    }

    [Fact]
    public void SaveRestoredIdleThresholdIsClearedOnSoundWake()
    {
        var (sim, hunter, heard, _) = Room();
        hunter.Brain!.SetChaseThreshold(100, false);
        var bytes = SimSavegame.Write(sim);
        hunter.Brain.SetChaseThreshold(1, false);
        SimSavegame.Apply(sim, bytes);
        hunter.Brain.Hear(sim, hunter, heard);
        Assert.Equal(0, hunter.Brain.Threshold);
        Assert.Equal(heard.Id, hunter.Brain.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 },
                new LevelThing { Type = 3002, X = 400 }],
        });
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
