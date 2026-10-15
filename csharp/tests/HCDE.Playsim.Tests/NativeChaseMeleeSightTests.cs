using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeChaseMeleeSightTests
{
    [Theory]
    [InlineData(true, 8)]
    [InlineData(false, 0)]
    public void NativeMeleeSeesPartialTargetWhileLegacyPolicyRemainsUnchanged(bool native, int windup)
    {
        var (sim, actor) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        Chase(sim, actor, native: native);
        Assert.Equal(windup, actor.Brain!.WindupTics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FearRunsBeforeMeleeVisibilityAndDisabledMissileDoesNotDrawAgain(bool frightened)
    {
        var (sim, actor) = Room(); var (reference, _) = Room(); actor.Frightened = frightened;
        var target = sim.Players.Single(); target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        for (var i = 0; i < 256; i++)
        {
            var expected = !frightened || reference.NextScaredyCatByte() < 43;
            if (expected) expected = reference.NextCheckSightByte() <= 50;
            actor.Brain = new MonsterBrain(MonsterAttack.Melee); actor.SetTargetPointer(target);
            actor.ReactionTime = 0;
            Chase(sim, actor);
            Assert.Equal(expected ? 8 : 0, actor.Brain.WindupTics);
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
            Assert.Equal(reference.CaptureState().ScaredyCatRandomState, sim.CaptureState().ScaredyCatRandomState);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutOfRangeOrVerticalSeparationSkipsVisibilityDraw(bool vertical)
    {
        var (sim, actor) = Room(); var target = sim.Players.Single();
        target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        if (vertical) target.Z = Fixed.FromInt(200); else actor.MeleeRange = Fixed.FromInt(32);
        Chase(sim, actor);
        Assert.Equal(0, actor.Brain!.WindupTics); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void NoAttackLabelsSkipVisibilityDraw()
    {
        var (sim, actor) = Room(); var target = sim.Players.Single();
        target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        Chase(sim, actor, useMelee: false);
        Assert.Equal(0, actor.Brain!.WindupTics); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedNativeMeleeDecisionReplaysSightStreamAndWindup()
    {
        var (sim, actor) = Room(); var target = sim.Players.Single();
        target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        var saved = SimSavegame.Write(sim, true); Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, bool native = true, bool useMelee = true)
        => actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            useMelee: useMelee, useMissile: false, nativeMovement: native);

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0; actor.MeleeRange = Fixed.FromInt(256);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
