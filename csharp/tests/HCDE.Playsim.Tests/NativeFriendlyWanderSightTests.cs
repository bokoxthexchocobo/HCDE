using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeFriendlyWanderSightTests
{
    [Theory]
    [InlineData(false, 0, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 1, false)]
    public void FollowCoinShortCircuitsNativeVisibilityDraw(bool native, int parity, bool draws)
    {
        var (sim, actor) = Room(native); PrepareNextFollowBit(sim, parity);
        ActorWanderActions.Wander(actor);
        Assert.Equal(draws, sim.CaptureState().CheckSightRandomState.HasValue);
        if (draws)
        {
            var (reference, _) = Room(native); reference.NextCheckSightByte();
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledFollowOrNonfriendlyActorSkipsVisibility(bool dontFollow)
    {
        var (sim, actor) = Room(true); actor.DontFollowPlayers = dontFollow; actor.Friendly = dontFollow;
        PrepareNextFollowBit(sim, 0); ActorWanderActions.Wander(actor);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void ExplicitNativeChaseUsesNativeWanderSightDespiteLegacySimulationPolicy()
    {
        var (sim, actor) = Room(false); PrepareNextFollowBit(sim, 0);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive, false, false, nativeMovement: true);
        Assert.NotNull(sim.CaptureState().CheckSightRandomState);
        Assert.Null(actor.TargetId);
    }

    [Fact]
    public void MvisBlockedFollowSightRejectsBeforeRandomVisibilityDraw()
    {
        var (sim, actor) = Room(true); sim.Players.Single().MVisBlocked = true;
        PrepareNextFollowBit(sim, 0); ActorWanderActions.Wander(actor);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedNativeWanderReplaysSightAndDirectionStreams()
    {
        var (sim, actor) = Room(true); PrepareNextFollowBit(sim, 0);
        var saved = SimSavegame.Write(sim, true);
        ActorWanderActions.Wander(actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); ActorWanderActions.Wander(actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void PrepareNextFollowBit(AuthoritySimulation sim, int parity)
    {
        for (var i = 0; i < 256; i++)
        {
            var before = sim.CaptureState();
            if ((sim.NextNewChaseDirByte() & 1) == parity) { sim.RestoreState(before); return; }
        }
        throw new InvalidOperationException("Expected follow parity was not found.");
    }

    private static (AuthoritySimulation, Actor) Room(bool native)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }] }, spawnOptions: new SpawnOptions(NativeMonsterMovement: native));
        var player = sim.Players.Single(); player.RenderStyle = ActorRenderStyle.Translucent; player.Alpha = 0;
        var actor = sim.AddBot(0, 0); actor.Friendly = true; actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MoveCount = 0; actor.OnGround = false; actor.Z = Fixed.FromInt(10);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
