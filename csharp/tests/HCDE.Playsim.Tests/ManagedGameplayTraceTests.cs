using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

/// <summary>Managed-only baseline for future Gate 5 native trace pairing. See <c>HCDE_CSHARP_PHASE1_NATIVE_TRACE.md</c>.</summary>
public class ManagedGameplayTraceTests
{
    [Fact]
    public void Map01RoomIdleBaseline_IsStable()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        }, rngSeed: 0x48534445);

        for (var tic = 0; tic < 35; tic++)
            sim.Tick();

        var player = sim.Players.Single();
        Assert.Equal(35, sim.Thinkers.Clock.Tic);
        Assert.Equal(32, player.X.ToDouble());
        Assert.Equal(64, player.Y.ToDouble());
        Assert.Equal(100, player.Health);
        // Includes wall and plane texture transforms in the managed diagnostic hash.
        Assert.Equal(1525314970u, sim.Checksum); // Includes configurable player JumpZ and actor gravity.
    }
}
