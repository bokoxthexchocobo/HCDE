using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawAttackTests
{
    [Theory]
    [InlineData(7, 7)]
    [InlineData(0, 2)]
    public void FixedDamageAndCustomHitSoundReachSharedImpactPath(int damage, int expected)
    {
        var (sim, player, target) = Room(); var health = target.Health;
        ChainsawActions.Attack(player, damage, spread: 0, flags: ChainsawAttackFlags.NoRandom
            | ChainsawAttackFlags.NoTurn | ChainsawAttackFlags.NoPullIn | ChainsawAttackFlags.NoRandomPuffZ, hitSound: "custom/hit");
        Assert.Equal(health - expected, target.Health); Assert.False(player.JustAttacked);
        Assert.Equal(BamAngle.FromDegrees(0), player.Angle);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.ActorId == player.Id && sound.Channel == 1 && sound.Sound == "custom/hit");
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(128, true)]
    public void ExplicitRangeControlsHitAndMissSound(double range, bool hits)
    {
        var (sim, player, target) = Room(); var health = target.Health;
        ChainsawActions.Attack(player, 5, range, 0, ChainsawAttackFlags.NoRandom, "custom/miss", "custom/hit");
        Assert.Equal(health - (hits ? 5 : 0), target.Health); Assert.Equal(hits, player.JustAttacked);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.ActorId == player.Id && sound.Sound == (hits ? "custom/hit" : "custom/miss"));
    }

    [Fact]
    public void UnsupportedFlagsDoNotChangeSimulation()
    {
        var (sim, player, _) = Room(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<NotSupportedException>(() => ChainsawActions.Attack(player, flags: (ChainsawAttackFlags)8));
        Assert.Equal(before, SimSavegame.Write(sim, true)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        sim.Actors[1].Brain!.Enabled = false; sim.DrainSoundRequests();
        return (sim, sim.Players.Single(), sim.Actors[1]);
    }
}
