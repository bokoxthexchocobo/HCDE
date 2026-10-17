using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileExtendedDefaultsTests
{
    [Theory]
    [InlineData("PASSMOBJ", true, false, false, false, false)]
    [InlineData("NOTELEPORT", false, true, false, false, false)]
    [InlineData("CANSLIDE", false, false, true, false, false)]
    [InlineData("INVULNERABLE", false, false, false, true, false)]
    [InlineData("DORMANT", false, false, false, false, true)]
    [InlineData("PASSMOBJ+NOTELEPORT+CANSLIDE+INVULNERABLE+DORMANT", true, true, true, true, true)]
    public void SupportedSecondaryFlagsApplyBeforeProjectileIsReturned(string bits, bool pass, bool noTeleport,
        bool slide, bool invulnerable, bool dormant)
    {
        var (sim, missile) = Room(bits);
        AssertFlags(missile, pass, noTeleport, slide, invulnerable, dormant);
        var bytes = SimSavegame.Write(sim, true);
        missile.CanPass = !pass; missile.NoTeleport = !noTeleport; missile.CanSlide = !slide;
        missile.Invulnerable = !invulnerable; missile.Dormant = !dormant;
        SimSavegame.Apply(sim, bytes);
        AssertFlags(missile, pass, noTeleport, slide, invulnerable, dormant);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void QuarterGravityDefaultDoesNotOverrideNoGravity(bool noGravity)
    {
        var (_, missile) = Room("LOGRAV"); Assert.Equal(0.25, missile.Gravity.ToDouble());
        Assert.True(missile.NoGravity);
        missile.NoGravity = noGravity; missile.SetXYZ(100, 0, 64);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(noGravity ? 0 : -0.25, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void InheritedLowGravitySideEffectSurvivesReplacingExtendedFlags()
    {
        var first = DehackedPatch.Apply("Thing 35\nBits = LOGRAV+NOTELEPORT\n");
        var second = DehackedPatch.Apply("Thing 35\nBits = CANSLIDE\n", first);
        var (sim, missile) = Start(second);
        Assert.Equal(0.25, missile.Gravity.ToDouble()); Assert.True(missile.CanSlide); Assert.False(missile.NoTeleport);
        var bytes = SimSavegame.Write(sim, true); missile.Gravity = Fixed.FromInt(2);
        SimSavegame.Apply(sim, bytes); Assert.Equal(0.25, missile.Gravity.ToDouble());
    }

    [Fact]
    public void UnpatchedSecondaryFlagsRetainStockProjectileDefaults()
    {
        var (_, missile) = Start(DehackedPatch.Apply("Thing 35\nHit points = 20\n"));
        Assert.True(missile.NoTeleport); Assert.True(missile.NoGravity); Assert.False(missile.CanPass);
        Assert.False(missile.CanSlide); Assert.False(missile.Invulnerable); Assert.False(missile.Dormant);
        Assert.Equal(1, missile.Gravity.ToDouble());
    }

    [Fact]
    public void PrimaryAmbushBitReachesProjectileDefaultLoader()
    {
        var (_, missile) = Room("AMBUSH+NOGRAVITY"); Assert.True(missile.Ambush);
    }

    [Fact]
    public void DefaultLoadingKeepsPatchedDimensionsAndSpeed()
    {
        var (_, missile) = Start(DehackedPatch.Apply("Thing 35\nBits = LOGRAV+PASSMOBJ\nSpeed = 12\nHeight = 786432\nWidth = 196608\n"));
        Assert.Equal(12, missile.Speed); Assert.Equal(12, missile.VelocityX.ToDouble());
        Assert.Equal(12, missile.Height.ToDouble()); Assert.Equal(3, missile.Radius.ToDouble());
        Assert.Equal(missile.Height, missile.ResurrectionHeight); Assert.Equal(missile.Radius, missile.ResurrectionRadius);
        Assert.Equal(missile.MovementSpeed.Raw, missile.SpawnTuning!.Value.SpeedRaw);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RuntimeSecondaryOverridesRoundTripAndLegacyResetsStockState(bool noTeleport, bool dormant)
    {
        var (sim, missile) = Start(DehackedPatch.Apply("")); var legacy = SimSavegame.Write(sim, true);
        missile.NoTeleport = noTeleport; missile.Dormant = dormant;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var bytes = SimSavegame.Write(sim, true);
        missile.NoTeleport = !noTeleport; missile.Dormant = !dormant; SimSavegame.Apply(sim, bytes);
        Assert.Equal(noTeleport, missile.NoTeleport); Assert.Equal(dormant, missile.Dormant); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, legacy); Assert.True(missile.NoTeleport); Assert.False(missile.Dormant);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void InvalidSecondaryArchiveFlagsAreRejected(int value)
    {
        var (sim, missile) = Room("DORMANT"); var bytes = SimSavegame.Write(sim, true);
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-projectilesecondary-value", error);
    }

    private static void AssertFlags(Actor actor, bool pass, bool noTeleport, bool slide, bool invulnerable, bool dormant)
    {
        Assert.Equal(pass, actor.CanPass); Assert.Equal(noTeleport, actor.NoTeleport); Assert.Equal(slide, actor.CanSlide);
        Assert.Equal(invulnerable, actor.Invulnerable); Assert.Equal(dormant, actor.Dormant);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string bits)
        => Start(DehackedPatch.Apply($"Thing 35\nBits = {bits}\n"));

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
