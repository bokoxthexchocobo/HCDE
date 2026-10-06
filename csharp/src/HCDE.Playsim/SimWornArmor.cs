namespace HCDE.Playsim;

public readonly record struct SimWornArmor(int Maximum, int ActualSaveAmount, int SavePercent,
    int MaxAbsorb, int MaxFullAbsorb, int AbsorbCount)
{
    internal static SimWornArmor Default => new(1, 0, 0, 0, 0, 0);
    internal static SimWornArmor? Capture(PlayerInventory inventory)
    {
        var value = new SimWornArmor(inventory.ArmorMaximum, inventory.ArmorActualSaveAmount,
            inventory.ArmorSavePercent, inventory.MaxAbsorb, inventory.MaxFullAbsorb, inventory.AbsorbCount);
        return value == Default ? null : value;
    }
    internal void Restore(PlayerInventory inventory)
    {
        inventory.ArmorMaximum = Maximum; inventory.ArmorActualSaveAmount = ActualSaveAmount;
        inventory.ArmorSavePercent = SavePercent; inventory.MaxAbsorb = MaxAbsorb;
        inventory.MaxFullAbsorb = MaxFullAbsorb; inventory.AbsorbCount = AbsorbCount;
    }
}
