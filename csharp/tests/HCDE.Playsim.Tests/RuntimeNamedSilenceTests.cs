using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RuntimeNamedSilenceTests
{
    public static IEnumerable<object[]> Cases => Enumerable.Range(0, 6)
        .SelectMany(route => new[] { "None", "none", "NONE" }.Select(name => new object[] { route, name }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void NativeNoneSoundDoesNotQueuePlaybackOrNotifyListeners(int route, string name)
    {
        var sim = Room(); var actor = sim.Players.Single(); var notifications = 0;
        sim.SoundRequestsReady += () => notifications++;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> {
            [ActorSoundType.See] = name, [ActorSoundType.Death] = name, [ActorSoundType.Active] = name,
        };
        switch (route)
        {
            case 0: ActorSoundActions.StartSound(actor, name); break;
            case 1: ActorSoundActions.StartGlobalSound(sim, name); break;
            case 2: ActorSoundActions.StartSoundAt(sim, 1, 2, 3, name); break;
            case 3: ActorSoundActions.SeeSound(actor); break;
            case 4: ActorSoundActions.Scream(actor); break;
            case 5: ActorSoundActions.PlayActiveSound(actor); break;
        }
        Assert.Empty(sim.DrainSoundRequests()); Assert.Equal(0, notifications);
    }

    [Fact]
    public void DetachedSilentPlaybackDoesNotRequireWorldButStillValidatesArguments()
    {
        ActorSoundActions.StartSound(new Actor(), "None");
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSoundActions.StartSound(new Actor(), "None", pitch: float.NaN));
    }

    [Fact]
    public void SoundControlsAreNotSuppressedAsSilentPlayback()
    {
        var sim = Room(); var actor = sim.Players.Single(); var notifications = 0;
        sim.SoundRequestsReady += () => notifications++;
        ActorSoundActions.StopSound(actor); ActorSoundActions.SoundVolume(actor, 4, 0.5f);
        ActorSoundActions.SoundPitch(actor, 4, 0.75f, "None");
        var requests = sim.DrainSoundRequests(); Assert.Equal(3, requests.Length); Assert.Equal(3, notifications);
        Assert.True(requests[0].Stop); Assert.True(requests[1].ChangeVolume); Assert.True(requests[2].ChangePitch);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
    });
}
