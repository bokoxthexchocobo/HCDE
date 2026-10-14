using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupSoundTests
{
    [Theory]
    [InlineData(PickupCatalog.HealthBonus, "misc/health_pkup")]
    [InlineData(PickupCatalog.BlueCard, "misc/k_pkup")]
    [InlineData(PickupCatalog.Stimpack, "misc/health_pkup")]
    [InlineData(PickupCatalog.Medikit, "misc/health_pkup")]
    [InlineData(PickupCatalog.ArmorBonus, "misc/armor_pkup")]
    [InlineData(PickupCatalog.GreenArmor, "misc/armor_pkup")]
    [InlineData(PickupCatalog.MegaArmor, "misc/armor_pkup")]
    [InlineData(PickupCatalog.Clip, "misc/ammo_pkup")]
    [InlineData(PickupCatalog.ShellBox, "misc/ammo_pkup")]
    [InlineData(PickupCatalog.Backpack, "misc/i_pkup")]
    [InlineData(PickupCatalog.Shotgun, "misc/w_pkup")]
    [InlineData(PickupCatalog.Chainsaw, "misc/w_pkup")]
    [InlineData(PickupCatalog.Soulsphere, "misc/p_pkup")]
    [InlineData(PickupCatalog.Megasphere, "misc/p_pkup")]
    [InlineData(PickupCatalog.Berserk, "misc/p_pkup")]
    public void AcceptedContactUsesNativeDoomSoundAndPlayerSource(int type, string expected)
    {
        var (sim, player, _) = Room(type); player.Health = 50; sim.Tick(); var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(expected, sound.Sound); Assert.Equal(player.Id, sound.ActorId); Assert.Equal(3, sound.Channel);
        Assert.Equal((int)SimulationSoundFlags.MaybeLocal, sound.Flags); Assert.Equal(1, sound.Attenuation);
        Assert.True(sound.PickupSound);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void QuietSuppressesSoundButNoScreenFlashDoesNot(bool quiet, bool noFlash, bool audible)
    {
        var (sim, player, item) = Room(PickupCatalog.HealthBonus); item.PickupQuiet = quiet; item.NoScreenFlash = noFlash;
        sim.Tick(); Assert.Equal(101, player.Health); Assert.True(item.Destroyed);
        Assert.Equal(audible ? 1 : 0, sim.DrainSoundRequests().Length); Assert.Equal(0, player.BonusCount);
    }

    [Fact]
    public void RejectedContactAndQuietGrantDoNotMakePickupSounds()
    {
        var (sim, player, _) = Room(PickupCatalog.Medikit); sim.Tick();
        Assert.Empty(sim.DrainSoundRequests());
        ActorInventoryActions.GiveInventory(player, "HealthBonus"); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void SoundIsRequestedBeforeFlashCounterIsSet()
    {
        var (sim, player, _) = Room(PickupCatalog.HealthBonus); int? observed = null;
        sim.SoundRequestsReady += () => observed = player.BonusCount;
        sim.Tick(); Assert.Equal(0, observed); Assert.Equal(6, player.BonusCount);
    }

    [Fact]
    public void RestoredContactReplaysSoundRequest()
    {
        var (sim, _, _) = Room(PickupCatalog.Berserk); var saved = SimSavegame.Write(sim, true);
        sim.Tick(); var sounds = sim.DrainSoundRequests();
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(sounds, sim.DrainSoundRequests());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Item) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }]
        });
        sim.DrainSoundRequests(); return (sim, sim.Players.Single(), sim.Actors.Single(a => a.DoomEdNum == type));
    }
}
