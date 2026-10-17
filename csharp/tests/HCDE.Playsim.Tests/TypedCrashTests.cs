using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TypedCrashTests
{
    [Theory]
    [InlineData(-100, 3, 4, 3)]
    [InlineData(-101, 3, 4, 4)]
    [InlineData(-101, 3, -1, 3)]
    [InlineData(-101, 3, 99, 3)]
    [InlineData(-101, -1, -1, 2)]
    [InlineData(-100, -1, 4, 1)]
    [InlineData(-100, 99, 4, 1)]
    public void LookupOrderMatchesNativeExactTypedThenUntyped(int health, int normal, int extreme, int expected)
    {
        var actor = Configured(); actor.Health = health; actor.DamageType = "fIrE";
        actor.SetTypedCrash("Fire", normal); actor.SetTypedCrash("FIRE", extreme, extreme: true);
        actor.Crash();
        Assert.Equal(expected, actor.States.Current); Assert.True(actor.Crashed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("Poison")]
    public void MissingDamageTypeUsesUntypedState(string? type)
    {
        var actor = Configured(); actor.DamageType = type; actor.SetTypedCrash("Fire", 3);
        actor.Crash(); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void TypedNormalBeatsUntypedExtremeAndExposesAcsLabels()
    {
        var actor = Configured(); actor.Health = -101; actor.DamageType = "Fire";
        actor.SetTypedCrash("Fire", 3); actor.SetTypedCrash("Fire", 4, extreme: true);
        Assert.True(AcsActorStates.TryFindNamedState(actor, "crash.fire", true, out var normal));
        Assert.True(AcsActorStates.TryFindNamedState(actor, "CRASH.EXTREME.FIRE", true, out var extreme));
        Assert.Equal(3, normal); Assert.Equal(4, extreme);
        Assert.False(AcsActorStates.TryFindNamedState(actor, "Crash.Poison", true, out _));
        actor.SetTypedCrash("Fire", -1, extreme: true); actor.Crash(); Assert.Equal(3, actor.States.Current);
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("Extreme")]
    public void ReservedTypeNamesAreRejected(string type)
    {
        Assert.Throws<ArgumentException>(() => Configured().SetTypedCrash(type, 3));
    }

    [Fact]
    public void SaveRestoresPersistentDamageTypeAndClassBindingsDriveLanding()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.DamageType = "Fire"; actor.Killed = true;
        var saved = SimSavegame.Write(sim, true);
        var restored = Room(); SimSavegame.Apply(restored, saved); var loaded = Assert.Single(restored.Actors);
        Assert.Equal("Fire", loaded.DamageType); Assert.Equal(saved, SimSavegame.Write(restored, true));
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(restored, loaded);
        Assert.Equal(3, loaded.States.Current); Assert.True(loaded.Crashed);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    [Fact]
    public void ClassBindingChecksumIsStableAcrossInsertionOrderAndNameCasing()
    {
        var left = Room(); var right = Room();
        var a = Assert.Single(left.Actors); var b = Assert.Single(right.Actors);
        a.SetTypedCrash("Poison", 4); a.SetTypedCrash("Fire", 3);
        b.SetTypedCrash("FIRE", 3); b.SetTypedCrash("POISON", 4);
        left.RestoreState(left.CaptureState()); right.RestoreState(right.CaptureState());
        Assert.Equal(left.Checksum, right.Checksum);
        b.SetTypedCrash("Fire", 4); right.RestoreState(right.CaptureState());
        Assert.NotEqual(left.Checksum, right.Checksum);
    }

    private static Actor Configured()
    {
        var actor = new Actor { Corpse = true, CrashState = 1, ExtremeCrashState = 2 };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3), new(-1, 4)], 0);
        return actor;
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 18 }] });
        var actor = Assert.Single(sim.Actors);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3), new(-1, 4)], 0);
        actor.SetTypedCrash("Fire", 3);
        return sim;
    }
}
