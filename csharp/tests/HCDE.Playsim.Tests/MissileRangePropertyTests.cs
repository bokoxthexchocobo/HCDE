using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileRangePropertyTests
{
    [Theory]
    [InlineData(3004, 300, 108, true)]
    [InlineData(3004, 300, 107.99, false)]
    [InlineData(3004, 300, 0, true)]
    [InlineData(3004, 300, -1, true)]
    [InlineData(64, 2000, 0, true)]
    [InlineData(3001, 300, 235, false)]
    public void MaximumRangeUsesAdjustedDistanceBeforeProbability(int type, short distance, double maximum, bool attacks)
    {
        var (sim, actor) = Room(type, distance); actor.MaxTargetRange = maximum;
        Tick(sim, actor);
        Assert.Equal(attacks, actor.Brain!.WindupTics > 0);
        Assert.Equal(attacks, sim.CaptureState().CheckMissileRandomState.HasValue);
    }

    [Theory]
    [InlineData(3001, 300, 236, true)]
    [InlineData(3001, 300, 236.01, false)]
    [InlineData(3004, 300, 1000, true)]
    [InlineData(66, 200, 0, true)]
    [InlineData(66, 260, 196, true)]
    [InlineData(66, 259, 196, false)]
    public void MeleeThresholdUsesStrictLowerBoundAndRequiresMeleeProfile(int type, short distance, double minimum, bool attacks)
    {
        var (sim, actor) = Room(type, distance); actor.MeleeThreshold = minimum;
        Tick(sim, actor);
        Assert.Equal(attacks, actor.Brain!.WindupTics > 0);
        Assert.Equal(attacks, sim.CaptureState().CheckMissileRandomState.HasValue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RangeOverridesRoundTripAndOlderSaveClearsThem(bool zero)
    {
        var (sim, actor) = Room(3004, 300); var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.MaxTargetRange = zero ? 0 : 108.5; actor.MeleeThreshold = zero ? 0 : -10.5;
        sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum; Assert.NotEqual(original, hash);
        var saved = SimSavegame.Write(sim, true); actor.MaxTargetRange = actor.MeleeThreshold = null;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(zero ? 0 : 108.5, actor.MaxTargetRange); Assert.Equal(zero ? 0 : -10.5, actor.MeleeThreshold);
        Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Null(actor.MaxTargetRange); Assert.Null(actor.MeleeThreshold);
        Assert.Equal(original, sim.Checksum);
    }

    [Theory]
    [InlineData(false, double.NaN)]
    [InlineData(false, double.PositiveInfinity)]
    [InlineData(false, double.NegativeInfinity)]
    [InlineData(true, double.NaN)]
    [InlineData(true, double.PositiveInfinity)]
    [InlineData(true, double.NegativeInfinity)]
    public void NonfinitePropertiesAndStructuredArchivesReject(bool melee, double invalid)
    {
        var (sim, actor) = Room(3004, 300);
        Assert.Throws<ArgumentOutOfRangeException>(() => { if (melee) actor.MeleeThreshold = invalid; else actor.MaxTargetRange = invalid; });
        var state = sim.CaptureState();
        if (melee) state.Actors[0].MeleeThresholdOverride = invalid; else state.Actors[0].MaxTargetRangeOverride = invalid;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Null(actor.MaxTargetRange); Assert.Null(actor.MeleeThreshold);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BinaryNonfiniteOverrideRejects(bool melee)
    {
        var (sim, actor) = Room(3004, 300);
        if (melee) actor.MeleeThreshold = 0; else actor.MaxTargetRange = 0;
        var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(melee ? 285 : 284, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 1);
        BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(start + 12), double.NaN);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(melee ? "save-meleethreshold-state" : "save-maxtargetrange-state", error);
    }

    private static void Tick(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, useMelee: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room(int type, short distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = distance }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = MonsterBrain.ForType(type); actor.MinMissileChance = 0;
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        sim.RestoreState(sim.CaptureState()); return (sim, actor);
    }
}
