using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TouchyImpactTests
{
    [Theory]
    [InlineData(true, true, -6, true)]
    [InlineData(true, true, -5, false)]
    [InlineData(true, false, -6, false)]
    [InlineData(false, true, -6, false)]
    public void NonSentientImpactRequiresTouchyArmedAndStrictVelocityThreshold(bool touchy, bool armed, int velocity, bool dies)
    {
        var sim = Room();
        var actor = new Actor { Health = 100, Shootable = true, Touchy = touchy, Armed = armed,
            VelocityZ = Fixed.FromInt(velocity), Invulnerable = true };
        Assert.False(actor.IsSentient());
        Assert.False(TerrainDamage.Land(sim, actor));
        Assert.Equal(dies, actor.IsDead);
        Assert.Equal(armed && !dies, actor.Armed);
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void SentientActorDoesNotNeedArmedFlag()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0);
        actor.SeeState = 0;
        Assert.True(actor.IsSentient());
        actor.Touchy = true; actor.VelocityZ = Fixed.FromInt(-6);
        Assert.False(TerrainDamage.Land(sim, actor));
        Assert.True(actor.IsDead); Assert.False(actor.Armed);
    }

    [Theory]
    [InlineData("actor.touchy")]
    [InlineData("ARMED")]
    public void AcsFlagsCanBeReadAndChanged(string name)
    {
        var actor = Room().Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out value)); Assert.False(value);
    }

    [Fact]
    public void SaveRestoresFlagsAndActualImpactContinuation()
    {
        var sim = Room(); var actor = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        actor.Touchy = actor.Armed = true;
        actor.Z = Fixed.FromInt(8); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-16);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(304, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().Touchy); Assert.True(restored.Players.Single().Armed);
        sim.Tick(); restored.Tick();
        Assert.True(actor.IsDead); Assert.False(actor.Armed);
        Assert.Equal(sim.Checksum, restored.Checksum);
        SimSavegame.Apply(restored, older);
        Assert.False(restored.Players.Single().Touchy); Assert.False(restored.Players.Single().Armed);
    }

    [Fact]
    public void UnknownSavedFlagBitsAreRejected()
    {
        var sim = Room(); sim.Players.Single().Touchy = true;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
