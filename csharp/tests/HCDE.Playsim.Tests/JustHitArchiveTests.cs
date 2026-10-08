using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class JustHitArchiveTests
{
    [Fact]
    public void MixedFlagsRoundTripAndOlderArchiveClearsLaterHit()
    {
        var sim = Room(); var old = SimSavegame.Write(sim);
        sim.Actors[1].JustHit = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(105, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); loaded.Players.Single().JustHit = true;
        SimSavegame.Apply(loaded, bytes);
        Assert.False(loaded.Players.Single().JustHit); Assert.True(loaded.Actors[1].JustHit);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, old);
        Assert.All(loaded.Actors, actor => Assert.False(actor.JustHit));
        Assert.Equal(old, SimSavegame.Write(loaded));
    }

    [Theory]
    [InlineData(0, "save-just-hit-size")]
    [InlineData(1, "save-just-hit-header")]
    [InlineData(2, "save-just-hit-value")]
    [InlineData(3, "save-just-hit-value")]
    public void MalformedTrailerRejectedBeforeMutation(int corruption, string expected)
    {
        var sim = Room(); sim.Actors[1].JustHit = true;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 105);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        if (corruption == 3) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 12), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
        Assert.Equal(expected, Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes)).Message);
        Assert.True(sim.Actors[1].JustHit); Assert.False(sim.Players.Single().JustHit);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 100 }],
        Sectors = [new LevelSector { CeilingHeight = 512 }] });
}
