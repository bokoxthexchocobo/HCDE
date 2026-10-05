using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGroupArchiveTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GroupsComposeWithFlagsAndPowerArchives(int powers)
    {
        var sim = Room(); var target = sim.Actors[1]; var source = sim.Actors[2];
        target.InfightingGroup = source.InfightingGroup = 7;
        target.ProjectileGroup = source.ProjectileGroup = 8;
        target.SplashGroup = source.SplashGroup = 9;
        target.StrifeDamage = true;
        if (powers == 1) sim.Players.Single().GivePowerDamage();
        if (powers == 2) sim.Players.Single().GivePowerBuddha();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(55, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        target = restored.Actors[1]; source = restored.Actors[2];
        Assert.Equal(7, target.InfightingGroup); Assert.Equal(8, target.ProjectileGroup); Assert.Equal(9, target.SplashGroup);
        Assert.True(target.StrifeDamage); Assert.True(target.ProjectileImmune(source)); Assert.True(target.SplashImmune(source));
        Assert.Equal(0, ActorDamage.Apply(target, 1, source).HealthLost);
        target.ProjectileGroup = -1;
        Assert.Equal(1, ActorDamage.Apply(target, 1, source).HealthLost); Assert.Null(target.Brain!.TargetId);
        Assert.Equal(sim.Players.Single().PowerDamageTics, restored.Players.Single().PowerDamageTics);
        Assert.Equal(sim.Players.Single().PowerBuddhaTics, restored.Players.Single().PowerBuddhaTics);
    }

    [Fact]
    public void PoseAndArchivePreserveSignedProjectileSentinel()
    {
        var sim = Room(); sim.Actors[1].ProjectileGroup = -1;
        var pose = sim.CaptureState(); sim.Actors[1].ProjectileGroup = 9;
        sim.RestoreState(pose); Assert.Equal(-1, sim.Actors[1].ProjectileGroup);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.Equal(-1, restored.Actors[1].ProjectileGroup);
        Assert.False(restored.Actors[1].ProjectileImmune(restored.Actors[2]));
    }

    [Fact]
    public void LegacyArchiveClearsGroupsAndKeepsDefaultBytes()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        sim.Actors[1].InfightingGroup = 7; sim.Actors[1].ProjectileGroup = -1; sim.Actors[1].SplashGroup = 9;
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Equal(0, sim.Actors[1].InfightingGroup); Assert.Equal(0, sim.Actors[1].ProjectileGroup); Assert.Equal(0, sim.Actors[1].SplashGroup);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MalformedTrailerIsRejected(int corruption)
    {
        var sim = Room(); sim.Actors[1].SplashGroup = 9;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 55);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(corruption == 0 ? "save-actor-group-size" : "save-actor-group-header", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 100 }],
    });
}
