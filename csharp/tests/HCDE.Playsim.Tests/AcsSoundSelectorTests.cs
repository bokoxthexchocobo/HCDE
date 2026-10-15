using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsSoundSelectorTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(12, false)]
    [InlineData(99, false)]
    public void ActorSoundUsesNativeSelectorRangeEvenWhenMetadataExists(int selector, bool plays)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.ActorSounds = new Dictionary<ActorSoundType, string> { [(ActorSoundType)selector] = "custom/sound" };
        Invoke(sim, AcsCallFunctions.PlayActorSound, [0, selector], []);
        Assert.Equal(plays ? 1 : 0, sim.PendingSoundRequests.Count);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("none")]
    [InlineData("")]
    public void SilentNamesDoNotQueueDirectOrActorPlayback(string name)
    {
        var sim = Room(); sim.Players.Single().ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = name };
        Invoke(sim, AcsCallFunctions.PlaySound, [0, 0], [name]);
        Invoke(sim, AcsCallFunctions.PlayActorSound, [0, 0], []);
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void RipMetadataRemainsAvailableToNonAcsConsumers()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        Invoke(sim, AcsCallFunctions.PlayActorSound, [0, (int)ActorSoundType.Rip], []);
        Assert.Equal("custom/rip", player.ActorSounds[ActorSoundType.Rip]); Assert.Empty(sim.PendingSoundRequests);
    }

    private static void Invoke(AuthoritySimulation sim, int function, int[] args, string[] module)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() }, module, new(),
            function, args.Length, out var result)); Assert.Equal(0, result); Assert.Equal(new[] { 77 }, stack);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
