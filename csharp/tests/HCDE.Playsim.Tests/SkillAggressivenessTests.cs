using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkillAggressivenessTests
{
    [Theory]
    [InlineData(3004, 0, 200)]
    [InlineData(3004, 0.5, 100)]
    [InlineData(3004, 1, 0)]
    [InlineData(3004, -2, 200)]
    [InlineData(3004, 2, 0)]
    [InlineData(16, 0.5, 80)]
    [InlineData(16, 0.333, 106)]
    public void NativeMissileCapUsesInvertedClampedSkillSetting(int type, double setting, int cap)
    {
        var (sim, actor) = Room(setting); var (reference, _) = Room(setting);
        for (var i = 0; i < 64; i++)
        {
            var expected = reference.NextCheckMissileByte() >= cap;
            actor.Brain = MonsterBrain.ForType(type); actor.SetTargetPointer(sim.Players.Single());
            actor.ReactionTime = 0; actor.MoveCount = 0; actor.JustAttacked = false;
            actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
                useMelee: false, nativeMovement: true);
            Assert.Equal(expected, actor.Brain.WindupTics > 0);
        }
        Assert.Equal(Math.Clamp(setting, 0, 1), sim.SkillAggressiveness);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteSettingsReject(double setting)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Room(setting));

    [Fact]
    public void NormalizedSettingParticipatesInChecksum()
    {
        var (ordinary, _) = Room(0); var (aggressive, _) = Room(1); var (clamped, _) = Room(2);
        Assert.NotEqual(ordinary.Checksum, aggressive.Checksum);
        Assert.Equal(aggressive.Checksum, clamped.Checksum);
    }

    [Fact]
    public void FreshSimulationLoadWithSamePolicyReplaysAutomaticDecisions()
    {
        var (sim, actor) = Room(0.5); actor.Brain = MonsterBrain.ForType(3004);
        actor.SetTargetPointer(sim.Players.Single()); sim.Tick();
        var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 5; i++) sim.Tick();
        var expected = SimSavegame.Write(sim, true);
        var (restored, _) = Room(0.5); SimSavegame.Apply(restored, saved);
        for (var i = 0; i < 5; i++) restored.Tick();
        Assert.Equal(expected, SimSavegame.Write(restored, true));
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation, Actor) Room(double setting)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 2000 }]
        }, spawnOptions: new SpawnOptions(NativeMonsterMovement: true, SkillAggressiveness: setting));
        var actor = sim.AddBot(0, 0); actor.Brain = MonsterBrain.ForType(3004);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
