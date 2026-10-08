using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeSlideTests
{
    [Theory]
    [InlineData(-20)]
    [InlineData(20)]
    public void FrozenActorSlidesAlongWallAfterFreshRestore(int tangent)
    {
        var source = Room(); var actor = source.Actors.Single();
        ActorFreezeActions.FreezeDeath(actor); actor.VelocityX = Fixed.FromInt(1000); actor.VelocityY = Fixed.FromInt(tangent);
        var saved = SimSavegame.Write(source); var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Actors.Single().CanSlide); Assert.Equal(saved, SimSavegame.Write(restored));
        source.Tick(); restored.Tick(); var loaded = restored.Actors.Single();
        Assert.Equal(actor.X, loaded.X); Assert.Equal(actor.Y, loaded.Y);
        Assert.InRange(loaded.Y.ToDouble(), tangent - 0.01, tangent + 0.01);
        Assert.Equal(0, loaded.VelocityX.Raw); Assert.Equal(source.Checksum, restored.Checksum);
    }

    [Fact]
    public void RuntimeSlideOverrideRoundTripsAndOlderSaveResetsDefault()
    {
        var sim = Room(); var actor = sim.Actors.Single(); var older = SimSavegame.Write(sim);
        actor.CanSlide = true; var saved = SimSavegame.Write(sim); actor.CanSlide = false;
        SimSavegame.Apply(sim, saved); Assert.True(actor.CanSlide);
        SimSavegame.Apply(sim, older); Assert.False(actor.CanSlide);
    }

    [Fact]
    public void ExplicitFalseOverridesPlayerDefaultAndOlderSaveRestoresTrue()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single(); var older = SimSavegame.Write(sim);
        Assert.True(actor.CanSlide); actor.CanSlide = false; var saved = SimSavegame.Write(sim);
        actor.CanSlide = true; SimSavegame.Apply(sim, saved); Assert.False(actor.CanSlide);
        SimSavegame.Apply(sim, older); Assert.True(actor.CanSlide);
    }

    [Theory]
    [InlineData(0, 118)]
    [InlineData(8, 2)]
    public void MalformedSlideArchiveFails(int offset, int value)
    {
        var sim = Room(); sim.Actors.Single().CanSlide = true; var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(offset == 0 ? "save-slide-header" : "save-slide-value", error);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 }],
        });
        sim.Actors.Single().Brain = null;
        return sim;
    }
}
