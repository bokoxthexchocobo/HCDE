using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorClearRefireTests
{
    [Theory]
    [InlineData(WeaponKind.Pistol, false)]
    [InlineData(WeaponKind.Pistol, true)]
    [InlineData(WeaponKind.Chaingun, false)]
    [InlineData(WeaponKind.Chaingun, true)]
    public void ClearRefireRestoresAccurateShotWithoutConsumingState(WeaponKind weapon, bool vertical)
    {
        var sim = Room(weapon); var player = sim.Players.Single(); sim.VerticalWeaponSpread = vertical;
        player.WeaponRefiring = true; player.WeaponCooldown = 3;
        var bullets = player.Inventory.Bullets; var random = sim.CaptureState().GunShotRandomState;
        ActorInventoryActions.ClearRefire(player);
        Assert.False(player.WeaponRefiring); Assert.Equal(3, player.WeaponCooldown);
        Assert.Equal(bullets, player.Inventory.Bullets); Assert.Equal(random, sim.CaptureState().GunShotRandomState);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(weapon); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Assert.False(fresh.Players.Single().WeaponRefiring);
        var reference = Room(weapon); reference.NextGunShotDamage();
        foreach (var world in new[] { sim, fresh })
        {
            var source = world.Players.Single(); source.WeaponCooldown = 0;
            Assert.True(HitscanCombat.Fire(world, source));
            Assert.Equal(reference.CaptureState().GunShotRandomState, world.CaptureState().GunShotRandomState);
        }
    }

    [Fact]
    public void StateActionCanClearRefire()
    {
        var player = Room(WeaponKind.Pistol).Players.Single(); player.WeaponRefiring = true;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, ActorInventoryActions.ClearRefire)], 0);
        player.States.Enter(player, 1); Assert.False(player.WeaponRefiring); Assert.Equal(1, player.States.Current);
    }

    [Fact]
    public void NonPlayerActionLeavesWorldUnchanged()
    {
        var sim = Room(WeaponKind.Pistol); var actor = sim.AddBot(200, 0);
        var saved = SimSavegame.Write(sim, true); ActorInventoryActions.ClearRefire(actor);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static AuthoritySimulation Room(WeaponKind weapon)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        }, rngSeed: 1);
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        return sim;
    }
}
