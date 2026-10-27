namespace HCDE.Playsim;

/// <summary>
/// <c>LineSpecials[]</c> continuation (opcodes **58–207**) from <c>p_lnspec.cpp</c> (**3605–3754**); chunk **935**.
/// </summary>
public static class VmPLnspecLineSpecialDispatch935Slice
{
    public const int DispatchBaseOpcode = 58;
    public const int DispatchEndOpcode = 207;

    public static readonly string[] DispatchOpcodes58Through207 =
    [
        "LS_NOP",
        "LS_Polyobj_OR_MoveToSpot",
        "LS_Plat_PerpetualRaise",
        "LS_Plat_Stop",
        "LS_Plat_DownWaitUpStay",
        "LS_Plat_DownByValue",
        "LS_Plat_UpWaitDownStay",
        "LS_Plat_UpByValue",
        "LS_Floor_LowerInstant",
        "LS_Floor_RaiseInstant",
        "LS_Floor_MoveToValueTimes8",
        "LS_Ceiling_MoveToValueTimes8",
        "LS_Teleport",
        "LS_Teleport_NoFog",
        "LS_ThrustThing",
        "LS_DamageThing",
        "LS_Teleport_NewMap",
        "LS_Teleport_EndGame",
        "LS_TeleportOther",
        "LS_TeleportGroup",
        "LS_TeleportInSector",
        "LS_Thing_SetConversation",
        "LS_ACS_Execute",
        "LS_ACS_Suspend",
        "LS_ACS_Terminate",
        "LS_ACS_LockedExecute",
        "LS_ACS_ExecuteWithResult",
        "LS_ACS_LockedExecuteDoor",
        "LS_Polyobj_MoveToSpot",
        "LS_Polyobj_Stop",
        "LS_Polyobj_MoveTo",
        "LS_Polyobj_OR_MoveTo",
        "LS_Polyobj_OR_RotateLeft",
        "LS_Polyobj_OR_RotateRight",
        "LS_Polyobj_OR_Move",
        "LS_Polyobj_OR_MoveTimes8",
        "LS_Pillar_BuildAndCrush",
        "LS_FloorAndCeiling_LowerByValue",
        "LS_FloorAndCeiling_RaiseByValue",
        "LS_Ceiling_LowerAndCrushDist",
        "LS_Sector_SetTranslucent",
        "LS_Floor_RaiseAndCrushDoom",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_Ceiling_CrushAndRaiseSilentDist",
        "LS_Door_WaitRaise",
        "LS_Door_WaitClose",
        "LS_Line_SetPortalTarget",
        "LS_NOP",
        "LS_Light_ForceLightning",
        "LS_Light_RaiseByValue",
        "LS_Light_LowerByValue",
        "LS_Light_ChangeToValue",
        "LS_Light_Fade",
        "LS_Light_Glow",
        "LS_Light_Flicker",
        "LS_Light_Strobe",
        "LS_Light_Stop",
        "LS_NOP",
        "LS_Thing_Damage",
        "LS_Radius_Quake",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_Thing_Move",
        "LS_NOP",
        "LS_Thing_SetSpecial",
        "LS_ThrustThingZ",
        "LS_UsePuzzleItem",
        "LS_Thing_Activate",
        "LS_Thing_Deactivate",
        "LS_Thing_Remove",
        "LS_Thing_Destroy",
        "LS_Thing_Projectile",
        "LS_Thing_Spawn",
        "LS_Thing_ProjectileGravity",
        "LS_Thing_SpawnNoFog",
        "LS_Floor_Waggle",
        "LS_Thing_SpawnFacing",
        "LS_Sector_ChangeSound",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_Line_SetHealth",
        "LS_Sector_SetHealth",
        "LS_NOP",
        "LS_NOP",
        "LS_Teleport_NoStop",
        "LS_NOP",
        "LS_NOP",
        "LS_SetGlobalFogParameter",
        "LS_FS_Execute",
        "LS_Sector_SetPlaneReflection",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_Ceiling_CrushAndRaiseDist",
        "LS_Generic_Crusher2",
        "LS_Sector_SetCeilingScale2",
        "LS_Sector_SetFloorScale2",
        "LS_Plat_UpNearestWaitDownStay",
        "LS_NoiseAlert",
        "LS_SendToCommunicator",
        "LS_Thing_ProjectileIntercept",
        "LS_Thing_ChangeTID",
        "LS_Thing_Hate",
        "LS_Thing_ProjectileAimed",
        "LS_ChangeSkill",
        "LS_Thing_SetTranslation",
        "LS_NOP",
        "LS_NOP",
        "LS_Line_AlignCeiling",
        "LS_Line_AlignFloor",
        "LS_Sector_SetRotation",
        "LS_Sector_SetCeilingPanning",
        "LS_Sector_SetFloorPanning",
        "LS_Sector_SetCeilingScale",
        "LS_Sector_SetFloorScale",
        "LS_NOP",
        "LS_SetPlayerProperty",
        "LS_Ceiling_LowerToHighestFloor",
        "LS_Ceiling_LowerInstant",
        "LS_Ceiling_RaiseInstant",
        "LS_Ceiling_CrushRaiseAndStayA",
        "LS_Ceiling_CrushAndRaiseA",
        "LS_Ceiling_CrushAndRaiseSilentA",
        "LS_Ceiling_RaiseByValueTimes8",
        "LS_Ceiling_LowerByValueTimes8",
        "LS_Generic_Floor",
        "LS_Generic_Ceiling",
        "LS_Generic_Door",
        "LS_Generic_Lift",
        "LS_Generic_Stairs",
        "LS_Generic_Crusher",
        "LS_Plat_DownWaitUpStayLip",
        "LS_Plat_PerpetualRaiseLip",
    ];

    public static int ContinuedIndex(int opcode) => opcode - DispatchBaseOpcode;

    public static bool ContinuedOpcodeInRange(int opcode) =>
        opcode >= DispatchBaseOpcode && opcode <= DispatchEndOpcode;

    public static string? ContinuedDispatchName(int opcode)
    {
        if (!ContinuedOpcodeInRange(opcode))
            return null;
        return DispatchOpcodes58Through207[ContinuedIndex(opcode)];
    }

    public static bool ContinuedDispatchIsNop(int opcode) =>
        ContinuedDispatchName(opcode) == "LS_NOP";

    public static bool ContinuedDispatchIsPlat(int opcode) => opcode is 60 or 61 or 62 or 63 or 64 or 65 or 172 or 206 or 207;

    public static bool ContinuedDispatchIsTeleport(int opcode) =>
        opcode is 70 or 71 or 74 or 75 or 76 or 77 or 78 or 154;

    public static bool ContinuedDispatchIsAcs(int opcode) => opcode is >= 80 and <= 85;

    public static bool ContinuedDispatchIsLight(int opcode) => opcode is >= 109 and <= 117;

    public static bool ContinuedDispatchIsThingSpawn(int opcode) =>
        (opcode >= 130 && opcode <= 137) || opcode == 139;

    public static int ContinuedDispatchEntryCount() => DispatchOpcodes58Through207.Length;
}
