using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpawnHealthTests
{
    [Theory]
    [InlineData(1, 20)]
    [InlineData(2.5, 50)]
    [InlineData(-37, 37)]
    public void RevivalUsesCapturedSpawnHealthAfterDamageAndSave(double health, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004, Health = health }],
        });
        var actor = Assert.Single(sim.Actors); actor.Health = 0;
        var bytes = SimSavegame.Write(sim.CaptureState());
        actor.Health = 10; SimSavegame.Apply(sim, bytes);
        Assert.Equal(expected, actor.SpawnHealth());
        ActorRaise.ReviveSupported(actor); Assert.Equal(expected, actor.Health);
    }
}
