using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UseRumbleTests
{
    [Theory]
    [InlineData(false, false, false, 2, 0)]
    [InlineData(true, false, false, 2, 112)]
    [InlineData(false, true, false, 1024, 0)]
    [InlineData(false, false, true, 2, 0)]
    public void SuccessfulUsePassesNativeMetadataAfterSpecialClear(bool repeat, bool back, bool through,
        int activation, int special)
    {
        var sim = Room(); var player = new ProbePlayer { Simulation = sim };
        var line = Line(repeat); line.PlayerUseBack = back; line.UseThrough = through;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true, back));
        Assert.Equal((activation, 42, 9, special), Assert.Single(player.Calls));
        Assert.Equal(200, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedOrCrossActivationDoesNotEmitUseRumble(bool cross)
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = Listen(sim); var line = Line();
        line.PlayerCross = cross; if (!cross) line.Special = 9999;
        Assert.Equal(cross, LineSpecials.ActivateMapLine(sim, player, line, !cross, false));
        Assert.Empty(requests);
    }

    [Fact]
    public void ActionPreferenceIsIndependentOfDamageAndWorldPreferences()
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = Listen(sim);
        sim.WorldHapticsEnabled = false; sim.DamageHapticsEnabled = false;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, Line(), true, false));
        Assert.Equal(new SimulationRumbleRequest(player.Id, "*usesuccess", true), Assert.Single(requests));
        requests.Clear(); sim.ActionHapticsEnabled = false;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, Line(), true, false)); Assert.Empty(requests);
    }

    [Fact]
    public void OneShotAndRestoreDoNotRepeatPresentation()
    {
        var line = Line(); var sim = Room(line); var player = sim.Players.Single(); var requests = Listen(sim);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, sim.Level.Lines.Single(), true, false));
        Assert.Single(requests); requests.Clear();
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        Assert.False(LineSpecials.ActivateMapLine(sim, player, sim.Level.Lines.Single(), true, false)); Assert.Empty(requests);
    }

    [Fact]
    public void ActualUseTraceEmitsSuccessfulUsePattern()
    {
        var sim = Room(Line()); var requests = Listen(sim); sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim); Assert.Equal("*usesuccess", Assert.Single(requests).Pattern);
    }

    [Fact]
    public void BlockedNonSpecialLineEmitsUseFailPattern()
    {
        var wall = new LevelLine { X1 = 16, X2 = 16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.DoomBinary,
            Things = [new LevelThing { Type = 1 }],
            Sectors =
            [
                new LevelSector { LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { LightLevel = 160, CeilingHeight = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [wall],
        });
        sim.Players.Single().Angle = BamAngle.FromDegrees(0);
        var requests = Listen(sim);
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal("*usefail", Assert.Single(requests).Pattern);
    }

    private sealed class ProbePlayer : PlayerPawn
    {
        public List<(int Activation, int Level, int Line, int Special)> Calls { get; } = new();
        public override void PlayerUsedSomethingMakeRumble(int activationType, int levelNum, int lineNum, int lineSpecial)
        { Calls.Add((activationType, levelNum, lineNum, lineSpecial)); base.PlayerUsedSomethingMakeRumble(activationType, levelNum, lineNum, lineSpecial); }
    }

    private static List<SimulationRumbleRequest> Listen(AuthoritySimulation sim)
    { var requests = new List<SimulationRumbleRequest>(); sim.RumbleRequested += requests.Add; return requests; }

    private static LevelLine Line(bool repeat = false) => new()
    {
        Index = 9, Special = 112, Arg0 = 7, Arg1 = 200, PlayerUse = true, Repeat = repeat,
        X1 = 32, Y1 = 32, X2 = 32, Y2 = -32, SideFront = 0, SideBack = -1,
    };

    private static AuthoritySimulation Room(LevelLine? line = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, LevelNumber = 42,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Sides = [new LevelSide { Sector = 0 }], Lines = line is null ? [] : [line],
        Things = [new LevelThing { Type = 1 }],
    });
}
