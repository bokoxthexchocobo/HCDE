using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerJumpCooldownTests
{
    [Theory]
    [InlineData(-17, true, -18, false)]
    [InlineData(-18, true, -1, true)]
    [InlineData(-18, false, -19, false)]
    [InlineData(1, true, -1, true)]
    public void JumpReadinessUsesCountdownAndGroundedState(int initial, bool ground, int expected, bool jumps)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.JumpTics = initial;
        if (!ground) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(expected, player.JumpTics);
        Assert.Equal(jumps ? 8 : ground ? 0 : 64, player.Z.ToDouble());
    }

    [Fact]
    public void EarlyLandingDoesNotPermitRepeatedHeldJump()
    {
        var sim = Room(); var player = sim.Players.Single(); player.JumpZ = Fixed.FromInt(1);
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(-1, player.JumpTics);
        player.Z = default; player.VelocityZ = default; player.OnGround = true;
        for (var i = 0; i < 17; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
            Assert.Equal(0, player.Z.Raw);
        }
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(1, player.Z.ToDouble()); Assert.Equal(-1, player.JumpTics);
    }

    [Fact]
    public void ArchiveRestoresCooldownAndFutureReadiness()
    {
        var sim = Room(); var player = sim.Players.Single(); player.JumpTics = -17;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(92, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        Assert.Equal(-17, loaded.Players.Single().JumpTics);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        for (var i = 0; i < 2; i++)
        {
            var command = new PlayerCommand { Jump = true };
            sim.QueueCommand(0, command); loaded.QueueCommand(0, command); sim.Tick(); loaded.Tick();
            Assert.Equal(player.JumpTics, loaded.Players.Single().JumpTics);
            Assert.Equal(player.Z, loaded.Players.Single().Z);
        }
    }

    [Fact]
    public void LegacyStandingSaveResetsCooldown()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        sim.Players.Single().JumpTics = -5; SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, sim.Players.Single().JumpTics);
    }

    [Fact]
    public void CounterChangesChecksumWithoutChangingPosition()
    {
        var first = Room(); var second = Room(); second.Players.Single().JumpTics = -1;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(0, 92)]
    [InlineData(4, -1)]
    [InlineData(8, 2)]
    public void InvalidArchiveHeaderOrPresenceIsRejected(int offset, int value)
    {
        var sim = Room(); sim.Players.Single().JumpTics = -1;
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
