using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontHarmClassTests
{
    [Theory]
    [InlineData(false, 3004, false)]
    [InlineData(true, 3004, true)]
    [InlineData(true, 3001, false)]
    public void RadiusUsesSourceFlagAndExactClassRatherThanSharedSpecies(bool flag, int targetClass, bool immune)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null; owner.DontHarmClass = flag;
        var direct = sim.AddBot(30, 0, 3001); direct.Brain = null; direct.Health = 1000;
        var nearby = sim.AddBot(30, 60, targetClass); nearby.Brain = null; nearby.Health = 1000;
        owner.Species = nearby.Species = "Shared"; nearby.DoHarmSpecies = true;
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        rocket.X = rocket.Y = default; rocket.Z = Fixed.FromInt(20);
        rocket.VelocityX = Fixed.FromInt(30); rocket.VelocityY = rocket.VelocityZ = default;
        rocket.Tick(); Assert.True(rocket.Destroyed);
        Assert.Equal(immune, nearby.Health == 1000);
    }

    [Fact]
    public void ClassFilterDoesNotProtectPlayers()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DontHarmClass = true;
        Assert.False(player.RadiusSourceImmune(player));
        Assert.False(sim.AddBot(80, 0).RadiusSourceImmune(null));
    }

    [Fact]
    public void FlagAccessSaveChecksumAndOldResetAreSupported()
    {
        var sim = Room(); var actor = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        var baseline = sim.Checksum;
        Assert.True(AcsActorFlags.TrySet(actor, "actor.dontharmclass", true));
        Assert.True(AcsActorFlags.TryGet(actor, "DONTHARMCLASS", out var value)); Assert.True(value);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(306, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().DontHarmClass); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older); Assert.False(restored.Players.Single().DontHarmClass);
    }

    [Fact]
    public void InvalidBinaryFlagIsRejected()
    {
        var sim = Room(); sim.Players.Single().DontHarmClass = true;
        var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = -500 }],
    });
}
