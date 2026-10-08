namespace HCDE.Playsim.Tests;

public class SkullChargeSpeedTests
{
    [Theory]
    [InlineData(1, 2, 20000)]
    [InlineData(-1, 2, 20000)]
    [InlineData(1, -2, 20000)]
    [InlineData(-1, -2, 20000)]
    [InlineData(3, 7, 20)]
    [InlineData(0, 0, 20)]
    public void LaunchVelocityUsesStoredFacingAndNativeTravelHelpers(int x, int y, double speed)
    {
        var soul = new Actor { Brain = MonsterBrain.ForType(3006), Z = Fixed.FromInt(8),
            MovementSpeed = Fixed.FromInt(99), VelocityZ = Fixed.FromInt(-5) };
        var target = new Actor { Id = 2, X = Fixed.FromInt(x), Y = Fixed.FromInt(y),
            Z = Fixed.FromInt(40), Height = Fixed.FromInt(20) };
        soul.Brain!.StartCharge(soul, target, speed);
        var expected = Actor.AngleToVector(soul.Angle.ToDegrees(), speed);
        Assert.Equal(Fixed.FromDouble(expected.X), soul.VelocityX);
        Assert.Equal(Fixed.FromDouble(expected.Y), soul.VelocityY);
        Assert.Equal(Fixed.FromDouble(42 / soul.DistanceBySpeed(target, speed)), soul.VelocityZ);
        Assert.Equal(Fixed.FromInt(99), soul.MovementSpeed);
        Assert.Equal(target.Id, soul.Brain.TargetId); Assert.True(soul.Brain.Charging);
        if (x == 1 && y == 2 && speed == 20000)
            Assert.Equal(586171805, soul.VelocityX.Raw);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(40, 40)]
    [InlineData(0, 20)]
    [InlineData(-5, 20)]
    [InlineData(2.5, 2.5)]
    public void ChargeUsesConfiguredSpeedOrNativeFallback(double supplied, double expected)
    {
        var soul = new Actor { Brain = MonsterBrain.ForType(3006), ReactionTime = 17 };
        var target = new Actor { Id = 2, X = Fixed.FromInt(100), Height = Fixed.FromInt(40) };
        soul.Brain!.StartCharge(soul, target, supplied);
        Assert.Equal(expected, soul.VelocityX.ToDouble());
        Assert.Equal(0, soul.VelocityY.ToDouble());
        Assert.Equal(expected / 5, soul.VelocityZ.ToDouble(), precision: 4);
        Assert.Equal(17, soul.ReactionTime);
        Assert.Equal(target.Id, soul.Brain.TargetId);
        Assert.True(soul.Brain.Charging);
    }

    [Theory]
    [InlineData(10, 40)]
    [InlineData(0, 40)]
    public void ShortDistanceClampsVerticalTravelToOneTic(int distance, double speed)
    {
        var soul = new Actor { Brain = MonsterBrain.ForType(3006), Z = Fixed.FromInt(8) };
        var target = new Actor { X = Fixed.FromInt(distance), Z = Fixed.FromInt(100), Height = Fixed.FromInt(40) };
        soul.Brain!.StartCharge(soul, target, speed);
        Assert.Equal(112, soul.VelocityZ.ToDouble());
    }

    [Fact]
    public void ConfiguredSpeedUsesTargetDirectionAndHeight()
    {
        var soul = new Actor { Brain = MonsterBrain.ForType(3006), Z = Fixed.FromInt(100) };
        var target = new Actor { Y = Fixed.FromInt(100), Height = Fixed.FromInt(40) };
        soul.Brain!.StartCharge(soul, target, 40);
        Assert.Equal(0, soul.VelocityX.ToDouble(), precision: 4);
        Assert.Equal(40, soul.VelocityY.ToDouble());
        Assert.Equal(-32, soul.VelocityZ.ToDouble());
        Assert.Equal(90, soul.Angle.ToDegrees(), precision: 4);
    }
}
