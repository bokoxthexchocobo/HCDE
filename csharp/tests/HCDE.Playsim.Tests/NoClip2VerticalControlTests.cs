using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClip2VerticalControlTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void VerticalFlightDragUsesNoFrictionFlag(bool noclip2, bool noFriction)
    {
        var (sim, player) = Room();
        if (noclip2) ActorSpecialActions.CallSpecial(player, 191, 0, 1, 20);
        else { player.Fly = true; player.NoGravity = true; }
        player.NoFriction = noFriction; player.Z = Fixed.FromInt(48); player.VelocityZ = Fixed.FromInt(2);
        player.Tick();
        Assert.Equal(50, player.Z.ToDouble());
        Assert.Equal(Fixed.FromDouble(noFriction ? 2 : 2 * ActorPhysics.FlyingFriction), player.VelocityZ);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StopFlightAllowsGravityForOneNoclip2Tick(bool fly)
    {
        var (_, player) = Room(); player.NoClip = player.NoClip2 = true; player.Fly = fly;
        player.Z = Fixed.FromInt(48); player.Pending = new PlayerCommand { UpMove = short.MinValue };
        player.Tick();
        Assert.False(player.NoGravity); Assert.True(player.NoClip2);
        Assert.Equal(Fixed.FromDouble(-ActorPhysics.Gravity), player.VelocityZ);
        player.Tick(); Assert.True(player.NoGravity);
        Assert.Equal(Fixed.FromDouble(-ActorPhysics.Gravity * ActorPhysics.FlyingFriction), player.VelocityZ);
    }

    [Fact]
    public void FrozenMovementMasksStopFlightCommand()
    {
        var (sim, player) = Room(); ActorSpecialActions.CallSpecial(player, 191, 0, 1, 20);
        player.Frozen = true; player.Z = Fixed.FromInt(48);
        player.Pending = new PlayerCommand { UpMove = short.MinValue }; player.Tick();
        Assert.True(player.NoGravity); Assert.Equal(0, player.VelocityZ.Raw);
    }

    [Fact]
    public void SavedStopFlightTransitionReplaysGravityAndDrag()
    {
        var (sim, player) = Room(); ActorSpecialActions.CallSpecial(player, 191, 0, 1, 20);
        player.Z = Fixed.FromInt(48); player.Pending = new PlayerCommand { UpMove = short.MinValue };
        player.Tick(); var saved = SimSavegame.Write(sim, true);
        player.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); player.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        return (sim, sim.Players.Single());
    }
}
