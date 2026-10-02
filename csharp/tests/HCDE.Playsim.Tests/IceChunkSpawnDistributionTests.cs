using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkSpawnDistributionTests
{
    [Theory]
    [InlineData(20, 64)]
    [InlineData(40, 128)]
    public void IceDebrisSpawnsWithinCorpseBoundsWithNativeVelocityScale(int radius, int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var corpse = sim.AddBot(100, -100); corpse.Radius = Fixed.FromInt(radius);
        corpse.Height = Fixed.FromInt(height); corpse.Z = Fixed.FromInt(100);
        sim.SpawnIceChunks(corpse);
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            Assert.InRange(chunk.X.ToDouble(), 100 - radius, 100 + radius);
            Assert.InRange(chunk.Y.ToDouble(), -100 - radius, -100 + radius);
            Assert.InRange(chunk.Z.ToDouble(), 100, 100 + height);
            Assert.InRange(chunk.VelocityX.ToDouble(), -255.0 / 128, 255.0 / 128);
            Assert.InRange(chunk.VelocityY.ToDouble(), -255.0 / 128, 255.0 / 128);
            Assert.InRange(chunk.VelocityZ.ToDouble(), 0, 4);
            Assert.Equal(chunk.X, chunk.PreviousX); Assert.Equal(chunk.Y, chunk.PreviousY);
        }
        Assert.Contains(chunks, chunk => chunk.X.ToDouble() < 100);
        Assert.Contains(chunks, chunk => chunk.X.ToDouble() > 100);
        Assert.Contains(chunks, chunk => chunk.Y.ToDouble() < -100);
        Assert.Contains(chunks, chunk => chunk.Y.ToDouble() > -100);
        Assert.Contains(chunks, chunk => Math.Abs(chunk.VelocityX.ToDouble()) > 1 || Math.Abs(chunk.VelocityY.ToDouble()) > 1);
    }
}
