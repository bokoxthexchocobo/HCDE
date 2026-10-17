using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FallingDamageTests
{
    [Theory]
    [InlineData(FallingDamageStyle.None, -84, 100)]
    [InlineData(FallingDamageStyle.ZDoom, -19, 100)]
    [InlineData(FallingDamageStyle.ZDoom, -20, 98)]
    [InlineData(FallingDamageStyle.Hexen, -23, 100)]
    [InlineData(FallingDamageStyle.Hexen, -24, 97)]
    [InlineData(FallingDamageStyle.Hexen, -30, 81)]
    [InlineData(FallingDamageStyle.Strife, -20, 100)]
    [InlineData(FallingDamageStyle.Strife, -21, 45)]
    public void NativeThresholdsAndTruncatedFormulas(FallingDamageStyle style, int velocity, int expected)
    {
        var sim = Room(style); var player = sim.Players.Single(); player.VelocityZ = Fixed.FromInt(velocity);
        FallingDamage.Apply(player); Assert.Equal(expected, player.Health);
        if (expected < 100)
            Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*land" && sound.Flags == 0);
        else Assert.Empty(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(20, 1)]
    [InlineData(45, 0)]
    [InlineData(1, -44)]
    public void HexenLowSpeedDeathLimitUsesStrictDamageGreaterThanHealth(int health, int expected)
    {
        var sim = Room(FallingDamageStyle.Hexen); var player = sim.Players.Single(); player.Health = health;
        player.VelocityZ = Fixed.FromInt(-38); FallingDamage.Apply(player); Assert.Equal(expected, player.Health);
    }

    [Theory]
    [InlineData(FallingDamageStyle.Hexen, -63)]
    [InlineData(FallingDamageStyle.ZDoom, -84)]
    public void AutomaticDeathAndGodBuddhaExceptions(FallingDamageStyle style, int velocity)
    {
        foreach (var protection in new[] { 0, 1, 2, 3 })
        {
            var sim = Room(style); var player = sim.Players.Single();
            player.GodMode = protection == 1; player.Buddha = protection == 2;
            if (protection == 3) player.PowerBuddhaTics = 10;
            player.VelocityZ = Fixed.FromInt(velocity); FallingDamage.Apply(player);
            if (protection == 0) Assert.True(player.IsDead);
            else Assert.Equal(protection == 1 ? 100 : 1, player.Health);
        }
    }

    [Fact]
    public void SectorOptOutSuppressesDamageAndDamageLandingSound()
    {
        var sim = Room(FallingDamageStyle.ZDoom, noFall: true); var player = sim.Players.Single();
        player.VelocityZ = Fixed.FromInt(-84); FallingDamage.Apply(player);
        Assert.Equal(100, player.Health); Assert.Empty(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualFloorAndSupportImpactApplyDamageBeforeGrunt(bool support)
    {
        var sim = Room(FallingDamageStyle.ZDoom); var player = sim.Players.Single();
        if (support) sim.AddBot(0, 0).Brain = null;
        player.Z = Fixed.FromInt(support ? 60 : 1); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-20);
        ActorPhysics.Step(sim, player);
        Assert.Equal(98, player.Health);
        Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*land" && sound.Flags == 0);
        Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*grunt");
    }

    [Fact]
    public void FatalImpactSkipsLivingGruntAndRetainsFallingTypedDeath()
    {
        var sim = Room(FallingDamageStyle.ZDoom); var player = sim.Players.Single();
        player.SetTypedDeath("Falling", player.SpawnState); player.VelocityZ = Fixed.FromInt(-84);
        player.Z = Fixed.FromInt(1); player.OnGround = false; ActorPhysics.Step(sim, player);
        Assert.True(player.IsDead); Assert.Equal("Falling", player.DamageType);
        Assert.DoesNotContain(sim.PendingSoundRequests, sound => sound.Sound == "*grunt");
    }

    [Fact]
    public void LevelPolicyCopiesHashesAndReplaysSavedAirborneImpact()
    {
        var sim = Room(FallingDamageStyle.Hexen); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(1); player.VelocityZ = Fixed.FromInt(-24); player.OnGround = false;
        var saved = SimSavegame.Write(sim, true); var restored = Room(FallingDamageStyle.Hexen);
        SimSavegame.Apply(restored, saved); sim.Tick(); restored.Tick();
        Assert.Equal(97, player.Health); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(sim.PendingSoundRequests, restored.PendingSoundRequests);
        Assert.Equal(FallingDamageStyle.Hexen, sim.Level.CopyForSimulation().FallingDamage);
        Assert.NotEqual(Room(FallingDamageStyle.None).Checksum, Room(FallingDamageStyle.Hexen).Checksum);
        Assert.NotEqual(Room(FallingDamageStyle.Hexen).Checksum, Room(FallingDamageStyle.Hexen, true).Checksum);
    }

    private static AuthoritySimulation Room(FallingDamageStyle style, bool noFall = false) => AuthoritySimulation.Start(new PlayLevel
    {
        FallingDamage = style, Sectors = [new LevelSector { CeilingHeight = 256, NoFallingDamage = noFall }],
        Things = [new LevelThing { Type = 1 }],
    });
}
