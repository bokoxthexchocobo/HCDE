using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoFrictionTests
{
    [Theory]
    [InlineData(false, 4)]
    [InlineData(true, 4)]
    [InlineData(false, 0.03125)]
    public void FlagPreservesHorizontalMomentumIncludingBelowStopSpeed(bool flight, double speed)
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorPropertyActions.ChangeFlag(player, "NOFRICTION", true);
        player.Fly = flight; player.NoGravity = flight;
        player.VelocityX = Fixed.FromDouble(speed); player.VelocityY = Fixed.FromDouble(-speed);
        sim.Tick(); Assert.True(player.NoFriction);
        Assert.Equal(speed, player.X.ToDouble()); Assert.Equal(-speed, player.Y.ToDouble());
        Assert.Equal(speed, player.VelocityX.ToDouble()); Assert.Equal(-speed, player.VelocityY.ToDouble());
        ActorPropertyActions.ChangeFlag(player, "NOFRICTION", false); sim.Tick();
        Assert.True(player.VelocityX.ToDouble() < speed);
    }

    [Fact]
    public void SaveRestoresFlagAndLegacySaveClearsIt()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.Players.Single().NoFriction = true;
        var bytes = SimSavegame.Write(sim); var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        Assert.True(loaded.Players.Single().NoFriction); Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, legacy); Assert.False(loaded.Players.Single().NoFriction);
    }

    [Fact]
    public void FlagChangesChecksumWithoutMotion()
    {
        var first = Room(); var second = Room(); second.Players.Single().NoFriction = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(0, 95)]
    [InlineData(4, -1)]
    [InlineData(8, 2)]
    public void MalformedFlagArchiveIsRejected(int offset, int value)
    {
        var sim = Room(); sim.Players.Single().NoFriction = true;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
