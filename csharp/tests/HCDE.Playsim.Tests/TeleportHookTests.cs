using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportHookTests
{
    [Theory]
    [InlineData(70, 0, 131)]
    [InlineData(70, 1, 129)]
    [InlineData(154, 0, 11)]
    [InlineData(154, 1, 9)]
    public void HooksReceiveNativeDestinationAngleAndFlags(int special, int noSourceFog, int flags)
    {
        var sim = Room(); var player = sim.Players.Single(); var calls = new List<string>();
        player.PreTeleportFunction = (actor, destination, angle, supplied) =>
        {
            Assert.Equal(0, actor.X.Raw); Assert.Equal((200.0, 0.0, 0.0), destination);
            Assert.Equal(90, angle); Assert.Equal(flags, supplied); calls.Add("pre"); return true;
        };
        player.PostTeleportFunction = (actor, destination, angle, supplied) =>
        {
            Assert.Equal(200, actor.X.ToDouble()); Assert.Equal((200.0, 0.0, 0.0), destination);
            Assert.Equal(90, angle); Assert.Equal(flags, supplied); calls.Add("post");
        };
        Assert.True(LineSpecials.ExecuteTeleportSpecial(sim, special, 0, 7, player, false, noSourceFog));
        Assert.Equal(new[] { "pre", "post" }, calls);
    }

    [Fact]
    public void PreHookCancellationPreventsTelefragAndMoverMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(200, 0);
        var health = target.Health; var post = false;
        player.VelocityX = Fixed.FromInt(3); player.ReactionTime = 7;
        player.PreTeleportFunction = (_, _, _, _) => false;
        player.PostTeleportFunction = (_, _, _, _) => post = true;
        Assert.False(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.Equal(health, target.Health); Assert.Equal(0, player.X.Raw);
        Assert.Equal(Fixed.FromInt(3), player.VelocityX); Assert.Equal(7, player.ReactionTime); Assert.False(post);
    }

    [Fact]
    public void BlockedTeleportCallsPreButNotPost()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.AddBot(200, 0).NoTelefrag = true;
        var pre = false; var post = false;
        player.PreTeleportFunction = (_, _, _, _) => { pre = true; return true; };
        player.PostTeleportFunction = (_, _, _, _) => post = true;
        Assert.False(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.True(pre); Assert.False(post);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void NoSourceFogArgumentReachesHooksThroughEveryManagedSpecialPath(int path)
    {
        var sim = Room(); var player = sim.Players.Single(); var flags = -1;
        player.PreTeleportFunction = (_, _, _, value) => { flags = value; return true; };
        if (path == 0) Assert.True(ActorSpecialActions.CallSpecial(player, 70, 0, 7, 1));
        else if (path == 1)
            Assert.True(LineSpecials.ActivateMapLine(sim, player,
                new LevelLine { Special = 70, Arg1 = 7, Arg2 = 1, PlayerCross = true }, false, false));
        else
        {
            int[] words = path == 2 ? [11, 70, 0, 7, 1, 1] : [3, 0, 3, 7, 3, 1, 6, 70, 1];
            var code = new byte[words.Length * 4];
            for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
            sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
            Assert.True(sim.Acs.Enqueue(1, [], player)); sim.Acs.Tick(sim);
        }
        Assert.Equal(129, flags);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200, Angle = 90 }],
    });
}
