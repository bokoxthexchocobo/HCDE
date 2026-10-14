using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InvisiblePlayerLookTests
{
    [Theory]
    [InlineData(0, 166, 193, 242)]
    [InlineData(1, 184, 33, 222)]
    [InlineData(12345, 82, 124, 123)]
    public void NativePlayerLookStreamVectors(int seed, int a, int b, int c)
    {
        var sim = Room(seed);
        Assert.Equal(new[] { a, b, c }, Enumerable.Range(0, 3).Select(_ => sim.NextPlayerLookByte()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DistantSneakingPlayerDoesNotConsumeRandomness(bool ghost)
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var player = sim.Players.Single();
        player.Ghost = ghost; player.Shadow = !ghost;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Null(hunter.TargetId); Assert.Null(sim.CaptureState().PlayerLookRandomState);
    }

    [Theory]
    [InlineData(128, 0, true)]
    [InlineData(129, 0, false)]
    [InlineData(800, 4, false)]
    [InlineData(800, 5, true)]
    public void DistanceAndSpeedBoundariesControlRandomDraw(int distance, int speed, bool draws)
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var player = sim.Players.Single();
        player.X = Fixed.FromInt(distance); player.VelocityX = Fixed.FromInt(speed); player.Shadow = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(draws, sim.CaptureState().PlayerLookRandomState.HasValue);
        Assert.Null(hunter.TargetId); // Seed zero's first byte is 166, below the detection threshold.
    }

    [Fact]
    public void MovingInvisiblePlayerUsesNativeDetectionThreshold()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var player = sim.Players.Single();
        player.Shadow = true; player.VelocityY = Fixed.FromInt(5);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive); Assert.Null(hunter.TargetId);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive); Assert.Null(hunter.TargetId);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive); Assert.Equal(player.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SeeInvisibleBypassesSneakingAndRandomness(bool ghost)
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var player = sim.Players.Single();
        player.Ghost = ghost; player.Shadow = !ghost; hunter.SeeInvisible = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(player.Id, hunter.TargetId); Assert.Null(sim.CaptureState().PlayerLookRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvisibilityCompatibilityBypassesShadowButNotGhost(bool ghost)
    {
        var sim = Room(); sim.CompatibilityInvisibility = true;
        var hunter = sim.AddBot(0, 0); var player = sim.Players.Single(); player.Shadow = true; player.Ghost = ghost;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(ghost ? (uint?)null : player.Id, hunter.TargetId);
        Assert.Null(sim.CaptureState().PlayerLookRandomState);
    }

    [Fact]
    public void OccludedPlayerDoesNotDrawDetectionRandomness()
    {
        var sim = Room(blocked: true);
        var hunter = sim.AddBot(0, 0); var player = sim.Players.Single(); player.Shadow = true; player.VelocityX = Fixed.FromInt(5);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Null(hunter.TargetId); Assert.Null(sim.CaptureState().PlayerLookRandomState);
    }

    [Fact]
    public void SaveRestoresStreamFlagsAndCompatibility()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0);
        hunter.SeeInvisible = true; sim.CompatibilityInvisibility = true; sim.NextPlayerLookByte();
        var saved = SimSavegame.Write(sim, true); var expected = sim.NextPlayerLookByte();
        hunter.SeeInvisible = false; sim.CompatibilityInvisibility = false;
        SimSavegame.Apply(sim, saved);
        Assert.True(hunter.SeeInvisible); Assert.True(sim.CompatibilityInvisibility);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Equal(expected, sim.NextPlayerLookByte());
    }

    [Fact]
    public void OlderSnapshotResetsPlayerLookState()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var saved = SimSavegame.Write(sim, true);
        hunter.SeeInvisible = true; sim.CompatibilityInvisibility = true; sim.NextPlayerLookByte();
        SimSavegame.Apply(sim, saved);
        Assert.False(hunter.SeeInvisible); Assert.False(sim.CompatibilityInvisibility);
        Assert.Null(sim.CaptureState().PlayerLookRandomState); Assert.Equal(166, sim.NextPlayerLookByte());
    }

    [Theory]
    [InlineData("SEEINVISIBLE")]
    [InlineData("Actor.SeeInvisible")]
    public void ScriptFlagResetsOnRevive(string name)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        ActorRaise.ReviveSupported(actor); Assert.False(actor.SeeInvisible);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedArchiveIsRejected(bool recursive)
    {
        var sim = Room(); sim.CompatibilityInvisibility = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4)); var start = bytes.Length - size;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + (recursive ? 0 : 20)), recursive ? 234 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(recursive ? "save-playerlook-header" : "save-playerlook-value", error);
    }

    private static AuthoritySimulation Room(int seed = 0, bool blocked = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Lines = blocked ? [new LevelLine { X1 = 100, X2 = 100, Y1 = -128, Y2 = 128, SideBack = -1 }] : [],
        Things = [new LevelThing { Type = 1, X = 800 }]
    }, rngSeed: seed);
}
