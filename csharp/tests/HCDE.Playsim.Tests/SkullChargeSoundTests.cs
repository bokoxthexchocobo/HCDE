using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargeSoundTests
{
    [Fact]
    public void DirectChargePlaysConfiguredAttackOnVoiceChannel()
    {
        var sim = Room(); var soul = Soul(sim);
        soul.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/skullattack" };
        soul.FullVolumeActive = soul.FullVolumeSee = soul.FullVolumeDeath = true;
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(soul.Id, sound.ActorId); Assert.Equal("custom/skullattack", sound.Sound);
        Assert.Equal(2, sound.Channel); Assert.Equal(0, sound.Flags);
        Assert.Equal(1, sound.Volume); Assert.Equal(1, sound.Attenuation);
        Assert.False(sound.OnlyIfChannelFree); Assert.True(soul.Brain.Charging);
    }

    [Fact]
    public void WindupLoadPlaysAtChargeEntryAndChargingLoadDoesNotReplay()
    {
        var sim = Room(); var soul = Soul(sim); soul.ReactionTime = 0;
        soul.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/skullattack" };
        for (var i = 0; i < 5; i++) Chase(soul);
        Assert.Empty(sim.DrainSoundRequests());
        var windupSave = SimSavegame.Write(sim);
        for (var i = 0; i < 6; i++) Chase(soul);
        var expected = Assert.Single(sim.DrainSoundRequests());
        Assert.True(soul.Brain!.Charging);
        SimSavegame.Apply(sim, windupSave);
        Assert.Empty(sim.DrainSoundRequests());
        for (var i = 0; i < 6; i++) Chase(soul);
        Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()));
        var chargeSave = SimSavegame.Write(sim);
        soul.Brain.StopCharge(soul);
        SimSavegame.Apply(sim, chargeSave);
        for (var i = 0; i < 12; i++) Chase(soul);
        Assert.Empty(sim.DrainSoundRequests()); Assert.True(soul.Brain.Charging);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UnconfiguredAttackHasNoSoundRequest(string? sound)
    {
        var sim = Room(); var soul = Soul(sim);
        if (sound != null) soul.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = sound };
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        Assert.Empty(sim.DrainSoundRequests()); Assert.True(soul.Brain.Charging);
        Assert.Equal(20, soul.VelocityX.ToDouble());
    }

    private static void Chase(Actor soul) => ActorChaseActions.ExtChase(soul, false, true, playActive: false);
    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 100 }, new LevelThing { Type = 3006 }] });
}
