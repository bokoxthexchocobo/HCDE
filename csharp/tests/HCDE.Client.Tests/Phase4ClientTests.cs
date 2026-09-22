using System.Buffers.Binary;
using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class Phase4ClientTests
{
    [Fact]
    public void SoftwareView_PaintsTheWallInFrontAndCeilingAboveIt()
    {
        var host = ClientHost.DemoRoom();
        SoftwareView.Draw(host.Simulation, host.Frame);
        var mid = host.Frame.Width / 2;
        host.Frame.Get(mid, 0, out var cr, out var cg, out var cb);
        Assert.Equal(SoftwareView.CeilingR, cr);
        Assert.Equal(SoftwareView.CeilingG, cg);
        Assert.Equal(SoftwareView.CeilingB, cb);
        host.Frame.Get(mid, host.Frame.Height / 2, out var wr, out var wg, out var wb);
        Assert.Equal(SoftwareView.WallR, wr);
        Assert.Equal(SoftwareView.WallG, wg);
        Assert.Equal(SoftwareView.WallB, wb);
    }

    [Fact]
    public void SoftwareView_IgnoresAWallBehindThePlayer()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
            Lines = new[]
            {
                new LevelLine { X1 = 0, Y1 = 0, X2 = 0, Y2 = 128, SideBack = LevelLine.NoSide },
            },
        };
        var frame = new SoftwareFrame(32, 32);
        SoftwareView.Draw(AuthoritySimulation.Start(level), frame);
        frame.Get(16, 16, out var r, out var g, out var b);
        Assert.Equal(SoftwareView.FloorR, r);
        Assert.Equal(SoftwareView.FloorG, g);
        Assert.Equal(SoftwareView.FloorB, b);
    }

    [Fact]
    public void HardwarePath_BlitsTheRecordedQuadAndDoesNotBindVulkan()
    {
        var host = ClientHost.DemoRoom();
        host.UseHardware = true;
        host.Tick(default);
        Assert.False(host.Hardware.IsNative);
        Assert.Equal("recorded", host.Hardware.BackendName);
        var quad = Assert.Single(host.Hardware.Quads);
        Assert.Equal(ClientHost.HardwareWall, quad.Argb);
        host.Frame.Get(host.Frame.Width / 2, host.Frame.Height / 2, out var r, out var g, out var b);
        Assert.Equal(0xB4, r);
        Assert.Equal(0x28, g);
        Assert.Equal(0x28, b);
        Assert.False(NativeProbes.Vulkan().Bound);
        Assert.Throws<InvalidOperationException>(() => host.Hardware.DrawQuad(0, 0, 1, 1, 0));
    }

    [Fact]
    public void ZScript_DamageNativeLowersHealthAndUnknownOpcodeStops()
    {
        var host = ClientHost.DemoRoom();
        host.ZScript.Add(new ZsProgram { Name = "hurt", Code = Code(ZsOpcode.LoadImm, 10, ZsOpcode.CallNative, ZsNative.DamagePlayer, ZsOpcode.Return) });
        Assert.True(host.ZScript.Start("hurt"));
        host.Tick(default);
        Assert.Equal(90, host.Simulation.Players.Single().Health);
        Assert.Equal(0, host.ZScript.RunningCount);

        host.ZScript.Add(new ZsProgram { Name = "bad", Code = new byte[] { 255 } });
        Assert.True(host.ZScript.Start("bad"));
        host.ZScript.Tick(host.Simulation);
        Assert.Equal(0, host.ZScript.RunningCount);
    }

    [Fact]
    public void ZScript_AddUsesTheStack()
    {
        var vm = new ZScriptVm();
        vm.Add(new ZsProgram
        {
            Name = "sum",
            Code = Code(ZsOpcode.LoadImm, 2, ZsOpcode.LoadImm, 3, ZsOpcode.Add, ZsOpcode.CallNative, ZsNative.Log, ZsOpcode.Return),
        });
        Assert.True(vm.Start("sum"));
        vm.Tick(AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" }));
        Assert.Equal("5", Assert.Single(vm.Log));
        Assert.False(vm.Start("missing"));
    }

    [Fact]
    public void ZScript_JumpIfZeroSkipsDamageAndLogsHealth()
    {
        var code = Code(
            ZsOpcode.LoadImm, 0,
            ZsOpcode.JumpIfZero, 21,
            ZsOpcode.LoadImm, 50,
            ZsOpcode.CallNative, ZsNative.DamagePlayer,
            ZsOpcode.Return,
            ZsOpcode.CallNative, ZsNative.PlayerHealth,
            ZsOpcode.CallNative, ZsNative.Log,
            ZsOpcode.Return);
        Assert.Equal((byte)ZsOpcode.CallNative, code[21]);

        var host = ClientHost.DemoRoom();
        host.ZScript.Add(new ZsProgram { Name = "skip", Code = code });
        Assert.True(host.ZScript.Start("skip"));
        host.ZScript.Tick(host.Simulation);
        Assert.Equal(100, host.Simulation.Players.Single().Health);
        Assert.Equal("100", Assert.Single(host.ZScript.Log));
        Assert.Equal(0, host.ZScript.RunningCount);
    }

    [Fact]
    public void ZScript_UnknownNativeStopsAndTheBudgetLeavesALoopRunning()
    {
        var vm = new ZScriptVm();
        vm.Add(new ZsProgram { Name = "badnative", Code = Code(ZsOpcode.CallNative, 99) });
        Assert.True(vm.Start("badnative"));
        vm.Tick(AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" }));
        Assert.Equal(0, vm.RunningCount);

        vm.Add(new ZsProgram
        {
            Name = "spin",
            Code = Code(ZsOpcode.Nop, ZsOpcode.LoadImm, 0, ZsOpcode.JumpIfZero, 0),
        });
        Assert.True(vm.Start("spin"));
        vm.Tick(AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" }));
        Assert.Equal(1, vm.RunningCount);
    }

    [Fact]
    public void Audio_SumsChannelsClampsAndMuteSilences()
    {
        var mixer = new AudioMixer();
        mixer.Play(new short[] { 20000, -20000 });
        mixer.Play(new short[] { 20000, -20000 });
        var mixed = mixer.Mix(2);
        Assert.Equal(short.MaxValue, mixed[0]);
        Assert.Equal(short.MinValue, mixed[1]);

        mixer.Muted = true;
        mixer.Play(new short[] { 1000 });
        Assert.All(mixer.Mix(2), sample => Assert.Equal(0, sample));
        Assert.False(NativeProbes.ZMusic().Bound);
    }

    [Fact]
    public void Launcher_RejectsABadSkillAndBuildsArgs()
    {
        var bad = new LauncherSelection { IwadPath = "doom2.wad", Skill = 0 };
        Assert.False(LauncherPlan.TryValidate(bad, out var error));
        Assert.Equal("skill-range", error);

        var good = new LauncherSelection { IwadPath = "doom2.wad", MapName = "MAP07", Skill = 4, SoftwareRenderer = false, Width = 640, Height = 480 };
        var args = LauncherPlan.BuildArgs(good);
        Assert.Contains("--map", args);
        Assert.Contains("MAP07", args);
        Assert.Contains("hardware", args);
        Assert.True(LauncherPlan.TryParse(args, out var parsed, out var parseError), parseError);
        Assert.Equal("doom2.wad", parsed.IwadPath);
        Assert.Equal(4, parsed.Skill);
        Assert.False(parsed.SoftwareRenderer);
    }

    [Fact]
    public void NativeProbes_DoNotBindJitVulkanOrZMusic()
    {
        Assert.False(NativeProbes.ZScriptJit().Bound);
        Assert.False(NativeProbes.Vulkan().Bound);
        Assert.False(NativeProbes.ZMusic().Bound);
    }

    [Fact]
    public void ClientTick_AdvancesTheSimDrawsAndMixes()
    {
        var host = ClientHost.DemoRoom();
        host.Tick(new PlayerCommand { ForwardMove = 8192 });
        Assert.Equal(1, host.Simulation.Thinkers.Clock.Tic);
        host.Frame.Get(16, 16, out var r, out _, out _);
        Assert.Equal(SoftwareView.WallR, r);
        Assert.Equal(1000, host.LastMix[0]);
    }

    [Fact]
    public void SelfTestMain_PrintsTheWallCenter()
    {
        var previous = Console.Out;
        using var buffer = new StringWriter();
        Console.SetOut(buffer);
        try
        {
            Assert.Equal(0, Program.Main(new[] { "--self-test" }));
        }
        finally
        {
            Console.SetOut(previous);
        }

        Assert.Contains("center=180,40,40", buffer.ToString(), StringComparison.Ordinal);
    }

    private static byte[] Code(params object[] parts)
    {
        using var stream = new MemoryStream();
        foreach (var part in parts)
        {
            switch (part)
            {
                case ZsOpcode opcode:
                    stream.WriteByte((byte)opcode);
                    break;
                case int value:
                    var word = new byte[4];
                    BinaryPrimitives.WriteInt32LittleEndian(word, value);
                    stream.Write(word);
                    break;
                default:
                    throw new InvalidOperationException(part.GetType().Name);
            }
        }

        return stream.ToArray();
    }
}
