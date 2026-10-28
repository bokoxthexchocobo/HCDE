using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAbsentCallFuncChunk11Tests
{
    [Fact]
    public void CallFunc_SetPointer_AssignsTarget()
    {
        var sim = RoomWithTwoActors();
        var activator = sim.Players.Single();
        var other = sim.Actors.First(a => a.ThingId == 2);
        RunCall(sim, 3, AcsCallFunctions.SetPointer, [AcsActorPointer.Target, 2, AcsActorPointer.Default], activator);
        Assert.Equal(other.Id, activator.TargetId);
    }

    [Fact]
    public void CallFunc_AcsNamedExecute_StartsScriptByName()
    {
        var sim = Room();
        var script = AcsNamedScriptIndex.FromName("OPEN");
        sim.Acs.Add(MakeProgram(script,
            [(int)AcsPcode.PushNumber, 77, (int)AcsPcode.SetResultValue, (int)AcsPcode.Terminate]));
        RunCall(sim, 1, AcsCallFunctions.AcsNamedExecute, [0], strings: ["OPEN"]);
        sim.Acs.Tick(sim);
        Assert.Equal(77, sim.Acs.ScriptResult);
    }

    [Fact]
    public void CallFunc_SetHudClipRect_StoresOnSimulation()
    {
        var sim = Room();
        RunCall(sim, 5, AcsCallFunctions.SetHudClipRect, [1, 2, 3, 4, 5]);
        Assert.Equal(1, sim.AcsHudClipLeft);
        Assert.Equal(5, sim.AcsHudWrapWidth);
    }

    [Fact]
    public void CallFunc_Warp_MovesActivatorToReference()
    {
        var sim = RoomWithTwoActors();
        var activator = sim.Players.Single();
        var dest = sim.Actors.First(a => a.ThingId == 2);
        dest.X = Fixed.FromInt(200);
        dest.Y = Fixed.FromInt(300);
        RunCall(sim, 6, AcsCallFunctions.Warp, [2, 0, 0, 0, 0, 0], activator);
        Assert.Equal(200, activator.X.ToDouble(), precision: 3);
        Assert.Equal(300, activator.Y.ToDouble(), precision: 3);
    }

    [Fact]
    public void CallFunc_SetSectorGlow_WritesSectorFields()
    {
        var sim = Room();
        RunCall(sim, 6, AcsCallFunctions.SetSectorGlow, [7, 0, 10, 20, 30, 65536]);
        Assert.Equal(0x1E140Au, sim.Level.Sectors[0].FloorGlowColor);
        Assert.Equal(1f, sim.Level.Sectors[0].FloorGlowHeight);
    }

    [Fact]
    public void CallFunc_SetFogDensity_ClampedPerSector()
    {
        var sim = Room();
        RunCall(sim, 2, AcsCallFunctions.SetFogDensity, [7, 400]);
        Assert.Equal(200, sim.Level.Sectors[0].FogDensity);
    }

    [Fact]
    public void CallFunc_AnnouncerSound_QueuesUiChannel()
    {
        var sim = Room();
        RunCall(sim, 1, AcsCallFunctions.AnnouncerSound, [0], strings: ["misc/chat"]);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("misc/chat", request.Sound);
        Assert.True(request.Local);
    }

    [Fact]
    public void CallFunc_StartSlideshowAndSubtitle_StorePresentationState()
    {
        var sim = Room();
        RunCall(sim, 1, AcsCallFunctions.StartSlideshow, [0], strings: ["Interpics"]);
        RunCall(sim, 1, AcsCallFunctions.SetSubtitleNumber, [42]);
        Assert.Equal("Interpics", sim.AcsSlideshowName);
        Assert.Equal(42, sim.AcsSubtitleNumber);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static AuthoritySimulation RoomWithTwoActors() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things =
        [
            new LevelThing { Type = 1, X = 64, Y = 64 },
            new LevelThing { Type = 3004, Id = 2, X = 128, Y = 128 },
        ],
    });

    private static AcsProgram MakeProgram(int number, int[] tail)
    {
        var bytes = new byte[tail.Length * 4];
        for (var i = 0; i < tail.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), tail[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }

    private static void RunCall(AuthoritySimulation sim, int argCount, int function, int[] callArgs, Actor? activator = null, string[]? strings = null)
    {
        var words = new List<int>();
        foreach (var arg in callArgs)
            words.AddRange([(int)AcsPcode.PushNumber, arg]);
        words.Add((int)AcsPcode.CallFunc);
        words.Add(argCount);
        words.Add(function);
        words.Add((int)AcsPcode.Terminate);
        var bytes = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = strings ?? [] });
        Assert.True(sim.Acs.TryExecute(1, [], activator ?? sim.Players.Single()));
        sim.Acs.Tick(sim);
    }
}
