using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RevenantMeleeSoundTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SwingAlwaysPlaysButHitRequiresTargetStillInRange(bool leavesRange)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 66);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        player.PainChance = 0;
        var combat = sim.CombatRandomState;
        ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        var swing = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("skeleton/swing", swing.Sound); Assert.Equal(1, swing.Channel); Assert.Equal(0, swing.Flags);
        if (leavesRange) player.X = Fixed.FromInt(500);
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        var hits = sim.DrainSoundRequests().Where(r => r.Sound == "skeleton/melee").ToArray();
        Assert.Equal(leavesRange ? 0 : 1, hits.Length);
        Assert.Equal(!leavesRange, player.Health < 100);
        if (leavesRange) Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(!leavesRange, sim.CaptureState().SkelFistRandomState.HasValue);
        if (!leavesRange) { Assert.Equal(1, hits[0].Channel); Assert.Equal(1, hits[0].Attenuation); }
    }

    [Fact]
    public void MissileOnlyEntryDoesNotPlayMeleeSounds()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 66); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.DoesNotContain(sim.PendingSoundRequests, r => r.Sound is "skeleton/swing" or "skeleton/melee");
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = 66 }] });
}
