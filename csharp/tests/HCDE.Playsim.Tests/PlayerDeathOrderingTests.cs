using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDeathOrderingTests
{
    [Fact]
    public void DeathActionSeesRespawnDelayAndReleasedAttack()
    {
        var sim = Room(); var player = sim.Players.Single(); player.AttackPressed = true;
        int? delay = null; bool? attack = null;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, _ =>
        {
            delay = player.RespawnEarliestTic; attack = player.AttackPressed;
        })], 0); player.DeathState = 1;
        ActorDamage.Apply(player, 1000);
        Assert.Equal(GameTicClock.TicRate, delay); Assert.False(attack);
    }

    [Fact]
    public void WeaponDropPrecedesDeathActionChangingSelectedWeapon()
    {
        var sim = Room(); sim.WeaponDrop = true; var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Pistol; bool? dropped = null;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, _ =>
        {
            dropped = sim.Actors.Any(a => a.SpawnedDroppedPickup && a.DoomEdNum == PickupCatalog.Pistol);
            player.Inventory.Selected = WeaponKind.Fist;
        })], 0); player.DeathState = 1;
        ActorDamage.Apply(player, 1000);
        Assert.True(dropped); Assert.Single(sim.Actors, a => a.SpawnedDroppedPickup);
    }

    [Fact]
    public void ExtremeDeathFlagIsVisibleInsideDeathAction()
    {
        var sim = Room(); var player = sim.Players.Single(); bool? observed = null;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, _ => observed = player.ExtremelyDead)], 0);
        player.ExtremeDeathState = 1;
        ActorDamage.Apply(player, 1000, damageType: "Extreme"); Assert.True(observed);
    }

    [Fact]
    public void DeathActionMayOverridePreparedRespawnInputWithoutBeingClearedAfterward()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.States.Configure(player, [new(-1, 0), new(-1, 1, _ => player.AttackPressed = true)], 0);
        player.DeathState = 1; ActorDamage.Apply(player, 1000); Assert.True(player.AttackPressed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
