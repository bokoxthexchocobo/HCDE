using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SourcelessPainJustHitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourcelessPainMarksHitWithOrWithoutAnimation(bool missingState)
    {
        var target = new Actor { Health = 100, PainState = missingState ? -1 : 1 };
        ActorDamage.Apply(target, 10);
        Assert.True(target.JustHit); Assert.Equal(90, target.Health);
        Assert.Equal(missingState ? 0 : 1, target.States.Current);
        Assert.Null(target.LastDamageSourceId);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void SourcelessForcedPainKeepsFriendTargetGate(int chaseKind, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }] });
        var victim = sim.AddBot(100, 0, 3001); victim.Friendly = true;
        var chase = sim.AddBot(200, 0, 3004); chase.Friendly = chaseKind == 2;
        victim.Brain!.RestoreTargetMemory(new SimTargetMemory(chaseKind == 0 ? null : chase.Id, null, null));
        var targetId = victim.Brain.TargetId;
        ActorDamage.Apply(victim, 0, inflictor: new Actor { ForcePain = true });
        Assert.Equal(expected, victim.JustHit); Assert.Equal(targetId, victim.Brain.TargetId);
        Assert.Null(victim.LastDamageSourceId);
        var bytes = SimSavegame.Write(sim); victim.JustHit = !expected;
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, victim.JustHit);
    }
}
