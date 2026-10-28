namespace HCDE.Playsim;

/// <summary>
/// `DElevator` class head through `OnDestroy` from <c>mapthinkers/a_floor.cpp</c> (**864–940**).
/// Exclusive to chunk **921**; defers `DElevator::Tick` (**941+**, chunk **6**).
/// </summary>
public static class VmAFloorElevatorHead921Slice
{
    public readonly record struct ElevatorSerializeBundle(
        int Type,
        int Direction,
        double FloorDestDist,
        double CeilingDestDist,
        double Speed,
        object? InterpFloor,
        object? InterpCeiling);

    public sealed class ElevatorSector
    {
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public Func<bool, object?>? SetInterpolationFloor { get; set; }
        public Func<bool, object?>? SetInterpolationCeiling { get; set; }
    }

    public sealed class ElevatorThinker
    {
        public required ElevatorSector Sector { get; init; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public double FloorDestDist { get; set; }
        public double CeilingDestDist { get; set; }
        public double Speed { get; set; }
        public object? InterpFloor { get; set; }
        public object? InterpCeiling { get; set; }
        public bool SuperOnDestroyCalled { get; set; }
    }

    public static IReadOnlyList<string> DefinedFieldNames =>
    [
        "m_Type", "m_Direction", "m_FloorDestDist", "m_CeilingDestDist", "m_Speed",
    ];

    public static void ConstructElevator(ElevatorThinker elevator, Action<ElevatorSector>? superConstruct)
    {
        superConstruct?.Invoke(elevator.Sector);
        elevator.Sector.FloorData = elevator;
        elevator.Sector.CeilingData = elevator;
        elevator.InterpFloor = elevator.Sector.SetInterpolationFloor?.Invoke(true);
        elevator.InterpCeiling = elevator.Sector.SetInterpolationCeiling?.Invoke(true);
    }

    public static ElevatorSerializeBundle CaptureSerializeFields(ElevatorThinker elevator) =>
        new(
            elevator.Type,
            elevator.Direction,
            elevator.FloorDestDist,
            elevator.CeilingDestDist,
            elevator.Speed,
            elevator.InterpFloor,
            elevator.InterpCeiling);

    public static void ApplySerializeFields(ElevatorThinker elevator, ElevatorSerializeBundle bundle)
    {
        elevator.Type = bundle.Type;
        elevator.Direction = bundle.Direction;
        elevator.FloorDestDist = bundle.FloorDestDist;
        elevator.CeilingDestDist = bundle.CeilingDestDist;
        elevator.Speed = bundle.Speed;
        elevator.InterpFloor = bundle.InterpFloor;
        elevator.InterpCeiling = bundle.InterpCeiling;
    }

    public static void OnDestroyElevator(ElevatorThinker elevator, Action<object?>? delRef, Action<ElevatorThinker>? superOnDestroy)
    {
        if (elevator.InterpCeiling != null)
        {
            delRef?.Invoke(elevator.InterpCeiling);
            elevator.InterpCeiling = null;
        }
        if (elevator.InterpFloor != null)
        {
            delRef?.Invoke(elevator.InterpFloor);
            elevator.InterpFloor = null;
        }
        superOnDestroy?.Invoke(elevator);
        elevator.SuperOnDestroyCalled = true;
    }
}
