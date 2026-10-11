using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerJumpSoundTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(0)]
    [InlineData(-2)]
    public void GroundJumpEmitsNativeBodySoundRegardlessOfImpulse(int jump)
    {
        var sim = Room(); var player = sim.Players.Single(); player.JumpZ = Fixed.FromInt(jump);
        Step(sim);
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("*jump", sound.Sound); Assert.Equal(4, sound.Channel);
        Assert.Equal(player.Id, sound.ActorId); Assert.Equal(1, sound.Volume);
        Assert.Equal(1, sound.Attenuation); Assert.Equal(0, sound.Flags);
    }

    [Fact]
    public void HeldJumpDoesNotRepeatSoundWhileAirborne()
    {
        var sim = Room(); Step(sim); Assert.Single(sim.DrainSoundRequests());
        for (var i = 0; i < 5; i++) Step(sim);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void FlightJumpDoesNotEmitGroundJumpSound()
    {
        var sim = Room(); sim.Players.Single().NoGravity = true;
        Step(sim); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void CrouchedJumpOnlyUncrouchesWithoutSound()
    {
        var sim = Room();
        for (var i = 0; i < 8; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        Step(sim); Assert.True(sim.Players.Single().UncrouchLocked);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void JumpCooldownPreventsSound()
    {
        var sim = Room(); sim.Players.Single().JumpTics = 5;
        Step(sim); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void MissingJumpInputDoesNotEmitSound()
    {
        var sim = Room(); sim.Tick(); Assert.Empty(sim.DrainSoundRequests());
    }

    private static void Step(AuthoritySimulation sim)
    { sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick(); }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
