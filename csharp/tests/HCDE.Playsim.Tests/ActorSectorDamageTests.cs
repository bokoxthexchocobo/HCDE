using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSectorDamageTests
{
    public static IEnumerable<object[]> EligibilityCases()
    {
        foreach (var player in new[] { false, true })
        foreach (var sectorOptIn in new[] { false, true })
        foreach (var immune in new[] { false, true })
        foreach (var forced in new[] { false, true })
            yield return [player, sectorOptIn, immune, forced];
    }

    [Theory]
    [MemberData(nameof(EligibilityCases))]
    public void ActorOverridesFollowNativePrecedence(bool player, bool sectorOptIn, bool immune, bool forced)
    {
        var sim = Room(player, sectorOptIn, 5); var actor = sim.Actors.Single();
        actor.NoSectorDamage = immune; actor.ForceSectorDamage = forced;
        var before = actor.Health;
        sim.Tick();
        var eligible = forced || !immune && (player || sectorOptIn);
        Assert.Equal(before - (eligible ? 5 : 0), actor.Health);
    }

    [Theory]
    [InlineData(false, 30)]
    [InlineData(true, 40)]
    public void OverridesAlsoControlHealing(bool forced, int expected)
    {
        var sim = Room(false, true, -10); var actor = sim.Actors.Single();
        actor.Health = 30; actor.NoSectorDamage = true; actor.ForceSectorDamage = forced;
        sim.Tick(); Assert.Equal(expected, actor.Health);
    }

    [Fact]
    public void ForceDoesNotBypassFloorContactOrInvulnerability()
    {
        var sim = Room(false, false, 5); var actor = sim.Actors.Single();
        actor.ForceSectorDamage = true; actor.NoGravity = true; actor.Z = Fixed.FromInt(16);
        sim.Tick(); Assert.Equal(60, actor.Health);
        actor.Z = Fixed.FromInt(0); actor.Invulnerable = true;
        sim.Tick(); Assert.Equal(60, actor.Health);
        actor.Invulnerable = false;
        sim.Tick(); Assert.Equal(55, actor.Health);
    }

    [Fact]
    public void EnablingForceUsesExistingLevelClock()
    {
        var sim = Room(false, false, 5); var actor = sim.Actors.Single();
        sim.Level.Sectors[0].DamageInterval = 4;
        sim.Tick(); actor.ForceSectorDamage = true;
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(60, actor.Health);
        sim.Tick(); Assert.Equal(55, actor.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActorFlagsParticipateInChecksumWithoutDamage(bool force)
    {
        var original = Room(true, false, 0); var changed = Room(true, false, 0);
        changed.Actors.Single().NoSectorDamage = !force;
        changed.Actors.Single().ForceSectorDamage = force;
        original.Tick(); changed.Tick();
        Assert.NotEqual(original.Checksum, changed.Checksum);
        Assert.Equal(original.Actors.Single().Health, changed.Actors.Single().Health);
    }

    private static AuthoritySimulation Room(bool player, bool sectorOptIn, int amount)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = amount, DamageInterval = 1,
                HurtMonsters = sectorOptIn }],
            Things = [new LevelThing { Type = player ? 1 : 3001 }],
        });
        sim.Actors.Single().Brain = null;
        return sim;
    }
}
