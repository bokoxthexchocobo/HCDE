using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ExitLineRestrictionTests
{
    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 11, true, false)]
    [InlineData(MapDataFormat.DoomBinary, 52, false, false)]
    [InlineData(MapDataFormat.DoomBinary, 51, true, true)]
    [InlineData(MapDataFormat.DoomBinary, 124, false, true)]
    [InlineData(MapDataFormat.HexenBinary, 243, true, false)]
    [InlineData(MapDataFormat.HexenBinary, 244, true, true)]
    public void ForbiddenExitDamagesActivatorWithoutConsumingLine(MapDataFormat format, int special, bool use, bool secret)
    {
        var sim = Room(format, true); var player = sim.Players.Single();
        player.GodMode = true; player.Invulnerable = true;
        player.Inventory.Armor = 200; player.Inventory.ArmorSavePercent = 100;
        var line = new LevelLine { Special = special, PlayerUse = use, PlayerCross = !use };
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, use));
        Assert.False(sim.Exited); Assert.True(player.IsDead); Assert.Equal(1, player.DeathCount);
        Assert.Equal(player.Id, player.LastDamageSourceId); Assert.Equal(special, line.Special);
        Assert.Equal(200, player.Inventory.Armor);

        var allowed = Room(format, false); var allowedLine = new LevelLine
            { Special = special, PlayerUse = use, PlayerCross = !use };
        Assert.True(LineSpecials.ActivateMapLine(allowed, allowed.Players.Single(), allowedLine, use));
        Assert.True(allowed.Exited); Assert.Equal(secret, allowed.SecretExit);
        Assert.Equal(100, allowed.Players.Single().Health);
        Assert.Equal(special == 124 ? special : 0, allowedLine.Special);
    }

    [Fact]
    public void WorldExitBypassesRestriction()
    {
        var sim = Room(MapDataFormat.HexenBinary, true);
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.ExitNormal, 0));
        Assert.True(sim.Exited); Assert.Equal(100, sim.Players.Single().Health);
    }

    private static AuthoritySimulation Room(MapDataFormat format, bool noExit) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch), noExit: noExit);
}
