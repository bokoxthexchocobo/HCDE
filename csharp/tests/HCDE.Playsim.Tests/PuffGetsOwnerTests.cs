using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffGetsOwnerTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BfgActionsAssignNativeOwnerOnlyWhenFlagged(bool radius, bool enabled)
    {
        var sim = Room(); var owner = sim.Players.Single();
        var target = sim.Actors.Single(a => a.ThingId == 7);
        var flash = sim.Actors.Single(a => a.ThingId == 8);
        ActorPropertyActions.ChangeFlag(flash, "Actor.PUFFGETSOWNER", enabled);
        Assert.True(AcsActorFlags.TryGet(flash, "puffgetsowner", out var flag)); Assert.Equal(enabled, flag);
        flash.SetTargetPointer(radius ? owner : target);
        if (radius)
        {
            owner.SetTargetPointer(target);
            ActorRadiusSelfActions.Apply(owner, 25, 500, spawnFlash: _ => flash);
        }
        else BfgSprayActions.Apply(sim, new Actor { Angle = BamAngle.FromDegrees(45) }, owner,
            numRays: 1, fixedDamage: 25, spawnSpray: _ => flash);
        Assert.Equal((radius == enabled ? target : owner).Id, flash.TargetId);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var restored = fresh.Actors.Single(a => a.Id == flash.Id);
        Assert.Equal(enabled, restored.PuffGetsOwner); Assert.Equal(flash.TargetId, restored.TargetId);
    }

    [Fact]
    public void LegacyRestoreResetsFlagAndFlagAffectsChecksum()
    {
        var sim = Room(); var control = Room(); var saved = SimSavegame.Write(sim, true);
        var flash = sim.Actors.Single(a => a.ThingId == 8); flash.PuffGetsOwner = true;
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.False(sim.Actors.Single(a => a.Id == flash.Id).PuffGetsOwner);
    }

    [Fact]
    public void InvalidSavedFlagRejectsWriteAndRestoreWithoutMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.Actors[0].PuffGetsOwnerFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void MalformedArchiveFlagIsRejected()
    {
        var sim = Room(); sim.Players.Single().PuffGetsOwner = true; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(359, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-puffgetsowner-value", error);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        var target = sim.AddBot(200, 0, 3001, thingId: 7); target.Brain = null; target.Health = 500;
        var flash = sim.AddBot(500, 500, 3001, thingId: 8); flash.Brain = null;
        return sim;
    }
}
