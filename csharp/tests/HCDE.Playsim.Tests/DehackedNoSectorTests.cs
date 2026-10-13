using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNoSectorTests
{
    [Theory]
    [InlineData("6", false)]
    [InlineData("14", true)]
    [InlineData("NOSECTOR+SOLID+SHOOTABLE", true)]
    public void NumericAndNamedDefaultsControlSectorMembershipWithoutBlockmapExclusion(string bits, bool excluded)
    {
        var sim = Room(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
        var actor = sim.Actors[1];
        Assert.Equal(excluded, actor.NoSector); Assert.Equal(!excluded, actor.IsSectorLinked);
        Assert.True(actor.IsBlockmapActor); Assert.True(actor.Shootable);
        Assert.Equal(!excluded, sim.SectorActors(0).Contains(actor));
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == actor.Id).SectorLinkFlag);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ResurrectionRestoresClassSectorDefault(bool noSector, bool archvile)
    {
        var sim = Room(Patch(noSector)); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        ActorPropertyActions.ChangeLinkFlags(corpse, sector: noSector ? 0 : 1);
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2));
        Assert.Equal(noSector, corpse.NoSector);
        Assert.Equal(!noSector, sim.SectorActors(0).Contains(corpse));
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Equal(noSector, corpse.NoSector); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BaselineRestoreUsesClassDefaultAndOverrideRoundTrips(bool noSector)
    {
        var sim = Room(Patch(noSector)); var actor = sim.Actors[1];
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeLinkFlags(actor, sector: noSector ? 0 : 1);
        var overridden = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, baseline);
        Assert.Equal(noSector, actor.NoSector); Assert.False(actor.HasSectorOverride);
        Assert.Equal(hash, sim.Checksum); Assert.Equal(baseline, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, overridden);
        Assert.Equal(!noSector, actor.NoSector); Assert.Equal(overridden, SimSavegame.Write(sim));
    }

    [Fact]
    public void ReplacingBitsClearsSectorDefaultWithoutChangingPriorPatch()
    {
        var first = Patch(true); var second = DehackedPatch.Apply("Thing 2\nBits = 6\n", first);
        Assert.True(Room(first).Actors[1].NoSector); Assert.False(Room(second).Actors[1].NoSector);
    }

    private static DehackedPatchResult Patch(bool noSector) => DehackedPatch.Apply(
        "Thing 2\nBits = SOLID+SHOOTABLE+COUNTKILL" + (noSector ? "+NOSECTOR" : "") + "\n");

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    }, dehacked: patch);
}
