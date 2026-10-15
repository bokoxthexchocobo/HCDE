using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeCheckSightTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InvisibleTargetsUseNativeCheckSightThresholdAndOneDraw(int kind)
    {
        var (sim, actor) = Room(); var (reference, _) = Room(); var target = sim.Players.Single();
        target.Invisible = kind is 0 or 3; target.MinVisible = kind == 1;
        if (kind == 2) { target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0; }
        actor.SeeInvisible = kind == 3;
        for (var i = 0; i < 256; i++)
        {
            Assert.Equal(reference.NextCheckSightByte() <= 50, CombatTrace.CheckSight(sim, actor, target));
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IgnoreVisibilityBypassesBlockedAndInvisibleTargetsWithoutDraw(bool blocked)
    {
        var (sim, actor) = Room(); var target = sim.Players.Single();
        target.MVisBlocked = blocked; target.Invisible = true;
        Assert.True(CombatTrace.CheckSight(sim, actor, target, ActorSightFlags.IgnoreVisibility));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void MvisBlockedRejectsBeforeRandomVisibilityCheckEvenForSelf()
    {
        var (sim, actor) = Room(); actor.MVisBlocked = actor.Invisible = true;
        Assert.False(CombatTrace.CheckSight(sim, actor, actor));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(6, true)]
    public void SeeingPastArbitraryBlockingLineRequiresBothNativeFlags(int flags, bool sees)
    {
        var (sim, actor) = Room(blockEverything: true);
        Assert.Equal(sees, CombatTrace.CheckSight(sim, actor, sim.Players.Single(), (ActorSightFlags)flags));
    }

    [Fact]
    public void IgnoreVisibilityDoesNotBypassGeometry()
    {
        var (sim, actor) = Room(blockEverything: true); sim.Players.Single().Invisible = true;
        Assert.False(CombatTrace.CheckSight(sim, actor, sim.Players.Single(), ActorSightFlags.IgnoreVisibility));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void NullActorsReturnFalseAndUnsupportedFlagsRejectBeforeDraw()
    {
        var (sim, actor) = Room(); actor.Invisible = true;
        Assert.False(CombatTrace.CheckSight(sim, null, actor)); Assert.False(CombatTrace.CheckSight(sim, actor, null));
        Assert.Throws<NotSupportedException>(() => CombatTrace.CheckSight(sim, actor, actor, (ActorSightFlags)8));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void MissileDecisionHonorsMvisBlockedBeforeConsumingRetaliation()
    {
        var (sim, actor) = Room(); sim.Players.Single().MVisBlocked = true; actor.JustHit = true;
        Assert.False(actor.Brain!.CheckMissileRange(sim, actor)); Assert.True(actor.JustHit);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedStreamRestoresChecksumAndOlderSaveClearsIt()
    {
        var (sim, _) = Room(); var old = SimSavegame.Write(sim, true);
        sim.NextCheckSightByte(); sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); var expected = sim.NextCheckSightByte();
        SimSavegame.Apply(sim, saved); Assert.Equal(hash, sim.Checksum); Assert.Equal(expected, sim.NextCheckSightByte());
        SimSavegame.Apply(sim, old); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void InvalidRandomTrailerPriorVersionRejects()
    {
        var (sim, _) = Room(); sim.NextCheckSightByte(); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 287);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-checksightrandom-header", error);
    }

    private static (AuthoritySimulation, Actor) Room(bool blockEverything = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = blockEverything ? [new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128,
                SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag | LevelLine.BlockEverythingFlag }] : [],
            Things = [new LevelThing { Type = 1, X = 64 }]
        });
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        return (sim, actor);
    }
}
