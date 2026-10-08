using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropStyleTests
{
    private static AuthoritySimulation Room(int seed = 0, CompatSurface compat = CompatSurface.None) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    }, rngSeed: seed, compat: compat);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(17)]
    [InlineData(-1)]
    public void StyleTwoUsesFixedOffsetAndMaskedHorizontalVelocity(int seed)
    {
        var sim = Room(seed); var reference = Room(seed); sim.DropStyle = 2;
        var x = (int)(reference.NextDropItemByte() & 7) - (int)(reference.NextDropItemByte() & 7);
        var y = (int)(reference.NextDropItemByte() & 7) - (int)(reference.NextDropItemByte() & 7);
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(40); source.Height = Fixed.FromInt(80);
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(Fixed.FromInt(64), drop.Z); Assert.Equal(drop.Z, drop.PreviousZ);
        Assert.Equal(Fixed.FromInt(x), drop.VelocityX); Assert.Equal(Fixed.FromInt(y), drop.VelocityY);
        Assert.Equal(default, drop.VelocityZ); Assert.False(drop.OnGround);
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
        source.X = Fixed.FromInt(200); sim.Tick(); Assert.Equal(64, drop.Z.ToDouble());
        Assert.True(drop.VelocityZ.Raw < 0);
        sim.Tick(); Assert.True(drop.Z.ToDouble() < 64);
    }

    [Fact]
    public void NoTossOverridesStyleTwoWithoutConsumingTossDraws()
    {
        var sim = Room(compat: CompatSurface.NoTossDrops); sim.DropStyle = 2;
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(40);
        var before = sim.CombatRandomState;
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(source.Z, drop.Z); Assert.Equal(default, drop.VelocityX);
        Assert.Equal(default, drop.VelocityY); Assert.Equal(default, drop.VelocityZ);
        Assert.Equal(before, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(-1)]
    public void OtherStylesUseNativeDefaultBranch(int style)
    {
        var first = Room(); var second = Room(); second.DropStyle = style;
        first.SpawnDroppedPickup(first.Players.Single(), PickupCatalog.Clip);
        second.SpawnDroppedPickup(second.Players.Single(), PickupCatalog.Clip);
        var a = first.Actors[^1]; var b = second.Actors[^1];
        Assert.Equal(a.Z, b.Z); Assert.Equal(a.VelocityX, b.VelocityX);
        Assert.Equal(a.VelocityY, b.VelocityY); Assert.Equal(a.VelocityZ, b.VelocityZ);
        Assert.Equal(first.CombatRandomState, second.CombatRandomState);
    }

    [Fact]
    public void DropStyleAffectsChecksumAndPoseRestorePreservesSetting()
    {
        var first = Room(); var second = Room(); second.DropStyle = 2;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
        SimSavegame.Apply(second, SimSavegame.Write(second));
        Assert.Equal(2, second.DropStyle);
    }
}
