using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetCVarTests
{
    [Fact]
    public void GenericFloatAndStringRepsFollowNativeKinds()
    {
        var sim = Room();
        var player = sim.Players.Single(p => p.PlayerNum == 0);
        sim.CVars.Set("flag", ConsoleVariable.BoolValue(true));
        sim.CVars.Set("off", ConsoleVariable.BoolValue(false));
        sim.CVars.Set("count", ConsoleVariable.IntValue(16777217));
        sim.CVars.Set("scale", ConsoleVariable.FloatValue(1.5f));
        sim.CVars.Set("tiny", ConsoleVariable.FloatValue(0.00001234f));
        sim.CVars.Set("word", ConsoleVariable.StringValue("true"));
        sim.CVars.Set("hex", ConsoleVariable.StringValue("0x10"));
        sim.CVars.Set("packed", ConsoleVariable.StringValue("0x1.8p1"));
        sim.CVars.Set("octalish", ConsoleVariable.StringValue("010"));
        sim.CVars.Set("sci", ConsoleVariable.StringValue("  -2.5e1xyz"));
        sim.CVars.Set("tint", ConsoleVariable.ColorValue(0x112233));

        Assert.Equal(1d, player.GetCVar("flag"));
        Assert.Equal("true", player.GetCVarString("flag"));
        Assert.Equal(0d, player.GetCVar("off"));
        Assert.Equal("false", player.GetCVarString("off"));
        Assert.Equal((double)(float)16777217, player.GetCVar("count"));
        Assert.Equal("16777217", player.GetCVarString("count"));
        Assert.Equal(1.5d, player.GetCVar("scale"));
        Assert.Equal("1.5", player.GetCVarString("scale"));
        Assert.Equal((double)0.00001234f, player.GetCVar("tiny"));
        Assert.Equal("1.234e-05", player.GetCVarString("tiny"));
        Assert.Equal(0d, player.GetCVar("word"));
        Assert.Equal("true", player.GetCVarString("word"));
        Assert.Equal(16d, player.GetCVar("hex"));
        Assert.Equal(3d, player.GetCVar("packed"));
        Assert.Equal(10d, player.GetCVar("octalish"));
        Assert.Equal(-25d, player.GetCVar("sci"));
        Assert.Equal((double)(float)0x112233, player.GetCVar("tint"));
        Assert.Equal("11 22 33", player.GetCVarString("tint"));
    }

    [Fact]
    public void StringFloatParsingMatchesCStrtodEdges()
    {
        var player = Room().Players.Single(p => p.PlayerNum == 0);
        player.Simulation!.CVars.Set("wide", ConsoleVariable.StringValue("0x8000000000000000"));
        player.Simulation.CVars.Set("gap", ConsoleVariable.StringValue("\u00A01"));
        player.Simulation.CVars.Set("negzero", ConsoleVariable.FloatValue(BitConverter.Int32BitsToSingle(unchecked((int)0x80000000))));
        Assert.Equal((double)(float)Math.ScaleB(1, 63), player.GetCVar("wide"));
        Assert.Equal(0d, player.GetCVar("gap"));
        Assert.Equal("-0", player.GetCVarString("negzero"));
    }

    [Fact]
    public void NamesAreCaseInsensitiveAndIgnoredOrMissingValuesAreEmpty()
    {
        var sim = Room();
        var player = sim.Players.Single(p => p.PlayerNum == 0);
        sim.CVars.Set("VertSpread", ConsoleVariable.IntValue(1));
        sim.CVars.Set("secret", ConsoleVariable.IntValue(9, ConsoleVariableFlags.Ignore));
        Assert.Equal(1d, player.GetCVar("vertspread"));
        Assert.Equal("1", player.GetCVarString("VERTSPREAD"));
        Assert.Equal(0d, player.GetCVar("secret"));
        Assert.Equal("", player.GetCVarString("secret"));
        Assert.Equal(0d, player.GetCVar("missing"));
        Assert.Equal("", player.GetCVarString(""));
        Assert.Equal(1d, sim.Actors.Single(a => a.DoomEdNum == 2007).GetCVar("vertspread"));
    }

    [Fact]
    public void UserInfoRedirectsToTheCallerOrConsolePlayer()
    {
        var sim = Room();
        var first = sim.Players.Single(p => p.PlayerNum == 0);
        var second = sim.Players.Single(p => p.PlayerNum == 1);
        var imp = sim.Actors.Single(a => a.DoomEdNum == 2007);
        sim.CVars.Set("gender", ConsoleVariable.StringValue("global", ConsoleVariableFlags.UserInfo));
        Assert.Equal("", first.GetCVarString("gender"));
        sim.CVars.SetUser(0, "gender", ConsoleVariable.StringValue("male"));
        sim.CVars.SetUser(1, "gender", ConsoleVariable.StringValue("female"));
        Assert.Equal("male", first.GetCVarString("GENDER"));
        Assert.Equal("female", second.GetCVarString("gender"));
        Assert.Equal("male", imp.GetCVarString("gender"));
        sim.ConsolePlayer = 1;
        Assert.Equal("female", imp.GetCVarString("gender"));
        sim.ConsolePlayer = 3;
        Assert.Equal("", imp.GetCVarString("gender"));
        Assert.Equal("male", first.GetCVarString("gender"));
        sim.ConsolePlayer = 0;
        sim.CVars.SetUser(0, "gender", ConsoleVariable.StringValue("hidden", ConsoleVariableFlags.Ignore));
        Assert.Equal("", first.GetCVarString("gender"));
        sim.CVars.Set("hidden", ConsoleVariable.StringValue("nope", ConsoleVariableFlags.UserInfo | ConsoleVariableFlags.Ignore));
        sim.CVars.SetUser(0, "hidden", ConsoleVariable.StringValue("kept"));
        Assert.Equal("", first.GetCVarString("hidden"));
        sim.CVars.SetUser(0, "orphan", ConsoleVariable.StringValue("lost"));
        Assert.Equal("", first.GetCVarString("orphan"));
    }

    [Fact]
    public void QueryDoesNotMutateTheSaveAndFreshRestoreDropsSessionCVars()
    {
        var sim = Room();
        var player = sim.Players.Single(p => p.PlayerNum == 0);
        sim.CVars.Set("sv_gravity", ConsoleVariable.FloatValue(800));
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(800d, player.GetCVar("sv_gravity"));
        Assert.Equal("800", player.GetCVarString("sv_gravity"));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room();
        SimSavegame.Apply(fresh, saved);
        var restored = fresh.Players.Single(p => p.PlayerNum == 0);
        Assert.Equal(0d, restored.GetCVar("sv_gravity"));
        Assert.Equal("", restored.GetCVarString("sv_gravity"));
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void DetachedActorsAndNullNamesFailBeforeLookup()
    {
        var actor = new Actor();
        Assert.Throws<InvalidOperationException>(() => actor.GetCVar("sv_gravity"));
        Assert.Throws<InvalidOperationException>(() => actor.GetCVarString("sv_gravity"));
        var player = Room().Players.Single(p => p.PlayerNum == 0);
        Assert.Throws<ArgumentNullException>(() => player.GetCVar(null!));
        Assert.Throws<ArgumentNullException>(() => player.GetCVarString(null!));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 256 }, new LevelThing { Type = 2007, X = -256 }]
    });
}
