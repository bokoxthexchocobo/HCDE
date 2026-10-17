using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileContactDefaultsTests
{
    [Theory]
    [InlineData("SPECIAL+NOGRAVITY", 1)]
    [InlineData("PICKUP+NOGRAVITY", 2)]
    [InlineData("DROPPED+NOGRAVITY", 4)]
    [InlineData("FRIEND+NOGRAVITY", 8 | 32)]
    [InlineData("STEALTH+NOGRAVITY", 0)]
    [InlineData("NOBLOOD+NOGRAVITY", 16)]
    [InlineData("noblood+NOGRAVITY", 16)]
    [InlineData("524800", 16)]
    [InlineData("SPECIAL+PICKUP+DROPPED+FRIEND+NOBLOOD+NOGRAVITY", 63)]
    public void PrimaryContactAndClassificationBitsApplyAndPersist(string bits, int expected)
    {
        var (sim, missile) = Room(bits); Assert.Equal(expected, Flags(missile));
        var bytes = SimSavegame.Write(sim, true);
        missile.SpecialPickup = !missile.SpecialPickup; missile.CanPickupItems = !missile.CanPickupItems;
        missile.Dropped = !missile.Dropped; missile.Friendly = !missile.Friendly;
        missile.NoBlood = !missile.NoBlood; missile.NoBlockMonsters = !missile.NoBlockMonsters;
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, Flags(missile)); Assert.Equal(bytes, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpecialOnlyMissileBecomesEligibleForMissileContactCallback(bool special)
    {
        var (sim, moving) = Room(special ? "SPECIAL+NOGRAVITY" : "NOGRAVITY");
        var other = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        moving.SetXYZ(0, 0, 32); other.SetXYZ(50, 0, 32);
        moving.VelocityX = Fixed.FromInt(64); moving.VelocityY = moving.VelocityZ = default;
        other.VelocityX = other.VelocityY = other.VelocityZ = default;
        var contacts = 0;
        moving.SpecialMissileHitFunction = (_, hit) => { if (hit == other) contacts++; return 1; };
        moving.Tick(); Assert.Equal(special, contacts > 0); Assert.False(moving.Destroyed);
    }

    [Fact]
    public void UnpatchedContactFlagsAndBloodRetainStockDefaults()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 35\nSpeed = 12\n"));
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(0, Flags(missile)); Assert.False(missile.DefaultNoBlood);
    }

    [Fact]
    public void NoBloodClassDefaultAndFriendlyDefaultAreRecorded()
    {
        var (_, missile) = Room("FRIEND+NOBLOOD+NOGRAVITY");
        Assert.True(missile.DefaultNoBlood); Assert.True(missile.DefaultFriendly);
    }

    [Fact]
    public void ContactFlagChangeAffectsChecksum()
    {
        var (sim, missile) = Room("NOGRAVITY"); sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        missile.SpecialPickup = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData("NOBLOOD+6")]
    [InlineData("524294")]
    public void NativeBloodMnemonicAndNumericBitAgreeForMapActors(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        var actor = sim.Actors.Single(); Assert.True(actor.NoBlood); Assert.True(actor.DefaultNoBlood);
    }

    private static int Flags(Actor actor) => (actor.SpecialPickup ? 1 : 0) | (actor.CanPickupItems ? 2 : 0)
        | (actor.Dropped ? 4 : 0) | (actor.Friendly ? 8 : 0) | (actor.NoBlood ? 16 : 0) | (actor.NoBlockMonsters ? 32 : 0);

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = -200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
