using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class SoftwareStrengthBlendTests
{
    [Fact]
    public void DefaultStrengthTintUsesTruncatedNativeAlpha()
    {
        var player = new PlayerPawn(); player.GivePowerStrength();
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        SoftwarePowerupBlend.Apply(frame, player.GetStrengthBlend());
        Assert.Equal(new byte[] { 93, 22, 15 }, frame.Pixels);
    }

    [Fact]
    public void TransparentTintLeavesPixelsUnchanged()
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        SoftwarePowerupBlend.Apply(frame, 0x00ff0000u); Assert.Equal(new byte[] { 40, 30, 20 }, frame.Pixels);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HostRendersStrengthBeforePainAndPoisonWithoutChangingSave(bool hardware)
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
        else SoftwareView.Draw(host.Simulation, expected);
        player.GivePowerStrength(); ActorDamage.Apply(player, 20); player.ApplyPoison(32);
        SoftwarePlayerBlend.Apply(expected, player);
        var saved = SimSavegame.Write(host.Simulation); host.Tick(default);
        Assert.Equal(expected.Pixels, host.Frame.Pixels); Assert.Equal(saved, SimSavegame.Write(host.Simulation));
    }
}
