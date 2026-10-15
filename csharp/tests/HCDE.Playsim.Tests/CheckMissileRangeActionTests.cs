using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CheckMissileRangeActionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FriendlyRetaliationUsesDefectThresholdAndExactlyOneDraw(bool playerTarget)
    {
        var (sim, actor) = Room(); var (reference, _) = Room(); actor.Friendly = true;
        var target = playerTarget ? (Actor)sim.Players.Single() : sim.AddBot(300, 0);
        target.Friendly = true; actor.SetTargetPointer(target);
        var attacks = 0;
        for (var i = 0; i < 256; i++)
        {
            actor.JustHit = true; var expected = reference.NextDefectByte() > 128;
            Assert.Equal(expected, actor.Brain!.CheckMissileRange(sim, actor));
            Assert.False(actor.JustHit);
            Assert.Equal(reference.CaptureState().DefectRandomState, sim.CaptureState().DefectRandomState);
            Assert.Null(sim.CaptureState().CheckMissileRandomState);
            if (expected) attacks++;
        }
        Assert.InRange(attacks, 1, 255);
    }

    [Fact]
    public void MutuallyJustHitFriendlyMonsterRejectsWithoutDefectDraw()
    {
        var (sim, actor) = Room(); actor.Friendly = true;
        var target = sim.AddBot(300, 0); target.Friendly = target.JustHit = true;
        actor.SetTargetPointer(target); actor.JustHit = true;
        Assert.False(actor.Brain!.CheckMissileRange(sim, actor));
        Assert.False(actor.JustHit); Assert.Null(sim.CaptureState().DefectRandomState);
    }

    [Fact]
    public void FriendlyRetaliationRejectsDeadTargetBeforeDraw()
    {
        var (sim, actor) = Room(); actor.Friendly = true; actor.JustHit = true;
        sim.Players.Single().Health = 0;
        Assert.False(actor.Brain!.CheckMissileRange(sim, actor));
        Assert.False(actor.JustHit); Assert.Null(sim.CaptureState().DefectRandomState);
    }

    [Fact]
    public void HostileRetaliationBypassesReactionRangeAndRandomDraws()
    {
        var (sim, actor) = Room(); actor.JustHit = true; actor.ReactionTime = 100; actor.MaxTargetRange = 1;
        Assert.True(actor.Brain!.CheckMissileRange(sim, actor));
        Assert.False(actor.JustHit); Assert.Null(sim.CaptureState().CheckMissileRandomState);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void NonzeroReactionRejectsOrdinaryAttackWithoutDraw(int reaction)
    {
        var (sim, actor) = Room(); actor.ReactionTime = reaction;
        Assert.False(actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Null(sim.CaptureState().CheckMissileRandomState);
    }

    [Fact]
    public void ActionUsesProbabilityForGenericBrainWithoutChaseWalkOrFearGates()
    {
        var (sim, actor) = Room(); var (reference, _) = Room();
        actor.MoveCount = 20; actor.Frightened = true;
        var expected = reference.NextCheckMissileByte() >= 108;
        Assert.Equal(expected, actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Equal(reference.CaptureState().CheckMissileRandomState, sim.CaptureState().CheckMissileRandomState);
        Assert.Null(sim.CaptureState().ScaredyCatRandomState); Assert.Equal(20, actor.MoveCount);
    }

    [Fact]
    public void MissingTargetPreservesPendingRetaliation()
    {
        var (sim, actor) = Room(); actor.SetTargetPointer(null); actor.JustHit = true;
        Assert.False(actor.Brain!.CheckMissileRange(sim, actor)); Assert.True(actor.JustHit);
    }

    [Fact]
    public void DefectStreamSaveRestoresChecksumAndOlderSaveClearsIt()
    {
        var (sim, _) = Room(); var old = SimSavegame.Write(sim, true);
        sim.NextDefectByte(); sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); var expected = sim.NextDefectByte();
        SimSavegame.Apply(sim, saved); Assert.Equal(hash, sim.Checksum); Assert.Equal(expected, sim.NextDefectByte());
        SimSavegame.Apply(sim, old); Assert.Null(sim.CaptureState().DefectRandomState);
    }

    [Fact]
    public void InvalidDefectArchiveHeaderRejects()
    {
        var (sim, _) = Room(); sim.NextDefectByte(); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 286);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-defectrandom-header", error);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        return (sim, actor);
    }
}
