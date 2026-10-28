namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppStateMapTests
{
    [Fact]
    public void ShouldAppendToStateMap_OnlyWhenEmpty()
    {
        Assert.True(VmDehackedDehSuppStateMap.ShouldAppendToStateMap(0));
        Assert.False(VmDehackedDehSuppStateMap.ShouldAppendToStateMap(1));
    }

    [Fact]
    public void IsPickupActor_DetectsMfSpecial()
    {
        Assert.True(VmDehackedDehSuppStateMap.IsPickupActor(VmDehackedDehSuppStateMap.MfSpecial));
        Assert.False(VmDehackedDehSuppStateMap.IsPickupActor(0));
    }

    [Fact]
    public void TryValidateActorType_RejectsNonActor()
    {
        Assert.False(VmDehackedDehSuppStateMap.TryValidateActorType(
            "NotAClass",
            typeExists: _ => true,
            isDescendantOfActor: _ => false,
            out var error));
        Assert.Contains("not an actor", error);
    }

    [Fact]
    public void TryParseStateMapSection_BuildsDehExtStates()
    {
        var rows = new[] { ("Weapon", "Fire", 3, 0u), ("HealthBonus", "Spawn", 2, VmDehackedDehSuppStateMap.MfSpecial) };
        Assert.True(VmDehackedDehSuppStateMap.TryParseStateMapSection(
            rows,
            [],
            typeExists: _ => true,
            isDescendantOfActor: _ => true,
            tryResolveState: (_, _) => true,
            ownsStateThroughSpan: (_, span) => span >= 1,
            out var map,
            out var dehExt,
            out _));
        Assert.Equal(2, map.Count);
        Assert.True(map[1].OwnerIsPickup);
        Assert.Equal(5, dehExt.Count);
        Assert.Equal(4, dehExt[4]);
    }

    [Fact]
    public void TryParseStateMapSection_SkipsAppendWhenExisting()
    {
        var existing = new List<VmDehackedStateMapEntry>
        {
            new() { ActorType = "Weapon", StateLabel = "Ready", StateSpan = 1, FirstDehIndex = 0 }
        };
        Assert.True(VmDehackedDehSuppStateMap.TryParseStateMapSection(
            [("Pistol", "Fire", 2, 0u)],
            existing,
            typeExists: _ => true,
            isDescendantOfActor: _ => true,
            tryResolveState: (_, _) => true,
            ownsStateThroughSpan: (_, _) => true,
            out var map,
            out _,
            out _));
        Assert.Single(map);
    }

    [Fact]
    public void TryParseStateMapSection_RejectsInvalidStateSpan()
    {
        Assert.False(VmDehackedDehSuppStateMap.TryParseStateMapSection(
            [("Weapon", "Fire", 0, 0u)],
            [],
            typeExists: _ => true,
            isDescendantOfActor: _ => true,
            tryResolveState: (_, _) => true,
            ownsStateThroughSpan: (_, _) => false,
            out _,
            out _,
            out var error));
        Assert.Contains("Invalid state range", error);
    }
}
