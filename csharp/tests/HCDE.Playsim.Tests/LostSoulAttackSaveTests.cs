using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LostSoulAttackSaveTests
{
    [Theory]
    [InlineData(5, false)]
    [InlineData(15, true)]
    public void SaveResumesWindupAndChargeWithoutRestart(int calls, bool charging)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 100 }, new LevelThing { Type = 3006 }] });
        var skull = sim.Actors.Single(a => a.DoomEdNum == 3006); skull.ReactionTime = 0;
        for (var i = 0; i < calls; i++) ActorChaseActions.ExtChase(skull, false, true, playActive: false);
        Assert.Equal(charging, skull.Brain!.Charging);
        var saved = skull.Brain.CaptureLostSoulAttack(); Assert.NotNull(saved);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(143, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(skull, false, true, playActive: false);
        Assert.True(skull.Brain.Charging);
        var expected = skull.Brain.CaptureLostSoulAttack();
        var velocity = (skull.VelocityX, skull.VelocityY, skull.VelocityZ);
        skull.Brain.StopCharge(skull);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(saved, skull.Brain.CaptureLostSoulAttack());
        Assert.Equal(charging, skull.Brain.Charging);
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(skull, false, true, playActive: false);
        Assert.Equal(expected, skull.Brain.CaptureLostSoulAttack());
        Assert.True(skull.Brain.Charging);
        Assert.Equal(velocity, (skull.VelocityX, skull.VelocityY, skull.VelocityZ));
        skull.Brain.StopCharge(skull);
        Assert.Null(skull.Brain.CaptureLostSoulAttack());
        Assert.False(skull.Brain.Charging);
        Assert.Equal(0, skull.VelocityX.Raw);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var index = sim.CaptureState().Actors.FindIndex(a => a.Id == skull.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + index * 24 + 4), 11);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-lostsoulattack-state", error);
    }
}
