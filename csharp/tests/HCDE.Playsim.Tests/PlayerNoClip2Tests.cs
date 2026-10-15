using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerNoClip2Tests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    public void PropertySetsBothNoclipModes(int value)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, value, 20));
        Assert.Equal(value != 0, player.NoClip); Assert.Equal(value != 0, player.NoClip2);
        Assert.Equal(value != 0, player.NoGravity);
    }

    [Theory]
    [InlineData(-128, -1)]
    [InlineData(128, 1)]
    public void VerticalInputWorksWithoutFlyFlag(int input, int direction)
    {
        var sim = Room(); var player = Enable(sim); player.Z = Fixed.FromInt(48);
        player.Pending = new PlayerCommand { UpMove = (short)input }; player.Tick();
        Assert.Equal(48 + direction, player.Z.ToDouble());
        Assert.False(player.Fly); Assert.True(player.NoGravity); Assert.True(player.OnGround);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BaseSectorPlanesStillClipNoclip2(bool ceiling)
    {
        var sim = Room(); var player = Enable(sim);
        player.Z = Fixed.FromInt(ceiling ? 200 : -20); ActorPhysics.FitToSector(sim, player, false);
        Assert.Equal(ceiling ? 128 - player.Height.ToDouble() : 0, player.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Noclip2UsesGroundFrictionAndSuppressesBobEvenWithFly(bool fly)
    {
        var sim = Room(); var player = Enable(sim); player.Fly = fly; player.Z = Fixed.FromInt(48);
        player.VelocityX = Fixed.FromInt(2); player.VelocityZ = Fixed.FromInt(1); player.Tick();
        Assert.Equal(Fixed.FromDouble(2 * ActorPhysics.GroundFriction), player.VelocityX);
        Assert.Equal(Fixed.FromDouble(ActorPhysics.FlyingFriction), player.VelocityZ);
        Assert.Equal(0, player.MovementBob); Assert.Equal(0, player.ViewBobOffset);
    }

    [Fact]
    public void ClearingOrdinaryNoclipClearsDependentModeOnTick()
    {
        var sim = Room(); var player = Enable(sim);
        ActorSpecialActions.CallSpecial(player, 191, 0, 0, 19); player.Tick();
        Assert.False(player.NoClip2); Assert.False(player.NoClip); Assert.False(player.NoGravity);
    }

    [Fact]
    public void DisablingNoclip2KeepsFlightGravityPolicy()
    {
        var sim = Room(); var player = Enable(sim); player.Fly = true;
        ActorSpecialActions.CallSpecial(player, 191, 0, 0, 20);
        Assert.False(player.NoClip); Assert.False(player.NoClip2); Assert.True(player.NoGravity);
    }

    [Fact]
    public void SavedFlightInputReplaysAndOlderSaveClearsMode()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        Enable(sim); player.Z = Fixed.FromInt(48); var saved = SimSavegame.Write(sim, true);
        Move(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Move(); Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.False(player.NoClip2);
        void Move() { player.Pending = new PlayerCommand { ForwardMove = 100, UpMove = 128 }; player.Tick(); }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidSavedModeRejectsBeforeMutation(int flag)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].PlayerNoClip2Flag = flag;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.False(sim.Players.Single().NoClip2);
    }

    private static PlayerPawn Enable(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 1, 20));
        return player;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
