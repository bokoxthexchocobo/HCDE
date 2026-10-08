using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSizeArchiveTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Bfg)]
    [InlineData(ProjectileKind.ImpBall)]
    [InlineData(ProjectileKind.BaronBall)]
    [InlineData(ProjectileKind.CacodemonBall)]
    [InlineData(ProjectileKind.CyberRocket)]
    [InlineData(ProjectileKind.ArachnotronPlasma)]
    [InlineData(ProjectileKind.MancubusBall)]
    [InlineData(ProjectileKind.RevenantTracer)]
    public void DirectDimensionsRoundTripAndAbsentDimensionsResetToSpawn(ProjectileKind kind)
    {
        var sim = Room(); var owner = sim.Players.Single();
        var changed = sim.SpawnProjectile(owner, kind);
        var unchanged = sim.SpawnProjectile(owner, kind);
        var radius = unchanged.Radius; var height = unchanged.Height;
        var legacy = SimSavegame.Write(sim);
        changed.Radius = Fixed.FromDouble(1.25); changed.Height = Fixed.FromDouble(3.5);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.NotNull(state.Actors.Single(a => a.Id == changed.Id).Size);
        Assert.Null(state.Actors.Single(a => a.Id == unchanged.Id).Size);

        var loaded = Room();
        var restored = loaded.SpawnProjectile(loaded.Players.Single(), kind);
        var other = loaded.SpawnProjectile(loaded.Players.Single(), kind);
        other.Radius = other.Height = Fixed.FromInt(100);
        SimSavegame.Apply(loaded, bytes);
        Assert.Equal(changed.Radius, restored.Radius); Assert.Equal(changed.Height, restored.Height);
        Assert.Equal(radius, other.Radius); Assert.Equal(height, other.Height);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, legacy);
        Assert.Equal(radius, restored.Radius); Assert.Equal(height, restored.Height);
        Assert.Equal(legacy, SimSavegame.Write(loaded));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(98304, 196608)]
    [InlineData(-65536, -131072)]
    public void OmittedSizeRestoresPatchedSpawnDimensions(int radiusRaw, int heightRaw)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nWidth = {radiusRaw}\nHeight = {heightRaw}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }] }, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(radiusRaw, missile.Radius.Raw); Assert.Equal(heightRaw, missile.Height.Raw);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Actors.Single(a => a.Id == missile.Id).Size);
        missile.Radius = missile.Height = Fixed.FromInt(99);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(radiusRaw, missile.Radius.Raw); Assert.Equal(heightRaw, missile.Height.Raw);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 1 }],
        Sectors = [new LevelSector { CeilingHeight = 512 }],
    });
}
