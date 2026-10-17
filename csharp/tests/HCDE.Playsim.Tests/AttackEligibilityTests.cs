using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AttackEligibilityTests
{
    [Fact]
    public void MonsterAcceptsVisiblePlayerAndMonsterButNotItself()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var other = sim.AddBot(100, 0);
        Assert.True(actor.IsOkayToAttack(other)); Assert.True(actor.IsOkayToAttack(sim.Players.Single()));
        Assert.False(actor.IsOkayToAttack(actor));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void RejectsNonmonsterNonshootableDormantAndNeverTargetCandidates(int gate)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        if (gate == 0) candidate.IsMonster = false;
        if (gate == 1) candidate.Shootable = false;
        if (gate == 2) candidate.Dormant = true;
        if (gate == 3) candidate.NeverTarget = true;
        var random = sim.CaptureState().CheckSightRandomState;
        Assert.False(actor.IsOkayToAttack(candidate)); Assert.Equal(random, sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void ShootableDeadActorsAreNotRejectedByAnExtraHealthGate()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        candidate.Health = 0; candidate.Shootable = true; candidate.IsMonster = true;
        Assert.True(actor.IsOkayToAttack(candidate));
    }

    [Fact]
    public void ProjectileUsesOwnerForFriendChecks()
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall); missile.SetXYZ(0, 0, 0);
        Assert.False(missile.IsOkayToAttack(owner)); Assert.True(missile.IsOkayToAttack(target));
        target.Friendly = true; Assert.False(missile.IsOkayToAttack(target));
    }

    [Fact]
    public void OwnerlessProjectileCannotAcquireTargets()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall); missile.ReassignOwner(null);
        Assert.False(missile.IsOkayToAttack(candidate));
    }

    [Fact]
    public void FriendlyPlayerReferenceExcludesItsPlayerAndAllies()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        actor.Friendly = true; actor.FriendPlayer = sim.Players.Single().PlayerNum + 1;
        Assert.False(actor.IsOkayToAttack(sim.Players.Single())); Assert.True(actor.IsOkayToAttack(candidate));
        candidate.Friendly = true; Assert.False(actor.IsOkayToAttack(candidate));
        actor.FriendPlayer = 8; candidate.Friendly = false; Assert.False(actor.IsOkayToAttack(candidate));
    }

    [Fact]
    public void FriendlyActorsWithoutSpecificPlayerRejectEachOther()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        actor.Friendly = candidate.Friendly = true;
        Assert.False(actor.IsOkayToAttack(candidate));
    }

    [Fact]
    public void NoTargetAndCantSeekAreNotExtraAttackEligibilityGates()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        candidate.NoTarget = candidate.CantSeek = true;
        Assert.True(actor.IsOkayToAttack(candidate)); Assert.False(actor.CanSeek(candidate));
    }

    [Fact]
    public void SightChecksOriginateAtProjectileRatherThanOwner()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Lines = [new LevelLine { X1 = 50, Y1 = -100, X2 = 50, Y2 = 100, SideBack = -1 }] };
        var sim = AuthoritySimulation.Start(level); var owner = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        Assert.False(missile.IsOkayToAttack(candidate)); missile.X = Fixed.FromInt(75);
        Assert.True(missile.IsOkayToAttack(candidate));
    }

    [Fact]
    public void CoopPlayersAreRejectedEvenWhenFriendlyFlagsAreCleared()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 100 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.Players.First(); var candidate = sim.Players.Last();
        owner.Friendly = candidate.Friendly = false;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        Assert.False(missile.IsOkayToAttack(candidate));
    }

    [Fact]
    public void NativeVisibilityDrawsAndSaveContinuationArePreserved()
    {
        var sim = Room(); var reference = Room(); var actor = sim.AddBot(0, 0); var candidate = sim.AddBot(100, 0);
        candidate.Invisible = true;
        for (var i = 0; i < 32; i++)
            Assert.Equal(reference.NextCheckSightByte() <= 50, actor.IsOkayToAttack(candidate));
        Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        var bytes = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 16).Select(_ => actor.IsOkayToAttack(candidate)).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => actor.IsOkayToAttack(candidate)).ToArray());
    }

    private static PlayLevel Level() => new()
    { Things = [new LevelThing { Type = 1, X = 200 }], Sectors = [new LevelSector { CeilingHeight = 256 }] };
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(Level());
}
