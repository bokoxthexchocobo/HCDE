using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkSectorRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FreshRestoreRetainsElevatedSectorAndContinuesPhysics(bool started)
    {
        var source = Room();
        source.SpawnIceChunks(new Actor
        {
            Simulation = source, Level = source.Level, X = Fixed.FromInt(384), Z = Fixed.FromInt(64),
            Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64),
        });
        if (started) source.Tick();
        var chunks = source.Actors.OfType<IceChunkActor>().ToArray();
        Assert.NotEmpty(chunks); Assert.All(chunks, chunk => Assert.Equal(1, chunk.SectorIndex));
        var saved = SimSavegame.Write(source); var restored = Room();
        SimSavegame.Apply(restored, saved);
        var loaded = restored.Actors.OfType<IceChunkActor>().ToArray();
        Assert.Equal(chunks.Length, loaded.Length);
        for (var i = 0; i < chunks.Length; i++)
        {
            Assert.Equal(1, loaded[i].SectorIndex);
            Assert.Equal(chunks[i].Z, loaded[i].Z);
            Assert.Equal(chunks[i].OnGround, loaded[i].OnGround);
            Assert.Equal(chunks[i].RemainingTics, loaded[i].RemainingTics);
        }
        Assert.Equal(saved, SimSavegame.Write(restored));
        source.Tick(); restored.Tick();
        Assert.Equal(source.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(source), SimSavegame.Write(restored));
    }

    private static AuthoritySimulation Room()
    {
        var lines = new List<LevelLine>();
        var level = new PlayLevel
        {
            Lines = lines,
            Sectors = [new LevelSector { CeilingHeight = 256 },
                new LevelSector { FloorHeight = 64, CeilingHeight = 320, Gravity = 0.5 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Things = [new LevelThing { Type = 1 }],
        };
        for (var sector = 0; sector < 2; sector++)
        {
            var x = sector * 384;
            lines.Add(new LevelLine { X1 = x - 128, Y1 = -128, X2 = x + 128, Y2 = -128, SideFront = sector, SideBack = -1 });
            lines.Add(new LevelLine { X1 = x + 128, Y1 = -128, X2 = x + 128, Y2 = 128, SideFront = sector, SideBack = -1 });
            lines.Add(new LevelLine { X1 = x + 128, Y1 = 128, X2 = x - 128, Y2 = 128, SideFront = sector, SideBack = -1 });
            lines.Add(new LevelLine { X1 = x - 128, Y1 = 128, X2 = x - 128, Y2 = -128, SideFront = sector, SideBack = -1 });
        }
        return AuthoritySimulation.Start(level, rngSeed: 42);
    }
}
