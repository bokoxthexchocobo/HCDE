using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsHealthActionTests
{
    [Theory]
    [InlineData(false, 248, 20, 70)]
    [InlineData(true, 248, 20, 70)]
    [InlineData(false, 73, 20, 30)]
    [InlineData(true, 73, 20, 30)]
    [InlineData(false, 73, -20, 70)]
    [InlineData(true, 73, -20, 70)]
    public void ScriptSpecialsReachHealthActions(bool stack, int special, int amount, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single(); player.Health = 50;
        int[] words = stack ? [3, amount, 3, 0, 5, special, 1] : [10, special, amount, 0, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], player));
        sim.Acs.Tick(sim);
        Assert.Equal(expected, player.Health);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ZeroDamageKillsAndDamageTypeIsPreserved()
    {
        var player = new PlayerPawn { Health = 50 };
        Assert.True(HealthActions.Execute(73, player, 0, 1000));
        Assert.True(player.IsDead);
        Assert.Equal("Massacre", player.DeathDamageType);
    }

    [Fact]
    public void NonplayerNegativeDamageCanReviveAndCapsAtSpawnHealth()
    {
        var actor = new Actor { Health = -10, ResurrectionHealth = 100 };
        Assert.True(HealthActions.Execute(73, actor, -200, 0));
        Assert.Equal(100, actor.Health);
        Assert.False(HealthActions.Execute(73, null, 10, 0));
    }
}
