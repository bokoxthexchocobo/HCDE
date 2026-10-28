using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsModCvarServiceTests
{
    [Fact]
    public void CallFunc_SetCVar_WritesIntegerModCvar()
    {
        var sim = Room();
        sim.CVars.Set("test_int", ConsoleVariable.IntValue(0));
        RunCall(sim, AcsCallFunctions.SetCVar, [0, 42], ["test_int"]);
        Assert.Equal(42, sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, "test_int")!.Int);
    }

    [Fact]
    public void CallFunc_SetCVarString_StoresText()
    {
        var sim = Room();
        sim.CVars.Set("test_str", ConsoleVariable.StringValue(""));
        RunCall(sim, AcsCallFunctions.SetCVarString, [0, 1], ["test_str", "hello"]);
        Assert.Equal("hello", sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, "test_str")!.Text);
    }

    [Fact]
    public void CallFunc_SetUserCVar_MirrorsConsolePlayerGlobal()
    {
        var sim = Room();
        sim.ConsolePlayer = 0;
        sim.CVars.Set("user_speed", ConsoleVariable.IntValue(0));
        sim.CVars.SetUser(0, "user_speed", ConsoleVariable.IntValue(0));
        RunCall(sim, AcsCallFunctions.SetUserCVar, [0, 0, 9], ["user_speed"]);
        Assert.Equal(9, sim.CVars.TryResolve(0, sim.IsPlayerInGame, sim.ConsolePlayer, "user_speed")!.Int);
        Assert.Equal(9, sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, "user_speed")!.Int);
    }

    private static void RunCall(AuthoritySimulation sim, int function, int[] args, string[] strings)
    {
        var stack = new List<int>(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), strings, new AcsGlobalStrings(), function, args.Length, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
