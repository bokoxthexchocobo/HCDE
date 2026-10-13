using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CanPassTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3005)]
    [InlineData(3006)]
    public void ArchiveWithoutCanPassOverridesUsesRebuiltClassDefaultsForResurrection(int type)
    {
        var source = Room(); var corpse = source.AddBot(0, 0, type);
        corpse.Health = 0; corpse.Corpse = true;
        var bytes = SimSavegame.Write(source);
        Assert.True(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)) < 194);
        var restored = Room(); var loaded = restored.AddBot(0, 0, type);
        loaded.CanPass = false;
        SimSavegame.Apply(restored, bytes);
        Assert.True(loaded.CanPass);
        loaded.CanPass = false; ActorRaise.ReviveSupported(loaded);
        Assert.True(loaded.CanPass);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CeilingRecoveryUsesExplicitFlag(bool playerActor, bool canPass)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.X = Fixed.FromInt(0); actor.Z = Fixed.FromInt(8);
        var support = sim.AddBot(0, 0); support.Brain = null; support.Height = Fixed.FromInt(16);
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", canPass);
        Assert.True(AcsActorFlags.TryGet(actor, "Actor.CanPass", out var value)); Assert.Equal(canPass, value);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            CeilingDirection = 1, TargetCeiling = 256, Speed = 16 });
        LineSpecials.TickMotions(sim); Assert.Equal(canPass ? 16 : 8, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FlagOverrideSaveAndLegacyRestore(bool playerActor)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        Assert.True(actor.CanPass);
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", false);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.False(actor.CanPass); Assert.NotEqual(hash, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline); Assert.True(actor.CanPass); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    [Fact]
    public void ArchiveRejectsInvalidFlagAndRecursiveVersion()
    {
        var sim = Room(); sim.Players.Single().CanPass = false;
        var bytes = SimSavegame.Write(sim);
        var invalid = (byte[])bytes.Clone();
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(invalid.AsSpan(invalid.Length - 8), 2);
        Assert.False(SimSavegame.TryRead(invalid, out _, out var error)); Assert.Equal("save-can-pass-value", error);
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 194);
        Assert.False(SimSavegame.TryRead(bytes, out _, out error)); Assert.Equal("save-can-pass-header", error);
    }

    [Fact]
    public void OrdinaryActorDefaultsToDisabled()
    {
        Assert.False(new Actor().CanPass);
        var sim = Room(); Assert.True(sim.Players.Single().CanPass); Assert.True(sim.AddBot(0, 0).CanPass);
    }

    [Fact]
    public void ResurrectionRestoresMonsterDefault()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = sim.Actors.Single(); Assert.True(actor.CanPass);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse); actor.CanPass = false;
        Assert.True(ThingRaise.Execute(sim, 17, actor, 0, 2)); Assert.True(actor.CanPass);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 200 }] });
}
