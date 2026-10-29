namespace HCDE.Playsim.Tests;

public class VmPMobjSpawn1153SliceTests
{
    [Fact]
    public void ConversationAndUdmf_ApplyOnlyDefinedFields()
    {
        var skipped = VmPMobjSpawn1153Slice.Finish(
            Thing() with { Conversation = 4, LookedUpRoot = -1, Alpha = -1, RenderStyle = VmPMobjSpawn1153Slice.StyleCount },
            Actor(),
            Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(3, skipped.ConversationRoot);
        Assert.False(skipped.DialogueAssigned);
        Assert.Equal(0.25, skipped.Alpha);
        Assert.Equal(2, skipped.RenderStyle);

        var applied = VmPMobjSpawn1153Slice.Finish(
            Thing() with
            {
                Conversation = 4,
                LookedUpRoot = 0,
                Alpha = 0,
                RenderStyle = 7,
                ScaleX = 2,
                ScaleY = 0,
                Pitch = -20,
                Roll = 0,
                Score = -4,
                FillColor = 0x00ABCDEF,
                PaletteIndex = 0x2A,
            },
            Actor(),
            Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(0, applied.ConversationRoot);
        Assert.True(applied.DialogueAssigned);
        Assert.Equal(0, applied.Alpha);
        Assert.Equal(7, applied.RenderStyle);
        Assert.Equal(8, applied.ScaleX);
        Assert.Equal(5, applied.ScaleY);
        Assert.Equal(-20, applied.Pitch);
        Assert.Equal(12, applied.Roll);
        Assert.Equal(-4, applied.Score);
        Assert.Equal(0x2AABCDEFu, applied.FillColor);
        Assert.True(applied.BeginPlay);
    }

    [Fact]
    public void Light_SplitsSpotColorAndCopiesAttachedLights()
    {
        int flags = VmPMobjSpawn1153Slice.NoShadowMap
            | VmPMobjSpawn1153Slice.DontLightActors
            | VmPMobjSpawn1153Slice.Spot
            | VmPMobjSpawn1153Slice.DontLightMap;
        var lights = new[]
        {
            new VmPMobjSpawn1153Slice.LightSlot(false, 1, 1, 1),
            new VmPMobjSpawn1153Slice.LightSlot(true, 4, 5, 6),
        };
        var spot = VmPMobjSpawn1153Slice.Finish(
            Thing() with
            {
                DynamicLight = true,
                Arg0 = 0x010203,
                Arg1 = 30,
                Arg2 = 60,
                NoShadowDefined = true,
                NoShadow = false,
                DontMapDefined = true,
                DontMap = true,
                SoftShadow = 0,
                Linearity = -1,
                ShadowQuality = 3,
            },
            Actor() with { LightFlags = flags, SoftShadow = -1, Linearity = 1.5f, ShadowQuality = -1 },
            lights);
        Assert.Equal(1, spot.Arg0);
        Assert.Equal(2, spot.Arg1);
        Assert.Equal(3, spot.Arg2);
        Assert.Equal(VmPMobjSpawn1153Slice.DontLightActors | VmPMobjSpawn1153Slice.Spot | VmPMobjSpawn1153Slice.DontLightMap, spot.LightFlags);
        Assert.Equal(0f, spot.SoftShadow);
        Assert.Equal(1.5f, spot.Linearity);
        Assert.Equal(3, spot.ShadowQuality);
        Assert.True(spot.SpotAngles);
        Assert.Equal(30, spot.SpotInner);
        Assert.Equal(60, spot.SpotOuter);
        Assert.False(spot.Lights[0].Present);
        Assert.Equal(0f, spot.Lights[1].Soft);
        Assert.Equal(1.5f, spot.Lights[1].Linearity);
        Assert.Equal(3, spot.Lights[1].Quality);

        var named = VmPMobjSpawn1153Slice.Finish(
            Thing() with { DynamicLight = true, HasColorName = true, ColorR = 9, ColorG = 8, ColorB = 7, Arg1 = 11, Arg2 = 13 },
            Actor() with { LightFlags = VmPMobjSpawn1153Slice.Spot, Arg0 = 1, Arg1 = 2, Arg2 = 3 },
            Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(9, named.Arg0);
        Assert.Equal(8, named.Arg1);
        Assert.Equal(7, named.Arg2);
        Assert.Equal(11, named.SpotInner);
        Assert.Equal(13, named.SpotOuter);

        var plain = VmPMobjSpawn1153Slice.Finish(
            Thing() with { SoftShadow = 4, Arg0 = 0x010203 },
            Actor() with { Arg0 = 70, LightFlags = VmPMobjSpawn1153Slice.Spot },
            new[] { new VmPMobjSpawn1153Slice.LightSlot(true, 8, 5, 6) });
        Assert.Equal(70, plain.Arg0);
        Assert.False(plain.SpotAngles);
        Assert.Equal(8f, plain.Lights[0].Soft);
        Assert.Equal(-1f, plain.SoftShadow);
    }

    [Fact]
    public void Health_ScalesPositiveValuesAndDiesAtZero()
    {
        var scaled = VmPMobjSpawn1153Slice.Finish(Thing() with { Health = 1.5 }, Actor() with { Health = 8, StartHealth = 4 }, Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(12, scaled.Health);
        Assert.True(scaled.StartHealthSet);
        Assert.Equal(12, scaled.StartHealth);
        Assert.True(scaled.LevelSpawned);
        Assert.False(scaled.DieFromSpawn);

        var identity = VmPMobjSpawn1153Slice.Finish(Thing() with { Health = 1 }, Actor() with { Health = 8, StartHealth = 4 }, Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(8, identity.Health);
        Assert.False(identity.StartHealthSet);
        Assert.Equal(4, identity.StartHealth);

        var negative = VmPMobjSpawn1153Slice.Finish(Thing() with { Health = -4 }, Actor() with { Health = 8 }, Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(4, negative.Health);
        Assert.Equal(4, negative.StartHealth);

        var truncated = VmPMobjSpawn1153Slice.Finish(Thing() with { Health = 0.5 }, Actor() with { Health = -5 }, Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(-2, truncated.Health);

        var dead = VmPMobjSpawn1153Slice.Finish(
            Thing() with { Health = 0, HasDieState = true },
            Actor() with { ObjectFlags = VmPMobjSpawn1153Slice.Euthanize, Health = 8, StartHealth = 4 },
            Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.Equal(0, dead.Health);
        Assert.True(dead.DieFromSpawn);
        Assert.False(dead.StartHealthSet);
        Assert.True(dead.BeginPlay);
        Assert.False(dead.LevelSpawned);

        var quiet = VmPMobjSpawn1153Slice.Finish(Thing() with { Health = 0 }, Actor(), Array.Empty<VmPMobjSpawn1153Slice.LightSlot>());
        Assert.False(quiet.DieFromSpawn);

        var first = VmPMobjSpawn1153Slice.OpenIndex(0);
        Assert.True(first.Register);
        Assert.True(first.SpawnCalled);
        var later = VmPMobjSpawn1153Slice.OpenIndex(4);
        Assert.False(later.Register);
        Assert.True(later.SpawnCalled);
    }

    private static VmPMobjSpawn1153Slice.ThingTail Thing()
    {
        return new VmPMobjSpawn1153Slice.ThingTail(
            0, -1, -1, VmPMobjSpawn1153Slice.StyleCount, 0, 0, 0, 0, 0, 0, 0,
            false, false, 0, 0, 0, 0, 0, 0,
            false, false, false, false, false, false,
            -1, -1, -1, 1, false);
    }

    private static VmPMobjSpawn1153Slice.ActorTail Actor()
    {
        return new VmPMobjSpawn1153Slice.ActorTail(
            3, false, 0.25, 2, 4, 5, 9, 12, 6, 0x11, 1, 2, 3, 0, -1, -1, -1, 0, 100, 100);
    }
}
