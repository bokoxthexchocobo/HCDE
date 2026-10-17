using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponFiringSoundTests
{
    public static IEnumerable<object[]> Weapons => new[] {
        new object[] { WeaponKind.Pistol, "weapons/pistol" },
        new object[] { WeaponKind.Shotgun, "weapons/shotgf" },
        new object[] { WeaponKind.Chaingun, "weapons/chngun" },
        new object[] { WeaponKind.SuperShotgun, "weapons/sshotf" },
    };

    [Theory]
    [MemberData(nameof(Weapons))]
    public void AcceptedFireQueuesOneNativeWeaponChannelSoundAcrossFreshSave(WeaponKind weapon, string sound)
    {
        var sim = Room(weapon); var saved = SimSavegame.Write(sim, true);
        var fresh = Room(weapon); SimSavegame.Apply(fresh, saved);
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single(); Assert.True(HitscanCombat.Fire(world, player));
            var request = Assert.Single(world.DrainSoundRequests());
            Assert.Equal(sound, request.Sound); Assert.Equal(player.Id, request.ActorId);
            Assert.Equal(1, request.Channel); Assert.Equal(0, request.Flags);
            Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation);
            Assert.False(request.Loop);
            Assert.False(HitscanCombat.Fire(world, player)); Assert.Empty(world.DrainSoundRequests());
        }
    }

    [Theory]
    [MemberData(nameof(Weapons))]
    public void DryFireDoesNotQueueSound(WeaponKind weapon, string _)
    {
        var sim = Room(weapon); var player = sim.Players.Single();
        player.Inventory.Bullets = 0; player.Inventory.Shells = 0;
        Assert.False(HitscanCombat.Fire(sim, player)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static AuthoritySimulation Room(WeaponKind weapon)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon;
        player.Inventory.Selected = weapon; player.Inventory.Shells = 20;
        return sim;
    }
}
