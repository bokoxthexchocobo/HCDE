using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomStaticPickupStateTests
{
    public static IEnumerable<object[]> Types => new ushort[] {
        2011, 2012, 2007, 2048, 2010, 2046, 2047, 17, 2008, 2049, 8,
        2006, 2002, 2005, 2003, 2004, 2001, 82, 5010,
    }.Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(Types))]
    public void NativeStaticSpawnHoldsAndInheritedCrushRemovalReconstruct(ushort editor)
    {
        var sim = Room(editor); var actor = Item(sim);
        Assert.True(actor.HasOnlySpawnLabel); Assert.Equal(1, actor.GenericCrushState); Assert.Equal(2, actor.NullState);
        for (var i = 0; i < 20; i++) { Assert.Equal(actor.SpawnState, actor.States.Current); Assert.Equal(-1, actor.States.RemainingTics); Assert.False(actor.FullBright); sim.Tick(); }
        actor.States.Enter(actor, actor.GenericCrushState); Assert.Equal(-1, actor.States.RemainingTics); Assert.False(actor.FullBright);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Item(sim)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Item(fresh).States.Enter(Item(fresh), Item(fresh).NullState); fresh.Tick(); Assert.DoesNotContain(fresh.Actors, a => a.MapPickupThingIndex.HasValue);
    }

    [Theory]
    [InlineData(2011)]
    [InlineData(2048)]
    [InlineData(8)]
    [InlineData(2006)]
    [InlineData(5010)]
    public void DroppedStaticPickupReplaysHeldCrushStateAfterFreshSave(ushort editor)
    {
        var sim = Room(2007); var source = sim.Players.Single(); source.X = Fixed.FromInt(-200);
        Assert.True(sim.SpawnDroppedPickup(source, editor)); var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        drop.SetXYZ(400, 0, 0); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        Assert.Equal(-1, drop.States.RemainingTics); Assert.False(drop.FullBright);
        drop.States.Enter(drop, drop.GenericCrushState); var saved = SimSavegame.Write(sim, true);
        var fresh = Room(2007); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 20; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void ShootablePatchCanUseRemovalStateWithoutGenericMonsterCorpseLoop()
    {
        var patch = DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SHOOTABLE\nHit points = 10\n"); Assert.Empty(patch.Errors);
        var sim = Room(2007, patch); var actor = Item(sim); actor.NoPain = true;
        ActorDamage.Apply(actor, 20);
        Assert.True(actor.IsDead, $"health={actor.Health} shootable={actor.Shootable}");
        Assert.True(actor.Destroyed);
        sim.Tick(); Assert.True(actor.Destroyed);
    }

    [Fact]
    public void PropertyPatchedWeaponKeepsNativeStaticTable()
    {
        var patch = DehackedPatch.Apply("Thing 78\nMass = 200\n"); var sim = Room(2001, patch);
        Assert.True(Item(sim).HasOnlySpawnLabel); Assert.Equal(-1, Item(sim).States.RemainingTics);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(2001, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(2007)]
    [InlineData(2011)]
    [InlineData(2001)]
    public void OrdinaryPickupUsesFlagsToRemainNonBlockingAndImmune(ushort editor)
    {
        var actor = Item(Room(editor)); Assert.False(actor.Solid); Assert.False(actor.Shootable);
        Assert.False(actor.BlocksActors); Assert.False(actor.CanTakeDamage);
        var health = actor.Health; ActorDamage.Apply(actor, 20); Assert.Equal(health, actor.Health);
    }

    [Fact]
    public void ExplicitSolidPickupUsesItsFlagsForActorBlocking()
    {
        var patch = DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SOLID\n");
        var actor = Item(Room(2007, patch)); Assert.True(actor.BlocksActors); Assert.False(actor.CanTakeDamage);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch = null) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
