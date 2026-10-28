using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SwitchTextureTests
{
    [Fact]
    public void UseLineChangesSwitchTextureAndPlaysSound()
    {
        var sim = Room();
        var line = sim.Level.Lines[0];
        var player = sim.Players.Single();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        Assert.Equal("SW2BRCOM", sim.Level.Sides[0].TopTexture);
        Assert.Contains(sim.DrainSoundRequests(), request => request.Sound.Contains("normbutn", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ButtonReturnsToOffTextureAfterHoldTime()
    {
        var sim = Room();
        var line = sim.Level.Lines[0];
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use: true));
        Assert.Equal("SW2BRCOM", sim.Level.Sides[0].TopTexture);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal("SW1BRCOM", sim.Level.Sides[0].TopTexture);
    }

    [Fact]
    public void RePressingActiveButtonAdvancesTimer()
    {
        var sim = Room();
        var line = sim.Level.Lines[0];
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use: true));
        var timer = sim.ActiveButtons.Single().Timer;
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use: true));
        Assert.Equal(1u, sim.ActiveButtons.Single().Timer);
        Assert.True(timer > 1);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Namespace = "ZDoom",
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0, TopTexture = "SW1BRCOM" }],
        Lines =
        [
            new LevelLine
            {
                X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0, Special = 112, Arg0 = 7, Arg1 = 1,
                PlayerUse = true, Repeat = true,
            },
        ],
        Things = [new LevelThing { Type = 1 }],
    });
}
