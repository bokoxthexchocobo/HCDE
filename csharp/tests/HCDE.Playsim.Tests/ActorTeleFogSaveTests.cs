using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTeleFogSaveTests
{
    [Theory]
    [InlineData("TeleportFog", "ItemFog")]
    [InlineData(null, "TeleportFog")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void OverridesAndExplicitClearsRoundTrip(string? source, string? destination)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetTeleFog(actor, source, destination);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetTeleFog(actor, "Changed", "Changed"); sim.RestoreState(state);
        Assert.Equal(source, actor.TeleFogSource); Assert.Equal(destination, actor.TeleFogDest);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void AcsSwapAndDirectNamesAreCaptured()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.TeleFogSource = "TeleportFog"; actor.TeleFogDest = "ItemFog";
        Assert.Equal(1, AcsActorTeleFog.Swap(sim, 0, actor));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.TeleFogSource = actor.TeleFogDest = null; sim.RestoreState(state);
        Assert.Equal("ItemFog", actor.TeleFogSource); Assert.Equal("TeleportFog", actor.TeleFogDest);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 74)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    [InlineData(12, int.MaxValue)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var bytes = Saved(); var start = Start(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-telefog-", error);
    }

    [Fact]
    public void InvalidUtf8NameIsRejected()
    {
        var bytes = Saved(); bytes[Start(bytes) + 16] = 0xff;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-telefog-value", error);
    }

    [Fact]
    public void AbsentOverridesRestoreDefaultNames()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.TeleFogSource = "TeleportFog"; sim.RestoreState(state);
        Assert.Null(actor.TeleFogSource);
        Assert.Null(actor.TeleFogDest);
        Assert.False(actor.HasTeleFogOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BaselineRestoreClearsLaterAcsOverridesAndSwapUsesRestoredNames()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(1, AcsActorTeleFog.Set(sim, 0, actor, "TeleportFog", "ItemFog"));
        SimSavegame.Apply(sim, bytes);
        Assert.Null(actor.TeleFogSource); Assert.Null(actor.TeleFogDest);
        Assert.False(actor.HasTeleFogOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(1, AcsActorTeleFog.Swap(sim, 0, actor));
        Assert.Null(actor.TeleFogSource); Assert.Null(actor.TeleFogDest);
        Assert.True(actor.HasTeleFogOverride);
    }

    [Fact]
    public void MixedSavedActorsRestoreEachFogOverrideIndependently()
    {
        var sim = Room(); var baseline = sim.AddBot(0, 0);
        var overridden = sim.AddBot(100, 0); var cleared = sim.AddBot(200, 0);
        ActorPropertyActions.SetTeleFog(overridden, "TeleportFog", "ItemFog");
        ActorPropertyActions.SetTeleFog(cleared, null, null);
        var bytes = SimSavegame.Write(sim);
        foreach (var actor in sim.Actors)
            ActorPropertyActions.SetTeleFog(actor, "LaterSource", "LaterDestination");
        SimSavegame.Apply(sim, bytes);
        Assert.Null(baseline.TeleFogSource); Assert.Null(baseline.TeleFogDest);
        Assert.False(baseline.HasTeleFogOverride);
        Assert.Equal("TeleportFog", overridden.TeleFogSource);
        Assert.Equal("ItemFog", overridden.TeleFogDest);
        Assert.True(overridden.HasTeleFogOverride);
        Assert.Null(cleared.TeleFogSource); Assert.Null(cleared.TeleFogDest);
        Assert.True(cleared.HasTeleFogOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static int Start(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static byte[] Saved()
    { var sim = Room(); ActorPropertyActions.SetTeleFog(sim.AddBot(0, 0), "TeleportFog", null); return SimSavegame.Write(sim); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
