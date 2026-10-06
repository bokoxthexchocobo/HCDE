using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTargetLosJumpTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(128, false)]
    [InlineData(256, false)]
    [InlineData(384, true)]
    public void TargetLosAndFlipFovSelectWhoseFacingIsChecked(int flags, bool jumps)
    {
        var sim = Room(); var caller = sim.AddBot(0, 40); var target = sim.AddBot(100, 40);
        caller.Brain!.SetSpecialTarget(target);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.JumpIfTargetInLOS(caller, 2, 90, flags));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(128, false)]
    [InlineData(256, false)]
    [InlineData(384, true)]
    public void PlayerAimVerifiesCallerFacingButTargetCanSupplyFov(int flags, bool jumps)
    {
        var sim = Room(); var player = sim.Players.Single(); sim.AddBot(100, 0);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.JumpIfTargetInLOS(player, 2, 90, flags));
    }

    [Fact]
    public void CombatantAndAllyFiltersUseSelectedTarget()
    {
        var sim = Room(); var caller = sim.AddBot(0, 40); var target = sim.AddBot(100, 40);
        caller.Brain!.SetSpecialTarget(target); target.IsMonster = false;
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 1024));
        target.IsMonster = true;
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 1024));
        caller.Friendly = target.Friendly = true;
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 512));
    }

    [Fact]
    public void DistanceCloseAndDeathFlagsApplyBeforeViewChecks()
    {
        var sim = Room(); var caller = sim.AddBot(0, 40); var target = sim.AddBot(100, 40);
        caller.Brain!.SetSpecialTarget(target);
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(caller, 2, maxDistance: 99));
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInLOS(caller, 2, maxDistance: 100));
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 16, closeDistance: 101));
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 16, closeDistance: 100));
        target.Health = 0;
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInLOS(caller, 2));
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: 32));
    }

    [Fact]
    public void ExplicitTracerWorksForNonseekerAndReturnedStateRunsOnCaller()
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket, target);
        rocket.RestoreTracerTarget(target.Id);
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(rocket, 2, flags: 3));
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInLOS(rocket, 2, flags: 4098));
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(rocket, 2, flags: 4162));
        rocket.States.Configure(rocket, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfTargetInLOS(self, 2, flags: 4098)), new(-1, 2)], 0);
        rocket.States.Enter(rocket, 1);
        Assert.Equal(2, rocket.States.Current);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    [InlineData(8, true)]
    public void MonsterSightCanBeBypassedExplicitlyOrAtCloseRange(int flags, bool jumps)
    {
        var sim = Room(blocked: true); var caller = sim.AddBot(0, 40); var target = sim.AddBot(100, 40);
        caller.Brain!.SetSpecialTarget(target);
        Assert.Equal(jumps ? 2 : (int?)null,
            ActorJumpActions.JumpIfTargetInLOS(caller, 2, flags: flags, closeDistance: 101));
    }

    [Fact]
    public void PlayerStillNeedsAimedTargetWithNoSightFlag()
    {
        var sim = Room(blocked: true); var player = sim.Players.Single(); sim.AddBot(100, 0);
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(player, 2, flags: 2));
    }

    private static AuthoritySimulation Room(bool blocked = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Sides = [new LevelSide { Sector = 0 }],
        Lines = blocked ? [new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
            SideFront = 0, SideBack = -1 }] : [],
        Things = [new LevelThing { Type = 1 }],
    });
}
