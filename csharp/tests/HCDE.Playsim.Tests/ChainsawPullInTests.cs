using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawPullInTests
{
    [Theory]
    [InlineData(WeaponKind.Chainsaw, true, true)]
    [InlineData(WeaponKind.Chainsaw, false, false)]
    [InlineData(WeaponKind.Fist, true, false)]
    public void OnlyChainsawActorHitRequestsPullIn(WeaponKind weapon, bool hit, bool expected)
    {
        var (sim, player) = Room(); player.Inventory.Selected = weapon;
        if (!hit) sim.Actors[1].X = Fixed.FromInt(500);
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(expected, player.JustAttacked);
    }

    [Fact]
    public void PullInOverridesMovementAndYawForOneTick()
    {
        var (_, player) = Room(); var (_, control) = Room();
        player.JustAttacked = true;
        player.TryQueueCommand(new() { ForwardMove = -1000, SideMove = 2000, YawDelta = 5000, PitchDelta = 100 });
        control.TryQueueCommand(new() { ForwardMove = 0x6400, PitchDelta = 100 });
        player.Tick(); control.Tick();
        Assert.False(player.JustAttacked); Assert.True(player.HasMovementInput);
        Assert.Equal(control.X, player.X); Assert.Equal(control.Y, player.Y);
        Assert.Equal(control.VelocityX, player.VelocityX); Assert.Equal(control.Angle, player.Angle);
        Assert.Equal(control.PitchDegrees, player.PitchDegrees);
        var angle = player.Angle;
        player.TryQueueCommand(new() { YawDelta = 1000 }); player.Tick();
        Assert.NotEqual(angle, player.Angle);
    }

    [Fact]
    public void SavedChainsawHitReplaysPullIn()
    {
        var (sim, player) = Room(); Assert.True(HitscanCombat.Fire(sim, player));
        Assert.True(player.JustAttacked);
        var saved = SimSavegame.Write(sim, true);
        player.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Assert.True(player.JustAttacked);
        player.Tick(); Assert.False(player.JustAttacked);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Chainsaw;
        player.Inventory.Selected = WeaponKind.Chainsaw;
        sim.Actors[1].Brain!.Enabled = false;
        return (sim, player);
    }
}
