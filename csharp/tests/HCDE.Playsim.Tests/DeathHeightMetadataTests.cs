namespace HCDE.Playsim.Tests;

public class DeathHeightMetadataTests
{
    [Theory]
    [InlineData(-1, -1, null, 14)]
    [InlineData(24, -1, null, 24)]
    [InlineData(24, 8, "Fire", 8)]
    [InlineData(24, 8, "fIrE", 8)]
    [InlineData(24, -1, "Fire", 24)]
    [InlineData(-1, -1, "Fire", 14)]
    [InlineData(-1, 8, "Ice", 14)]
    [InlineData(24, 8, "Ice", 24)]
    [InlineData(0, 8, null, 0)]
    [InlineData(24, 0, "Fire", 0)]
    [InlineData(-2, -3, "Fire", 14)]
    [InlineData(24.25, -1, "Fire", 24.25)]
    public void MetadataUsesNativeFireThenDeathThenCurrentHeightOrder(double death, double burn, string? type, double expected)
    {
        var actor = new MetadataActor(death, burn);
        ActorDamage.Apply(actor, 1000, damageType: type);
        Assert.True(actor.IsDead); Assert.Equal(Fixed.FromDouble(expected), actor.Height);
    }

    [Fact]
    public void FireHeightIsSelectedBeforeUnsupportedDamageTypeIsCleared()
    {
        var actor = new MetadataActor(24, 8);
        ActorDamage.Apply(actor, 1000, damageType: "Fire");
        Assert.Equal(Fixed.FromInt(8), actor.Height); Assert.Null(actor.DamageType);
    }

    [Fact]
    public void FirstGibQuerySeesLivingHeightAndSecondSeesDeathHeight()
    {
        var actor = new OrderingActor();
        ActorDamage.Apply(actor, 1000);
        Assert.Equal(new[] { Fixed.FromInt(56), Fixed.FromInt(14) }, actor.ObservedHeights);
        Assert.Equal(new[] { true, false }, actor.ObservedShootable);
    }

    private sealed class MetadataActor(double death, double burn) : Actor
    {
        public override double DeathHeight => death;
        public override double BurnHeight => burn;
    }

    private sealed class OrderingActor : Actor
    {
        public List<Fixed> ObservedHeights { get; } = [];
        public List<bool> ObservedShootable { get; } = [];
        public override int GetGibHealth()
        {
            ObservedHeights.Add(Height); ObservedShootable.Add(Shootable); return base.GetGibHealth();
        }
    }
}
