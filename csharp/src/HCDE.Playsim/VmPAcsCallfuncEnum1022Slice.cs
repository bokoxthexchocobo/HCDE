namespace HCDE.Playsim;

/// <summary>
/// <c>SOUND_*</c> slots, <c>GetActorSound</c>, and <c>EACSFunctions</c> enum head from <c>p_acs.cpp</c> (**4728–4877**).
/// Chunk **1022**; <c>GetActorSound</c> (**4740–4757**) stays on <see cref="VmAcsActorPropertySound"/>.
/// </summary>
public static class VmPAcsCallfuncEnum1022Slice
{
    public const int AcsfGetLineUdmfInt = 1;
    public const int AcsfGetLineUdmfFixed = 2;
    public const int AcsfGetThingUdmfInt = 3;
    public const int AcsfGetThingUdmfFixed = 4;
    public const int AcsfGetSectorUdmfInt = 5;
    public const int AcsfGetSectorUdmfFixed = 6;
    public const int AcsfGetSideUdmfInt = 7;
    public const int AcsfGetSideUdmfFixed = 8;
    public const int AcsfGetActorVelX = 9;
    public const int AcsfGetActorVelY = 10;
    public const int AcsfGetActorVelZ = 11;
    public const int AcsfSetActivator = 12;
    public const int AcsfSetActivatorToTarget = 13;
    public const int AcsfGetActorViewHeight = 14;
    public const int AcsfGetChar = 15;
    public const int AcsfGetAirSupply = 16;
    public const int AcsfSetAirSupply = 17;
    public const int AcsfSetSkyScrollSpeed = 18;
    public const int AcsfGetArmorType = 19;
    public const int AcsfSpawnSpotForced = 20;
    public const int AcsfSpawnSpotFacingForced = 21;
    public const int AcsfCheckActorProperty = 22;
    public const int AcsfSetActorVelocity = 23;
    public const int AcsfSetUserVariable = 24;
    public const int AcsfGetUserVariable = 25;
    public const int AcsfRadiusQuake2 = 26;
    public const int AcsfCheckActorClass = 27;
    public const int AcsfSetUserArray = 28;
    public const int AcsfGetUserArray = 29;
    public const int AcsfSoundSequenceOnActor = 30;
    public const int AcsfSoundSequenceOnSector = 31;
    public const int AcsfSoundSequenceOnPolyobj = 32;
    public const int AcsfGetPolyobjX = 33;
    public const int AcsfGetPolyobjY = 34;
    public const int AcsfCheckSight = 35;
    public const int AcsfSpawnForced = 36;
    public const int AcsfAnnouncerSound = 37;
    public const int AcsfSetPointer = 38;
    public const int AcsfAcsNamedExecute = 39;
    public const int AcsfAcsNamedSuspend = 40;
    public const int AcsfAcsNamedTerminate = 41;
    public const int AcsfAcsNamedLockedExecute = 42;
    public const int AcsfAcsNamedLockedExecuteDoor = 43;
    public const int AcsfAcsNamedExecuteWithResult = 44;
    public const int AcsfAcsNamedExecuteAlways = 45;
    public const int AcsfUniqueTid = 46;
    public const int AcsfIsTidUsed = 47;
    public const int AcsfSqrt = 48;
    public const int AcsfFixedSqrt = 49;
    public const int AcsfVectorLength = 50;
    public const int AcsfSetHudClipRect = 51;
    public const int AcsfSetHudWrapWidth = 52;
    public const int AcsfSetCvar = 53;
    public const int AcsfGetUserCvar = 54;
    public const int AcsfSetUserCvar = 55;
    public const int AcsfGetCvarString = 56;
    public const int AcsfSetCvarString = 57;
    public const int AcsfGetUserCvarString = 58;
    public const int AcsfSetUserCvarString = 59;
    public const int AcsfLineAttack = 60;
    public const int AcsfPlaySound = 61;
    public const int AcsfStopSound = 62;
    public const int AcsfStrcmp = 63;
    public const int AcsfStricmp = 64;
    public const int AcsfStrLeft = 65;
    public const int AcsfStrRight = 66;
    public const int AcsfStrMid = 67;
    public const int AcsfGetActorClass = 68;
    public const int AcsfGetWeapon = 69;
    public const int AcsfSoundVolume = 70;
    public const int AcsfPlayActorSound = 71;
    public const int AcsfSpawnDecal = 72;
    public const int AcsfCheckFont = 73;
    public const int AcsfDropItem = 74;
    public const int AcsfCheckFlag = 75;
    public const int AcsfSetLineActivation = 76;
    public const int AcsfGetLineActivation = 77;
    public const int AcsfGetActorPowerupTics = 78;
    public const int AcsfChangeActorAngle = 79;
    public const int AcsfChangeActorPitch = 80;
    public const int AcsfGetArmorInfo = 81;
    public const int AcsfDropInventory = 82;
    public const int AcsfPickActor = 83;
    public const int AcsfIsPointerEqual = 84;
    public const int AcsfCanRaiseActor = 85;
    public const int AcsfSetActorTeleFog = 86;
    public const int AcsfSwapActorTeleFog = 87;
    public const int AcsfSetActorRoll = 88;
    public const int AcsfChangeActorRoll = 89;
    public const int AcsfGetActorRoll = 90;
    public const int AcsfQuakeEx = 91;
    public const int AcsfWarp = 92;
    public const int AcsfGetMaxInventory = 93;
    public const int AcsfSetSectorDamage = 94;
    public const int AcsfSetSectorTerrain = 95;
    public const int AcsfSpawnParticle = 96;
    public const int AcsfSetMusicVolume = 97;
    public const int AcsfCheckProximity = 98;
    public const int AcsfCheckActorState = 99;

    public const int AcsfCheckClass = 200;
    public const int AcsfDamageActor = 201;
    public const int AcsfSetActorFlag = 202;
    public const int AcsfSetTranslation = 203;
    public const int AcsfGetActorFloorTexture = 204;
    public const int AcsfGetActorFloorTerrain = 205;
    public const int AcsfStrArg = 206;
    public const int AcsfFloor = 207;

    public const int PrimaryCallFunctionMax = AcsfCheckActorState;
    public const int ExtendedCallFunctionMin = AcsfCheckClass;
    public const int ExtendedCallFunctionMax = AcsfFloor;

    public static bool IsPrimaryCallFunctionIndex(int funcIndex) =>
        funcIndex >= AcsfGetLineUdmfInt && funcIndex <= PrimaryCallFunctionMax;

    public static bool IsExtendedCallFunctionIndex(int funcIndex) =>
        funcIndex >= ExtendedCallFunctionMin && funcIndex <= ExtendedCallFunctionMax;

    public static bool IsReservedZandronumGap(int funcIndex) =>
        funcIndex > PrimaryCallFunctionMax && funcIndex < ExtendedCallFunctionMin;

    public static bool UsesHudOrCvarStack(int funcIndex) =>
        funcIndex is AcsfSetHudClipRect or AcsfSetHudWrapWidth
            or AcsfSetCvar or AcsfGetUserCvar or AcsfSetUserCvar
            or AcsfGetCvarString or AcsfSetCvarString
            or AcsfGetUserCvarString or AcsfSetUserCvarString;

    public static bool UsesLineAttackStack(int funcIndex) => funcIndex == AcsfLineAttack;

    public static bool UsesPlayActorSoundPath(int funcIndex) =>
        funcIndex == AcsfPlayActorSound;

    public static int ResolvePlayActorSoundSlot(int soundType, int seeSound, int attackSound, int painSound,
        int deathSound, int activeSound, int useSound, int bounceSound, int wallBounceSound, int crushPainSound,
        int howlSound, int pushSound) =>
        VmAcsActorPropertySound.GetActorSoundNative(
            soundType, seeSound, attackSound, painSound, deathSound, activeSound, useSound,
            bounceSound, wallBounceSound, crushPainSound, howlSound, pushSound);
}
