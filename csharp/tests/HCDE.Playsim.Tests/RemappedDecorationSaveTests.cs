using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RemappedDecorationSaveTests
{
    [Theory]
    [InlineData(129, 73)]
    [InlineData(130, 74)]
    [InlineData(131, 75)]
    [InlineData(132, 76)]
    [InlineData(133, 77)]
    [InlineData(134, 78)]
    public void RemappedIdentityReconstructsDestroyedDecorationAndFreshSave(int index, int original)
    {
        var sim = Room(index); var actor = Assert.Single(sim.Actors); actor.ProjectilePassHeight = Fixed.FromInt(23);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(356, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        var identity = state.Actors.Single().MapDecorationIdentity!.Value;
        Assert.Equal(9073, identity.DoomEdNum); Assert.Equal(original, identity.DefinitionDoomEdNum);
        var expectedClass = original switch {
            73 => "HangNoGuts", 74 => "HangBNoBrain", 75 => "HangTLookingDown",
            76 => "HangTSkull", 77 => "HangTLookingUp", _ => "HangTNoBrain",
        };
        Assert.Equal(expectedClass, AcsCallFunctions.ClassNameOf(actor));
        actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        var loaded = Assert.Single(sim.Actors); Assert.NotSame(actor, loaded); Assert.Equal(original, loaded.ClassDoomEdNum);
        Assert.Equal(9073, loaded.DoomEdNum); Assert.Equal(23, loaded.ProjectilePassHeight.ToDouble());
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(index); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(expectedClass, AcsCallFunctions.ClassNameOf(fresh.Actors.Single()));
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData(-1, "save-remappeddecoration-value")]
    [InlineData(65536, "save-remappeddecoration-value")]
    [InlineData(73, "save-remappeddecoration-state")]
    public void InvalidRemapValueRejectsBeforeApplying(int value, string expected)
    {
        var saved = SimSavegame.Write(Room(129), true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void InvalidTrailerHeaderRejects(int corruption)
    {
        var saved = SimSavegame.Write(Room(129), true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + corruption * 4), corruption == 0 ? 356 : int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-remappeddecoration-header", error);
    }

    [Fact]
    public void MatchingEditorNumberWithDifferentOriginalClassRejectsWithoutMutation()
    {
        var saved = SimSavegame.Write(Room(129), true); var other = Room(130); var before = SimSavegame.Write(other, true);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(other, saved));
        Assert.Equal(before, SimSavegame.Write(other, true));
    }

    [Fact]
    public void OrdinaryDecorationKeepsLegacyFormatAndIdentity()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 73 }] });
        var saved = SimSavegame.Write(sim, true); Assert.NotEqual(356, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        Assert.Equal(0, state.Actors.Single().MapDecorationIdentity!.Value.DefinitionDoomEdNum);
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(int index)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 9073\nPhysical height = 2097152\nProjectile pass height = 1048576\n");
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 9073, Id = 7 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
