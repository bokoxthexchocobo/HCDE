using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DestroyedTargetMemoryTests
{
    [Fact]
    public void DestructionClearsMatchingMemoryBeforeNotificationAndPreservesOtherReferences()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 65 }, new LevelThing { Type = 9, Y = 100 }] });
        var target = sim.Players.Single(); var first = sim.Actors.Single(a => a.DoomEdNum == 65);
        var other = sim.Actors.Single(a => a.DoomEdNum == 9);
        first.Brain!.RestoreTargetMemory(new(target.Id, target.Id, target.Id)); first.LastHeardTargetId = target.Id;
        other.Brain!.RestoreTargetMemory(new(first.Id, first.Id, first.Id)); other.LastHeardTargetId = first.Id;
        var notified = false;
        sim.ActorDestroyed += destroyed =>
        {
            Assert.Same(target, destroyed); Assert.Null(first.Brain.TargetId);
            Assert.Null(first.Brain.LastEnemyId); Assert.Null(first.LastHeardTargetId); notified = true;
        };
        target.Destroy(); Assert.True(notified);
        Assert.Equal(first.Id, other.Brain.TargetId); Assert.Equal(first.Id, other.Brain.LastEnemyId);
        Assert.Equal(first.Id, other.LastHeardTargetId);
    }
}
