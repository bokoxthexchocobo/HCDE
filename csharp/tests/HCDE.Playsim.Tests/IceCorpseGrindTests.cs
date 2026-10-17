using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceCorpseGrindTests
{
    [Fact]
    public void FreezeActionSetsNativeNoBloodFlag()
    {
        var (sim, actor) = Frozen();
        Assert.True(actor.NoBlood); Assert.True(actor.IceCorpse);
        var saved = SimSavegame.Write(sim, true);
        actor.NoBlood = false; SimSavegame.Apply(sim, saved); Assert.True(actor.NoBlood);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 100)]
    public void GrinderShortensTimerAndStopsMotionUnlessDontGib(bool dontGib, int expectedTics)
    {
        var (_, actor) = Frozen(); actor.DontGib = dontGib;
        actor.States.ForceRemainingTics(100);
        actor.VelocityX = actor.VelocityY = actor.VelocityZ = Fixed.FromInt(2);
        Assert.False(ActorPhysics.GrindActor(actor));
        Assert.Equal(expectedTics, actor.States.RemainingTics);
        Assert.Equal(dontGib ? Fixed.FromInt(2) : default, actor.VelocityX);
        Assert.Equal(dontGib ? Fixed.FromInt(2) : default, actor.VelocityY);
        Assert.Equal(dontGib ? Fixed.FromInt(2) : default, actor.VelocityZ);
        Assert.True(actor.Solid); Assert.True(actor.IceCorpse);
    }

    [Fact]
    public void GroundIceCorpseCanReachExistingChunkActionWithoutMovingDelay()
    {
        var (sim, actor) = Frozen();
        actor.VelocityX = Fixed.FromInt(4);
        ActorPhysics.GrindActor(actor);
        sim.SpawnIceChunks(actor);
        Assert.True(actor.Destroyed);
        Assert.NotEmpty(sim.Actors.OfType<IceChunkActor>());
    }

    [Fact]
    public void CeilingCrusherSchedulesIceBreakAndSaveRestoresTimerAndMotion()
    {
        var (sim, actor) = Frozen();
        actor.Height = Fixed.FromInt(56); actor.VelocityX = Fixed.FromInt(4);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            CeilingDirection = -1, TargetCeiling = 32, Speed = 32, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(1, actor.States.RemainingTics); Assert.Equal(0, actor.VelocityX.Raw);
        var saved = SimSavegame.Write(sim, true);
        actor.States.ForceRemainingTics(100); actor.VelocityX = Fixed.FromInt(4);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, actor.States.RemainingTics); Assert.Equal(0, actor.VelocityX.Raw);
        Assert.True(actor.NoBlood); Assert.True(actor.IceCorpse);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Frozen()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1, X = -100 }] });
        var actor = sim.AddBot(80, 0); actor.Brain = null;
        ActorDamage.Apply(actor, 1000); actor.Corpse = true;
        ActorFreezeActions.FreezeDeath(actor);
        return (sim, actor);
    }
}
