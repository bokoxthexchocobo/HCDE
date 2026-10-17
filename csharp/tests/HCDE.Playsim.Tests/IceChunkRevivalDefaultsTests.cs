using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkRevivalDefaultsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ClassFlagsRestoreAfterRuntimeOverridesAndFreshSave(bool blockmap, bool moveWithSector)
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunk = sim.Actors.OfType<IceChunkActor>().First();
        Assert.True(chunk.DefaultMoveWithSector);
        ActorPropertyActions.ChangeLinkFlags(chunk, blockmap: blockmap ? 0 : 1);
        ActorPropertyActions.ChangeFlag(chunk, "MOVEWITHSECTOR", moveWithSector);
        ActorPropertyActions.ChangeFlag(chunk, "SOLID", true);
        chunk.CannotPush = false; chunk.NoTeleport = false; chunk.AllowDropOff = false;
        chunk.FloorClipEnabled = false;
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = Assert.IsType<IceChunkActor>(fresh.Actors.Single(a => a.Id == chunk.Id));
        Assert.Equal(!blockmap, loaded.NoBlockmap); Assert.Equal(moveWithSector, loaded.MoveWithSector);
        foreach (var actor in new[] { chunk, loaded })
        {
            ActorRaise.ReviveSupported(actor);
            Assert.False(actor.Solid); Assert.False(actor.Shootable); Assert.True(actor.NoBlockmap);
            Assert.True(actor.MoveWithSector); Assert.True(actor.CannotPush); Assert.True(actor.NoTeleport);
            Assert.True(actor.AllowDropOff); Assert.True(actor.FloorClipEnabled);
            Assert.Equal(3, actor.Radius.ToDouble()); Assert.Equal(4, actor.Height.ToDouble());
        }
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
