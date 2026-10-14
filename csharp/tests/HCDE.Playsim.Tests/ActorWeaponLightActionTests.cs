using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponLightActionTests
{
    [Theory]
    [InlineData(int.MinValue, -20)]
    [InlineData(-21, -20)]
    [InlineData(-20, -20)]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(20, 20)]
    [InlineData(21, 20)]
    [InlineData(int.MaxValue, 20)]
    public void ParameterizedLightClampsToNativeRange(int value, int expected)
    {
        var player = new PlayerPawn(); ActorWeaponLightActions.Light(player, value);
        Assert.Equal(expected, player.ExtraLight);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FixedActionsWorkAsStateCallbacks(int value)
    {
        var player = new PlayerPawn { ExtraLight = -10 };
        Action<Actor> action = value switch {
            0 => ActorWeaponLightActions.Light0,
            1 => ActorWeaponLightActions.Light1,
            _ => ActorWeaponLightActions.Light2
        };
        player.States.Configure(player, [new(-1, 0, action)], 0);
        Assert.Equal(value, player.ExtraLight);
    }

    [Fact]
    public void NonPlayerActionsLeaveActorUnchanged()
    {
        var actor = new Actor { Health = 73 };
        ActorWeaponLightActions.Light(actor, 20); ActorWeaponLightActions.Light0(actor);
        ActorWeaponLightActions.Light1(actor); ActorWeaponLightActions.Light2(actor);
        Assert.Equal(73, actor.Health);
    }

    [Fact]
    public void NegativeLightActionRoundTripsAndOlderSaveClearsIt()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim, true);
        ActorWeaponLightActions.Light(player, -100); var saved = SimSavegame.Write(sim, true);
        ActorWeaponLightActions.Light2(player); SimSavegame.Apply(sim, saved);
        Assert.Equal(-20, player.ExtraLight); Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.ExtraLight);
    }
}
