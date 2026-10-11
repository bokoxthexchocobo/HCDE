using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RevenantFacingTimelineTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BothAttackEntriesRunFacingAction(bool melee)
    {
        var (sim, hunter) = Room(melee);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        Tick(sim, hunter);
        Assert.False(hunter.Ambush);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
        Assert.Equal(MonsterMode.Windup, hunter.Brain!.Mode);
    }

    [Theory]
    [InlineData(true, 6)]
    [InlineData(false, 20)]
    public void CorrectTimelineFacesAtItsIntermediateBoundary(bool melee, int boundary)
    {
        var (sim, hunter) = Room(melee);
        Tick(sim, hunter);
        for (var tic = 1; tic < boundary; tic++) Tick(sim, hunter);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        Tick(sim, hunter);
        Assert.False(hunter.Ambush);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(true, 6)]
    [InlineData(false, 20)]
    public void SaveBeforeFacingBoundaryRestoresItsNextAction(bool melee, int boundary)
    {
        var (sim, hunter) = Room(melee);
        Tick(sim, hunter);
        for (var tic = 1; tic < boundary; tic++) Tick(sim, hunter);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        var bytes = SimSavegame.Write(sim);
        Tick(sim, hunter);
        var expected = hunter.Brain!.CaptureRevenantAttack();
        Assert.False(hunter.Ambush);
        SimSavegame.Apply(sim, bytes);
        hunter = sim.Actors.Single(actor => actor.DoomEdNum == 66);
        Assert.True(hunter.Ambush);
        Assert.Equal(180, hunter.Angle.ToDegrees(), 5);
        Tick(sim, hunter);
        Assert.False(hunter.Ambush);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
        Assert.Equal(expected, hunter.Brain!.CaptureRevenantAttack());
    }

    [Fact]
    public void MissileTimelineDoesNotUseMeleeFacingBoundary()
    {
        var (sim, hunter) = Room(false);
        Tick(sim, hunter);
        for (var tic = 1; tic < 6; tic++) Tick(sim, hunter);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        Tick(sim, hunter);
        Assert.True(hunter.Ambush);
        Assert.Equal(180, hunter.Angle.ToDegrees(), 5);
    }

    private static void Tick(AuthoritySimulation sim, Actor hunter) => hunter.Brain!.Tick(sim, hunter,
        MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);

    private static (AuthoritySimulation, Actor) Room(bool melee)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = melee ? 40 : 500 }, new LevelThing { Type = 66 }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == 66);
        hunter.ReactionTime = 0;
        sim.Players.Single().Invulnerable = true;
        return (sim, hunter);
    }
}
