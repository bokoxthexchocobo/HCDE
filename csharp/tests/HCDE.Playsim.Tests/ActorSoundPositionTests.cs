namespace HCDE.Playsim.Tests;

public class ActorSoundPositionTests
{
    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(-10.5, 20.25, -30.125)]
    [InlineData(0, 0, 0)]
    public void AudioPositionSwapsWorldAxesWithoutMutation(double x, double y, double z)
    {
        var actor = new Actor(); actor.SetXYZ(x, y, z); actor.RememberPosition();
        actor.VelocityX = Fixed.FromInt(7);
        Assert.Equal((x, y, z), actor.Pos());
        Assert.Equal(((float)x, (float)z, (float)y), actor.SoundPos());
        Assert.Equal(Fixed.FromDouble(x), actor.PreviousX);
        Assert.Equal(Fixed.FromDouble(y), actor.PreviousY);
        Assert.Equal(Fixed.FromDouble(z), actor.PreviousZ);
        Assert.Equal(7, actor.VelocityX.ToDouble());
    }

    [Fact]
    public void AudioQueryUsesCurrentPositionAndNativeSinglePrecision()
    {
        var actor = new Actor(); actor.SetXYZ(1, 2, 3); actor.RememberPosition();
        var value = 16384 + 1.0 / Fixed.Unit;
        actor.SetXYZ(value, -value, value);
        Assert.Equal((value, -value, value), actor.Pos());
        Assert.Equal((16384f, 16384f, -16384f), actor.SoundPos());
        Assert.Equal(1, actor.PreviousX.ToDouble()); Assert.Equal(3, actor.PreviousZ.ToDouble());
    }
}
