using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CorpseGrindTests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void GenericCorpseGrindRequiresBloodAndNoDontGibFlag(bool noBlood, bool dontGib, bool crushed)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        actor.NoBlood = noBlood; actor.DontGib = dontGib;
        actor.VelocityX = actor.VelocityY = actor.VelocityZ = Fixed.FromInt(1);
        ActorPhysics.GrindActor(actor);
        Assert.Equal(crushed, actor.Height.Raw == 0);
        if (!crushed) return;
        Assert.False(actor.Solid); Assert.True(actor.DontGib);
        Assert.Equal(0, actor.Radius.Raw); Assert.Equal(default, actor.VelocityX);
        Assert.Equal(default, actor.VelocityY); Assert.Equal(default, actor.VelocityZ);
        Assert.Equal(actor.GenericCrushState, actor.States.Current);
        Assert.Equal("misc/fallingsplat", Assert.Single(sim.DrainSoundRequests()).Sound);
        ActorPhysics.GrindActor(actor); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void LoweringCeilingGrindsCorpseAndResultSurvivesFreshSave()
    {
        var sim = Room(); var actor = sim.Actors.Single();
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            CeilingDirection = -1, Speed = 64, TargetCeiling = 0, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.True(actor.DontGib); Assert.Equal(0, actor.Height.Raw);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(307, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = Assert.Single(restored.Actors);
        Assert.True(loaded.DontGib); Assert.Equal(0, loaded.Height.Raw); Assert.Equal(0, loaded.Radius.Raw);
        Assert.Equal(loaded.GenericCrushState, loaded.States.Current);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
    }

    [Fact]
    public void AcsDontGibFlagAndOlderSaveClearingWork()
    {
        var sim = Room(); var actor = sim.Actors.Single(); var older = SimSavegame.Write(sim, true);
        Assert.True(AcsActorFlags.TrySet(actor, "actor.dontgib", true));
        Assert.True(AcsActorFlags.TryGet(actor, "DONTGIB", out var value)); Assert.True(value);
        SimSavegame.Apply(sim, older); Assert.False(actor.DontGib);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 18 }],
        });
        var actor = Assert.Single(sim.Actors); actor.Health = 0; actor.Corpse = true;
        return sim;
    }
}
