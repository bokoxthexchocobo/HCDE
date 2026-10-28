namespace HCDE.Playsim.Tests;

public class MapThingActorSpawnComposerTests
{
    [Fact]
    public void ComposesFloorSpawnWithMapArgs()
    {
        var placement = new MapThingActorSpawnComposer.MapThingSpawnPlacement
        {
            X = 128,
            Y = 256,
            ZOffset = 8,
            AngleDegrees = 90,
            ThingId = 7,
            Special = 11,
            Args = [1, 2, 3, 4, 5],
        };
        var composed = MapThingActorSpawnComposer.Compose(placement, hexenFloatBobCompat: false);
        Assert.Equal(8, composed.Z);
        Assert.Equal(11, composed.Special);
        Assert.Equal(7u, composed.ThingId);
        Assert.Equal(90, composed.AngleDegrees);
    }

    [Fact]
    public void SkipsMapArgsWhenActorDefinesArgs()
    {
        var placement = new MapThingActorSpawnComposer.MapThingSpawnPlacement
        {
            ArgsDefinedOnActor = true,
            Special = 99,
        };
        var composed = MapThingActorSpawnComposer.Compose(placement, hexenFloatBobCompat: true);
        Assert.Equal(0, composed.Special);
    }
}

public class PlayerSpawnPlacementPolicyTests
{
    [Fact]
    public void UsesLastSafePositionForSameSpotRespawn()
    {
        var origin = PlayerSpawnPlacementPolicy.Resolve(
            new PlayerSpawnPlacementPolicy.PlayerSpawnContext
            {
                SameSpawnSpotDmFlag = true,
                PlayerRebornState = true,
                LastSafePosValid = true,
                LastSafeX = 10,
                LastSafeY = 20,
                LastSafeZ = 3,
                LastSafeAngle = 45,
            },
            startX: 0,
            startY: 0,
            startZ: 0,
            startAngle: 0,
            badAnglesCompat: false);
        Assert.True(origin.UsedLastSafePos);
        Assert.Equal(10, origin.X);
    }
}
