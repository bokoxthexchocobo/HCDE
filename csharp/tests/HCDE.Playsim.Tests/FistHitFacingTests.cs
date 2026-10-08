using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FistHitFacingTests
{
    [Theory]
    [InlineData(-8, false)]
    [InlineData(8, false)]
    [InlineData(-8, true)]
    [InlineData(8, true)]
    public void FistTurnsToTraceTargetEvenWhenDamageIsBlocked(int y, bool invulnerable)
    {
        var (sim, player) = Room(); var target = sim.AddBot(48, y, 3003);
        target.Brain = null; target.Health = 1000; target.Invulnerable = invulnerable;
        var expected = BamAngle.FromDegrees(player.AngleTo(target));
        var pitch = player.PitchDegrees;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(expected, player.Angle);
        Assert.Equal(pitch, player.PitchDegrees);
        if (invulnerable) Assert.Equal(1000, target.Health);
        else Assert.True(target.Health < 1000);
        var bytes = SimSavegame.Write(sim); player.Angle = BamAngle.FromDegrees(180);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(expected, player.Angle);
    }

    [Fact]
    public void FistMissLeavesFacingUnchanged()
    {
        var (sim, player) = Room(); var target = sim.AddBot(48, 200, 3003);
        target.Brain = null; var health = target.Health; var angle = player.Angle;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(angle, player.Angle); Assert.Equal(health, target.Health);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var player = sim.Players.Single(); player.Inventory.Selected = WeaponKind.Fist;
        return (sim, player);
    }
}
