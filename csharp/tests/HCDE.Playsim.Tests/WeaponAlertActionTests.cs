using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponAlertActionTests
{
    [Fact]
    public void AlertBlamesOwnerWithoutFiringOrSpendingAmmo()
    {
        var (sim, player, listener) = Room(); player.SetTargetPointer(listener);
        var ammo = player.Inventory.Bullets; var cooldown = player.WeaponCooldown;
        ActorNoiseActions.WeaponAlert(player);
        Assert.Equal(player.Id, listener.LastHeardTargetId); Assert.Equal(player.Id, listener.TargetId);
        Assert.Equal(ammo, player.Inventory.Bullets); Assert.Equal(cooldown, player.WeaponCooldown);
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
    }

    [Fact]
    public void AlertWorksAsFrameAction()
    {
        var (sim, player, listener) = Room();
        player.States.Configure(player, [new(-1, 0, ActorNoiseActions.WeaponAlert)], 0);
        Assert.Equal(player.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void NoTargetSuppressesWeaponAlert()
    {
        var (_, player, listener) = Room(); player.NoTarget = true;
        ActorNoiseActions.WeaponAlert(player); Assert.Null(listener.LastHeardTargetId); Assert.Null(listener.TargetId);
    }

    [Fact]
    public void NoSplashAlertDoesNotSuppressWeaponNoise()
    {
        var (_, player, listener) = Room(); listener.NoSplashAlert = true;
        ActorNoiseActions.WeaponAlert(player); Assert.Equal(player.Id, listener.LastHeardTargetId);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Listener) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }] });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.ClassDoomEdNum == 3004));
    }
}
