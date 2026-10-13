using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSectorLinkTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    public void SectorFlagControlsMembershipWithoutRemovingCollision(int value, bool linked)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); var blocker = sim.AddBot(200, 0);
        ActorPropertyActions.ChangeLinkFlags(blocker, sector: value);
        Assert.Equal(linked, blocker.IsSectorLinked);
        Assert.Equal(linked, sim.SectorActors(0).Contains(blocker));
        Assert.True(blocker.IsBlockmapActor); Assert.False(ActorPhysics.CanOccupy(sim, actor));
        var bytes = SimSavegame.Write(sim);
        blocker.NoSector = !blocker.NoSector;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(linked, blocker.IsSectorLinked); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnlinkedActorsDoNotReceiveCarryScroll(bool player)
    {
        var sim = Room(); Actor actor = player ? sim.Players.Single() : sim.AddBot(200, 0);
        actor.InScrollSector = true; sim.SetSectorScroll(0, 4, 0);
        ActorPropertyActions.ChangeLinkFlags(actor, sector: 1);
        var x = actor.X;
        ActorPhysics.ApplySectorScroll(sim, actor); Assert.Equal(x, actor.X);
        ActorPropertyActions.ChangeLinkFlags(actor, sector: 0);
        ActorPhysics.ApplySectorScroll(sim, actor); Assert.Equal(x.ToDouble() + 4, actor.X.ToDouble());
    }

    [Fact]
    public void AcsFlagAndBaselineRestoreResetMembershipAndChecksum()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeFlag(actor, "NOSECTOR", true);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.NotEqual(hash, sim.Checksum); Assert.False(actor.IsSectorLinked);
        SimSavegame.Apply(sim, baseline);
        Assert.True(actor.IsSectorLinked); Assert.False(actor.HasSectorOverride);
        Assert.Equal(hash, sim.Checksum); Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
