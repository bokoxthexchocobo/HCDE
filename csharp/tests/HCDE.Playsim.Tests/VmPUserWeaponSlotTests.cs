namespace HCDE.Playsim.Tests;

public class VmPUserWeaponSlotTests
{
    [Fact]
    public void HasWeaponsInSlot_FindsInventory()
    {
        var weapons = new[] { "a", null, "b" };
        var owned = new HashSet<object?> { "b" };
        Assert.True(VmPUserWeaponSlot.HasWeaponsInSlot(
            weapons.Length,
            i => weapons[i],
            w => w != null && owned.Contains(w)));
    }

    [Fact]
    public void HasWeaponsInSlot_EmptySlot()
    {
        Assert.False(VmPUserWeaponSlot.HasWeaponsInSlot(2, _ => null, _ => false));
    }

    [Fact]
    public void GetNextPlayerNumber_SkipsBotsWhenRequested()
    {
        var slots = new (bool InGame, bool IsBot)[]
        {
            (true, false),
            (true, true),
            (true, false),
        };
        Assert.Equal(2, VmPUserWeaponSlot.GetNextPlayerNumber(0, true, 3, i => slots[i]));
    }

    [Fact]
    public void GetNextPlayerNumber_ReturnsNegativeWhenExhausted() =>
        Assert.Equal(-1, VmPUserWeaponSlot.GetNextPlayerNumber(1, false, 2, _ => (false, false)));

    [Fact]
    public void GetNextPlayerIndex_FromNullStartsAtZero()
    {
        var slots = new[] { (true, false), (true, false) };
        Assert.Equal(0, VmPUserWeaponSlot.GetNextPlayerIndex(null, false, 2, i => slots[i]));
    }
}
