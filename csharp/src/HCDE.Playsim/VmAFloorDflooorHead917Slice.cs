namespace HCDE.Playsim;

/// <summary>
/// `StartFloorSound`, `DFloor::Serialize` / field defs, and `DFloor::Tick` head through stairlock promote from <c>mapthinkers/a_floor.cpp</c> (**1–200**).
/// Exclusive to chunk **917**; defers `SetFloorChangeType` (**233+**) and `CreateFloor` (**281+**).
/// </summary>
public static class VmAFloorDflooorHead917Slice
{
    public const uint SecfSilentmove = 1u << 14;
    public const int ChanFloor = 2;
    public const int SeqPlatform = 3;
    public const int BuildStair = 23;
    public const int WaitStair = 24;
    public const int ResetStair = 25;
    public const int DonutRaise = 22;
    public const int GenFloorChg0 = 26;
    public const int GenFloorChgT = 27;
    public const int GenFloorChg = 28;
    public const int FloorLowerAndChange = 15;
    public const int StairlockBuilding = -2;
    public const int StairlockDone = -1;

    public sealed class FloorSector
    {
        public uint Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public object? FloorData { get; set; }
        public int Stairlock { get; set; }
        public int Prevsec { get; set; } = -1;
        public int Nextsec { get; set; } = -1;
        public Action<SectorSpecial>? SetSpecial { get; set; }
        public Action<string>? SetFloorTexture { get; set; }
    }

    public sealed class SectorSpecial
    {
        public int Tag { get; set; }
    }

    public sealed class FloorThinker
    {
        public required FloorSector Sector { get; init; }
        public int Type { get; set; }
        public int Crush { get; set; }
        public int Direction { get; set; }
        public SectorSpecial NewSpecial { get; set; } = new();
        public string Texture { get; set; } = "";
        public double FloorDestDist { get; set; }
        public double Speed { get; set; }
        public int ResetCount { get; set; }
        public double OrgDist { get; set; }
        public int Delay { get; set; }
        public int PauseTime { get; set; }
        public int StepTime { get; set; }
        public int PerStepTime { get; set; }
        public bool Hexencrush { get; set; }
        public bool Instant { get; set; }
        public bool StopInterpolationCalled { get; set; }
    }

    public readonly record struct FloorSerializeBundle(
        int Type,
        int Crush,
        int Direction,
        SectorSpecial NewSpecial,
        string Texture,
        double FloorDestDist,
        double Speed,
        int ResetCount,
        double OrgDist,
        int Delay,
        int PauseTime,
        int StepTime,
        int PerStepTime,
        bool Hexencrush,
        bool Instant);

    public static FloorSerializeBundle CaptureSerializeFields(FloorThinker floor) =>
        new(
            floor.Type,
            floor.Crush,
            floor.Direction,
            floor.NewSpecial,
            floor.Texture,
            floor.FloorDestDist,
            floor.Speed,
            floor.ResetCount,
            floor.OrgDist,
            floor.Delay,
            floor.PauseTime,
            floor.StepTime,
            floor.PerStepTime,
            floor.Hexencrush,
            floor.Instant);

    public static void ApplySerializeFields(FloorThinker floor, FloorSerializeBundle bundle)
    {
        floor.Type = bundle.Type;
        floor.Crush = bundle.Crush;
        floor.Direction = bundle.Direction;
        floor.NewSpecial = bundle.NewSpecial;
        floor.Texture = bundle.Texture;
        floor.FloorDestDist = bundle.FloorDestDist;
        floor.Speed = bundle.Speed;
        floor.ResetCount = bundle.ResetCount;
        floor.OrgDist = bundle.OrgDist;
        floor.Delay = bundle.Delay;
        floor.PauseTime = bundle.PauseTime;
        floor.StepTime = bundle.StepTime;
        floor.PerStepTime = bundle.PerStepTime;
        floor.Hexencrush = bundle.Hexencrush;
        floor.Instant = bundle.Instant;
    }

    public static IReadOnlyList<string> DfloorDefinedFieldNames =>
    [
        "m_Type", "m_Crush", "m_Direction", "m_NewSpecial", "m_Texture", "m_FloorDestDist",
        "m_Speed", "m_ResetCount", "m_OrgDist", "m_Delay", "m_PauseTime", "m_StepTime",
        "m_PerStepTime", "m_Hexencrush", "m_Instant",
    ];

    public static bool ShouldSkipFloorSound(FloorSector sec) => (sec.Flags & SecfSilentmove) != 0;

    public static void PlayFloorSound(
        FloorSector sec,
        Action<FloorSector, int, int, int, int>? startByType,
        Action<FloorSector, int, string, int>? startByName,
        Action<FloorSector, int, string, int>? startDefault)
    {
        if (ShouldSkipFloorSound(sec))
            return;
        if (sec.SeqType >= 0)
            startByType?.Invoke(sec, ChanFloor, sec.SeqType, SeqPlatform, 0);
        else if (sec.SeqName.Length > 0)
            startByName?.Invoke(sec, ChanFloor, sec.SeqName, 0);
        else
            startDefault?.Invoke(sec, ChanFloor, "Floor", 0);
    }

    public static bool AdvanceStairTiming(FloorThinker floor)
    {
        if (floor.Type != BuildStair && floor.Type != WaitStair)
            return false;
        if (floor.ResetCount != 0)
        {
            if (--floor.ResetCount == 0)
            {
                floor.Type = ResetStair;
                floor.Direction = floor.Direction > 0 ? -1 : 1;
                floor.FloorDestDist = floor.OrgDist;
            }
        }
        if (floor.PauseTime != 0)
        {
            floor.PauseTime--;
            return true;
        }
        if (floor.StepTime != 0 && --floor.StepTime == 0)
        {
            floor.PauseTime = floor.Delay;
            floor.StepTime = floor.PerStepTime;
        }
        return false;
    }

    public static void ApplyPastDestFloorChange(FloorThinker floor)
    {
        var sec = floor.Sector;
        if (floor.Direction == 1)
        {
            switch (floor.Type)
            {
                case DonutRaise:
                case GenFloorChgT:
                case GenFloorChg0:
                    sec.SetSpecial?.Invoke(floor.NewSpecial);
                    goto case GenFloorChg;
                case GenFloorChg:
                    sec.SetFloorTexture?.Invoke(floor.Texture);
                    break;
            }
        }
        else if (floor.Direction == -1)
        {
            switch (floor.Type)
            {
                case FloorLowerAndChange:
                case GenFloorChgT:
                case GenFloorChg0:
                    sec.SetSpecial?.Invoke(floor.NewSpecial);
                    goto case GenFloorChg;
                case GenFloorChg:
                    sec.SetFloorTexture?.Invoke(floor.Texture);
                    break;
            }
        }
    }

    public static bool PromoteStairlockBuilding(FloorSector sec)
    {
        if (sec.Stairlock != StairlockBuilding)
            return false;
        sec.Stairlock = StairlockDone;
        return true;
    }

    public static void ReleaseStairlockChainIfComplete(
        FloorSector origin,
        Func<int, FloorSector?>? sectorByIndex)
    {
        if (origin.Stairlock != StairlockBuilding)
            return;
        var sec = origin;
        sec.Stairlock = StairlockDone;
        while (sec.Prevsec != -1)
        {
            var prev = sectorByIndex?.Invoke(sec.Prevsec);
            if (prev == null || prev.Stairlock == StairlockBuilding)
                break;
            sec = prev;
        }
        if (sec.Prevsec != -1)
            return;
        sec = origin;
        while (sec.Nextsec != -1)
        {
            var next = sectorByIndex?.Invoke(sec.Nextsec);
            if (next == null || next.Stairlock == StairlockBuilding)
                break;
            sec = next;
        }
        if (sec.Nextsec != -1)
            return;
        while (sec.Prevsec != -1)
        {
            sec.Stairlock = 0;
            var prev = sectorByIndex?.Invoke(sec.Prevsec);
            if (prev == null)
                return;
            sec = prev;
        }
        sec.Stairlock = 0;
    }

    public static bool TickFloorMover(
        FloorThinker floor,
        Func<FloorSector, double, double, int, int, bool, bool, VmADoorTickSound.MoveResult> moveFloor,
        Action<FloorSector>? stopSequence,
        Action<FloorThinker>? stopInterpolation,
        Func<int, FloorSector?>? sectorByIndex = null,
        Action<FloorThinker>? destroy = null)
    {
        if (AdvanceStairTiming(floor))
            return false;
        if (floor.Type == WaitStair)
            return false;

        var res = moveFloor(
            floor.Sector,
            floor.Speed,
            floor.FloorDestDist,
            floor.Crush,
            floor.Direction,
            floor.Hexencrush,
            floor.Instant);

        if (res != VmADoorTickSound.MoveResult.PastDest)
            return false;

        stopSequence?.Invoke(floor.Sector);

        if (floor.Type == BuildStair)
            floor.Type = WaitStair;

        if (floor.Type != WaitStair || floor.ResetCount == 0)
        {
            ApplyPastDestFloorChange(floor);
            floor.Sector.FloorData = null;
            stopInterpolation?.Invoke(floor);
            floor.StopInterpolationCalled = true;
            ReleaseStairlockChainIfComplete(floor.Sector, sectorByIndex);
            destroy?.Invoke(floor);
            return true;
        }

        return false;
    }
}
