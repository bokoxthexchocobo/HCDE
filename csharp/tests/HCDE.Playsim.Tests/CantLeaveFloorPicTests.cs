using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CantLeaveFloorPicTests
{
    [Theory]
    [InlineData(false, "OTHER", 0, true)]
    [InlineData(true, "OTHER", 0, false)]
    [InlineData(true, "FLOOR", 0, true)]
    [InlineData(true, "floor", 0, true)]
    [InlineData(true, "FLOOR", 8, false)]
    public void MovementRequiresMatchingTextureAndHeight(bool enabled, string texture, short floor, bool moves)
    {
        var (sim, actor) = TwoRooms(texture, floor); actor.CantLeaveFloorPic = enabled;
        Assert.Equal(moves, ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.Equal(Fixed.FromInt(moves ? 40 : -40), actor.X);
        Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void CurrentFloorTextureChangesAreObserved()
    {
        var (sim, actor) = TwoRooms("OTHER", 0); actor.CantLeaveFloorPic = true;
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        sim.Level.Sectors[1].FloorPic = "FLOOR";
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    [Fact]
    public void WanderRetainsPartialProgressWhenTextureBoundaryBlocks()
    {
        var (sim, actor) = TwoRooms("OTHER", 0); actor.CantLeaveFloorPic = true;
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        Assert.True(actor.X.ToDouble() > -40); Assert.True(actor.X.ToDouble() < 0);
        Assert.Equal(0, actor.SectorIndex);
    }

    [Fact]
    public void NativeChaseUsesFloorRestriction()
    {
        var (sim, actor) = TwoRooms("OTHER", 0); actor.CantLeaveFloorPic = true;
        var target = sim.AddBot(80, 0);
        actor.Brain = new MonsterBrain(MonsterAttack.Melee); actor.ReactionTime = 0;
        actor.SetTargetPointer(target);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1; actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.MoveCount = 100; actor.MoveDirection = 0; actor.MovementSpeed = Fixed.FromInt(80);
        actor.Brain.Tick(sim, actor, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.NoRandomTurn | MonsterChaseFlags.StopIfBlocked,
            useMelee: false, useMissile: false, nativeMovement: true);
        Assert.InRange(actor.X.ToDouble(), -39, -1); Assert.Equal(0, actor.SectorIndex);
    }

    private static (AuthoritySimulation, Actor) TwoRooms(string texture, short floor)
    {
        var geometry = GameplayFoundationTests.TwoRooms(floor, 128).Level;
        geometry.Sectors[0].FloorPic = "FLOOR"; geometry.Sectors[1].FloorPic = texture;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides,
            Things = [new LevelThing { Type = 3004, X = -40 }]
        });
        return (sim, sim.Actors.Single());
    }

    [Theory]
    [InlineData("CANTLEAVEFLOORPIC")]
    [InlineData("Actor.cantleavefloorpic")]
    public void FlagSupportsSharedScriptAccess(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.CantLeaveFloorPic);
    }

    [Fact]
    public void SavedFlagRestoresChecksumAndOlderSaveClearsIt()
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.CantLeaveFloorPic = true; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.CantLeaveFloorPic = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.CantLeaveFloorPic); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(actor.CantLeaveFloorPic); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void InvalidFlagRejectsStructuredAndBinaryArchives()
    {
        var sim = Room(); sim.Actors[0].CantLeaveFloorPic = true;
        var state = sim.CaptureState(); state.Actors[0].CantLeaveFloorPicFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-cantleavefloorpic-state", error);
    }

    [Fact]
    public void RevivalRestoresRepresentedDefault()
    {
        var actor = Room().Actors[0]; actor.CantLeaveFloorPic = true;
        ActorRaise.ReviveSupported(actor); Assert.False(actor.CantLeaveFloorPic);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64, Friendly = true },
            new LevelThing { Type = 3001, X = 320, Y = 64 }, new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
