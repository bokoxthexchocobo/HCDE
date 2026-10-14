using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MeleeWeaponSoundTests
{
    [Theory]
    [InlineData(WeaponKind.Fist, true, "*fist")]
    [InlineData(WeaponKind.Fist, false, null)]
    [InlineData(WeaponKind.Chainsaw, true, "weapons/sawhit")]
    [InlineData(WeaponKind.Chainsaw, false, "weapons/sawfull")]
    public void MeleeImpactSelectsNativeWeaponSound(WeaponKind weapon, bool hit, string? expected)
    {
        var (sim, player) = Room(weapon, hit);
        Assert.True(HitscanCombat.Fire(sim, player));
        var sounds = sim.DrainSoundRequests().Where(sound => sound.ActorId == player.Id && sound.Channel == 1).ToArray();
        if (expected is null) Assert.Empty(sounds);
        else Assert.Equal(expected, Assert.Single(sounds).Sound);
    }

    [Theory]
    [InlineData(WeaponKind.Fist)]
    [InlineData(WeaponKind.Chainsaw)]
    public void CooldownDoesNotPlayImpactSound(WeaponKind weapon)
    {
        var (sim, player) = Room(weapon, true); player.WeaponCooldown = 1;
        Assert.False(HitscanCombat.Fire(sim, player)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player) Room(WeaponKind weapon, bool hit)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = hit ? 48 : 500 }]
        });
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        foreach (var actor in sim.Actors) if (actor.Brain is { } brain) brain.Enabled = false;
        sim.DrainSoundRequests();
        return (sim, player);
    }
}
