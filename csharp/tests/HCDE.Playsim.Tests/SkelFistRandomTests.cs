using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkelFistRandomTests
{
    [Fact]
    public void NamedFistStreamRestoresAcrossSaveAndLegacyState()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var first = sim.NextSkelFistDamage(); Assert.InRange(first, 6, 60); Assert.Equal(0, first % 6);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(129, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextSkelFistDamage()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextSkelFistDamage()).ToArray());
        Assert.Equal(combat, sim.CombatRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().SkelFistRandomState);
        Assert.Equal(first, sim.NextSkelFistDamage());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 129);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-skelfistrandom-header", error);
    }
}
