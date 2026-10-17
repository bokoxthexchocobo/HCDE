using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontHarmSpeciesTests
{
    [Theory]
    [InlineData(false, "Shared", true)]
    [InlineData(true, "shared", false)]
    [InlineData(true, "Other", true)]
    public void TouchyMineChecksItsOwnFlagAndResolvedSpecies(bool flag, string moverSpecies, bool dies)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0, 3001); var mine = sim.AddBot(50, 0, 3004);
        mover.Brain = mine.Brain = null; mover.Species = moverSpecies; mine.Species = "Shared";
        mine.Touchy = mine.Armed = true; mine.DontHarmSpecies = flag;
        ActorPhysics.TryMove(sim, mover, 20, 0, out _);
        Assert.Equal(dies, mine.IsDead);
    }

    [Theory]
    [InlineData(false, "Shared", false)]
    [InlineData(true, "shared", true)]
    [InlineData(true, "Other", false)]
    public void ProjectileRadiusUsesOwnersSpeciesProtection(bool flag, string targetSpecies, bool immune)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        owner.Species = "Shared"; owner.DontHarmSpecies = flag;
        var direct = sim.AddBot(30, 0, 3001); direct.Brain = null; direct.Health = 1000;
        var nearby = sim.AddBot(30, 60, 3001); nearby.Brain = null; nearby.Health = 1000; nearby.Species = targetSpecies;
        nearby.DoHarmSpecies = true;
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        rocket.X = rocket.Y = default; rocket.Z = Fixed.FromInt(20);
        rocket.VelocityX = Fixed.FromInt(30); rocket.VelocityY = rocket.VelocityZ = default;
        rocket.Tick();
        Assert.True(rocket.Destroyed);
        Assert.Equal(immune, nearby.Health == 1000);
    }

    [Fact]
    public void RadiusSpeciesRuleExcludesPlayersAndRequiresSourceFlag()
    {
        var sim = Room(); var source = sim.AddBot(80, 0); source.DontHarmSpecies = true; source.Species = "Shared";
        var player = sim.Players.Single(); player.Species = "Shared";
        Assert.False(player.RadiusSpeciesImmune(source));
        Assert.False(source.RadiusSpeciesImmune(null));
    }

    [Fact]
    public void QualifiedAcsFlagSupportsReadAndWrite()
    {
        var actor = Room().Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "actor.dontharmspecies", true));
        Assert.True(AcsActorFlags.TryGet(actor, "DONTHARMSPECIES", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "DONTHARMSPECIES", false)); Assert.False(actor.DontHarmSpecies);
    }

    [Fact]
    public void SaveAndChecksumPreserveFlagAndOlderSaveClearsIt()
    {
        var sim = Room(); var actor = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        var baseline = sim.Checksum; actor.DontHarmSpecies = true;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(305, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().DontHarmSpecies); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older); Assert.False(restored.Players.Single().DontHarmSpecies);
    }

    [Fact]
    public void InvalidBinaryFlagFailsReading()
    {
        var sim = Room(); sim.Players.Single().DontHarmSpecies = true;
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
