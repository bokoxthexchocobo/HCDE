using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoAttackSectorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SectorFlagBlocksMeleeEvenWithVerticalBypass(bool noAttack)
    {
        var sim = Room(noAttack); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.NoVerticalMeleeRange = true; actor.Brain!.SetSpecialTarget(target);
        Assert.Equal(!noAttack, MonsterBrain.CheckMeleeRange(actor, target, true));
        Assert.Equal(noAttack ? (int?)null : 2, ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
        Assert.Equal(noAttack ? 2 : (int?)null, ActorJumpActions.JumpIfTargetOutsideMeleeRange(actor, 2));
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(noAttack); restored.AddBot(0, 0); restored.RestoreState(state);
        Assert.Equal(noAttack, restored.Level.Sectors.Single().NoAttack);
    }

    private static AuthoritySimulation Room(bool noAttack) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = noAttack }],
        Things = [new LevelThing { Type = 1, X = 20 }],
    });
}
