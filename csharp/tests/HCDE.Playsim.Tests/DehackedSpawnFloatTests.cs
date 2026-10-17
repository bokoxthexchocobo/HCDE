using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSpawnFloatTests
{
    [Theory]
    [InlineData(104, "SPAWNFLOAT+6", false, false)]
    [InlineData(105, "SPAWNFLOAT+6", true, false)]
    [InlineData(256, "SPAWNFLOAT+6", true, false)]
    [InlineData(256, "SPAWNFLOAT+SPAWNCEILING+6", false, true)]
    public void MapHeightUsesNativeStrictSpaceFormulaAndCeilingPrecedence(short ceiling, string bits, bool draw, bool ceilingSpawn)
    {
        var sim = Room(ceiling, bits); var actor = sim.Actors.Single();
        var state = NativeStateRandom.Seed(17, 0x05ec38a2u); var random = NativeStateRandom.Next(ref state) & 255;
        var expected = ceilingSpawn ? ceiling - actor.Height.ToDouble() - 12
            : draw ? 40 + (ceiling - actor.Height.ToDouble() - 40) * random / 256.0 : 0;
        Assert.Equal(Fixed.FromDouble(expected), actor.Z);
        Assert.Equal(draw, sim.CaptureState().FloatSpawnRandomState.HasValue);
        if (draw) Assert.Equal(state, sim.CaptureState().FloatSpawnRandomState);
        Assert.Equal(expected <= 0, actor.OnGround);
    }

    [Fact]
    public void DynamicPlacementContinuesSameRandomSequenceAfterFreshRestore()
    {
        var source = Room(256, "SPAWNFLOAT+NOGRAVITY+6"); var actor = source.AddBot(200, 0);
        Assert.InRange(actor.Z.ToDouble(), 40, 200); actor.SpawnFloat = false;
        var saved = SimSavegame.Write(source, true); var fresh = Room(256, "SPAWNFLOAT+NOGRAVITY+6");
        SimSavegame.Apply(fresh, saved);
        var originalState = source.CaptureState(); var restoredState = fresh.CaptureState();
        Assert.Equal(originalState.FloatSpawnRandomState, restoredState.FloatSpawnRandomState);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == actor.Id); Assert.False(loaded.SpawnFloat); Assert.True(loaded.DefaultSpawnFloat);
        var next = source.AddBot(400, 0); var restoredNext = fresh.AddBot(400, 0);
        Assert.Equal(next.Z, restoredNext.Z); Assert.Equal(source.CaptureState().FloatSpawnRandomState, fresh.CaptureState().FloatSpawnRandomState);
        ActorRaise.ReviveSupported(loaded); Assert.True(loaded.SpawnFloat);
    }

    [Fact]
    public void ExplicitProjectilePositionDoesNotConsumeFloatHeightRandom()
    {
        var patch = DehackedPatch.Apply("Thing 35\nBits = SPAWNFLOAT+NOGRAVITY\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3001 }] }, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Actors.Single(), ProjectileKind.Plasma);
        Assert.True(missile.SpawnFloat); Assert.Equal(32, missile.Z.ToDouble()); Assert.Null(sim.CaptureState().FloatSpawnRandomState);
        var saved = SimSavegame.Write(sim, true); missile.SpawnFloat = false;
        SimSavegame.Apply(sim, saved); Assert.True(missile.SpawnFloat);
    }

    [Fact]
    public void SecondaryReplacementClearsFlagWhilePrimaryReplacementPreservesIt()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = SPAWNFLOAT+6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", original);
        var cleared = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", inherited);
        Assert.True(Start(inherited).Actors.Single().SpawnFloat); Assert.False(Start(cleared).Actors.Single().SpawnFloat);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void InvalidFlagsAreRejected(int flags)
    {
        var sim = Room(104, "SPAWNFLOAT+6"); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(353, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-spawnfloat-value", error);
        var state = sim.CaptureState(); state.Actors[0].SpawnFloatFlags = flags;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    [Fact]
    public void LegacyRestoreClearsFlagsAndResetsPlacementRandom()
    {
        var sim = Room(256, "6"); var saved = SimSavegame.Write(sim, true); var actor = sim.Actors.Single();
        actor.SpawnFloat = actor.DefaultSpawnFloat = true; sim.RandomFloatSpawnZ(actor);
        SimSavegame.Apply(sim, saved); Assert.False(actor.SpawnFloat); Assert.False(actor.DefaultSpawnFloat);
        Assert.Null(sim.CaptureState().FloatSpawnRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void MalformedRandomTrailerIsRejected(int mutation)
    {
        var sim = Room(256, "SPAWNFLOAT+6"); var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(354, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        if (mutation == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        else BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 354);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(mutation == 0 ? "save-floatspawnrandom-size" : "save-floatspawnrandom-header", error);
    }

    [Fact]
    public void CurrentAndDefaultFlagsAndRandomHistoryAffectChecksum()
    {
        var sim = Room(104, "SPAWNFLOAT+6"); var actor = sim.Actors.Single();
        sim.RestoreState(sim.CaptureState()); var initial = sim.Checksum;
        actor.SpawnFloat = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(initial, sim.Checksum);
        var runtimeDisabled = sim.Checksum; actor.DefaultSpawnFloat = false;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(runtimeDisabled, sim.Checksum);
        var noRandom = sim.Checksum; sim.RandomFloatSpawnZ(new Actor { SectorIndex = 0, Height = Fixed.FromInt(16) });
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(noRandom, sim.Checksum);
    }

    private static AuthoritySimulation Room(short ceiling, string bits) => Start(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"), ceiling);
    private static AuthoritySimulation Start(DehackedPatchResult patch, short ceiling = 256)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = ceiling }],
            Things = [new LevelThing { Type = 3004, Z = 12 }] }, rngSeed: 17, dehacked: patch);
    }
}
