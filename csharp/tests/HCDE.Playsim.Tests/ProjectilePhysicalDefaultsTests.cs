using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePhysicalDefaultsTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("SOLID", 1)]
    [InlineData("SHOOTABLE", 2)]
    [InlineData("NOBLOCKMAP", 4)]
    [InlineData("NOSECTOR", 8)]
    [InlineData("NOGRAVITY", 16)]
    [InlineData("DROPOFF", 32)]
    [InlineData("FLOAT", 64)]
    [InlineData("SOLID+SHOOTABLE+NOBLOCKMAP+NOSECTOR+NOGRAVITY+DROPOFF+FLOAT", 127)]
    public void PrimaryPhysicalBitsApplyAndSurviveSaveRestore(string bits, int expected)
    {
        var (sim, missile) = Room(bits); Assert.Equal(expected, Flags(missile));
        var bytes = SimSavegame.Write(sim, true);
        missile.Solid = !missile.Solid; missile.Shootable = !missile.Shootable;
        missile.NoBlockmap = !missile.NoBlockmap; missile.NoSector = !missile.NoSector;
        missile.NoGravity = !missile.NoGravity; missile.AllowDropOff = !missile.AllowDropOff; missile.Floating = !missile.Floating;
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, Flags(missile));
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData("0", -1)]
    [InlineData("NOGRAVITY", 0)]
    [InlineData("LOGRAV+0", -0.25)]
    [InlineData("LOGRAV+NOGRAVITY", 0)]
    public void PatchedGravityFlagsControlActualProjectileAcceleration(string bits, double expected)
    {
        var (_, missile) = Room(bits); missile.SetXYZ(100, 0, 64);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(expected, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void ShootablePatchedMissileCanTakeDamage()
    {
        var (_, missile) = Room("SHOOTABLE+NOGRAVITY"); var before = missile.Health;
        ActorDamage.Apply(missile, 1); Assert.Equal(before - 1, missile.Health);
    }

    [Fact]
    public void UnpatchedPrimaryBitsKeepStockDefaultsAndRenderStyle()
    {
        var (sim, _) = Room("0");
        var stock = AuthoritySimulation.Start(sim.Level, dehacked: DehackedPatch.Apply("Thing 35\nSpeed = 12\n"));
        var missile = stock.SpawnProjectile(stock.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(16 | 32, Flags(missile)); Assert.Equal(0.75, missile.Alpha);
    }

    [Fact]
    public void PrimaryBitApplicationPreservesProjectileVisualDefaults()
    {
        var (_, missile) = Room("SOLID+SHOOTABLE+NOGRAVITY");
        Assert.Equal(0.75, missile.Alpha); Assert.Equal(ActorRenderStyle.FromNative(7), missile.RenderStyle);
    }

    private static int Flags(Actor actor) => (actor.Solid ? 1 : 0) | (actor.Shootable ? 2 : 0)
        | (actor.NoBlockmap ? 4 : 0) | (actor.NoSector ? 8 : 0) | (actor.NoGravity ? 16 : 0)
        | (actor.AllowDropOff ? 32 : 0) | (actor.Floating ? 64 : 0);

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
