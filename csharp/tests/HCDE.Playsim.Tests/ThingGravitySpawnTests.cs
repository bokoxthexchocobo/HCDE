using HCDE.MapLoader;
using HCDE.Gamedata;
namespace HCDE.Playsim.Tests;
public class ThingGravitySpawnTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0.25, 0.25, false)]
    [InlineData(-0.5, 0.5, false)]
    [InlineData(2, 2, false)]
    public void ImportedThingGravityControlsSpawn(double input, double expected, bool noGravity)
    {
        var text = FormattableString.Invariant($"namespace = \"ZDoom\"; thing {{ type = 3004; skill3 = true; single = true; gravity = {input}; }}");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(expected, actor.Gravity.ToDouble()); Assert.Equal(noGravity, actor.NoGravity);
    }
    [Theory]
    [InlineData(2, 0.5)]
    [InlineData(-2, 2)]
    public void MapGravityAppliesAfterPatchedClassGravity(double input, double expected)
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = LOGRAV\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3004, Gravity = input }],
        }, dehacked: patch);
        Assert.Equal(expected, Assert.Single(sim.Actors).Gravity.ToDouble());
    }
    [Fact]
    public void NonzeroGravityDoesNotClearExistingNoGravityFlag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3005, Gravity = -0.5 }],
        });
        var actor = Assert.Single(sim.Actors);
        Assert.True(actor.NoGravity); Assert.Equal(0.5, actor.Gravity.ToDouble());
    }
}