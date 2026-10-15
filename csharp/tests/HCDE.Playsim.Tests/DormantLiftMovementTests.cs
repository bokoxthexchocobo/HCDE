using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DormantLiftMovementTests
{
    [Fact]
    public void SimulationInitializationRecognizesDormantLiftAndLeavingInvalidatesDirection()
    {
        var (sim, actor) = Room(); Assert.Empty(sim.Motions); Assert.True(ActorMovementHazards.IsOnLift(sim, actor));
        var state = sim.CaptureState(); state.StayOnLiftRandomState = 0; sim.RestoreState(state);
        Walk(actor); Assert.Equal(Fixed.FromInt(40), actor.X); Assert.Equal(8, actor.MoveDirection);
    }

    [Fact]
    public void MarkerSurvivesLineSpecialChangeAndSavedReconstruction()
    {
        var (sim, actor) = Room(); sim.Level.Lines[^1].Special = 0;
        var saved = SimSavegame.Write(sim, true);
        var (restored, _) = Room();
        SimSavegame.Apply(restored, saved);
        var restoredActor = restored.Actors.Single(a => a.Id == actor.Id);
        Assert.True(ActorMovementHazards.IsOnLift(restored, restoredActor));
        Assert.Equal(0, restored.Level.Lines[^1].Special);
        Walk(actor); Walk(restoredActor); Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(restored, true));
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 100; actor.MoveDirection = 0; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var marker = new LevelLine { X1 = -120, Y1 = -120, X2 = -110, Y2 = -120,
            Special = 62, Repeat = true, Arg0 = 7, SideFront = -1, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.UdmfText, Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7 }, new LevelSector { CeilingHeight = 128, Tag = 8 }],
            Sides = geometry.Sides, Lines = geometry.Lines.Append(marker).ToArray(),
            Things = [new LevelThing { Type = 3004, X = -40 }]
        });
        // Give the target the same primary tag through the original lift sector.
        var actor = sim.Actors.Single(); var target = sim.AddBot(-80, 0); actor.SetTargetPointer(target); actor.StayOnLift = true;
        return (sim, actor);
    }
}
