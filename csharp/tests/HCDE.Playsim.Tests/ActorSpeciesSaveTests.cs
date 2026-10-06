using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpeciesSaveTests
{
    [Theory]
    [InlineData("Squad")]
    [InlineData("Equipe")]
    [InlineData(null)]
    public void OverridesAndExplicitClearsRoundTrip(string? name)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 58);
        ActorPropertyActions.SetSpecies(actor, "Previous");
        ActorPropertyActions.SetSpecies(actor, name);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetSpecies(actor, "Changed"); sim.RestoreState(state);
        Assert.Equal(name, actor.Species);
        Assert.Equal(2, ActorJumpActions.CheckSpecies(actor, 2, name ?? "Demon"));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DirectNameRestoresSpeciesImmunity()
    {
        var sim = Room(); var first = sim.AddBot(0, 0, 58); var second = sim.AddBot(100, 0, 3001);
        first.Species = "Squad"; second.Species = "squad";
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        first.Species = second.Species = null; sim.RestoreState(state);
        Assert.True(first.IsSameSpecies(second)); Assert.True(first.ProjectileImmune(second));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 76)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    [InlineData(12, int.MaxValue)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var bytes = Saved();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(Start(bytes) + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-species-", error);
    }

    [Fact]
    public void InvalidUtf8NameIsRejected()
    {
        var bytes = Saved(); bytes[Start(bytes) + 16] = 0xff;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-species-value", error);
    }

    [Fact]
    public void LegacyWithoutOverridesRestoresDefaultSpecies()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Species = "Squad"; sim.RestoreState(state); Assert.Null(actor.Species);
    }

    private static int Start(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static byte[] Saved()
    { var sim = Room(); ActorPropertyActions.SetSpecies(sim.AddBot(0, 0), "Squad"); return SimSavegame.Write(sim); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
