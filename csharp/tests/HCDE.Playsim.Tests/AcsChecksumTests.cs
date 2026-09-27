using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsChecksumTests
{
    [Fact]
    public void RegistrationOrderDoesNotChangeChecksum()
    {
        var a = Room(); var b = Room();
        Add(a, 1, 1); Add(a, 2, 56, 3, 1);
        Add(b, 2, 56, 3, 1); Add(b, 1, 1);
        Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
    }

    [Fact]
    public void ExecutionOrderChangesChecksum()
    {
        var a = Room(); var b = Room();
        foreach (var sim in new[] { a, b }) { Add(sim, 1, 56, 3, 1); Add(sim, 2, 56, 3, 1); }
        a.Acs.Enqueue(1); a.Acs.Enqueue(2); b.Acs.Enqueue(2); b.Acs.Enqueue(1);
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PendingStackOrLocalValueChangesChecksumWithSameCodeAndGlobals(bool local)
    {
        var a = Room(); var b = Room();
        Add(a, 9, 3, 10, 181, 0, 1); Add(b, 9, 3, 20, 181, 0, 1);
        foreach (var sim in new[] { a, b })
        {
            sim.Acs.Enqueue(9); sim.Acs.Tick(sim);
            Add(sim, 1, local ? [182, 0, 25, 0, 56, 5, 1] : [182, 0, 56, 5, 1]);
            sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
            Add(sim, 9, 3, 0, 181, 0, 1); sim.Acs.Enqueue(9); sim.Acs.Tick(sim);
        }
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
        a.Tick(); b.Tick(); Assert.NotEqual(a.Checksum, b.Checksum);
    }

    [Fact]
    public void WaitCountdownChangesChecksumWithoutWorldMutation()
    {
        var sim = Room(); Add(sim, 1, 56, 5, 1); sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
        var before = sim.Acs.Checksum; sim.Acs.Tick(sim);
        Assert.NotEqual(before, sim.Acs.Checksum); Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void CallerCannotMutateRegisteredOrRunningBytecode()
    {
        var sim = Room(); var code = Words(56, 1, 10, 112, 7, 35, 1);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        var registered = sim.Acs.Checksum; code.AsSpan().Clear();
        Assert.Equal(registered, sim.Acs.Checksum);
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SameProgramsAndInputsReplayFullChecksums()
    {
        var a = Room(); var b = Room();
        foreach (var sim in new[] { a, b }) { Add(sim, 1, 3, 7, 25, 0, 56, 3, 28, 0, 181, 0, 1); sim.Acs.Enqueue(1); }
        for (var i = 0; i < 10; i++) { a.Tick(); b.Tick(); Assert.Equal(a.Checksum, b.Checksum); }
    }

    private static void Add(AuthoritySimulation sim, int number, params int[] words) =>
        sim.Acs.Add(new AcsProgram { Number = number, Code = Words(words) });
    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }], Format = MapDataFormat.HexenBinary });
}
