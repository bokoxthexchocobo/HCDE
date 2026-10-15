using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TraceBlockSelfTests
{
    [Theory]
    [InlineData(0, LevelLine.BlockingFlag, true)]
    [InlineData(1, LevelLine.BlockPlayersFlag, true)]
    [InlineData(0, LevelLine.BlockPlayersFlag, false)]
    [InlineData(2, LevelLine.BlockingFlag, false)]
    [InlineData(2, LevelLine.BlockProjectileFlag, true)]
    [InlineData(0, LevelLine.BlockProjectileFlag, false)]
    [InlineData(0, LevelLine.BlockMonstersFlag, true)]
    [InlineData(1, LevelLine.BlockMonstersFlag, false)]
    [InlineData(2, LevelLine.BlockMonstersFlag, true)]
    [InlineData(3, LevelLine.BlockFloatersFlag, true)]
    [InlineData(0, LevelLine.BlockFloatersFlag, false)]
    [InlineData(4, LevelLine.BlockPlayersFlag, true)]
    [InlineData(5, LevelLine.BlockPlayersFlag, true)]
    [InlineData(5, LevelLine.BlockingFlag, true)]
    public void NativeMaskDependsOnTracingActor(int role, int wallFlags, bool blocks)
    {
        var sim = Room(wallFlags); var source = Source(sim, role);
        Assert.Null(Trace(sim, source, false).Wall);
        Assert.Equal(blocks, Trace(sim, source, true).Wall is not null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MonsterBlockingExemptionsApply(bool friendlyCompatibility)
    {
        var sim = Room(LevelLine.BlockMonstersFlag, friendlyCompatibility ? CompatSurface.NoBlockFriends : 0);
        var source = Source(sim, 0); source.NoBlockMonsters = !friendlyCompatibility; source.Friendly = friendlyCompatibility;
        Assert.Null(Trace(sim, source, true).Wall);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockAsPlayerAlsoAffectsProjectileMovement(bool enabled)
    {
        var sim = Room(LevelLine.BlockPlayersFlag); var missile = (ProjectileActor)Source(sim, 2);
        missile.BlockAsPlayer = enabled; missile.VelocityX = Fixed.FromInt(80); missile.VelocityY = missile.VelocityZ = default;
        sim.Tick(); Assert.Equal(enabled, missile.X.ToDouble() < 64);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockAsPlayerAffectsOrdinaryMove(bool enabled)
    {
        var sim = Room(LevelLine.BlockPlayersFlag); var source = Source(sim, 0); source.BlockAsPlayer = enabled;
        Assert.Equal(!enabled, ActorPhysics.TryMove(sim, source, 80, 0, out _));
    }

    [Theory]
    [InlineData("BLOCKASPLAYER")]
    [InlineData("Actor.blockasplayer")]
    public void FlagScriptAccessAndRevival(string name)
    {
        var sim = Room(0); var actor = Source(sim, 0);
        Assert.True(AcsActorFlags.TrySet(actor, name, true)); Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        ActorRaise.ReviveSupported(actor); Assert.False(actor.BlockAsPlayer);
    }

    [Fact]
    public void SavedFlagAndChecksumRestoreAndLegacyClears()
    {
        var sim = Room(0); var actor = Source(sim, 0); var old = SimSavegame.Write(sim, true);
        actor.BlockAsPlayer = true; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, old); Assert.False(actor.BlockAsPlayer);
        SimSavegame.Apply(sim, saved); Assert.True(actor.BlockAsPlayer); Assert.Equal(hash, sim.Checksum);
    }

    [Fact]
    public void InvalidSavedFlagRejects()
    {
        var sim = Room(0); var actor = Source(sim, 0); actor.BlockAsPlayer = true;
        var state = sim.CaptureState(); state.Actors[0].BlockAsPlayerFlag = 2;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-blockasplayer-state", error);
    }

    private static CombatTrace.LineAttackHit Trace(AuthoritySimulation sim, Actor source, bool blockSelf)
        => CombatTrace.LineTrace(sim, source, default, 256,
            flags: ActorLineTraceFlags.ThroughActors | (blockSelf ? ActorLineTraceFlags.BlockSelf : 0), offsetZ: source is ProjectileActor ? 0 : 28);

    private static Actor Source(AuthoritySimulation sim, int role)
    {
        Actor source = role is 1 or 2 or 5 ? sim.Players.Single() : sim.AddBot(0, 0);
        if (role is 2 or 5) source = sim.SpawnProjectile(source, ProjectileKind.Rocket);
        source.Floating = role == 3; source.BlockAsPlayer = role is 4 or 5;
        return source;
    }

    private static AuthoritySimulation Room(int flags, CompatSurface compat = 0) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }, new LevelSector { CeilingHeight = 256 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = -128, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Flags = flags }],
        Things = [new LevelThing { Type = 1 }]
    }, compat: compat);
}
