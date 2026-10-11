using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FullVolumeSoundFlagTests
{
    [Theory]
    [InlineData("fullvolactive", ActorSoundType.Active)]
    [InlineData("Actor.FullVolSee", ActorSoundType.See)]
    [InlineData("FULLVOLDEATH", ActorSoundType.Death)]
    public void ScriptFlagChangesSoundAttenuationAndSurvivesSave(string flag, ActorSoundType type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = sim.Actors.Single();
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [type] = "sound" };
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var initial)); Assert.False(initial);
        Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        var bytes = SimSavegame.Write(sim);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false));
        SimSavegame.Apply(sim, bytes); actor = sim.Actors.Single();
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var restored)); Assert.True(restored);
        Emit(actor, type); Assert.Equal(0, Assert.Single(sim.DrainSoundRequests()).Attenuation);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false));
        Emit(actor, type); Assert.Equal(type == ActorSoundType.Active ? 1.001f : 1,
            Assert.Single(sim.DrainSoundRequests()).Attenuation);
    }

    private static void Emit(Actor actor, ActorSoundType type)
    {
        switch (type)
        {
            case ActorSoundType.Active: ActorSoundActions.PlayActiveSound(actor); break;
            case ActorSoundType.See: ActorSoundActions.SeeSound(actor); break;
            case ActorSoundType.Death: ActorSoundActions.Scream(actor); break;
        }
    }
}
