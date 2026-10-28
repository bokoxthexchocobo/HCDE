using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScriptCallServiceTests
{
    [Fact]
    public void CallFunc_ScriptCall_InvokesRegisteredStaticIntFunction()
    {
        var sim = Room();
        sim.ScriptCalls.Register(new AcsScriptCallBinding
        {
            ClassName = "Probe",
            FunctionName = "Add",
            ParameterTypes = [AcsScriptValueKind.Int32, AcsScriptValueKind.Int32],
            ReturnType = AcsScriptValueKind.Int32,
            Invoke = invocation =>
            {
                var sum = invocation.Arguments[0].IntValue + invocation.Arguments[1].IntValue;
                return new AcsScriptCallOutcome(sum);
            },
        });

        var stack = new List<int> { 0, 1, 40, 2 };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new(), ["Probe", "Add"], globals, AcsCallFunctions.ScriptCall, 4, out var result));
        Assert.Equal(42, result);
        Assert.Empty(stack);
    }

    [Fact]
    public void CallFunc_ScriptCall_ReturnsStringThroughGlobalPool()
    {
        var sim = Room();
        sim.ScriptCalls.Register(new AcsScriptCallBinding
        {
            ClassName = "Probe",
            FunctionName = "Label",
            ReturnType = AcsScriptValueKind.String,
            Invoke = _ => new AcsScriptCallOutcome(StringResult: "hello"),
        });

        var stack = new List<int> { 0, 1 };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new(), ["Probe", "Label"], globals, AcsCallFunctions.ScriptCall, 2, out var result));
        Assert.Equal("hello", globals.Get(result));
    }

    [Fact]
    public void CallFunc_ScriptCall_CoercesBoolAndFixedReturn()
    {
        var sim = Room();
        sim.ScriptCalls.Register(new AcsScriptCallBinding
        {
            ClassName = "Probe",
            FunctionName = "Scale",
            ParameterTypes = [AcsScriptValueKind.Bool],
            ReturnType = AcsScriptValueKind.Float64,
            Invoke = invocation => new(DoubleResult: invocation.Arguments[0].IntValue != 0 ? 1.5 : 0),
        });

        var stack = new List<int> { 0, 1, 1 };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new(), ["Probe", "Scale"], globals, AcsCallFunctions.ScriptCall, 3, out var result));
        Assert.Equal(AcsFixedPoint.FromDouble(1.5), result);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
