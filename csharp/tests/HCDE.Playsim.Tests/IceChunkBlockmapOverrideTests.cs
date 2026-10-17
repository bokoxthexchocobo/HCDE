using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkBlockmapOverrideTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void ScriptRelinkingMakesSolidIceDebrisTraceableAndRestores(int mask)
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.NotEmpty(chunks);
        foreach (var item in chunks) { item.SetXYZ(200, 200, 28); item.VelocityX = item.VelocityY = item.VelocityZ = default; }
        var chunk = chunks[0]; chunk.SetXYZ(32, 0, sim.Players.Single().AttackOriginZ() - 2);
        ActorPropertyActions.ChangeFlag(chunk, "SOLID", true);
        Assert.True(chunk.NoBlockmap); Assert.False(chunk.IsBlockmapActor); Assert.Null(Trace(sim, mask));
        ActorPropertyActions.ChangeLinkFlags(chunk, blockmap: 0);
        Assert.False(chunk.NoBlockmap); Assert.True(chunk.IsBlockmapActor); Assert.Same(chunk, Trace(sim, mask));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == chunk.Id);
        Assert.True(loaded.IsBlockmapActor); Assert.Same(loaded, Trace(fresh, mask));
        ActorPropertyActions.ChangeLinkFlags(chunk, blockmap: 1);
        ActorPropertyActions.ChangeLinkFlags(loaded, blockmap: 1);
        Assert.Null(Trace(sim, mask)); Assert.Null(Trace(fresh, mask));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static Actor? Trace(AuthoritySimulation sim, int mask)
    {
        var source = sim.Players.Single();
        return CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 128, mask);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
