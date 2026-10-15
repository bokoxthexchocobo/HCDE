using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsSupportedClassTests
{
    [Theory]
    [InlineData("DoomImp")]
    [InlineData("DoomPlayer")]
    [InlineData("RadSuit")]
    [InlineData("Berserk")]
    [InlineData("PowerIronFeet")]
    [InlineData("PowerMask")]
    [InlineData("PowerStrength")]
    [InlineData("Shotgun")]
    [InlineData("MedPatch")]
    [InlineData("SurgeryKit")]
    [InlineData("ArtiHealth")]
    [InlineData("ArtiSuperHealth")]
    public void SupportedClassesExistForModuleAndPooledNames(string name)
    {
        var sim = Room(); var strings = new AcsGlobalStrings();
        foreach (var pooled in new[] { false, true })
        {
            var stack = new List<int> { 91, pooled ? strings.Add(name.ToLowerInvariant()) : 0 };
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), [name], strings,
                AcsCallFunctions.CheckClass, 1, out var result));
            Assert.Equal(1, result); Assert.Equal(new[] { 91 }, stack);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("UnimplementedCustomActor")]
    public void UnknownNamesDoNotExist(string name)
    {
        var stack = new List<int> { 0 }; Assert.True(AcsCallFunctions.TryInvoke(Room(), stack, new(), [name], new(),
            AcsCallFunctions.CheckClass, 1, out var result)); Assert.Equal(0, result);
    }

    [Fact]
    public void InvalidPoolIndexDoesNotExist()
    {
        var stack = new List<int> { AcsStringIds.StringPoolMarker + 500 };
        Assert.True(AcsCallFunctions.TryInvoke(Room(), stack, new(), [], new(), AcsCallFunctions.CheckClass, 1, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
