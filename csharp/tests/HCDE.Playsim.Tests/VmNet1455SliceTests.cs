using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1455SliceTests
{
    [Fact]
    public void MuteBitsAndList()
    {
        var world = new VmNet1455Slice.World { Console = 1 };
        world.Names[2] = "ada";
        world.Names[3] = "bea";
        VmNet1455Slice.Mute(world);
        Assert.Contains("Usage: mute", Assert.Single(world.Logs));
        world.Logs.Clear();
        VmNet1455Slice.Mute(world, "2");
        Assert.Contains("only be used when playing in multiplayer", Assert.Single(world.Logs));

        world.Multiplayer = true;
        world.Playing[2] = true;
        world.Logs.Clear();
        VmNet1455Slice.Mute(world, "nope", "2", "2", "1", "64");
        Assert.Equal(1UL << 2, world.Muted);
        Assert.Contains(world.Logs, line => line.Contains("Bad player number nope"));
        Assert.Contains(world.Logs, line => line.Contains("Bad player number 64"));
        Assert.Equal("Muted player ada [2]", world.Logs[^1]);

        world.Playing[3] = true;
        world.Playing[0] = true;
        VmNet1455Slice.MuteAll(world);
        Assert.Equal((1UL << 0) | (1UL << 2) | (1UL << 3), world.Muted);
        world.Logs.Clear();
        VmNet1455Slice.ListMuted(world);
        Assert.Equal(new[] { "0. ", "2. ada", "3. bea" }, world.Logs);
        VmNet1455Slice.Unmute(world, "3", "9");
        Assert.Contains("Unmuted player bea [3]", world.Logs);
        Assert.Contains(world.Logs, line => line.Contains("Player 9 is not in game"));
        Assert.Equal(0UL, world.Muted & (1UL << 3));
        VmNet1455Slice.UnmuteAll(world);
        Assert.Equal(0UL, world.Muted);
        world.Logs.Clear();
        VmNet1455Slice.ListMuted(world);
        Assert.Equal("No one currently muted", Assert.Single(world.Logs));
    }

    [Fact]
    public void SettingsControllers()
    {
        var world = new VmNet1455Slice.World { Console = 2, Netgame = true };
        world.Names[2] = "ada";
        world.Names[4] = "bea";
        world.Authority[0] = true;
        world.Clients.AddRange(new[] { 0, 2, 4 });
        world.ClientInGame[2] = true;
        world.ClientInGame[4] = true;
        world.Controller[2] = true;

        VmNet1455Slice.AddControllers(world);
        Assert.Contains("Usage: addsettingscontrollers", Assert.Single(world.Logs));
        world.Logs.Clear();
        VmNet1455Slice.AddControllers(world, "nope", "0", "2", "4");
        Assert.Contains(world.Logs, line => line.Contains("Bad client number nope"));
        Assert.Contains(world.Logs, line => line.Contains("only accessible to the authority"));
        Assert.Empty(world.Writes);

        world.LocalAuthority = true;
        world.Logs.Clear();
        VmNet1455Slice.AddControllers(world, "2", "4", "4");
        Assert.Contains("Client 2 is already a settings controller", world.Logs);
        Assert.Equal(new[] { VmNet1455Slice.AddController, 4 }, world.Writes);

        world.Writes.Clear();
        world.Logs.Clear();
        VmNet1455Slice.RemoveControllers(world, "zz", "4");
        Assert.Contains(world.Logs, line => line.Contains("Bad player number zz"));
        Assert.Contains("Client 4 is already not a settings controller", world.Logs);
        world.Controller[4] = true;
        world.Logs.Clear();
        VmNet1455Slice.RemoveControllers(world, "4");
        Assert.Equal(new[] { VmNet1455Slice.DelController, 4 }, world.Writes);

        world.Controller[2] = true;
        world.Controller[4] = true;
        world.Netgame = false;
        world.Logs.Clear();
        VmNet1455Slice.RemoveAllControllers(world);
        Assert.Contains("only be used when playing in a net game", Assert.Single(world.Logs));
        world.Netgame = true;
        world.Logs.Clear();
        VmNet1455Slice.ListControllers(world);
        Assert.Equal("The following players can change the game settings:", world.Logs[0]);
        Assert.Contains("2. ada [*]", world.Logs);
        Assert.Contains("4. bea", world.Logs);
        Assert.DoesNotContain(world.Logs, line => line.StartsWith("0."));
    }
}
