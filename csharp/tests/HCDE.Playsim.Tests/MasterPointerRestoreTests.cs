using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MasterPointerRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentMasterClearsLaterRelationshipAndPreservesArchiveBytes(bool otherOverride)
    {
        var (sim, hunter, source) = Room();
        if (otherOverride) source.MasterId = hunter.Id;
        var bytes = SimSavegame.Write(sim);
        hunter.MasterId = source.Id;
        SimSavegame.Apply(sim, bytes);
        Assert.Null(hunter.MasterId);
        Assert.False(hunter.HasMasterOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ExplicitNullOverrideClearsRelationshipAndRemainsExplicit()
    {
        var (sim, hunter, source) = Room(); hunter.MasterId = null;
        var bytes = SimSavegame.Write(sim);
        hunter.MasterId = source.Id;
        SimSavegame.Apply(sim, bytes);
        Assert.Null(hunter.MasterId);
        Assert.True(hunter.HasMasterOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void SavedMasterRelationshipRestoresRetaliationProtection()
    {
        var (sim, hunter, source) = Room(); hunter.MasterId = source.Id;
        var bytes = SimSavegame.Write(sim);
        hunter.MasterId = null;
        SimSavegame.Apply(sim, bytes);
        ActorDamage.Apply(hunter, 1, source, DamageFlags.NoPain);
        Assert.Equal(source.Id, hunter.MasterId);
        Assert.Null(hunter.Brain!.TargetId);
    }

    [Fact]
    public void BaselineRestoreRemovesLaterRetaliationProtection()
    {
        var (sim, hunter, source) = Room(); var bytes = SimSavegame.Write(sim);
        hunter.MasterId = source.Id;
        SimSavegame.Apply(sim, bytes);
        ActorDamage.Apply(hunter, 1, source, DamageFlags.NoPain);
        Assert.Equal(source.Id, hunter.Brain!.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }],
        });
        return (sim, sim.Actors[0], sim.Actors[1]);
    }
}
