using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorLookExHateTests
{
    [Theory]
    [InlineData(false, 31, false)]
    [InlineData(false, 32, true)]
    [InlineData(true, 31, false)]
    [InlineData(true, 32, true)]
    public void LocalAndGlobalHateSearchHonorCustomDistance(bool local, double maximum, bool found)
    {
        var sim = Room(local); var actor = sim.Actors[0]; var hatee = sim.Actors[1]; actor.TidToHate = 7;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, maxSeeDistance: maximum);
        Assert.Equal(found ? hatee.Id : (uint?)null, actor.TargetId);
        if (found) Assert.Equal(hatee.Id, actor.LastLookActorId);
        if (local && found) Assert.Null(sim.CaptureState().HateSearchRandomState);
        else Assert.NotNull(sim.CaptureState().HateSearchRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GlobalSearchHonorsConeUnlessLookAllAroundIsSet(bool allAround)
    {
        var sim = Room(false); var actor = sim.Actors[0]; var hatee = sim.Actors[1];
        actor.TidToHate = 7; actor.LookAllAround = allAround; hatee.X = Fixed.FromInt(-256);
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, fieldOfView: 60);
        Assert.Equal(allAround ? hatee.Id : (uint?)null, actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoSightCheckBypassesCustomVisibilityForHatees(bool local)
    {
        var sim = Room(local); var actor = sim.Actors[0]; var hatee = sim.Actors[1];
        actor.TidToHate = 7; actor.NoSightCheck = true; hatee.MVisBlocked = true;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, maxSeeDistance: 1);
        Assert.Equal(hatee.Id, actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerFallbackRequiresHuntPlayers(bool huntPlayers)
    {
        var sim = Room(false); var actor = sim.Actors[0]; actor.TidToHate = 999; actor.HuntPlayers = huntPlayers;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump);
        Assert.Equal(huntPlayers ? sim.Players.Single().Id : (uint?)null, actor.TargetId);
    }

    [Fact]
    public void SavedCursorAndBudgetRestoreNextLookExResult()
    {
        var sim = Room(false); var actor = sim.Actors[0]; actor.TidToHate = 7;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, maxSeeDistance: 1);
        var saved = SimSavegame.Write(sim, true);
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, maxSeeDistance: 128);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, maxSeeDistance: 128);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ExistingHateTargetIsIndependentOfSoundChecksAndHearingDistance()
    {
        var sim = Room(false); var actor = sim.Actors[0]; var hatee = sim.Actors[1];
        actor.TidToHate = 7; actor.SetTargetPointer(hatee);
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoSoundCheck | ActorLookFlags.NoJump,
            maxHearDistance: 1);
        Assert.Equal(hatee.Id, actor.TargetId);
    }

    private static AuthoritySimulation Room(bool local) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = local ? new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []) : null,
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64 },
            new LevelThing { Type = 3001, Id = 7, X = 96, Y = 64 },
            new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
