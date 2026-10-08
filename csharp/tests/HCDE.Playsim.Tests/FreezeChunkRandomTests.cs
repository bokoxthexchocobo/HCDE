using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeChunkRandomTests
{
    [Fact]
    public void FixedRangeSkipsDrawAndByteOutputUsesNativeNamedSeed()
    {
        var sim = Room(); var state = NativeStateRandom.Seed(42, 0xd9e163dbu);
        Assert.Equal(0u, sim.NextFreezeChunkRange(1));
        for (var i = 0; i < 64; i++) Assert.Equal(NativeStateRandom.Next(ref state) & 255, sim.NextFreezeChunkByte());
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.NextFreezeChunkRange(0));
    }

    [Fact]
    public void ChunkCreationFollowsNativeDrawOrderWithoutCombatDraws()
    {
        var sim = Room(); var control = Room();
        var count = 40 + (int)control.NextFreezeChunkRange(10) + 1;
        sim.SpawnIceChunks(new Actor
        { Simulation = sim, Level = sim.Level, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.Equal(count, chunks.Length);
        foreach (var chunk in chunks)
        {
            var x = ((int)control.NextFreezeChunkByte() - 128) * 20.0 / 128;
            var y = ((int)control.NextFreezeChunkByte() - 128) * 20.0 / 128;
            var z = control.NextFreezeChunkByte() * 64.0 / 255;
            Assert.Equal(Fixed.FromDouble(x), chunk.X); Assert.Equal(Fixed.FromDouble(y), chunk.Y);
            Assert.Equal(Fixed.FromDouble(z), chunk.Z);
            Assert.Equal((int)control.NextFreezeChunkRange(3), chunk.States.Current);
            Assert.Equal(control.NextIceTics(), chunk.RemainingTics);
            Assert.Equal(Fixed.FromDouble(((int)control.NextFreezeChunkByte() - (int)control.NextFreezeChunkByte()) / 128.0), chunk.VelocityX);
            Assert.Equal(Fixed.FromDouble(((int)control.NextFreezeChunkByte() - (int)control.NextFreezeChunkByte()) / 128.0), chunk.VelocityY);
        }
        Assert.Equal(control.NextFreezeChunkByte(), sim.NextFreezeChunkByte());
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
    }

    [Fact]
    public void BoundedRandomRejectsLowValuesBeforeModulo()
    {
        ulong actual = 0, expected = 0;
        const uint bound = 0x80000001;
        var threshold = unchecked(0u - bound) % bound;
        uint value; var draws = 0;
        do { value = NativeStateRandom.Next(ref expected); draws++; } while (value < threshold);
        Assert.True(draws > 1);
        Assert.Equal(value % bound, NativeStateRandom.NextBounded(ref actual, bound));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SaveContinuesStreamAndOlderSaveResetsIt()
    {
        var sim = Room(); var older = SimSavegame.Write(sim);
        sim.NextFreezeChunkByte(); var saved = SimSavegame.Write(sim); var next = sim.NextFreezeChunkByte();
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored)); Assert.Equal(next, restored.NextFreezeChunkByte());
        SimSavegame.Apply(sim, older); Assert.Equal(Room().NextFreezeChunkByte(), sim.NextFreezeChunkByte());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidArchiveTrailerFailsValidation(bool header)
    {
        var sim = Room(); sim.NextFreezeChunkByte(); var saved = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(header ? saved.Length - 16 : saved.Length - 4), header ? 115 : 15);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-freezechunks-header" : "save-freezechunks-size", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] }, rngSeed: 42);
}
