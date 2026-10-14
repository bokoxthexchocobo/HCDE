using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawVerticalSpreadTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(-30)]
    [InlineData(180)]
    public void VerticalSpreadUsesSecondRandomPairAndPreservesPlayerPitch(double spread)
    {
        var (sim, player, target) = Room(); var saved = sim.CaptureState();
        sim.NextSawSpread(); var pitchSpread = sim.NextSawSpread() * spread;
        var expectedHit = CombatTrace.TraceLineAttack(sim, player, player.Angle,
            BamAngle.FromDegrees(player.PitchDegrees + pitchSpread), 128).Victim is not null;
        var nextRandom = sim.NextSawByte(); sim.RestoreState(saved);
        var pitch = player.PitchDegrees; var health = target.Health;
        ChainsawActions.Attack(player, 10, 128, 0, ChainsawAttackFlags.NoRandom | ChainsawAttackFlags.NoTurn,
            verticalSpread: spread);
        Assert.Equal(health - (expectedHit ? 10 : 0), target.Health);
        Assert.Equal(expectedHit, player.JustAttacked); Assert.Equal(pitch, player.PitchDegrees);
        Assert.Equal(nextRandom, sim.NextSawByte());
    }

    [Fact]
    public void VerticalSpreadReplaysAcrossArchiveRestore()
    {
        var (sim, player, _) = Room(); var saved = SimSavegame.Write(sim, true);
        ChainsawActions.Attack(player, verticalSpread: 45); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); ChainsawActions.Attack(player, verticalSpread: 45);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void InvalidVerticalSpreadRejectsBeforeStateChanges()
    {
        var (sim, player, _) = Room(); var saved = SimSavegame.Write(sim, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => ChainsawActions.Attack(player, verticalSpread: double.PositiveInfinity));
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        var target = sim.Actors[1]; target.Brain!.Enabled = false; sim.DrainSoundRequests();
        return (sim, sim.Players.Single(), target);
    }
}
