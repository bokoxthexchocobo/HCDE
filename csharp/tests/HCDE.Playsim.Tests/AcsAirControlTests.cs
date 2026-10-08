using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAirControlTests
{
    [Fact]
    public void OlderSaveResetsOverrideAndInvalidTrailerFails()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.SetAirControl(0);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0.5, sim.AirControl);
        sim.SetAirControl(0); var saved = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt64LittleEndian(saved.AsSpan(saved.Length - 12), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-aircontrol-value", error);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 16384)]
    [InlineData(false, -65536)]
    public void ScriptedAirControlUpdatesMovementAndSurvivesSave(bool direct, int value)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Z = Fixed.FromInt(64); player.OnGround = false;
        var words = direct ? new[] { 141, value, 1 } : new[] { 3, value, 140, 1 };
        Add(sim, words); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(value / 65536.0, sim.AirControl);
        var saved = SimSavegame.Write(sim); var restored = Room(); Add(restored, words); SimSavegame.Apply(restored, saved);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick(); restored.Tick(); Assert.Equal(Fixed.FromDouble(value / 65536.0), player.X);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    [Fact]
    public void TruncatedDirectOperandDoesNotChangeAirControl()
    {
        var sim = Room(); Add(sim, [141]); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0.5, sim.AirControl);
    }

    private static void Add(AuthoritySimulation sim, int[] words)
    {
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        AirControl = 0.5, Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    }, rngSeed: 42);
}
