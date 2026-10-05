using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseBlockmapTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresClassBlockmapDefault(bool noBlockmap, bool archvile)
    {
        var sim = Room(noBlockmap);
        var corpse = Prepare(sim);
        corpse.NoBlockmap = archvile ? false : !noBlockmap;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(noBlockmap, corpse.NoBlockmap);
        Assert.Equal(!noBlockmap, corpse.IsBlockmapActor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchvileCannotFindExcludedCorpseButScriptCan(bool classNoBlockmap)
    {
        var sim = Room(classNoBlockmap);
        var corpse = Prepare(sim);
        corpse.NoBlockmap = true;
        corpse.VelocityX = Fixed.FromInt(3);
        Assert.False(ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single()));
        Assert.Equal(Fixed.FromInt(3), corpse.VelocityX);
        Assert.True(ActorRaise.CanRaise(sim, corpse));
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 2));
    }

    [Fact]
    public void BlockmapRevivalDefaultAffectsChecksum()
    {
        var first = Room(false); var second = Room(false);
        second.Actors[1].ResurrectionCollisionFlags = 7;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].NoBlockmap, second.Actors[1].NoBlockmap);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static Actor Prepare(AuthoritySimulation sim)
    {
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        return corpse;
    }

    private static AuthoritySimulation Room(bool noBlockmap) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
        + (noBlockmap ? " + NOBLOCKMAP" : "") + "\n"));
}
