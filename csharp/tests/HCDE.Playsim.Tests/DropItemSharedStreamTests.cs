using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropItemSharedStreamTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(1, true)]
    public void ProbabilityAndTossShareNativeStreamWithNoCombatDraws(int style, bool noToss)
    {
        var sim = Room(noToss); sim.DropStyle = style;
        var state = NativeStateRandom.Seed(42, 0x2d1fda00u);
        uint Draw() => NativeStateRandom.Next(ref state) & 255;
        var player = sim.Players.Single(); player.Z = Fixed.FromInt(16);
        var combat = sim.CombatRandomState;
        Draw();
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, player, "Clip", 0, 256));
        var item = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        var mask = style == 2 ? 7u : 255u;
        var divisor = style == 2 ? 1.0 : 256.0;
        var vx = noToss ? 0 : ((int)(Draw() & mask) - (int)(Draw() & mask)) / divisor;
        var vy = noToss ? 0 : ((int)(Draw() & mask) - (int)(Draw() & mask)) / divisor;
        var vz = noToss || style == 2 ? 0 : 5 + Draw() / 64.0;
        Assert.Equal(Fixed.FromDouble(vx), item.VelocityX);
        Assert.Equal(Fixed.FromDouble(vy), item.VelocityY);
        Assert.Equal(Fixed.FromDouble(vz), item.VelocityZ);
        Assert.Equal(Fixed.FromDouble(16 + (noToss ? 0 : style == 2 ? 24 : player.Height.ToDouble() / 2)), item.Z);
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(Draw(), sim.NextDropItemByte());
        var saved = SimSavegame.Write(sim); var next = sim.NextDropItemByte();
        SimSavegame.Apply(sim, saved); Assert.Equal(next, sim.NextDropItemByte());
    }

    private static AuthoritySimulation Room(bool noToss) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] },
        rngSeed: 42, compat: noToss ? CompatSurface.NoTossDrops : CompatSurface.None);
}
