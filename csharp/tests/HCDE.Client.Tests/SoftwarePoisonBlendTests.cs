using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class SoftwarePoisonBlendTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HostPaletteSettingChangesPoisonTintInEitherRenderer(bool hardware)
    {
        using var host = ClientHost.DemoRoom(); host.GamePaused = true; host.UseHardware = hardware;
        host.Simulation.Players.Single().ApplyPoison(64);
        host.Tick(default); var continuous = host.Frame.Pixels.ToArray();
        var saved = SimSavegame.Write(host.Simulation);
        host.ApplyViewPreferences(ClientViewPreferences.Read(new ClientViewPreferences { PoisonPaletteFlash = true }.Write()));
        host.Tick(default);
        Assert.NotEqual(continuous, host.Frame.Pixels);
        host.Frame.Get(0, 0, out var r, out var g, out var b);
        if (hardware) Assert.Equal(new byte[] { 35, 73, 28 }, new byte[] { r, g, b });
        else Assert.Equal(new byte[] { 43, 81, 44 }, new byte[] { r, g, b });
        Assert.True(host.CaptureViewPreferences().PoisonPaletteFlash);
        Assert.Equal(saved, SimSavegame.Write(host.Simulation));
        host.PoisonPaletteFlash = false; host.Tick(default); Assert.Equal(continuous, host.Frame.Pixels);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void InvalidCounterIsRejectedBeforeChangingPixels(int count)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePoisonBlend.Apply(frame, count));
        Assert.Equal(new byte[] { 40, 30, 20 }, frame.Pixels);
    }
    [Theory]
    [InlineData(0, false, 0, 0, 0)]
    [InlineData(32, false, 3, 22, 0)]
    [InlineData(64, false, 7, 44, 0)]
    [InlineData(100, false, 7, 44, 0)]
    [InlineData(8, true, 4, 9, 3)]
    [InlineData(64, true, 35, 73, 28)]
    public void TintMatchesNativeContinuousAndPaletteBranches(int count, bool palette, int r, int g, int b)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(0, 0, 0);
        SoftwarePoisonBlend.Apply(frame, count, palette);
        Assert.Equal(new byte[] { (byte)r, (byte)g, (byte)b }, frame.Pixels);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClientDrawsPoisonAfterPainWithoutChangingSave(bool hardware)
    {
        using var host = ClientHost.DemoRoom(); host.GamePaused = true; host.UseHardware = hardware;
        var player = host.Simulation.Players.Single(); player.NoPain = true;
        var expected = new SoftwareFrame(host.Frame.Width, host.Frame.Height);
        if (hardware)
        {
            expected.Clear(0, 0, 0); var device = new RecordedHardwareDevice(); device.BeginFrame();
            device.DrawQuad(0, expected.Height / 4, expected.Width, expected.Height / 2, ClientHost.HardwareWall);
            device.Blit(expected);
        }
        else
        {
            SoftwareView.Draw(host.Simulation, expected);
        }
        ActorDamage.Apply(player, 20); player.ApplyPoison(32);
        SoftwarePlayerBlend.Apply(expected, player);
        var saved = SimSavegame.Write(host.Simulation); host.Tick(default);
        Assert.Equal(expected.Pixels, host.Frame.Pixels); Assert.Equal(saved, SimSavegame.Write(host.Simulation));
    }
}
