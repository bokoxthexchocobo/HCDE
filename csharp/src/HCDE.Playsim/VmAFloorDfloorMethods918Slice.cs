namespace HCDE.Playsim;

/// <summary>
/// `DFloor::SetFloorChangeType`, `DFloor::StartFloorSound`, and `DFloor::Construct` from <c>mapthinkers/a_floor.cpp</c> (**233–273**).
/// Exclusive to chunk **918**; pairs with <see cref="VmAFloorCreate"/> for <c>CreateFloor</c> (**281–514**).
/// </summary>
public static class VmAFloorDfloorMethods918Slice
{
    public const int GenFloorChg0 = 26;
    public const int GenFloorChgT = 27;
    public const int GenFloorChg = 28;

    public static void SetFloorChangeType(VmAFloorCreate.FloorThinker floor, VmAFloorCreate.FloorSector model, int change)
    {
        floor.Texture = model.FloorTexture;
        switch (change & 3)
        {
            case 1:
                floor.NewSpecial = new VmAFloorTickElevator.SectorSpecial();
                floor.Type = GenFloorChg0;
                break;
            case 2:
                floor.Type = GenFloorChg;
                break;
            case 3:
                floor.NewSpecial = new VmAFloorTickElevator.SectorSpecial { Tag = model.Special.Tag };
                floor.Type = GenFloorChgT;
                break;
        }
    }

    public static void DfloorStartFloorSound(
        VmAFloorCreate.FloorThinker floor,
        Action<VmAFloorCreate.FloorSector>? startStaticFloorSound) =>
        startStaticFloorSound?.Invoke(floor.Sector);

    public static void ConstructThinker(
        VmAFloorCreate.FloorSector sec,
        Action<VmAFloorCreate.FloorSector>? superConstruct) =>
        superConstruct?.Invoke(sec);
}
