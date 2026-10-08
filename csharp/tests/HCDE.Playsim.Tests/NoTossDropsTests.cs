using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoTossDropsTests
{
    private static AuthoritySimulation Room(CompatSurface compat = CompatSurface.NoTossDrops) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }],
    }, compat: compat);

    [Theory]
    [InlineData(0)]
    [InlineData(80)]
    [InlineData(10.5)]
    public void CompatibleDropUsesSourceZWithoutMomentumOrRandomDraws(double z)
    {
        var sim = Room(); var source = sim.Players.Single();
        source.Z = Fixed.FromDouble(z);
        source.VelocityX = Fixed.FromInt(10); source.VelocityZ = Fixed.FromInt(20);
        var before = sim.CombatRandomState;
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(source.Z, drop.Z); Assert.Equal(drop.Z, drop.PreviousZ);
        Assert.Equal(default, drop.VelocityX); Assert.Equal(default, drop.VelocityY); Assert.Equal(default, drop.VelocityZ);
        Assert.Equal(z == 0, drop.OnGround); Assert.False(drop.NoGravity);
        Assert.Equal(5, drop.PickupAmount);
        Assert.Equal(before, sim.CombatRandomState);
    }

    [Fact]
    public void AcsDropStillConsumesChanceDrawWhenTossIsDisabled()
    {
        var sim = Room(); var reference = Room(); reference.NextDropItemByte();
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "Clip", 0, 256));
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
        Assert.Equal(reference.NextDropItemByte(), sim.NextDropItemByte());
    }

    [Fact]
    public void VanillaDeathDropUsesCompatibilityPath()
    {
        var sim = Room(); var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        monster.Z = Fixed.FromInt(40);
        ActorDropItem.DropVanillaDeathItem(sim, monster);
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(monster.Z, drop.Z); Assert.Equal(default, drop.VelocityZ);
    }

    [Fact]
    public void CompatibilitySwitchChangesChecksumAndCombinesWithOtherFlags()
    {
        var plain = Room(CompatSurface.BoomScroll);
        var compatible = Room(CompatSurface.BoomScroll | CompatSurface.NoTossDrops);
        plain.Tick(); compatible.Tick();
        Assert.NotEqual(plain.Checksum, compatible.Checksum);
        Assert.True(compatible.SpawnDroppedPickup(compatible.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(default, compatible.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip).VelocityZ);
    }
}
