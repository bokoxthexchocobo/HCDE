using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingPitchTests
{
    [Theory]
    [InlineData(1, 90)]
    [InlineData(2, -90)]
    [InlineData(2035, -90)]
    [InlineData(3001, 450)]
    [InlineData(2035, 65537)]
    [InlineData(2035, 32768)]
    [InlineData(2035, 32767)]
    public void ImportedPitchMatchesSpawnPathAndSurvivesArchive(int type, int pitch)
    {
        var text = $"namespace = \"ZDoom\"; sector {{ heightceiling = 128; }} "
            + $"thing {{ type = {type}; pitch = {pitch}; roll = 90; skill3 = true; single = true; coop = true; }}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var actor = Assert.Single(sim.Actors);
        var playerStart = actor is PlayerPawn;
        double expected = playerStart ? 0 : unchecked((short)pitch);
        Assert.Equal(expected, actor.PitchDegrees);
        Assert.Equal(playerStart ? 0u : BamAngle.FromDegrees(90).Raw, actor.Roll.Raw);
        var archive = SimSavegame.Write(sim);
        actor.PitchDegrees = 0;
        SimSavegame.Apply(sim, archive);
        Assert.Equal(expected, actor.PitchDegrees);
    }
}
