namespace HCDE.Playsim.Tests;

public class SpawnResurrectionDefaultTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnCallbacksDoNotReplaceOriginalDimensionsAndCollision(bool mapSpawn)
    {
        var actor = new ResizeActor { Radius = Fixed.FromInt(20), Height = Fixed.FromInt(56), Solid = true, Shootable = true };
        ThingActivation.InitializeSpawn(actor, false, mapSpawn);
        Assert.Equal(Fixed.FromInt(20), actor.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(56), actor.ResurrectionHeight);
        Assert.Equal(3, actor.ResurrectionCollisionFlags);
        Assert.Equal(Fixed.FromInt(8), actor.Radius); Assert.Equal(Fixed.FromInt(16), actor.Height);
        Assert.False(actor.Solid);
    }

    [Fact]
    public void ExistingClassDefaultsArePreserved()
    {
        var actor = new ResizeActor { ResurrectionRadius = Fixed.FromInt(24), ResurrectionHeight = Fixed.FromInt(64),
            ResurrectionCollisionFlags = 2 };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.Equal(Fixed.FromInt(24), actor.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(64), actor.ResurrectionHeight); Assert.Equal(2, actor.ResurrectionCollisionFlags);
    }

    private sealed class ResizeActor : Actor
    {
        public override void BeginPlay()
        {
            base.BeginPlay(); Radius = Fixed.FromInt(8); Height = Fixed.FromInt(16); Solid = false;
        }
    }
}
