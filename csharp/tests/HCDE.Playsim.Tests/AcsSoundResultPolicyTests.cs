using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsSoundResultPolicyTests
{
    [Theory]
    [InlineData(AcsCallFunctions.PlaySound)]
    [InlineData(AcsCallFunctions.PlayActorSound)]
    [InlineData(AcsCallFunctions.StopSound)]
    [InlineData(AcsCallFunctions.SoundVolume)]
    public void SoundCallsReturnZeroWhileQueuingRequests(int function)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "custom/see" };
        int[] args = function == AcsCallFunctions.SoundVolume ? [0, 4, 32768]
            : function == AcsCallFunctions.PlayActorSound ? [0, (int)ActorSoundType.See] : [0, 0];
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, ["custom/sound"], new(), function, args.Length, out var result));
        Assert.Equal(0, result); Assert.Equal(new[] { 77 }, stack); Assert.Single(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void OptionalLoopAndLocalArgumentsSetNativePlaybackFlags(bool loop, bool local)
    {
        var sim = Room(); var stack = new List<int> { 0, 0, 19, 65536, loop ? 1 : 0, 65536, local ? 1 : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() }, ["custom/sound"], new(),
            AcsCallFunctions.PlaySound, 7, out var result)); Assert.Equal(0, result);
        var request = Assert.Single(sim.PendingSoundRequests); Assert.Equal(3, request.Channel);
        var expected = 16 | (loop ? (int)(SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop) : 0)
            | (local ? (int)SimulationSoundFlags.Local : 0);
        Assert.Equal(expected, request.Flags); Assert.Equal(loop, request.Loop); Assert.Equal(local, request.Local);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
