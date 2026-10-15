using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainStepChecksumTests
{
    [Theory]
    [InlineData("liquid")]
    [InlineData("stepvolume 0.3")]
    [InlineData("walksteptics 12")]
    [InlineData("runsteptics 6")]
    [InlineData("stepdistance 32.125")]
    [InlineData("stepdistanceminvel 0.25")]
    [InlineData("stepsounds step/generic")]
    [InlineData("leftstepsounds step/left")]
    [InlineData("rightstepsounds step/right")]
    public void ConvertedTerrainMetadataParticipatesInChecksum(string fields)
    {
        var baseline = Create("");
        var changed = Create(fields);
        Assert.NotEqual(baseline.Checksum, changed.Checksum);
        Assert.Equal(changed.Checksum, Create(fields).Checksum);
    }

    private static AuthoritySimulation Create(string fields)
    {
        Assert.True(TerrainDefinitionParser.TryParse($"terrain Mud {{ {fields} }}", out var definitions, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = definitions,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
    }
}
