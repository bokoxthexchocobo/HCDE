using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileAttackSightTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AttackFacesBeforeSightGateAndSuppressesDamageAndLaunchWhenBlocked(bool blocked, bool fireExists)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, Y = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Lines = blocked ? [new LevelLine { X1 = -100, X2 = 100, Y1 = 150, Y2 = 150, SideBack = -1 }] : [],
        }, rngSeed: 42);
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        target.VelocityZ = Fixed.FromInt(-3);
        var archvile = sim.AddBot(0, 0, 64); archvile.Brain = null;
        archvile.Angle = BamAngle.FromDegrees(180); archvile.PitchDegrees = 25; archvile.Ambush = true;
        ArchvileActions.Attack(sim, archvile, target, fireExists);
        Assert.Equal(BamAngle.FromDegrees(90), archvile.Angle);
        Assert.False(archvile.Ambush); Assert.Equal(25, archvile.PitchDegrees);
        Assert.Equal(blocked ? 1000 : fireExists ? 918 : 980, target.Health);
        Assert.Equal(blocked ? -3 : 10, target.VelocityZ.ToDouble());
        Assert.Equal(blocked ? (uint?)null : archvile.Id, target.LastDamageSourceId);
        var sounds = sim.DrainSoundRequests().Where(s => s.Sound == "vile/stop").ToArray();
        Assert.Equal(blocked ? 0 : 1, sounds.Length);
        if (!blocked)
        {
            Assert.Equal(archvile.Id, sounds[0].ActorId);
            Assert.Equal(1, sounds[0].Channel);
            Assert.Equal(0, sounds[0].Flags);
            Assert.Equal(1, sounds[0].Volume);
            Assert.Equal(1, sounds[0].Attenuation);
        }
    }

    [Theory]
    [InlineData("custom/blast")]
    [InlineData("")]
    public void AttackUsesSuppliedSoundWithoutChangingDamage(string sound)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        var target = sim.Players.Single(); target.NoPain = true;
        var archvile = sim.AddBot(0, 0, 64); archvile.Brain = null;
        ArchvileActions.Attack(sim, archvile, target, false, sound: sound);
        var requests = sim.DrainSoundRequests();
        if (sound.Length == 0) Assert.Empty(requests);
        else Assert.Equal(sound, Assert.Single(requests).Sound);
        Assert.Equal(80, target.Health);
    }
}
