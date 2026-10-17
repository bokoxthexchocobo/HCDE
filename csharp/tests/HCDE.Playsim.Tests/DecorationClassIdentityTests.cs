using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DecorationClassIdentityTests
{
    [Theory]
    [InlineData(10, "GibbedMarine")]
    [InlineData(12, "GibbedMarineExtra")]
    [InlineData(15, "DeadMarine")]
    [InlineData(18, "DeadZombieMan")]
    [InlineData(19, "DeadShotgunGuy")]
    [InlineData(20, "DeadDoomImp")]
    [InlineData(21, "DeadDemon")]
    [InlineData(22, "DeadCacodemon")]
    [InlineData(23, "DeadLostSoul")]
    [InlineData(24, "Gibs")]
    [InlineData(25, "DeadStick")]
    [InlineData(26, "LiveStick")]
    [InlineData(27, "HeadOnAStick")]
    [InlineData(28, "HeadsOnAStick")]
    [InlineData(29, "HeadCandles")]
    [InlineData(30, "TallGreenColumn")]
    [InlineData(31, "ShortGreenColumn")]
    [InlineData(32, "TallRedColumn")]
    [InlineData(33, "ShortRedColumn")]
    [InlineData(34, "Candlestick")]
    [InlineData(35, "Candelabra")]
    [InlineData(36, "HeartColumn")]
    [InlineData(37, "SkullColumn")]
    [InlineData(41, "EvilEye")]
    [InlineData(42, "FloatingSkull")]
    [InlineData(43, "TorchTree")]
    [InlineData(44, "BlueTorch")]
    [InlineData(45, "GreenTorch")]
    [InlineData(46, "RedTorch")]
    [InlineData(47, "Stalagtite")]
    [InlineData(48, "TechPillar")]
    [InlineData(49, "BloodyTwitch")]
    [InlineData(50, "Meat2")]
    [InlineData(51, "Meat3")]
    [InlineData(52, "Meat4")]
    [InlineData(53, "Meat5")]
    [InlineData(54, "BigTree")]
    [InlineData(55, "ShortBlueTorch")]
    [InlineData(56, "ShortGreenTorch")]
    [InlineData(57, "ShortRedTorch")]
    [InlineData(59, "NonsolidMeat2")]
    [InlineData(60, "NonsolidMeat4")]
    [InlineData(61, "NonsolidMeat3")]
    [InlineData(62, "NonsolidMeat5")]
    [InlineData(63, "NonsolidTwitch")]
    [InlineData(70, "BurningBarrel")]
    [InlineData(73, "HangNoGuts")]
    [InlineData(74, "HangBNoBrain")]
    [InlineData(75, "HangTLookingDown")]
    [InlineData(76, "HangTSkull")]
    [InlineData(77, "HangTLookingUp")]
    [InlineData(78, "HangTNoBrain")]
    [InlineData(79, "ColonGibs")]
    [InlineData(80, "SmallBloodPool")]
    [InlineData(81, "BrainStem")]
    [InlineData(85, "TechLamp")]
    [InlineData(86, "TechLamp2")]
    [InlineData(2028, "Column")]
    [InlineData(5050, "Stalagmite")]
    public void NativeDecorationIdentityDrivesQueriesAndFiltersAcrossFreshSave(int type, string name)
    {
        var sim = Room(type); var actor = sim.Actors.Single(a => a.ThingId == 9);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var decoration = world.Actors.Single(a => a.Id == actor.Id);
            var globals = new AcsGlobalStrings(); var stack = new List<int> { 9 };
            Assert.True(AcsCallFunctions.TryInvoke(world, stack, new() { Value = decoration }, [], globals,
                AcsCallFunctions.GetActorClass, 1, out var result));
            Assert.Equal(name, globals.Get(result)); Assert.Empty(stack);
            Assert.Equal(1, ActorProximity.CountProximity(world.Players.Single(), name.ToLowerInvariant(), 256, flags: 4));
            ActorRemovalActions.Remove(decoration, flags: 8 | 16, classFilter: name); Assert.False(decoration.Destroyed);
            ActorRemovalActions.Remove(decoration, flags: 8, classFilter: name); Assert.True(decoration.Destroyed);
        }
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 128, Id = 9 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
