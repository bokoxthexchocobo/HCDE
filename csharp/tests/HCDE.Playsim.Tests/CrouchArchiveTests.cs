using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrouchArchiveTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(8, false)]
    [InlineData(8, true)]
    public void LoadedPlayerContinuesTheSavedCrouch(int ticks, bool jump)
    {
        var source = Room();
        for (var i = 0; i < ticks; i++) Step(source, new PlayerCommand { Crouch = true });
        if (jump) Step(source, new PlayerCommand { Jump = true });
        var player = source.Players.Single();
        var bytes = SimSavegame.Write(source);
        Assert.Equal(91, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var loaded = Room(); loaded.RestoreState(state);
        var restored = loaded.Players.Single();
        Assert.Equal(player.CrouchFactor, restored.CrouchFactor);
        Assert.Equal(player.FullHeight, restored.FullHeight);
        Assert.Equal(player.ViewHeight, restored.ViewHeight);
        Assert.Equal(player.UncrouchLocked, restored.UncrouchLocked);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        for (var i = 0; i < 8; i++)
        {
            Step(source, new PlayerCommand()); Step(loaded, new PlayerCommand());
            Assert.Equal(player.CrouchFactor, restored.CrouchFactor);
            Assert.Equal(player.Height, restored.Height);
            Assert.Equal(player.ViewHeight, restored.ViewHeight);
        }
    }

    [Fact]
    public void LoadingStandingSaveResetsLaterCrouch()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim);
        for (var i = 0; i < 8; i++) Step(sim, new PlayerCommand { Crouch = true });
        SimSavegame.Apply(sim, saved);
        var player = sim.Players.Single();
        Assert.Equal(1, player.CrouchFactor);
        Assert.Equal(56, player.Height.ToDouble());
        Assert.Equal(41, player.ViewHeight);
        Assert.False(player.UncrouchLocked);
    }

    [Fact]
    public void CustomHeightAndDeathViewSurviveLoad()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.DefaultViewHeight = Fixed.FromInt(50); player.ViewHeight = 19;
        player.FullHeight = 72; player.Health = 0;
        var saved = SimSavegame.Write(sim);
        var loaded = Room(); SimSavegame.Apply(loaded, saved);
        var restored = loaded.Players.Single();
        Assert.Equal(72, restored.FullHeight);
        Assert.Equal(50, restored.DefaultViewHeight.ToDouble());
        Assert.Equal(19, restored.ViewHeight);
        loaded.Tick(); Assert.Equal(18, restored.ViewHeight);
    }

    [Theory]
    [InlineData(8, 2)]
    [InlineData(40, 2)]
    [InlineData(0, 91)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); Step(sim, new PlayerCommand { Crouch = true });
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static void Step(AuthoritySimulation sim, PlayerCommand command) { sim.QueueCommand(0, command); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
