using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLevelGravityTests
{
    [Fact]
    public void ConfiguredGravityFallsAndOlderSaveRestoresMapFallback()
    {
        var level = new PlayLevel { LevelGravity = 400, Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] };
        var sim = AuthoritySimulation.Start(level, rngSeed: 42); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(64); player.OnGround = false;
        var legacy = SimSavegame.Write(sim); sim.SetLevelGravity(0); SimSavegame.Apply(sim, legacy);
        Assert.Equal(400, sim.LevelGravity); sim.Tick(); Assert.Equal(Fixed.FromDouble(-0.5), player.VelocityZ);
        Assert.NotEqual(Room().Checksum, AuthoritySimulation.Start(level, rngSeed: 42).Checksum);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 400)]
    [InlineData(false, -800)]
    public void ScriptedGravityUsesNativeUnitsAndResumesAfterSave(bool direct, int gravity)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Z = Fixed.FromInt(64); player.OnGround = false;
        var words = direct ? new[] { 139, gravity * 65536, 1 } : new[] { 3, gravity * 65536, 138, 1 };
        Add(sim, words); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var restored = Room(); Add(restored, words); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.Tick(); restored.Tick(); Assert.Equal(Fixed.FromDouble(-gravity / 800.0), player.VelocityZ);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    [Fact]
    public void OlderSaveResetsGravityAndMalformedTrailerFails()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.SetLevelGravity(0);
        SimSavegame.Apply(sim, legacy); Assert.Equal(800, sim.LevelGravity);
        sim.SetLevelGravity(0); var saved = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt64LittleEndian(saved.AsSpan(saved.Length - 12), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-levelgravity-value", error);
    }

    [Fact]
    public void TruncatedDirectGravityDoesNotMutateLevel()
    {
        var sim = Room(); Add(sim, [139]); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(800, sim.LevelGravity);
    }

    private static void Add(AuthoritySimulation sim, int[] words)
    {
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    }, rngSeed: 42);
}
