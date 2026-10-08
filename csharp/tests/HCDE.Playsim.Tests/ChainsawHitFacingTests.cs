using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawHitFacingTests
{
    [Theory]
    [InlineData(-8, 0, false)]
    [InlineData(8, 0, false)]
    [InlineData(-2, 0, false)]
    [InlineData(2, 0, false)]
    [InlineData(0, 0, false)]
    [InlineData(-3, 350, false)]
    [InlineData(3, 10, false)]
    [InlineData(8, 0, true)]
    public void DefaultSawTurnsWithNativeCloseAndFarBranches(int y, double yaw, bool invulnerable)
    {
        var (sim, player) = Room(); player.Angle = BamAngle.FromDegrees(yaw);
        var target = sim.AddBot(48, y, 3003); target.Brain = null;
        target.Health = 1000; target.Invulnerable = invulnerable;
        var targetAngle = BamAngle.FromDegrees(Math.Atan2(y, 48) * 180 / Math.PI).ToDegrees();
        var delta = (targetAngle - yaw + 540) % 360 - 180;
        var expected = BamAngle.FromDegrees(Math.Abs(delta) <= 4.5
            ? yaw + (delta < 0 ? -4.5 : 4.5)
            : targetAngle - Math.Sign(delta) * (90.0 / 21));
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(expected, player.Angle);
        if (invulnerable) Assert.Equal(1000, target.Health);
        else Assert.True(target.Health < 1000);
        var bytes = SimSavegame.Write(sim); player.Angle = BamAngle.FromDegrees(180);
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, player.Angle);
    }

    [Fact]
    public void SawMissPreservesFacing()
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
        var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Chainsaw;
        player.Inventory.Selected = WeaponKind.Chainsaw;
        return (sim, player);
    }
}
