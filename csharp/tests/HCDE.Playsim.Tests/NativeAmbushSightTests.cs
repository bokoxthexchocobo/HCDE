using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeAmbushSightTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void NativeAmbushHearingCanSeeQualifyingImpactScriptLine(int path, bool native)
    {
        var (sim, actor) = Room(native, impact: true);
        Hear(sim, actor, path);
        Assert.Equal(native ? 1 : 0, actor.States.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NativeAmbushHearingRejectsScriptForOtherMap(int path)
    {
        var (sim, actor) = Room(true, impact: true, map: 2);
        Hear(sim, actor, path); Assert.Equal(0, actor.States.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NativeAmbushHearingSeesPartialTargetHeight(int path)
    {
        var (sim, actor) = Room(true, impact: false);
        sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        Assert.False(CombatTrace.HasLineOfSight(sim, actor, sim.Players.Single()));
        Hear(sim, actor, path); Assert.Equal(1, actor.States.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NonAmbushHearingDoesNotAddVisibilityDraw(int path)
    {
        var (sim, actor) = Room(true, impact: true, map: 2); actor.Ambush = false;
        var target = sim.Players.Single(); target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        Hear(sim, actor, path);
        Assert.Equal(1, actor.States.Current); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedStoredHearingReplaysNativeAmbushWake()
    {
        var (sim, actor) = Room(true, impact: true); actor.LastHeardTargetId = sim.Players.Single().Id;
        var saved = SimSavegame.Write(sim, true);
        Hear(sim, actor, 1); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Hear(sim, actor, 1);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Hear(AuthoritySimulation sim, Actor actor, int path)
    {
        var target = sim.Players.Single();
        if (path == 0) actor.Brain!.Hear(sim, actor, target);
        else
        {
            actor.LastHeardTargetId = target.Id;
            if (path == 1) actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive,
                false, false, nativeMovement: sim.NativeMonsterMovement);
            else ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoSeeSound);
        }
    }

    private static (AuthoritySimulation, Actor) Room(bool native, bool impact, int map = 0)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level; var lines = geometry.Lines.ToArray();
        lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
            Flags = LevelLine.TwoSidedFlag | (impact ? LevelLine.BlockEverythingFlag : 0),
            Impact = impact, Special = impact ? 80 : 0, Arg1 = map };
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01", Sectors = geometry.Sectors,
            Sides = geometry.Sides, Lines = lines, Things = [new LevelThing { Type = 1, X = 64 }] },
            spawnOptions: new SpawnOptions(NativeMonsterMovement: native));
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.Ambush = true; actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        return (sim, actor);
    }
}
