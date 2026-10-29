namespace HCDE.Playsim.Tests;

public class VmAKeys1186SliceTests
{
    static VmAKeys1186Slice.KeyClass Red() => new() { Name = "RedCard", TypeName = "RedCard" };

    [Fact]
    public void OneKey_MatchesTheCarrierThePickupAndTheInventory()
    {
        var key = new VmAKeys1186Slice.OneKey { Key = Red(), Count = 1 };
        var carrier = new VmAKeys1186Slice.Actor { ClassName = "RedCard" };
        Assert.True(key.Check(carrier));

        var species = new VmAKeys1186Slice.Actor { ClassName = "Other", SpeciesName = "RedCard" };
        Assert.True(key.Check(species));

        var speciesFieldOnly = new VmAKeys1186Slice.Actor { ClassName = "Other", Species = "RedCard" };
        Assert.False(key.Check(speciesFieldOnly));

        var pickup = new VmAKeys1186Slice.Actor
        {
            ClassName = "DehackedPickup",
            DehackedPickup = true,
            Replacee = "RedCard",
        };
        Assert.True(key.Check(pickup));

        var pickupSpecies = new VmAKeys1186Slice.Actor
        {
            ClassName = "DehackedPickup",
            DehackedPickup = true,
            Species = "RedCard",
        };
        Assert.True(key.Check(pickupSpecies));

        var held = new VmAKeys1186Slice.Actor { ClassName = "RedCard" };
        var pocket = new VmAKeys1186Slice.Actor { Inventory = held };
        Assert.True(key.Check(pocket));

        var heldSpecies = new VmAKeys1186Slice.Actor { SpeciesName = "RedCard" };
        var pocketSpecies = new VmAKeys1186Slice.Actor { Inventory = heldSpecies };
        Assert.True(key.Check(pocketSpecies));

        Assert.False(key.Check(new VmAKeys1186Slice.Actor { ClassName = "BlueCard" }));
    }

    [Fact]
    public void Locks_RequireEveryGroupUnlessTheListIsEmpty()
    {
        var red = new VmAKeys1186Slice.OneKey { Key = Red() };
        var blue = new VmAKeys1186Slice.OneKey { Key = new VmAKeys1186Slice.KeyClass { Name = "BlueCard", TypeName = "BlueCard" } };
        var any = new VmAKeys1186Slice.KeyGroup();
        any.AnyKeyList.Add(red);
        any.AnyKeyList.Add(blue);
        var owner = new VmAKeys1186Slice.Actor { ClassName = "BlueCard" };
        Assert.True(any.Check(owner));
        Assert.False(new VmAKeys1186Slice.KeyGroup().Check(owner));

        var second = new VmAKeys1186Slice.KeyGroup();
        second.AnyKeyList.Add(red);
        var both = new VmAKeys1186Slice.Lock();
        both.KeyList.Add(any);
        both.KeyList.Add(second);
        Assert.False(both.Check(owner));

        var onlyBlue = new VmAKeys1186Slice.Lock();
        onlyBlue.KeyList.Add(any);
        Assert.True(onlyBlue.Check(owner));

        var open = new VmAKeys1186Slice.Lock();
        var junk = new VmAKeys1186Slice.Actor { Inventory = new VmAKeys1186Slice.Actor() };
        Assert.False(open.Check(junk));
        junk.Inventory!.IsKey = true;
        Assert.True(open.Check(junk));

        var doll = new VmAKeys1186Slice.Actor
        {
            Player = new VmAKeys1186Slice.PlayerPawn
            {
                Mo = new VmAKeys1186Slice.Actor { Inventory = new VmAKeys1186Slice.Actor { IsKey = true } },
            },
        };
        Assert.True(open.Check(doll));
    }

    [Fact]
    public void Keywords_ListTheLockdefWordsAndStartUninitialized()
    {
        Assert.Equal(
            new string?[] { "ANY", "MESSAGE", "REMOTEMESSAGE", "MAPCOLOR", "LOCKEDSOUND", null },
            VmAKeys1186Slice.LockKeywords);
        Assert.False(VmAKeys1186Slice.KeysDone);
        Assert.Empty(VmAKeys1186Slice.KeyTypes);
        Assert.Equal(0, new VmAKeys1186Slice.Lock().Rgb);
    }
}
