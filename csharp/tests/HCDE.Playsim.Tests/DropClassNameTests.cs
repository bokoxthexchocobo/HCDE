using System.Globalization;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropClassNameTests
{
    [Fact]
    public void AllSupportedDropNamesAcceptUpperAndLowerCase()
    {
        string[] names = ["Clip", "ClipBox", "Shell", "ShellBox", "RocketAmmo", "RocketBox", "Cell",
            "CellPack", "Stimpack", "Medikit", "HealthBonus", "Soulsphere", "Megasphere", "GreenArmor",
            "BlueArmor", "ArmorBonus", "Backpack", "BlueCard", "BlueSkull", "RedCard", "RedSkull",
            "YellowCard", "YellowSkull", "Chainsaw", "Shotgun", "SuperShotgun", "Chaingun",
            "RocketLauncher", "PlasmaRifle", "BFG9000"];
        foreach (var name in names)
        {
            Assert.True(PickupCatalog.TryEditorNumberForDropName(name, out var expected));
            Assert.True(PickupCatalog.TryEditorNumberForDropName(name.ToLowerInvariant(), out var lower));
            Assert.True(PickupCatalog.TryEditorNumberForDropName(name.ToUpperInvariant(), out var upper));
            Assert.Equal(expected, lower); Assert.Equal(expected, upper);
        }
    }

    [Theory]
    [InlineData("bLuEsKuLl", PickupCatalog.BlueSkull)]
    [InlineData("cLiP", PickupCatalog.Clip)]
    [InlineData("bLuEaRmOr", PickupCatalog.MegaArmor)]
    public void MixedCaseNamesSpawnRequestedClassThroughAcsDrop(string name, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, sim.Players.Single(), name, 0, 256));
        Assert.Equal(expected, Assert.Single(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum)).DoomEdNum);
    }

    [Fact]
    public void LookupIsIndependentOfCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.True(PickupCatalog.TryEditorNumberForDropName("clip", out var type));
            Assert.Equal(PickupCatalog.Clip, type);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UnknownPickup")]
    [InlineData("Cl ip")]
    [InlineData("ClipSuffix")]
    public void InvalidClassNamesRemainRejected(string? name)
    {
        Assert.False(PickupCatalog.TryEditorNumberForDropName(name, out var type));
        Assert.Equal(0, type);
    }

    [Theory]
    [InlineData("Bullet")]
    [InlineData("AmmoClip")]
    [InlineData("Bullets")]
    [InlineData("BulletBox")]
    [InlineData("Shells")]
    [InlineData("Rocket")]
    [InlineData("Cells")]
    [InlineData("Armor")]
    [InlineData("MegaArmor")]
    [InlineData("Plasma")]
    [InlineData("BFG")]
    [InlineData(" Clip")]
    [InlineData("Clip ")]
    public void ShorthandAndPaddedNamesDoNotSpawnCatalogPickups(string name)
    {
        Assert.False(PickupCatalog.TryEditorNumberForDropName(name, out var type));
        Assert.Equal(0, type);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        Assert.Equal(0, ActorDropItem.Drop(sim, 0, sim.Players.Single(), name, 0, 256));
        Assert.DoesNotContain(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum));
    }
}
