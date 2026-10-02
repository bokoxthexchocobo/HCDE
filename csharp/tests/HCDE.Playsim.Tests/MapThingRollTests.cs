using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingRollTests
{
    [Theory]
    [InlineData(1, 90)]
    [InlineData(2035, -90)]
    [InlineData(3001, 450)]
    [InlineData(2035, 65537)]
    public void ImportedRollReachesActorAndArchive(int type, int roll)
    {
        var text = $"namespace = \"ZDoom\"; sector {{ heightceiling = 128; }} "
            + $"thing {{ type = {type}; roll = {roll}; skill3 = true; single = true; coop = true; }}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var actor = Assert.Single(sim.Actors);
        var expected = type == 1 ? 0u : BamAngle.FromDegrees(unchecked((short)roll)).Raw;
        Assert.Equal(expected, actor.Roll.Raw);
        var archive = SimSavegame.Write(sim);
        actor.Roll = new BamAngle(0);
        SimSavegame.Apply(sim, archive);
        Assert.Equal(expected, actor.Roll.Raw);
    }
}
