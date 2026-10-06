using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsJumpZTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(1048576)]
    public void SetterAndGetterPreserveSignedFixedValue(int value)
    {
        var sim = Room(); var player = sim.Players.Single();
        Run(sim, player, [3, 0, 3, 12, 3, value, 245, 3, 7, 3, 0, 3, 12, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, player.JumpZ.Raw); Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(12)]
    public void JumpUsesScriptConfiguredVelocity(int velocity)
    {
        var sim = Room(); var player = sim.Players.Single();
        Run(sim, player, [3, 0, 3, 12, 3, velocity * 65536, 245, 1]);
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(velocity - 1, player.VelocityZ.ToDouble());
        Assert.Equal(velocity, player.Z.ToDouble()); // Native movement precedes gravity.
    }

    [Fact]
    public void NonPlayerSetterIsIgnoredAndGetterReturnsZero()
    {
        var sim = Room(); var monster = sim.AddBot(128, 0);
        Run(sim, monster, [3, 0, 3, 12, 3, 12345, 245, 3, 7, 3, 0, 3, 12, 246, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void ConfiguredJumpParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().JumpZ = Fixed.FromInt(12);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
