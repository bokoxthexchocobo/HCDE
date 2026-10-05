using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamagePowerArchiveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Fact]
    public void TimersAndDamageFlagsRoundTripAndExpire()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.PowerDamageTics = 2; player.PowerProtectionTics = 3; player.PierceArmor = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(51, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        var loaded = restored.Players.Single();
        Assert.Equal(2, loaded.PowerDamageTics); Assert.Equal(3, loaded.PowerProtectionTics);
        Assert.True(loaded.PierceArmor);
        Assert.Equal(80, ActorDamage.Apply(new Actor { Health = 100 }, 20, loaded).HealthLost);
        Assert.Equal(5, ActorDamage.Apply(loaded, 20).HealthLost);
        loaded.Tick(); loaded.Tick(); loaded.Tick();
        Assert.Equal(0, loaded.PowerDamageTics); Assert.Equal(0, loaded.PowerProtectionTics);
    }

    [Fact]
    public void PoseRestoresAndLegacyClearsTimers()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        var player = sim.Players.Single(); player.GivePowerDamage(); player.GivePowerProtection();
        var pose = sim.CaptureState(); player.PowerDamageTics = player.PowerProtectionTics = 0;
        sim.RestoreState(pose);
        Assert.Equal(875, player.PowerDamageTics); Assert.Equal(875, player.PowerProtectionTics);
        Assert.True(SimSavegame.TryRead(bytes, out var old, out var error), error);
        sim.RestoreState(old);
        Assert.Equal(0, player.PowerDamageTics); Assert.Equal(0, player.PowerProtectionTics);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void NegativeTimerIsRejected()
    {
        var sim = Room(); sim.Players.Single().GivePowerDamage();
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-damage-power-timer", error);
    }
}
