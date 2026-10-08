using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DefaultDropStyleTests
{
    [Theory]
    [InlineData(null, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void ParsedGameDefaultFlowsThroughLevelCopyAndCallerOverride(int? explicitDefault, int expected)
    {
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse("GameInfo { DefaultDropStyle = 2 }", out var info, out var error), error);
        var level = new PlayLevel
        {
            DefaultDropStyle = info.DefaultDropStyle,
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
        };
        var sim = AuthoritySimulation.Start(level.CopyForSimulation(), spawnOptions: new SpawnOptions(DefaultDropStyle: explicitDefault));
        Assert.Equal(expected, sim.DefaultDropStyle);
        Assert.Equal(expected, sim.EffectiveDropStyle);
    }

    [Theory]
    [InlineData(1, 0, 1)]
    [InlineData(2, 0, 2)]
    [InlineData(2, 1, 1)]
    [InlineData(1, 2, 2)]
    public void GameDefaultResolvesZeroWhileExplicitStyleOverridesIt(int gameDefault, int selected, int effective)
    {
        var sim = Room(gameDefault); sim.DropStyle = selected;
        var control = Room(1); control.DropStyle = effective;
        Assert.Equal(effective, sim.EffectiveDropStyle);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        Assert.True(control.SpawnDroppedPickup(control.Players.Single(), PickupCatalog.Clip));
        var drop = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        var reference = control.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(reference.Z, drop.Z); Assert.Equal(reference.VelocityX, drop.VelocityX);
        Assert.Equal(reference.VelocityY, drop.VelocityY); Assert.Equal(reference.VelocityZ, drop.VelocityZ);
        Assert.Equal(control.NextDropItemByte(), sim.NextDropItemByte());
        var saved = SimSavegame.Write(sim);
        // Existing dynamic drop objects are outside this test's reconstruction scope.
        SimSavegame.Apply(sim, saved); Assert.Equal(effective, sim.EffectiveDropStyle);
        Assert.Equal(gameDefault, sim.DefaultDropStyle);
    }

    [Fact]
    public void GameDefaultParticipatesInChecksumAndDefaultRemainsDoom()
    {
        var doom = Room(1); var strife = Room(2);
        Assert.Equal(1, doom.DefaultDropStyle); Assert.Equal(1, doom.EffectiveDropStyle);
        doom.Tick(); strife.Tick(); Assert.NotEqual(doom.Checksum, strife.Checksum);
    }

    private static AuthoritySimulation Room(int gameDefault) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] },
        rngSeed: 42, spawnOptions: new SpawnOptions(DefaultDropStyle: gameDefault));
}
