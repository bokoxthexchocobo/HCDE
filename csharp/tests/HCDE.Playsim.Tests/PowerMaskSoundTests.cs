using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PowerMaskSoundTests
{
    [Fact]
    public void BreathingUsesAutomaticChannelEverySixtyFourLevelTics()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerMask();
        sim.Tick(); var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("misc/mask", sound.Sound); Assert.Equal(player.Id, sound.ActorId);
        Assert.Equal(0, sound.Channel); Assert.Equal(1f, sound.Volume); Assert.Equal(1f, sound.Attenuation);
        Assert.False(sound.Loop); Assert.False(sound.Local);
        for (var i = 1; i < 64; i++) sim.Tick();
        Assert.Empty(sim.DrainSoundRequests());
        sim.Tick(); Assert.Equal("misc/mask", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void MissingOrExpiringMaskDoesNotEmitBreathing(int duration)
    {
        var sim = Room(); sim.Players.Single().GivePowerMask(duration); sim.Tick();
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void RadiationSuitAloneDoesNotEmitMaskSound()
    {
        var sim = Room(); sim.Players.Single().GivePowerIronFeet(); sim.Tick();
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void RestoreContinuesLevelCadenceWithoutRestartingIt()
    {
        var sim = Room(); sim.Players.Single().GivePowerMask();
        for (var i = 0; i < 63; i++) sim.Tick();
        sim.DrainSoundRequests(); var saved = SimSavegame.Write(sim, true);
        sim.Tick(); Assert.Empty(sim.DrainSoundRequests());
        sim.Tick(); var expected = Assert.Single(sim.DrainSoundRequests());
        SimSavegame.Apply(sim, saved);
        sim.Tick(); Assert.Empty(sim.DrainSoundRequests());
        sim.Tick(); Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
