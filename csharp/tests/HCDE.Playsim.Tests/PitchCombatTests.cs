using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PitchCombatTests
{
    [Fact]
    public void VersionThreePoseStillReadsCooldownAndRandomState()
    {
        var bytes = new byte[80]; // v3: header 16, actor 52, sector count 4, RNG footer 8.
        "HCSV"u8.CopyTo(bytes);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 3);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), 1);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(64), 7);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(72), 123);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(76), 1);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Equal(7, Assert.Single(state.Actors).WeaponCooldown);
        Assert.Equal(123u, state.CombatRandomState);
        Assert.Equal(0, state.Actors[0].Pitch);
        Assert.False(state.Actors[0].UseHeld);
    }
    private static AuthoritySimulation Range(LevelLine? wall = null) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 100, Z = 100 }],
        Lines = wall == null ? [] : [wall],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }, new LevelSector { Index = 1, CeilingHeight = 60 }],
    });

    [Fact]
    public void PitchHitsElevatedCylinderWhereHorizontalRayMisses()
    {
        var sim = Range(); var player = sim.Players.Single(); var target = sim.Actors.Last();
        Assert.Null(CombatTrace.FindTarget(sim, player, 2048));
        player.PitchDegrees = -45;
        Assert.Same(target, CombatTrace.FindTarget(sim, player, 2048));
        player.PitchDegrees = 45;
        Assert.Null(CombatTrace.FindTarget(sim, player, 2048));
    }

    [Fact]
    public void PitchChecksOpeningAtRayIntersectionHeight()
    {
        var sim = Range(new LevelLine { X1 = 50, X2 = 50, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1 });
        sim.Players.Single().PitchDegrees = -45;
        Assert.Null(CombatTrace.FindTarget(sim, sim.Players.Single(), 2048));
    }

    [Fact]
    public void SteepProjectileAimPreservesSpeedAndPoseArchiveRestoresPitchAndUseLatch()
    {
        var sim = Range(); var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { PitchDelta = -16384, Use = true }); sim.Tick();
        Assert.Equal(-89, player.PitchDegrees);
        var projectile = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        Assert.True(projectile.VelocityZ.ToDouble() > 24);
        var speed = Math.Sqrt(Math.Pow(projectile.VelocityX.ToDouble(), 2) + Math.Pow(projectile.VelocityZ.ToDouble(), 2));
        Assert.InRange(speed, 24.99, 25.01);
        var bytes = SimSavegame.Write(sim);
        player.PitchDegrees = 0;
        sim.QueueCommand(0, default); sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(-89, player.PitchDegrees);
        Assert.True(player.UseHeld);
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.UsePressed);
    }
}
