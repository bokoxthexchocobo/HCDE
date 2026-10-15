using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsSectorDamageRulesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageTypeUsesModuleOrPooledName(bool pooled)
    {
        var sim = Room(); var strings = new AcsGlobalStrings(); var id = pooled ? strings.Add("CustomAcid") : 0;
        Apply(sim, [7, 5, id], ["CustomAcid"], strings);
        Assert.Equal("CustomAcid", sim.Level.Sectors[0].DamageType); Assert.Equal(0, sim.Level.Sectors[1].DamageAmount);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(50)]
    [InlineData(65536)]
    public void OmittedOptionsHaveFixedNativeDefaultsAndAmountDoesNotNarrow(int amount)
    {
        var sim = Room(); Apply(sim, [7, amount]); var sector = sim.Level.Sectors[0];
        Assert.Equal(amount, sector.DamageAmount); Assert.Equal("None", sector.DamageType);
        Assert.Equal(32, sector.DamageInterval); Assert.Equal(0, sector.Leakiness);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(32768, -32768)]
    public void ExplicitIntervalClampsBeforeNativeShortAssignment(int interval, int expected)
    {
        var sim = Room(); Apply(sim, [7, 5, 0, interval, 65538]);
        Assert.Equal(expected, sim.Level.Sectors[0].DamageInterval); Assert.Equal(2, sim.Level.Sectors[0].Leakiness);
    }

    [Fact]
    public void ScriptConfiguredFireDamageAffectsGameplayAndReplaysAfterSave()
    {
        var sim = Room(); Apply(sim, [7, 5, 0, 1], ["Fire"]); var saved = SimSavegame.Write(sim, true);
        sim.Tick(); Assert.Equal(95, sim.Players.Single().Health); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Apply(AuthoritySimulation sim, int[] args, string[]? module = null, AcsGlobalStrings? strings = null)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), module ?? [], strings ?? new(),
            AcsCallFunctions.SetSectorDamage, args.Length, out var result));
        Assert.Equal(0, result); Assert.Equal(new[] { 77 }, stack);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Lines = GameplayFoundationTests.TwoRooms(0, 128).Level.Lines,
        Sides = GameplayFoundationTests.TwoRooms(0, 128).Level.Sides,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { Tag = 8, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = -40 }]
    });
}
