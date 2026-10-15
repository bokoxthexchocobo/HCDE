using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseDropOffEscapeTests
{
    [Theory]
    [InlineData(false, -32)]
    [InlineData(true, 32)]
    public void EscapeVectorPointsTowardHighFloor(bool highRight, int expected)
    {
        var (sim, actor) = Room(highRight);
        Assert.True(ActorPhysics.TryDropOffEscapeVector(sim, actor, out var dx, out var dy));
        Assert.Equal(expected, dx); Assert.Equal(0, dy);
    }

    [Fact]
    public void AvoidanceMoveCanRetreatWhileStillOverlappingLedge()
    {
        var (sim, actor) = Room();
        Assert.False(ActorPhysics.TryWanderMove(sim, actor, -16, 0, out _, out _));
        Assert.True(ActorPhysics.TryWanderMove(sim, actor, -16, 0, out _, out _, avoidingDropOff: true));
        Assert.Equal(Fixed.FromInt(-16), actor.X); Assert.Equal(default, actor.Z);
    }

    [Fact]
    public void AvoidanceMoveStillRejectsFallingToLowerFloor()
    {
        var (sim, actor) = Room();
        Assert.False(ActorPhysics.TryWanderMove(sim, actor, 40, 0, out _, out _, avoidingDropOff: true));
        Assert.Equal(Fixed.FromInt(-8), actor.X);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void NativeEligibilityGatesEscapeGeneration(int gate)
    {
        var (sim, actor) = Room(compat: gate == 4 ? CompatSurface.DropOff : CompatSurface.None);
        actor.AllowDropOff = gate == 0; actor.Floating = gate == 1; actor.OnMobj = gate == 2;
        if (gate == 3) actor.Z = Fixed.FromInt(1);
        Assert.False(ActorPhysics.TryDropOffEscapeVector(sim, actor, out _, out _));
    }

    [Fact]
    public void NativeChaseRetreatsAndUsesShortMoveCount()
    {
        var (sim, actor) = Room(); Chase(sim, actor);
        Assert.Equal(Fixed.FromInt(-16), actor.X); Assert.Equal(4, actor.MoveDirection);
        Assert.Equal(1, actor.MoveCount); Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void SavedEscapeContinuationReplaysExactly()
    {
        var (sim, actor) = Room(); Chase(sim, actor); var saved = SimSavegame.Write(sim, true);
        Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ContactedCornerEdgesContributeCumulativeNormals()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { FloorHeight = -64, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1 = 0, Y1 = 128, X2 = 0, Y2 = -128, SideFront = 0, SideBack = 1 },
                new LevelLine { X1 = -128, Y1 = 0, X2 = 128, Y2 = 0, SideFront = 0, SideBack = 1 }]
        });
        var actor = sim.AddBot(-8, -8); actor.SectorIndex = 0; actor.Z = default;
        Assert.True(ActorPhysics.TryDropOffEscapeVector(sim, actor, out var dx, out var dy));
        Assert.Equal(-32, dx); Assert.Equal(-32, dy);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.NoPlayActive, useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room(bool highRight = false, CompatSurface compat = CompatSurface.None)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-64, 128).Level;
        var lines = geometry.Lines.Take(geometry.Lines.Count - 1).Append(new LevelLine {
            X1 = 0, Y1 = 128, X2 = 0, Y2 = -128, SideFront = 0, SideBack = 1
        }).ToArray();
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { FloorHeight = highRight ? (short)-64 : (short)0, CeilingHeight = 128 },
                new LevelSector { FloorHeight = highRight ? (short)0 : (short)-64, CeilingHeight = 128 }],
            Lines = lines, Sides = geometry.Sides, Things = [new LevelThing { Type = 1, X = 80 }]
        }, compat: compat);
        var actor = sim.AddBot(highRight ? 8 : -8, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MovementSpeed = Fixed.FromInt(8); actor.MoveDirection = 8;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
