using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClipCarrierTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NoclipCarrierDoesNotTransportRider(bool noclip, bool vertical)
    {
        var (sim, carrier, rider) = Room(); carrier.NoClip = noclip;
        Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        var dx = vertical ? 0 : 8; var dz = vertical ? 8 : 0;
        ActorPhysics.CarryStandingRiders(sim, carrier, dx, 0, dz, [rider]);
        Assert.Equal(noclip ? 0 : dx, rider.X.ToDouble());
        Assert.Equal(16 + (noclip ? 0 : dz), rider.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoclipCarrierDoesNotTransmitCrushDamage(bool noclip)
    {
        var (sim, carrier, rider) = Room(); carrier.NoClip = noclip; rider.NoPain = true;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, sim.Thinkers.Clock.Tic, "Crush");
        Assert.Equal(noclip ? 100 : 90, rider.Health);
        Assert.Equal(20, carrier.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SupportRelationDoesNotAliasCarrierMovementPolicy(bool noclip)
    {
        var (_, carrier, rider) = Room(); carrier.NoClip = noclip;
        Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        rider.Z = Fixed.FromInt(20); Assert.False(ActorPhysics.IsStandingOn(carrier, rider));
    }

    [Fact]
    public void SavedCarrierPolicyReplaysCarryAndCrushSuppression()
    {
        var (sim, carrier, rider) = Room(); carrier.NoClip = true; rider.NoPain = true;
        var saved = SimSavegame.Write(sim, true); Apply();
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Apply();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(100, rider.Health);
        void Apply()
        {
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            ActorPhysics.CrushStandingRiders(sim, carrier, 10, sim.Thinkers.Clock.Tic, "Crush");
        }
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var carrier = sim.AddBot(0, 0); carrier.Brain = null; carrier.Height = Fixed.FromInt(16);
        var rider = sim.Players.Single(); rider.Z = Fixed.FromInt(16);
        rider.VelocityX = rider.VelocityY = rider.VelocityZ = default;
        return (sim, carrier, rider);
    }
}
