namespace HCDE.Playsim;

/// <summary><c>LineSpecials[]</c> dispatch indices from <c>p_lnspec.cpp</c> (3545–3694).</summary>
public static class VmPLnspecLineSpecialsTable
{
    public const int MaxIndexed = 149;

    public static bool IsNopSlot(int index) =>
        index is 0 or 1 or 5 or 9 or 16 or 48 or 50 or 57 or 58 or 100 or 101 or 102 or 103 or 108 or 118 or 121 or 122 or 123 or 124 or 126 or 141 or 142 or 143 or 144 or 145 or 146 or 147 or 148 or 149;

    public static bool IsPolyobjSlot(int index) =>
        index is 2 or 3 or 4 or 6 or 7 or 8 or 59 or 86 or 87 or 88 or 89 or 90 or 91 or 92 or 93;

    public static bool IsDoorSlot(int index) =>
        index is 10 or 11 or 12 or 13 or 14;

    public static bool IsFloorSlot(int index) =>
        index is 20 or 21 or 22 or 23 or 24 or 25 or 28 or 35 or 36 or 37 or 66 or 67 or 68;

    public static bool IsCeilingSlot(int index) =>
        index is 38 or 40 or 41 or 42 or 43 or 44 or 45 or 47 or 69;

    public static bool IsPlatSlot(int index) =>
        index is 60 or 61 or 62 or 63 or 64 or 65;

    public static bool IsTeleportSlot(int index) =>
        index is 39 or 70 or 71 or 74 or 75 or 76 or 77 or 78;

    public static bool IsAcsSlot(int index) =>
        index is 80 or 81 or 82 or 83 or 84 or 85;

    public static bool IsThingSlot(int index) =>
        index is 17 or 19 or 73 or 79 or 119 or 125 or 127 or 128 or 129 or 130 or 131 or 132 or 133 or 134 or 135 or 136 or 137 or 139;

    public static bool IsLightSlot(int index) =>
        index is 109 or 110 or 111 or 112 or 113 or 114 or 115 or 116 or 117;

    public static bool SlotInRange(int index) => index >= 0 && index <= MaxIndexed;

    public static bool MapsToCallable(int index) => SlotInRange(index) && !IsNopSlot(index);

    public static int NormalizeSpecialIndex(int special) => special < 0 ? 0 : special;

    public static bool IsScrollSlot(int index) => index is 52 or 221 or 223 or 224;

    public static bool IsSectorEnvSlot(int index) =>
        index is 51 or 54 or 98 or 140 or 151 or 157 or 159 or 212 or 213 or 214 or 216 or 218 or 219 or 220;

    public static bool IsStairsSlot(int index) => index is 26 or 27 or 31 or 32 or 204 or 217;

    public static bool IsPillarSlot(int index) => index is 29 or 30 or 94;

    public static bool IsGlassOrForceSlot(int index) => index is 33 or 34 or 49;

    public static bool IsConversationSlot(int index) => index is 18 or 79;

    public static bool IsHealthSlot(int index) => index is 150 or 151;

    public static bool IsFsExecuteSlot(int index) => index is 158;

    public static bool IsCameraSlot(int index) => index is 237;

    public static bool IsElevatorSlot(int index) => index is 245 or 246 or 247;

    public static bool ClassifyFamily(int index) =>
        IsPolyobjSlot(index) || IsDoorSlot(index) || IsFloorSlot(index) || IsCeilingSlot(index) ||
        IsPlatSlot(index) || IsTeleportSlot(index) || IsAcsSlot(index) || IsThingSlot(index) ||
        IsLightSlot(index) || IsScrollSlot(index) || IsSectorEnvSlot(index);

    public static string FamilyName(int index)
    {
        if (IsPolyobjSlot(index)) return "polyobj";
        if (IsDoorSlot(index)) return "door";
        if (IsFloorSlot(index)) return "floor";
        if (IsCeilingSlot(index)) return "ceiling";
        if (IsPlatSlot(index)) return "plat";
        if (IsTeleportSlot(index)) return "teleport";
        if (IsAcsSlot(index)) return "acs";
        if (IsThingSlot(index)) return "thing";
        if (IsLightSlot(index)) return "light";
        if (IsNopSlot(index)) return "nop";
        return "other";
    }
}
