using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VulnerableTests
{
    [Theory]
    [InlineData(false, false, 100)]
    [InlineData(false, true, 93)]
    [InlineData(true, false, 93)]
    [InlineData(true, true, 93)]
    public void DamageRequiresShootableOrVulnerable(bool shootable, bool vulnerable, int health)
    {
        var actor = new Actor { Shootable = shootable, Vulnerable = vulnerable };
        ActorDamage.Apply(actor, 7); Assert.Equal(health, actor.Health);
    }

    [Theory]
    [InlineData("VULNERABLE")]
    [InlineData("vulnerable")]
    [InlineData("Actor.Vulnerable")]
    public void LegacyFlagHandlerReadsSetsAndClears(string name)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TryGet(actor, name, out var initial)); Assert.False(initial);
        Assert.True(AcsActorFlags.TrySet(actor, name, true)); Assert.True(actor.Vulnerable);
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.Vulnerable);
    }

    [Fact]
    public void VulnerableStillRespectsInvulnerability()
    {
        var actor = new Actor { Shootable = false, Vulnerable = true, Invulnerable = true };
        ActorDamage.Apply(actor, 7); Assert.Equal(100, actor.Health);
        ActorDamage.Apply(actor, 7, flags: DamageFlags.Forced); Assert.Equal(93, actor.Health);
    }

    [Fact]
    public void LethalDamageUsesExistingDeathLifecycle()
    {
        var actor = new Actor { Shootable = false, Vulnerable = true, IsMonster = true };
        Assert.True(ActorDamage.Apply(actor, 100).Killed); Assert.True(actor.Corpse); Assert.Equal(1, actor.DeathCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SaveRestoresVulnerabilityWithOrWithoutReconstruction(bool reconstruct)
    {
        var sim = Room(); Actor actor = sim.AddBot(100, 0); var id = actor.Id;
        actor.Shootable = false; actor.Vulnerable = true; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(230, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else actor.Vulnerable = false;
        SimSavegame.Apply(sim, saved); actor = sim.Actors.Single(a => a.Id == id);
        Assert.True(actor.Vulnerable); Assert.False(actor.Shootable); Assert.Equal(saved, SimSavegame.Write(sim, true));
        ActorDamage.Apply(actor, 7); Assert.Equal(13, actor.Health);
    }

    [Fact]
    public void OlderSaveResetsLaterVulnerableFlag()
    {
        var sim = Room(); var player = sim.Players.Single(); var saved = SimSavegame.Write(sim);
        player.Vulnerable = true; SimSavegame.Apply(sim, saved); Assert.False(player.Vulnerable);
    }

    [Fact]
    public void MalformedFlagIsRejected()
    {
        var sim = Room(); sim.Players.Single().Vulnerable = true; var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-vulnerable-value", error);
    }

    [Fact]
    public void RocketSplashCanDamageNonShootableVulnerableActor()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.Brain = null;
        actor.Shootable = false; actor.Vulnerable = true;
        var rocket = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        rocket.X = Fixed.FromInt(80); rocket.Y = default; rocket.Z = default;
        rocket.ExplodeForAction(); Assert.True(actor.Health < 20);
    }

    [Fact]
    public void VulnerabilityDoesNotMakeActorAnOrdinaryAimTarget()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.Shootable = false; actor.Vulnerable = true;
        Assert.Null(CombatTrace.FindTarget(sim, sim.Players.Single(), 1024));
    }

    [Fact]
    public void VulnerableFlagParticipatesInChecksum()
    {
        var sim = Room(); var other = Room(); other.Players.Single().Vulnerable = true;
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
