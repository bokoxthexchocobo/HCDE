using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialSectorTests
{
    [Theory]
    [InlineData(false, 7)]
    [InlineData(true, 7)]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    public void GravityUsesTagAndClampsFraction(bool death, int tag)
    {
        var sim = Room(tag); var actor = sim.Actors[0]; Configure(actor, 216, tag, 2, 150);
        Run(sim, actor, death);
        Assert.Equal(2.99, sim.Level.Sectors[0].Gravity, 8);
        Assert.Equal(1, sim.Level.Sectors[1].Gravity);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageForwardsIntervalAndLeakiness(bool death)
    {
        var sim = Room(7); var actor = sim.Actors[0]; Configure(actor, 214, 7, 12, 0, 3, 27);
        Run(sim, actor, death);
        var sector = sim.Level.Sectors[0];
        Assert.Equal(12, sector.DamageAmount); Assert.Equal(3, sector.DamageInterval);
        Assert.Equal(27, sector.Leakiness); Assert.Equal("None", sector.DamageType);
        Assert.Equal(0, sim.Level.Sectors[1].DamageAmount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RotationForwardsBothAngles(bool death)
    {
        var sim = Room(7); var actor = sim.Actors[0]; Configure(actor, 185, 7, 90, -45);
        Run(sim, actor, death);
        Assert.Equal(LevelBuilder.TextureAngleFromDegrees(90), sim.Level.Sectors[0].FloorTextureAngle);
        Assert.Equal(LevelBuilder.TextureAngleFromDegrees(-45), sim.Level.Sectors[0].CeilingTextureAngle);
        Assert.Equal(0u, sim.Level.Sectors[1].FloorTextureAngle);
    }

    [Theory]
    [InlineData(185)]
    [InlineData(214)]
    [InlineData(216)]
    public void MissingTagRetainsNativeSuccessAndClearsSpecial(int special)
    {
        var sim = Room(7); var actor = sim.Actors[0]; Configure(actor, special, 99, 12, 0);
        actor.ActivationType = 32;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(0, actor.Special);
        Assert.Equal(1, sim.Level.Sectors[0].Gravity);
        Assert.Equal(0, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(0u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    private static void Run(AuthoritySimulation sim, Actor actor, bool death)
    {
        var special = actor.Special;
        if (death) ActorDamage.Apply(actor, 1000, null);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(death ? 0 : special, actor.Special);
    }

    private static void Configure(Actor actor, int special, params int[] args)
    {
        actor.Special = special; Array.Copy(args, actor.SpecialArgs, args.Length);
    }

    private static AuthoritySimulation Room(int tag) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = tag, CeilingHeight = 128 }, new LevelSector { Tag = 8, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 20 }],
    });
}
