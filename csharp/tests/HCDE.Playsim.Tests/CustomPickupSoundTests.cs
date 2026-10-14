using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CustomPickupSoundTests
{
    [Theory]
    [InlineData(false, 1f)]
    [InlineData(true, 0f)]
    public void CustomSoundUsesConfiguredAttenuationAndCollector(bool noAttenuation, float expected)
    {
        var (sim, player, item) = Room(); item.NoAttenPickupSound = noAttenuation;
        item.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pickup] = "custom/pickup" };
        sim.Tick(); var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("custom/pickup", sound.Sound); Assert.Equal(expected, sound.Attenuation);
        Assert.Equal(player.Id, sound.ActorId); Assert.Equal(3, sound.Channel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    public void SilentOverrideStillGrantsItemAndFlash(string name)
    {
        var (sim, player, item) = Room();
        item.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pickup] = name };
        sim.Tick(); Assert.Empty(sim.DrainSoundRequests()); Assert.Equal(101, player.Health); Assert.Equal(6, player.BonusCount);
    }

    [Fact]
    public void SoundAndAttenuationRestoreBeforeContactAndReplay()
    {
        var (sim, _, item) = Room(); var old = SimSavegame.Write(sim, true); item.NoAttenPickupSound = true;
        item.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pickup] = "custom/pickup" };
        var saved = SimSavegame.Write(sim, true); Assert.Equal(254, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        sim.Tick(); var expected = sim.DrainSoundRequests(); SimSavegame.Apply(sim, saved);
        sim.Tick(); Assert.Equal(expected, sim.DrainSoundRequests()); SimSavegame.Apply(sim, old);
        var restored = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus);
        Assert.False(restored.NoAttenPickupSound); Assert.False(restored.ActorSounds.ContainsKey(ActorSoundType.Pickup));
    }

    [Fact]
    public void InvalidSavedAttenuationRejects()
    {
        var (sim, _, item) = Room(); item.NoAttenPickupSound = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Item) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus));
    }
}
