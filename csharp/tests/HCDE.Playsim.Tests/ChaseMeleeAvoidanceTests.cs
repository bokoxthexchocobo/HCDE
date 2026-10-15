using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseMeleeAvoidanceTests
{
    [Theory]
    [InlineData(WeaponKind.Fist, 100, true)]
    [InlineData(WeaponKind.Chainsaw, 100, true)]
    [InlineData(WeaponKind.Fist, 192, false)]
    [InlineData(WeaponKind.Fist, 191, true)]
    [InlineData(WeaponKind.Pistol, 100, false)]
    public void PlayerWeaponAndStrictDistanceControlBacking(WeaponKind weapon, int distance, bool backs)
    {
        var (sim, actor) = Room(); var player = sim.Players.Single();
        player.Inventory.Selected = weapon; player.X = Fixed.FromInt(distance);
        var expected = Room().Item1; var count = expected.NextEnemyStrafeByte() & 15;
        Chase(sim, actor);
        Assert.Equal(Fixed.FromInt(backs ? -8 : 8), actor.X);
        Assert.Equal(backs ? count : 0, actor.StrafeCount);
        Assert.Equal(backs, sim.CaptureState().EnemyStrafeRandomState.HasValue);
        if (backs && count != 0) Assert.Equal(count, actor.MoveCount);
    }

    [Fact]
    public void DisabledFlagDoesNotBackAwayOrDraw()
    {
        var (sim, actor) = Room(); actor.AvoidMelee = false; sim.Players.Single().Inventory.Selected = WeaponKind.Fist;
        Chase(sim, actor); Assert.Equal(Fixed.FromInt(8), actor.X); Assert.Null(sim.CaptureState().EnemyStrafeRandomState);
    }

    [Theory]
    [InlineData(MonsterAttack.Melee, true)]
    [InlineData(MonsterAttack.Fireball, false)]
    public void RepresentedMonsterAttackControlsMeleeThreat(MonsterAttack attack, bool backs)
    {
        var (sim, actor) = Room(); sim.Players.Single().X = Fixed.FromInt(1000);
        var enemy = sim.AddBot(100, 0); enemy.Brain = new MonsterBrain(attack); actor.SetTargetPointer(enemy);
        Chase(sim, actor); Assert.Equal(Fixed.FromInt(backs ? -8 : 8), actor.X);
        Assert.Equal(backs, sim.CaptureState().EnemyStrafeRandomState.HasValue);
    }

    [Fact]
    public void PreExtensionSaveClearsAvoidanceFlagCounterAndStream()
    {
        var (sim, actor) = Room(); actor.AvoidMelee = false; var old = SimSavegame.Write(sim, true);
        actor.AvoidMelee = true; actor.StrafeCount = 5; sim.NextEnemyStrafeByte();
        SimSavegame.Apply(sim, old);
        Assert.False(actor.AvoidMelee); Assert.Equal(0, actor.StrafeCount); Assert.Null(sim.CaptureState().EnemyStrafeRandomState);
    }

    [Fact]
    public void AssignedGoalIsExemptFromMeleeAvoidance()
    {
        var (sim, actor) = Room(); sim.Players.Single().Inventory.Selected = WeaponKind.Fist;
        actor.GoalId = sim.Players.Single().Id;
        Chase(sim, actor); Assert.Equal(Fixed.FromInt(8), actor.X); Assert.Null(sim.CaptureState().EnemyStrafeRandomState);
    }

    [Fact]
    public void ActiveStrafingFacesTargetEvenWhenDirectionTurningSuppressed()
    {
        var (sim, actor) = Room(); actor.StrafeCount = 5; actor.MoveCount = 8;
        actor.MoveDirection = 4; actor.Angle = BamAngle.FromDegrees(90);
        Chase(sim, actor, MonsterChaseFlags.NoDirectionTurn);
        Assert.Equal(BamAngle.FromDegrees(0), actor.Angle); Assert.Equal(4, actor.StrafeCount);
        Assert.Equal(Fixed.FromInt(-8), actor.X);
    }

    [Fact]
    public void SavedAvoidanceReplaysRandomCounterAndPositionAndOlderSaveClearsState()
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true);
        sim.Players.Single().Inventory.Selected = WeaponKind.Fist; Chase(sim, actor);
        var saved = SimSavegame.Write(sim, true); Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor); Assert.Equal(expected, SimSavegame.Write(sim, true));
        actor.AvoidMelee = false; SimSavegame.Apply(sim, old); Assert.True(actor.AvoidMelee);
        Assert.Equal(0, actor.StrafeCount); Assert.Null(sim.CaptureState().EnemyStrafeRandomState);
    }

    [Theory]
    [InlineData("AVOIDMELEE")]
    [InlineData("Actor.avoidmelee")]
    public void SharedFlagAccessAndRevivalRestoreDefaults(string name)
    {
        var (_, actor) = Room(); Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.AvoidMelee);
        actor.AvoidMelee = true; actor.StrafeCount = 7; ActorRaise.ReviveSupported(actor);
        Assert.False(actor.AvoidMelee); Assert.Equal(0, actor.StrafeCount);
    }

    [Fact]
    public void ArchiveRejectsInvalidFlagAndHiddenRandomPayload()
    {
        var (sim, _) = Room(); var state = sim.CaptureState(); state.Actors[0].AvoidMeleeFlag = 2;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(bytes.Length - size + 12), 1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-meleeavoidance-random", error);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = 0) => actor.Brain!.Tick(sim, actor,
        flags | MonsterChaseFlags.NoPlayActive, useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 100 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MovementSpeed = Fixed.FromInt(8); actor.MoveDirection = 8; actor.AvoidMelee = true;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
