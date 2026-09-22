using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client;

public sealed class ClientHost
{
    public const uint HardwareWall = 0x00B42828;

    public ClientHost(AuthoritySimulation simulation, int width = 32, int height = 32)
    {
        Simulation = simulation;
        Frame = new SoftwareFrame(width, height);
        Audio = new AudioMixer();
        ZScript = new ZScriptVm();
        Hardware = new RecordedHardwareDevice();
    }

    public AuthoritySimulation Simulation { get; }
    public SoftwareFrame Frame { get; }
    public AudioMixer Audio { get; }
    public ZScriptVm ZScript { get; }
    public RecordedHardwareDevice Hardware { get; }
    public bool UseHardware { get; set; }
    public short[] LastMix { get; private set; } = Array.Empty<short>();

    public static ClientHost DemoRoom(int width = 32, int height = 32)
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
            Lines = new[]
            {
                new LevelLine
                {
                    X1 = 64,
                    Y1 = 0,
                    X2 = 64,
                    Y2 = 128,
                    SideBack = LevelLine.NoSide,
                },
            },
        };
        return new ClientHost(AuthoritySimulation.Start(level), width, height);
    }

    public void Tick(PlayerCommand command)
    {
        var player = Simulation.Players.FirstOrDefault();
        if (player != null)
            Simulation.QueueCommand(player.PlayerNum, command);
        Simulation.Tick();
        ZScript.Tick(Simulation);
        if (UseHardware)
        {
            Frame.Clear(0, 0, 0);
            Hardware.BeginFrame();
            Hardware.DrawQuad(0, Frame.Height / 4, Frame.Width, Frame.Height / 2, HardwareWall);
            Hardware.Blit(Frame);
        }
        else
        {
            SoftwareView.Draw(Simulation, Frame);
        }

        Audio.Play(new short[] { 1000, 1000, 1000, 1000 });
        LastMix = Audio.Mix(4);
    }
}
