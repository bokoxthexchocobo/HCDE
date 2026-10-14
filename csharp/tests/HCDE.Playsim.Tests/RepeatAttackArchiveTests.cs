using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RepeatAttackArchiveTests
{
    [Fact]
    public void ActiveSnapshotRestoresAttackPositionAndLegacySnapshotClearsIt()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 65 }] });
        var legacy = sim.CaptureState(); var monster = sim.Actors.Single(a => a.DoomEdNum == 65);
        monster.ReactionTime = 0;
        for (var i = 0; i < 7; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var active = sim.CaptureState(); var attack = active.Actors.Single(a => a.Id == monster.Id).RepeatAttack;
        Assert.NotNull(attack);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        sim.RestoreState(active);
        Assert.Equal(attack, sim.Actors.Single(a => a.DoomEdNum == 65).Brain!.CaptureRepeatAttack());
        var bytes = SimSavegame.Write(sim);
        Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 138) >= 16);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 138);
        var index = active.Actors.FindIndex(a => a.Id == monster.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + index * 24 + 4), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-repeatattack-state", error);
        sim.RestoreState(legacy);
        Assert.Null(sim.Actors.Single(a => a.DoomEdNum == 65).Brain!.CaptureRepeatAttack());
    }
}
