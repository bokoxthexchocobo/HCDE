using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainProtectionChecksumTests
{
    [Fact]
    public void ProtectionMetadataChangesSimulationChecksum()
    {
        static AuthoritySimulation Create(bool protection) => AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = [new LevelTerrainDefinition("Lava", AllowProtection: protection)],
            DefaultTerrain = "Lava",
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }]
        });
        Assert.NotEqual(Create(false).Checksum, Create(true).Checksum);
    }
}
