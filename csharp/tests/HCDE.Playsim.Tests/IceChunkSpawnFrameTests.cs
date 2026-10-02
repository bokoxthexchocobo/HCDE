using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkSpawnFrameTests
{
    [Theory]
    [InlineData(0, 70)]
    [InlineData(1, 70)]
    [InlineData(2, 70)]
    [InlineData(0, 133)]
    [InlineData(1, 133)]
    [InlineData(2, 133)]
    public void SelectedSpawnFrameRunsOnlyRemainingFrames(int frame, int duration)
    {
        var chunk = new IceChunkActor(duration); chunk.States.Enter(chunk, frame);
        Assert.Equal(frame, chunk.States.Current); Assert.Equal(duration, chunk.RemainingTics);
        var lifetime = (4 - frame) * duration;
        for (var tic = 1; tic < lifetime; tic++) { chunk.Tick(); Assert.False(chunk.Destroyed); }
        chunk.Tick(); Assert.True(chunk.Destroyed);
    }

    [Fact]
    public void ShatterSelectsFirstThreeFramesAndPreservesManagedDeterminism()
    {
        var left = Spawn(); var right = Spawn();
        var chunks = left.Actors.OfType<IceChunkActor>().ToArray();
        Assert.NotEmpty(chunks);
        Assert.Contains(chunks, chunk => chunk.States.Current == 0);
        Assert.Contains(chunks, chunk => chunk.States.Current == 1);
        Assert.Contains(chunks, chunk => chunk.States.Current == 2);
        foreach (var chunk in chunks)
        {
            Assert.InRange(chunk.States.Current, 0, 2); Assert.InRange(chunk.RemainingTics, 70, 133);
        }
        Assert.Equal(left.Checksum, right.Checksum);
        left.Tick(); right.Tick(); Assert.Equal(left.Checksum, right.Checksum);
    }

    private static AuthoritySimulation Spawn()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var corpse = sim.AddBot(100, 0); corpse.Radius = Fixed.FromInt(20); corpse.Height = Fixed.FromInt(64);
        sim.SpawnIceChunks(corpse);
        return sim;
    }
}
