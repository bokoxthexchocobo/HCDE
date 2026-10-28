namespace HCDE.Playsim;

/// <summary>
/// ACS P-code enum tail, script constant enums, and fixed-point conversion head from
/// <c>p_acs.cpp</c> (**451–600**). Exclusive to chunk **971**.
/// </summary>
public static class VmPAcsHeadConstants971Slice
{
    public const int PcodeCommandCount = 385;

    public const int PcdDivscriptarray = 369;
    public const int PcdModscriptarray = 370;
    public const int PcdIncscriptarray = 371;
    public const int PcdDecscriptarray = 372;
    public const int PcdAndscriptarray = 373;
    public const int PcdEorscriptarray = 374;
    public const int PcdOrscriptarray = 375;
    public const int PcdLsscriptarray = 376;
    public const int PcdRsscriptarray = 377;
    public const int PcdPrintscriptchararray = 378;
    public const int PcdPrintscriptchrange = 379;
    public const int PcdStrcpytoscriptchrange = 380;
    public const int PcdLspec5Ex = 381;
    public const int PcdLspec5Exresult = 382;
    public const int PcdTranslationrange4 = 383;
    public const int PcdTranslationrange5 = 384;

    public const int EnumTailOpcodeMin = PcdDivscriptarray;
    public const int EnumTailOpcodeMax = PcdTranslationrange5;

    public const int LineFront = 0;
    public const int LineBack = 1;
    public const int SideFront = 0;
    public const int SideBack = 1;
    public const int TextureTop = 0;
    public const int TextureMiddle = 1;
    public const int TextureBottom = 2;

    public const int GameSinglePlayer = 0;
    public const int GameNetCooperative = 1;
    public const int GameNetDeathmatch = 2;
    public const int GameTitleMap = 3;

    public const int ClassFighter = 0;
    public const int ClassCleric = 1;
    public const int ClassMage = 2;

    public const int SkillVeryEasy = 0;
    public const int SkillEasy = 1;
    public const int SkillNormal = 2;
    public const int SkillHard = 3;
    public const int SkillVeryHard = 4;

    public const int BlockNothing = 0;
    public const int BlockCreatures = 1;
    public const int BlockEverything = 2;
    public const int BlockRailing = 3;
    public const int BlockPlayers = 4;

    public const int LevelinfoParTime = 0;
    public const int LevelinfoClusternum = 1;
    public const int LevelinfoLevelnum = 2;
    public const int LevelinfoTotalSecrets = 3;
    public const int LevelinfoFoundSecrets = 4;
    public const int LevelinfoTotalItems = 5;
    public const int LevelinfoFoundItems = 6;
    public const int LevelinfoTotalMonsters = 7;
    public const int LevelinfoKilledMonsters = 8;
    public const int LevelinfoSuckTime = 9;

    public const int PlayerinfoTeam = 0;
    public const int PlayerinfoAimdist = 1;
    public const int PlayerinfoColor = 2;
    public const int PlayerinfoGender = 3;
    public const int PlayerinfoNeverswitch = 4;
    public const int PlayerinfoMovebob = 5;
    public const int PlayerinfoStillbob = 6;
    public const int PlayerinfoPlayerclass = 7;
    public const int PlayerinfoFov = 8;
    public const int PlayerinfoDesiredfov = 9;
    public const int PlayerinfoFviewbob = 10;

    public const int StackSize = 4096;

    public const uint HudmsgLog = 0x80000000;
    public const uint HudmsgColorstring = 0x40000000;
    public const uint HudmsgAddblend = 0x20000000;
    public const uint HudmsgAlpha = 0x10000000;
    public const uint HudmsgNowrap = 0x08000000;
    public const int HudmsgLayerShift = 12;
    public const uint HudmsgLayerMask = 0x0000F000;
    public const int HudmsgVisibilityShift = 16;
    public const uint HudmsgVisibilityMask = 0x00070000;

    public const int FhfNorandompuffz = 1;
    public const int FhfNoimpactdecal = 2;

    public const int SdfAbsangle = 1;
    public const int SdfPermanent = 2;
    public const int SdfFixedZoff = 4;
    public const int SdfFixedDistance = 8;

    public const int ArmorinfoClassname = 0;
    public const int ArmorinfoSaveamount = 1;
    public const int ArmorinfoSavepercent = 2;
    public const int ArmorinfoMaxabsorb = 3;
    public const int ArmorinfoMaxfullabsorb = 4;
    public const int ArmorinfoActualsaveamount = 5;

    public const int PickafForcetid = 1;
    public const int PickafReturntid = 2;

    public const double AcsFixedScale = 65536.0;

    public static bool IsEnumTailOpcode(int opcode) =>
        opcode >= EnumTailOpcodeMin && opcode <= EnumTailOpcodeMax;

    public static bool IsScriptArrayOpcode(int opcode) =>
        opcode >= PcdDivscriptarray && opcode <= PcdRsscriptarray;

    public static double AcsToDouble(int acsVal) => acsVal / AcsFixedScale;

    public static float AcsToFloat(int acsVal) => (float)(acsVal / AcsFixedScale);

    public static int DoubleToAcs(double val) =>
        (int)Math.Round(val * AcsFixedScale);

    public static bool ValidPcodeOpcode(int opcode) =>
        opcode >= 0 && opcode < PcodeCommandCount;
}
