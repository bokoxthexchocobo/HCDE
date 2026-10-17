using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SummonedTargetingTests
{
    [Fact]
    public void SummonerTracerExcludesOwnerAndFriendlyAllies()
    {
        var (sim, minion) = Room(); var player = sim.Players.Single(); var enemy = sim.AddBot(250, 192);
        minion.SummonedMonster = true; minion.TracerId = player.Id;
        Assert.False(minion.IsOkayToAttack(player)); Assert.True(minion.IsOkayToAttack(enemy));
        enemy.Friendly = true; Assert.False(minion.IsOkayToAttack(enemy));
    }

    [Fact]
    public void MissingOrDestroyedSummonerPreventsTargetAcquisition()
    {
        var (sim, minion) = Room(); var enemy = sim.AddBot(250, 192); minion.SummonedMonster = true;
        Assert.False(minion.IsOkayToAttack(enemy));
        var summoner = sim.AddBot(100, 192); minion.TracerId = summoner.Id; summoner.Destroy();
        Assert.False(minion.IsOkayToAttack(enemy));
    }

    [Fact]
    public void SummonedFlagTakesPrecedenceOverProjectileOwner()
    {
        var (sim, summoner) = Room(); var player = sim.Players.Single();
        var missile = sim.SpawnProjectile(player, ProjectileKind.ImpBall); missile.SummonedMonster = true;
        missile.TracerId = summoner.Id; missile.SetXYZ(192, 192, 0);
        Assert.False(missile.IsOkayToAttack(summoner)); Assert.True(missile.IsOkayToAttack(player));
        missile.SummonedMonster = false; Assert.False(missile.IsOkayToAttack(player)); Assert.True(missile.IsOkayToAttack(summoner));
    }

    [Fact]
    public void SummonerTracerTakesPrecedenceOverFriendlyPlayerReference()
    {
        var (sim, minion) = Room(); var summoner = sim.AddBot(100, 192); minion.Friendly = true;
        minion.FriendPlayer = sim.Players.Single().PlayerNum + 1; minion.SummonedMonster = true; minion.TracerId = summoner.Id;
        Assert.False(minion.IsOkayToAttack(summoner));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void CandidateMinionOnSameSideIsExcludedEvenWithoutFriendlyFlags(bool summoned, bool accepted)
    {
        var (sim, observer) = Room(); var candidate = sim.AddBot(250, 192);
        candidate.TracerId = observer.Id; candidate.SummonedMonster = summoned;
        Assert.Equal(accepted, observer.IsOkayToAttack(candidate));
    }

    [Fact]
    public void TwoSummonsWithSameSummonerCannotAttackEachOther()
    {
        var (sim, first) = Room(); var second = sim.AddBot(250, 192); var summoner = sim.AddBot(100, 192);
        first.SummonedMonster = second.SummonedMonster = true; first.TracerId = second.TracerId = summoner.Id;
        Assert.False(first.IsOkayToAttack(second)); Assert.False(second.IsOkayToAttack(first));
        second.TracerId = sim.Players.Single().Id; Assert.True(first.IsOkayToAttack(second));
    }

    [Fact]
    public void RoughSearchSkipsOwnSummonAndFindsEnemy()
    {
        var (sim, observer) = Room(); var enemy = sim.AddBot(220, 210); var summon = sim.AddBot(210, 210);
        summon.SummonedMonster = true; summon.TracerId = observer.Id;
        Assert.Same(enemy, ActorEnemySearchActions.RoughMonsterSearch(observer, 0));
    }

    [Fact]
    public void SameSideRejectionOccursBeforeVisibilityRandomness()
    {
        var (sim, observer) = Room(); var summon = sim.AddBot(210, 210);
        summon.SummonedMonster = true; summon.TracerId = observer.Id; summon.Invisible = true;
        Assert.False(observer.IsOkayToAttack(summon)); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void FlagAndTracerRoundTripAndLegacySaveRestoresDefault()
    {
        var (sim, minion) = Room(); var player = sim.Players.Single(); minion.TracerId = player.Id;
        var old = SimSavegame.Write(sim, true); minion.SummonedMonster = true;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(328, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        minion.SummonedMonster = false; minion.TracerId = null; SimSavegame.Apply(sim, bytes);
        Assert.True(minion.SummonedMonster); Assert.Equal(player.Id, minion.TracerId);
        Assert.False(minion.IsOkayToAttack(player)); Assert.Equal(checksum, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old); Assert.False(minion.SummonedMonster);
    }

    [Fact]
    public void FlagParticipatesInChecksum()
    {
        var (sim, actor) = Room(); sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        actor.SummonedMonster = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Fact]
    public void AcsCanSetQueryAndClearFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "SUMMONEDMONSTER", true));
        Assert.True(AcsActorFlags.TryGet(actor, "summonedmonster", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "SummonedMonster", false)); Assert.False(actor.SummonedMonster);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidArchiveFlagIsRejected(int flag)
    {
        var (sim, actor) = Room(); actor.SummonedMonster = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flag);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-summonedmonster-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 5, 5), []), Things = [new LevelThing { Type = 1, X = 400, Y = 192 }] });
        return (sim, sim.AddBot(192, 192));
    }
}
