namespace HCDE.Gamedata.Tests;

public class DecorateActorFlagAndPropertyTests
{
    [Fact]
    public void ModifiesSolidAndShootableFlags()
    {
        var state = new DecorateActorFlagState();
        Assert.True(DecorateActorFlagModifier.TryModify(state, "SOLID", true, out _));
        Assert.True(DecorateActorFlagModifier.IsSet(state, "SOLID"));
        Assert.True(DecorateActorFlagModifier.TryModify(state, "SHOOTABLE", false, out _));
        Assert.False(DecorateActorFlagModifier.IsSet(state, "SHOOTABLE"));
    }

    [Fact]
    public void PropertyRouterAppliesSignedFlagModifiers()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty("+SOLID", "Actor", [], def, out _));
        Assert.True(def.Flags.Solid);
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty("-SHOOTABLE", "Actor", [], def, out _));
        Assert.False(def.Flags.Shootable);
    }

    [Fact]
    public void KillItemSecretCountersTrackFlagMutations()
    {
        var state = new DecorateActorFlagState { Health = 500, CountsAsKill = true };
        var counters = new DecorateActorFlagModifier.FlagMutationCounters();
        Assert.True(DecorateActorFlagModifier.TryModify(state, "COUNTITEM", true, counters, out _));
        Assert.Equal(1, counters.Items);
        Assert.True(DecorateActorFlagModifier.TryModify(state, "COUNTITEM", false, counters, out _));
        Assert.Equal(0, counters.Items);
    }

    [Fact]
    public void BuiltinHealthPropertyUpdatesDefaults()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty("Health", "Actor", ["500"], def, out _));
        Assert.Equal(500, def.Flags.Health);
    }

    [Fact]
    public void StateLabelPropertyStoresLabel()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty("Death", "Actor", ["Death1"], def, out _));
        Assert.Equal("Death1", def.StateLabels["Death"]);
    }

    [Fact]
    public void UnknownPropertyFails()
    {
        var def = new DecorateActorDefinition();
        Assert.False(DecorateActorPropertyRouter.TryApplyProperty("NotARealProperty", "Actor", [], def, out var error));
        Assert.Contains("unknown", error, StringComparison.OrdinalIgnoreCase);
    }
}
