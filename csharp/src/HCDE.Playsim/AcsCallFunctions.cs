using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>CallFunction</c> subset beyond invasion zero-arg queries.</summary>
internal static class AcsCallFunctions
{
    /// <summary>Native <c>ACSF_GetActorVelX</c> (9).</summary>
    public const int GetActorVelX = 9;
    /// <summary>Native <c>ACSF_GetActorVelY</c> (10).</summary>
    public const int GetActorVelY = 10;
    /// <summary>Native <c>ACSF_GetActorVelZ</c> (11).</summary>
    public const int GetActorVelZ = 11;
    /// <summary>Native <c>ACSF_SetActivator</c> (12).</summary>
    public const int SetActivator = 12;
    /// <summary>Native <c>ACSF_SetActivatorToTarget</c> (13).</summary>
    public const int SetActivatorToTarget = 13;
    /// <summary>Native <c>ACSF_GetActorViewHeight</c> (14).</summary>
    public const int GetActorViewHeight = 14;
    /// <summary>Native <c>ACSF_GetChar</c> (15).</summary>
    public const int GetChar = 15;
    /// <summary>Native <c>ACSF_GetArmorType</c> (19).</summary>
    public const int GetArmorType = 19;
    /// <summary>Native <c>ACSF_Radius_Quake2</c> (26).</summary>
    public const int RadiusQuake2 = 26;
    /// <summary>Native <c>ACSF_GetPolyobjX</c> (33).</summary>
    public const int GetPolyobjX = 33;
    /// <summary>Native <c>ACSF_GetPolyobjY</c> (34).</summary>
    public const int GetPolyobjY = 34;
    /// <summary>Native <c>ACSF_CheckSight</c> (35).</summary>
    public const int CheckSight = 35;
    /// <summary>Native ACS fixed-point sentinel when a polyobj is missing.</summary>
    internal const int FixedMax = int.MaxValue;
    /// <summary>Native <c>ACSF_CheckActorProperty</c> (22).</summary>
    public const int CheckActorProperty = 22;
    /// <summary>Native <c>ACSF_SetActorVelocity</c> (23).</summary>
    public const int SetActorVelocity = 23;
    /// <summary>Native <c>ACSF_GetMaxInventory</c> (93).</summary>
    public const int GetMaxInventory = 93;
    /// <summary>Native <c>ACSF_DamageActor</c> (201).</summary>
    public const int DamageActor = 201;
    /// <summary>Native <c>ACSF_CheckClass</c> (200).</summary>
    public const int CheckClass = 200;
    /// <summary>Native <c>ACSF_CheckActorClass</c> (27).</summary>
    public const int CheckActorClass = 27;
    /// <summary>Native <c>ACSF_UniqueTID</c> (46).</summary>
    public const int UniqueTid = 46;
    /// <summary>Native <c>ACSF_IsTIDUsed</c> (47).</summary>
    public const int IsTidUsed = 47;
    /// <summary>Native <c>ACSF_Sqrt</c> (48).</summary>
    public const int Sqrt = 48;
    /// <summary>Native <c>ACSF_FixedSqrt</c> (49).</summary>
    public const int FixedSqrt = 49;
    /// <summary>Native <c>ACSF_VectorLength</c> (50).</summary>
    public const int VectorLength = 50;
    /// <summary>Native <c>ACSF_LineAttack</c> (60).</summary>
    public const int LineAttack = 60;
    /// <summary>Native <c>ACSF_PlaySound</c> (61).</summary>
    public const int PlaySound = 61;
    /// <summary>Native <c>ACSF_StopSound</c> (62).</summary>
    public const int StopSound = 62;
    /// <summary>Native <c>ACSF_strcmp</c> (63).</summary>
    public const int StrCmp = 63;
    /// <summary>Native <c>ACSF_stricmp</c> (64).</summary>
    public const int StrICmp = 64;
    /// <summary>Native <c>ACSF_StrLeft</c> (65).</summary>
    public const int StrLeft = 65;
    /// <summary>Native <c>ACSF_StrRight</c> (66).</summary>
    public const int StrRight = 66;
    /// <summary>Native <c>ACSF_StrMid</c> (67).</summary>
    public const int StrMid = 67;
    /// <summary>Native <c>ACSF_GetActorClass</c> (68).</summary>
    public const int GetActorClass = 68;
    /// <summary>Native <c>ACSF_GetWeapon</c> (69).</summary>
    public const int GetWeapon = 69;
    /// <summary>Native <c>ACSF_SoundVolume</c> (70).</summary>
    public const int SoundVolume = 70;
    /// <summary>Native <c>ACSF_PlayActorSound</c> (71).</summary>
    public const int PlayActorSound = 71;
    /// <summary>Native <c>ACSF_SpawnDecal</c> (72).</summary>
    public const int SpawnDecal = 72;
    /// <summary>Native <c>ACSF_CheckFont</c> (73).</summary>
    public const int CheckFont = 73;
    /// <summary>Native <c>ACSF_SetLineActivation</c> (76).</summary>
    public const int SetLineActivation = 76;
    /// <summary>Native <c>ACSF_GetLineActivation</c> (77).</summary>
    public const int GetLineActivation = 77;
    /// <summary>Native <c>ACSF_GetActorPowerupTics</c> (78).</summary>
    public const int GetActorPowerupTics = 78;
    /// <summary>Native <c>ACSF_DropItem</c> (74).</summary>
    public const int DropItem = 74;
    /// <summary>Native <c>ACSF_CheckProximity</c> (98).</summary>
    public const int CheckProximity = 98;
    /// <summary>Native <c>ACSF_CheckActorState</c> (99).</summary>
    public const int CheckActorState = 99;
    /// <summary>Native <c>ACSF_CheckFlag</c> (75).</summary>
    public const int CheckFlag = 75;
    /// <summary>Native <c>ACSF_SetActorFlag</c> (202).</summary>
    public const int SetActorFlag = 202;
    /// <summary>Native <c>ACSF_GetActorFloorTexture</c> (204).</summary>
    public const int GetActorFloorTexture = 204;
    /// <summary>Native <c>ACSF_GetActorFloorTerrain</c> (205).</summary>
    public const int GetActorFloorTerrain = 205;
    /// <summary>Native <c>ACSF_StrArg</c> (206).</summary>
    public const int StrArg = 206;
    /// <summary>Native <c>ACSF_ChangeActorAngle</c> (79).</summary>
    public const int ChangeActorAngle = 79;
    /// <summary>Native <c>ACSF_ChangeActorPitch</c> (80).</summary>
    public const int ChangeActorPitch = 80;
    /// <summary>Native <c>ACSF_GetArmorInfo</c> (81).</summary>
    public const int GetArmorInfo = 81;
    /// <summary>Native <c>ACSF_DropInventory</c> (82).</summary>
    public const int DropInventory = 82;
    /// <summary>Native <c>ACSF_PickActor</c> (83).</summary>
    public const int PickActor = 83;
    /// <summary>Native <c>ACSF_IsPointerEqual</c> (84).</summary>
    public const int IsPointerEqual = 84;
    /// <summary>Native <c>ACSF_CanRaiseActor</c> (85).</summary>
    public const int CanRaiseActor = 85;
    /// <summary>Native <c>ACSF_SetActorTeleFog</c> (86).</summary>
    public const int SetActorTeleFog = 86;
    /// <summary>Native <c>ACSF_SwapActorTeleFog</c> (87).</summary>
    public const int SwapActorTeleFog = 87;
    /// <summary>Native <c>ACSF_SetActorRoll</c> (88).</summary>
    public const int SetActorRoll = 88;
    /// <summary>Native <c>ACSF_ChangeActorRoll</c> (89).</summary>
    public const int ChangeActorRoll = 89;
    /// <summary>Native <c>ACSF_GetActorRoll</c> (90).</summary>
    public const int GetActorRoll = 90;
    /// <summary>Native <c>ACSF_QuakeEx</c> (91).</summary>
    public const int QuakeEx = 91;
    /// <summary>Native <c>ACSF_SetSectorDamage</c> (94).</summary>
    public const int SetSectorDamage = 94;
    /// <summary>Native <c>ACSF_SetSectorTerrain</c> (95).</summary>
    public const int SetSectorTerrain = 95;
    /// <summary>Native <c>ACSF_SpawnParticle</c> (96).</summary>
    public const int SpawnParticle = 96;
    /// <summary>Native <c>ACSF_SetMusicVolume</c> (97).</summary>
    public const int SetMusicVolume = 97;
    /// <summary>Native <c>ACSF_Floor</c> (207).</summary>
    public const int Floor = 207;
    /// <summary>Native <c>ACSF_Round</c> (208).</summary>
    public const int Round = 208;
    /// <summary>Native <c>ACSF_Ceil</c> (209).</summary>
    public const int Ceil = 209;
    /// <summary>Native <c>ACSF_GetSectorHealth</c> (212).</summary>
    public const int GetSectorHealth = 212;
    /// <summary>Native <c>ACSF_GetLineHealth</c> (213).</summary>
    public const int GetLineHealth = 213;
    /// <summary>Native <c>ACSF_GetNetID</c> (215).</summary>
    public const int GetNetId = 215;
    /// <summary>Native <c>ACSF_SetActivatorByNetID</c> (216).</summary>
    public const int SetActivatorByNetId = 216;
    /// <summary>Native <c>ACSF_GetLineX</c> (300).</summary>
    public const int GetLineX = 300;
    /// <summary>Native <c>ACSF_GetLineY</c> (301).</summary>
    public const int GetLineY = 301;

    public static bool TryInvoke(
        AuthoritySimulation sim,
        List<int> stack,
        AcsActivatorBinding activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int function,
        int argCount,
        out int result)
    {
        result = 0;
        var scriptActivator = activator.Value;
        switch (function)
        {
            case GetActorVelX:
                return TryGetActorVelocity(sim, stack, scriptActivator, 0, argCount, out result);
            case GetActorVelY:
                return TryGetActorVelocity(sim, stack, scriptActivator, 1, argCount, out result);
            case GetActorVelZ:
                return TryGetActorVelocity(sim, stack, scriptActivator, 2, argCount, out result);
            case SetActivator:
                return TrySetActivator(sim, stack, activator, argCount, out result);
            case SetActivatorToTarget:
                return TrySetActivatorToTarget(sim, stack, activator, argCount, out result);
            case SetActivatorByNetId:
                return TrySetActivatorByNetId(sim, stack, activator, argCount, out result);
            case GetActorViewHeight:
                return TryGetActorViewHeight(sim, stack, scriptActivator, argCount, out result);
            case GetChar:
                return TryGetChar(stack, stringTable, globalStrings, argCount, out result);
            case SetActorVelocity:
                return TrySetActorVelocity(sim, stack, scriptActivator, argCount, out result);
            case GetArmorType:
                return TryGetArmorType(sim, stack, stringTable, globalStrings, argCount, out result);
            case RadiusQuake2:
                return TryRadiusQuake2(stack, argCount, out result);
            case GetPolyobjX:
            case GetPolyobjY:
                return TryGetPolyobj(stack, function == GetPolyobjY, argCount, out result);
            case CheckSight:
                return TryCheckSight(sim, stack, scriptActivator, argCount, out result);
            case CheckActorProperty:
                return TryCheckActorProperty(sim, stack, scriptActivator, argCount, out result);
            case GetMaxInventory:
                return TryGetMaxInventory(sim, stack, scriptActivator, stringTable, argCount, out result);
            case DamageActor:
                return TryDamageActor(sim, stack, scriptActivator, stringTable, argCount, out result);
            case CheckClass:
                return TryCheckClass(stack, stringTable, argCount, out result);
            case CheckActorClass:
                return TryCheckActorClass(sim, stack, scriptActivator, stringTable, argCount, out result);
            case UniqueTid:
                return TryUniqueTid(sim, stack, argCount, out result);
            case IsTidUsed:
                return TryIsTidUsed(sim, stack, argCount, out result);
            case Sqrt:
                return TrySqrt(stack, argCount, out result);
            case FixedSqrt:
                return TryFixedSqrt(stack, argCount, out result);
            case VectorLength:
                return TryVectorLength(stack, argCount, out result);
            case LineAttack:
                return TryLineAttack(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case PlaySound:
                return TryPlaySound(sim, stack, scriptActivator, argCount, out result);
            case StopSound:
                return TryStopSound(stack, argCount, out result);
            case GetActorClass:
                return TryGetActorClass(sim, stack, scriptActivator, globalStrings, argCount, out result);
            case GetWeapon:
                return TryGetWeapon(scriptActivator, globalStrings, out result);
            case SoundVolume:
                return TrySoundVolume(stack, argCount, out result);
            case PlayActorSound:
                return TryPlayActorSound(sim, stack, scriptActivator, argCount, out result);
            case SpawnDecal:
                return TrySpawnDecal(sim, stack, scriptActivator, argCount, out result);
            case CheckProximity:
                return TryCheckProximity(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case CheckActorState:
                return TryCheckActorState(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case StrCmp:
            case StrICmp:
                return TryStringCompare(stack, stringTable, globalStrings, function == StrICmp, argCount, out result);
            case StrLeft:
            case StrRight:
                return TryStrEdge(stack, stringTable, globalStrings, function == StrRight, argCount, out result);
            case StrMid:
                return TryStrMid(stack, stringTable, globalStrings, argCount, out result);
            case CheckFont:
                return TryCheckFont(stack, stringTable, globalStrings, argCount, out result);
            case SetLineActivation:
                return TrySetLineActivation(sim, stack, argCount, out result);
            case GetLineActivation:
                return TryGetLineActivation(sim, stack, argCount, out result);
            case GetActorPowerupTics:
                return TryGetActorPowerupTics(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case DropItem:
                return TryDropItem(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case CheckFlag:
                return TryCheckFlag(sim, stack, scriptActivator, stringTable, argCount, out result);
            case SetActorFlag:
                return TrySetActorFlag(sim, stack, scriptActivator, stringTable, argCount, out result);
            case GetActorFloorTexture:
                return TryGetActorFloorTexture(sim, stack, scriptActivator, globalStrings, argCount, out result);
            case GetActorFloorTerrain:
                return TryGetActorFloorTerrain(sim, stack, scriptActivator, globalStrings, argCount, out result);
            case StrArg:
                return TryStrArg(sim, stack, globalStrings, argCount, out result);
            case ChangeActorAngle:
                return TryChangeActorAngle(sim, stack, scriptActivator, argCount, out result);
            case ChangeActorPitch:
                return TryChangeActorPitch(sim, stack, scriptActivator, argCount, out result);
            case GetArmorInfo:
                return TryGetArmorInfo(stack, scriptActivator, globalStrings, argCount, out result);
            case DropInventory:
                return TryDropInventory(sim, stack, scriptActivator, stringTable, argCount, out result);
            case PickActor:
                return TryPickActor(sim, stack, scriptActivator, argCount, out result);
            case IsPointerEqual:
                return TryIsPointerEqual(sim, stack, scriptActivator, argCount, out result);
            case CanRaiseActor:
                return TryCanRaiseActor(sim, stack, scriptActivator, argCount, out result);
            case SetActorTeleFog:
                return TrySetActorTeleFog(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case SwapActorTeleFog:
                return TrySwapActorTeleFog(sim, stack, scriptActivator, argCount, out result);
            case SetActorRoll:
            case ChangeActorRoll:
                return TrySetActorRoll(sim, stack, scriptActivator, argCount, out result);
            case GetActorRoll:
                return TryGetActorRoll(sim, stack, scriptActivator, argCount, out result);
            case QuakeEx:
                return TryQuakeEx(stack, argCount, out result);
            case SetSectorDamage:
                return TrySetSectorDamage(sim, stack, argCount, out result);
            case SetSectorTerrain:
                return TrySetSectorTerrain(sim, stack, stringTable, globalStrings, argCount, out result);
            case SpawnParticle:
                return TrySpawnParticle(stack, argCount, out result);
            case SetMusicVolume:
                return TrySetMusicVolume(sim, stack, argCount, out result);
            case Floor:
            case Round:
            case Ceil:
                return TryFixedQuantize(stack, function, argCount, out result);
            case GetLineX:
            case GetLineY:
                return TryGetLineCoordinate(sim, stack, function == GetLineY, argCount, out result);
            case GetSectorHealth:
            case GetLineHealth:
                return TryGetDestructibleHealth(sim, stack, function == GetLineHealth, argCount, out result);
            case GetNetId:
                return TryGetNetId(sim, stack, scriptActivator, argCount, out result);
            default:
                return false;
        }
    }

    private static bool TryGetActorViewHeight(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.Destroyed)
            return true;

        var height = actor is PlayerPawn player
            ? player.ViewHeight
            : actor.Height.ToDouble() * 0.5;
        result = DoubleToAcs(height);
        return true;
    }

    private static bool TryGetChar(
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var stringId = stack[start];
        var index = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var text = AcsStringIds.Lookup(stringId, stringTable, globalStrings);
        if (index >= 0 && index < text.Length)
            result = text[index];
        return true;
    }

    private static bool TryGetArmorType(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var typeStringId = stack[start];
        var playerNumber = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var typeName = AcsStringIds.Lookup(typeStringId, stringTable, globalStrings);
        result = AcsPlayerInventory.ArmorTypeAmount(sim, typeName, playerNumber);
        return true;
    }

    private static bool TryCheckActorProperty(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var property = stack[start + 1];
        var value = stack[start + 2];
        stack.RemoveRange(start, argCount);

        result = AcsActorProperties.Check(sim, activator, tid, property, value) ? 1 : 0;
        return true;
    }

    private static bool TrySetActorVelocity(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 6 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var velX = AcsToDouble(stack[start + 1]);
        var velY = AcsToDouble(stack[start + 2]);
        var velZ = AcsToDouble(stack[start + 3]);
        var add = stack[start + 4] != 0;
        _ = stack[start + 5];
        stack.RemoveRange(start, argCount);

        void Apply(Actor actor)
        {
            if (!add)
            {
                actor.VelocityX = Fixed.FromInt(0);
                actor.VelocityY = Fixed.FromInt(0);
                actor.VelocityZ = Fixed.FromInt(0);
            }
            actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + velX);
            actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + velY);
            actor.VelocityZ = Fixed.FromDouble(actor.VelocityZ.ToDouble() + velZ);
        }

        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                Apply(activator);
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                Apply(actor);
        }
        return true;
    }

    private static bool TryGetActorVelocity(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int axis,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.Destroyed)
            return true;

        var velocity = axis switch
        {
            0 => actor.VelocityX.ToDouble(),
            1 => actor.VelocityY.ToDouble(),
            _ => actor.VelocityZ.ToDouble(),
        };
        result = DoubleToAcs(velocity);
        return true;
    }

    private static bool TryGetMaxInventory(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var stringId = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        result = actor is { Destroyed: false }
            ? AcsPlayerInventory.Count(actor, stringTable, stringId, max: true)
            : 0;
        return true;
    }

    private static bool TryDamageActor(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 6 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var targetTid = stack[start];
        _ = stack[start + 1];
        var inflictorTid = stack[start + 2];
        _ = stack[start + 3];
        var amount = stack[start + 4];
        var damageTypeStringId = stack[start + 5];
        stack.RemoveRange(start, argCount);

        var target = ResolveActor(sim, targetTid, activator);
        if (target is not { Destroyed: false })
            return true;

        var inflictor = ResolveActor(sim, inflictorTid, activator) ?? activator;
        var damageType = damageTypeStringId >= 0 && damageTypeStringId < stringTable.Length
            ? stringTable[damageTypeStringId]
            : null;
        var damage = ActorDamage.Apply(target, amount, activator, DamageFlags.None, damageType, inflictor);
        result = damage.HealthLost > 0 || damage.Killed ? 1 : 0;
        return true;
    }

    private static bool TryCheckActorClass(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var stringId = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        var className = stringId >= 0 && stringId < stringTable.Length ? stringTable[stringId] : null;
        result = actor is { Destroyed: false } && DoomActorCatalog.MatchesClassName(actor.DoomEdNum, className) ? 1 : 0;
        return true;
    }

    private static bool TryUniqueTid(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var startTid = argCount > 0 ? stack[start] : 0;
        var searchSpan = argCount > 1 ? stack[start + 1] : 0;
        stack.RemoveRange(start, argCount);
        result = AcsActorTid.FindUniqueTid(sim, startTid, searchSpan);
        return true;
    }

    private static bool TryIsTidUsed(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);
        result = AcsActorTid.IsTidUsed(sim, tid) ? 1 : 0;
        return true;
    }

    private static bool TrySqrt(List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var value = stack[start];
        stack.RemoveRange(start, argCount);
        result = value < 0 ? 0 : (int)Math.Floor(Math.Sqrt(value));
        return true;
    }

    private static bool TryFixedSqrt(List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var fixedValue = stack[start];
        stack.RemoveRange(start, argCount);
        if (fixedValue < 0)
            return true;
        result = DoubleToAcs(Math.Sqrt(AcsToDouble(fixedValue)));
        return true;
    }

    private static bool TryVectorLength(List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var fixedX = stack[start];
        var fixedY = stack[start + 1];
        stack.RemoveRange(start, argCount);
        var length = Math.Sqrt(AcsToDouble(fixedX) * AcsToDouble(fixedX) + AcsToDouble(fixedY) * AcsToDouble(fixedY));
        result = DoubleToAcs(length);
        return true;
    }

    private static double AcsToDouble(int value) => value / 65536.0;

    private static int DoubleToAcs(double value) => (int)Math.Floor(value * 65536.0);

    private static bool TryGetActorClass(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        result = globalStrings.Add(ClassNameOf(actor));
        return true;
    }

    private static bool TryGetWeapon(Actor? activator, AcsGlobalStrings globalStrings, out int result)
    {
        result = globalStrings.Add(AcsPlayerInventory.ReadyWeaponClassName(activator));
        return true;
    }

    private static bool TryCheckProximity(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var classStringId = stack[start + 1];
        var distanceFixed = stack[start + 2];
        var required = argCount > 3 ? stack[start + 3] : 1;
        var flags = argCount > 4 ? stack[start + 4] : 0;
        var pointerSelector = argCount > 5 ? stack[start + 5] : 0;
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null)
            return true;

        var className = AcsStringIds.Lookup(classStringId, stringTable, globalStrings);
        result = ActorProximity.Check(actor, className, AcsToDouble(distanceFixed), required, flags, pointerSelector) ? 1 : 0;
        return true;
    }

    private static bool TryCheckActorState(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var stateStringId = stack[start + 1];
        var exact = argCount > 2 && stack[start + 2] != 0;
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null)
            return true;

        var stateName = AcsStringIds.Lookup(stateStringId, stringTable, globalStrings);
        result = AcsActorStates.HasNamedState(actor, stateName, exact) ? 1 : 0;
        return true;
    }

    private static bool TryStrArg(
        AuthoritySimulation sim,
        List<int> stack,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var index = stack[start];
        stack.RemoveRange(start, argCount);

        var text = index >= 0 && index < sim.StrArgs.Count ? sim.StrArgs[index] : string.Empty;
        result = globalStrings.Add(text);
        return true;
    }

    private static string ClassNameOf(Actor? actor)
    {
        if (actor is null || actor.Destroyed)
            return "None";
        if (actor is PlayerPawn)
            return "DoomPlayer";
        return DoomActorCatalog.TrySpawnClassName(actor.DoomEdNum, out var className) ? className : "None";
    }

    private static bool TryStringCompare(
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        bool ignoreCase,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var leftId = stack[start];
        var rightId = stack[start + 1];
        var maxLength = argCount > 2 ? stack[start + 2] : -1;
        stack.RemoveRange(start, argCount);

        if (leftId == rightId)
            return true;

        var left = AcsStringIds.Lookup(leftId, stringTable, globalStrings);
        var right = AcsStringIds.Lookup(rightId, stringTable, globalStrings);
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        result = maxLength >= 0
            ? string.Compare(left, 0, right, 0, maxLength, comparison)
            : string.Compare(left, right, comparison);
        return true;
    }

    private static bool TryStrEdge(
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        bool fromRight,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var stringId = stack[start];
        var length = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var source = AcsStringIds.Lookup(stringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(source))
        {
            result = globalStrings.Add(string.Empty);
            return true;
        }

        var take = length < 0 ? 0 : Math.Min(length, source.Length);
        var slice = take == 0 ? string.Empty : fromRight ? source[^take..] : source[..take];
        result = globalStrings.Add(slice);
        return true;
    }

    private static bool TryStrMid(
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var stringId = stack[start];
        var position = stack[start + 1];
        var length = stack[start + 2];
        stack.RemoveRange(start, argCount);

        var source = AcsStringIds.Lookup(stringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(source) || position >= source.Length)
        {
            result = globalStrings.Add(string.Empty);
            return true;
        }

        if (position < 0)
            position = 0;
        var maxLength = source.Length - position;
        if (length < 0 || position + length > source.Length)
            length = maxLength;
        var slice = source.Substring(position, Math.Min(length, maxLength));
        result = globalStrings.Add(slice);
        return true;
    }

    private static bool TrySetLineActivation(
        AuthoritySimulation sim,
        List<int> stack,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var lineId = stack[start];
        var activation = stack[start + 1];
        var repeat = argCount > 2 ? stack[start + 2] : -1;
        stack.RemoveRange(start, argCount);

        foreach (var line in AcsLineActivation.LinesFromId(sim, lineId))
            AcsLineActivation.Apply(line, activation, repeat);
        return true;
    }

    private static bool TryGetLineActivation(
        AuthoritySimulation sim,
        List<int> stack,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var lineId = stack[start];
        stack.RemoveRange(start, argCount);

        var line = AcsLineActivation.FirstLineFromId(sim, lineId);
        result = line is null ? 0 : AcsLineActivation.Pack(line);
        return true;
    }

    private static bool TryGetDestructibleHealth(
        AuthoritySimulation sim,
        List<int> stack,
        bool lineHealth,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var id = stack[start];
        if (lineHealth)
        {
            stack.RemoveRange(start, argCount);
            result = AcsDestructibleHealth.GetLineHealth(sim, id);
            return true;
        }

        if (argCount < 2)
            return false;

        var part = stack[start + 1];
        stack.RemoveRange(start, argCount);
        result = AcsDestructibleHealth.GetSectorHealth(sim, id, part);
        return true;
    }

    private static bool TryGetLineCoordinate(
        AuthoritySimulation sim,
        List<int> stack,
        bool yAxis,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var lineId = stack[start];
        var along = AcsToDouble(stack[start + 1]);
        var perpRaw = stack[start + 2];
        stack.RemoveRange(start, argCount);

        var line = AcsLineActivation.FirstLineFromId(sim, lineId);
        if (line is null)
            return true;

        var deltaX = line.X2 - line.X1;
        var deltaY = line.Y2 - line.Y1;
        var value = (yAxis ? line.Y1 : line.X1) + (yAxis ? deltaY : deltaX) * along;
        if (perpRaw != 0)
        {
            var perpDistance = AcsToDouble(perpRaw);
            var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length > 0)
            {
                var normalX = deltaY / length;
                var normalY = -deltaX / length;
                value += (yAxis ? normalY : normalX) * perpDistance;
            }
        }

        result = DoubleToAcs(value);
        return true;
    }

    private static bool TryGetActorPowerupTics(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var powerStringId = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var powerName = AcsStringIds.Lookup(powerStringId, stringTable, globalStrings);
        var actor = ResolveActor(sim, tid, activator);
        result = AcsActorPowerups.RemainingTics(actor, powerName);
        return true;
    }

    private static bool TrySoundVolume(List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TryLineAttack(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 4 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var angleRaw = stack[start + 1];
        var pitchRaw = stack[start + 2];
        var damage = stack[start + 3];
        _ = argCount > 4 ? stack[start + 4] : -1; // puff class name; cosmetic only for now.
        var damageTypeId = argCount > 5 ? stack[start + 5] : -1;
        var rangeFixed = argCount > 6 && stack[start + 6] != 0 ? stack[start + 6] : 2048 << 16;
        _ = argCount > 7 ? stack[start + 7] : 0; // line-attack flags.
        var puffTid = argCount > 8 ? stack[start + 8] : 0;
        stack.RemoveRange(start, argCount);

        var damageType = damageTypeId > 0
            ? AcsStringIds.Lookup(damageTypeId, stringTable, globalStrings)
            : null;
        var sources = tid == 0 ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, tid).ToArray();
        foreach (var source in sources)
            AcsLineAttack.Attack(sim, source, unchecked(angleRaw << 16), unchecked(pitchRaw << 16),
                damage, damageType, AcsToDouble(rangeFixed), puffTid, absoluteAngles: true);
        return true;
    }

    private static bool TryGetPolyobj(List<int> stack, bool _, int argCount, out int result)
    {
        result = FixedMax;
        if (argCount < 1 || stack.Count < argCount)
            return false;
        stack.RemoveRange(stack.Count - argCount, argCount);
        // Polyobjects are not simulated yet; native returns FIXED_MAX when missing.
        return true;
    }

    private static bool TryRadiusQuake2(List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 6 || stack.Count < argCount)
            return false;
        stack.RemoveRange(stack.Count - argCount, argCount);
        // Native P_StartQuake is absent; accept the call so scripts keep running.
        result = 1;
        return true;
    }

    private static bool TryCheckSight(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var sourceTid = stack[start];
        var destTid = stack[start + 1];
        _ = stack[start + 2]; // SF_IGNOREVISIBILITY and optional water/seepast bits not modeled yet.
        stack.RemoveRange(start, argCount);

        if (sourceTid == 0)
        {
            if (activator is not { Destroyed: false })
                return true;
            if (destTid == 0)
            {
                result = 1;
                return true;
            }

            foreach (var dest in AcsActorTid.AllFromTid(sim, destTid))
            {
                if (CombatTrace.HasLineOfSight(sim, activator, dest))
                {
                    result = 1;
                    return true;
                }
            }

            return true;
        }

        foreach (var source in AcsActorTid.AllFromTid(sim, sourceTid))
        {
            if (destTid != 0)
            {
                foreach (var dest in AcsActorTid.AllFromTid(sim, destTid))
                {
                    if (CombatTrace.HasLineOfSight(sim, source, dest))
                    {
                        result = 1;
                        return true;
                    }
                }
            }
            else if (activator is { Destroyed: false } && CombatTrace.HasLineOfSight(sim, source, activator))
            {
                result = 1;
                return true;
            }
        }

        return true;
    }

    private static bool TryQuakeEx(List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 8 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TryPlaySound(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        result = CountAcsTidTargets(sim, tid, activator);
        return true;
    }

    private static bool TryStopSound(List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TrySetSectorDamage(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tag = stack[start];
        var amount = stack[start + 1];
        var damageType = argCount > 2 ? stack[start + 2] : 0;
        var interval = argCount > 3 ? stack[start + 3] : 0;
        var leakiness = argCount > 4 ? stack[start + 4] : 0;
        stack.RemoveRange(start, argCount);

        result = SectorDamage.ApplyTagged(sim, tag, amount, damageType, interval, leakiness);
        return true;
    }

    private static bool TrySetSectorTerrain(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tag = stack[start];
        var terrainId = stack[start + 1];
        var position = stack[start + 2];
        stack.RemoveRange(start, argCount);

        var terrainName = AcsStringIds.Lookup(terrainId, stringTable, globalStrings);
        result = SectorTerrain.ApplyTagged(sim, tag, terrainName, position);
        return true;
    }

    private static bool TrySpawnParticle(List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TrySetMusicVolume(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var volumeFixed = stack[start];
        stack.RemoveRange(start, argCount);

        sim.MusicVolume = AcsToDouble(volumeFixed);
        return true;
    }

    private static bool TryPlayActorSound(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        _ = stack[start + 1];
        stack.RemoveRange(start, argCount);

        result = CountAcsTidTargets(sim, tid, activator);
        return true;
    }

    private static bool TrySpawnDecal(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        result = CountAcsTidTargets(sim, tid, activator);
        return true;
    }

    private static int CountAcsTidTargets(AuthoritySimulation sim, int tid, Actor? activator)
    {
        if (tid == 0)
            return activator is { Destroyed: false } ? 1 : 0;

        var count = 0;
        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            count++;
        return count;
    }

    private static bool TryCheckFont(
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var stringId = stack[start];
        stack.RemoveRange(start, argCount);

        var fontName = AcsStringIds.Lookup(stringId, stringTable, globalStrings);
        result = AcsHudFonts.Exists(fontName) ? 1 : 0;
        return true;
    }

    private static bool TryDropItem(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var typeStringId = stack[start + 1];
        var amount = argCount > 2 ? stack[start + 2] : 0;
        var chance = argCount > 3 ? stack[start + 3] : 256;
        stack.RemoveRange(start, argCount);

        var typeName = AcsStringIds.Lookup(typeStringId, stringTable, globalStrings);
        result = ActorDropItem.Drop(sim, tid, activator, typeName, amount, chance);
        return true;
    }

    private static bool TryCheckFlag(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var flagStringId = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null)
            return true;

        var flagName = flagStringId >= 0 && flagStringId < stringTable.Length ? stringTable[flagStringId] : null;
        result = AcsActorFlags.TryGet(actor, flagName, out var value) && value ? 1 : 0;
        return true;
    }

    private static bool TrySetActorFlag(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var flagStringId = stack[start + 1];
        var flagValue = stack[start + 2];
        stack.RemoveRange(start, argCount);

        var flagName = flagStringId >= 0 && flagStringId < stringTable.Length ? stringTable[flagStringId] : null;
        var set = flagValue != 0;
        if (tid == 0)
        {
            if (activator is { Destroyed: false } && AcsActorFlags.TrySet(activator, flagName, set))
                result = 1;
            return true;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
        {
            if (AcsActorFlags.TrySet(actor, flagName, set))
                result++;
        }
        return true;
    }

    private static bool TryChangeActorAngle(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var angleRaw = unchecked((uint)stack[start + 1]);
        stack.RemoveRange(start, argCount);

        var angle = new BamAngle(unchecked(angleRaw << 16));
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                activator.Angle = angle;
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                actor.Angle = angle;
        }
        return true;
    }

    private static bool TryChangeActorPitch(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var pitchRaw = unchecked((uint)stack[start + 1]);
        stack.RemoveRange(start, argCount);

        var pitch = PitchDegreesFromAcs(pitchRaw);
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                activator.PitchDegrees = pitch;
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                actor.PitchDegrees = pitch;
        }
        return true;
    }

    private static double PitchDegreesFromAcs(uint angleRaw)
    {
        return unchecked((short)angleRaw) * (360.0 / 65536);
    }

    private static bool TrySetActorTeleFog(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var sourceId = stack[start + 1];
        var destId = stack[start + 2];
        stack.RemoveRange(start, argCount);

        var source = AcsStringIds.Lookup(sourceId, stringTable, globalStrings);
        var dest = AcsStringIds.Lookup(destId, stringTable, globalStrings);
        result = AcsActorTeleFog.Set(sim, tid, activator, source, dest);
        return true;
    }

    private static bool TrySwapActorTeleFog(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        result = AcsActorTeleFog.Swap(sim, tid, activator);
        return true;
    }

    private static bool TrySetActorRoll(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var rollRaw = unchecked((uint)stack[start + 1]);
        _ = argCount > 2 ? stack[start + 2] : 0;
        stack.RemoveRange(start, argCount);

        var roll = new BamAngle(unchecked(rollRaw << 16));
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                activator.Roll = roll;
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                actor.Roll = roll;
        }
        return true;
    }

    private static bool TryGetActorRoll(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.Destroyed)
            return true;

        result = (int)(actor.Roll.Raw >> 16);
        return true;
    }

    private static bool TryGetArmorInfo(
        List<int> stack,
        Actor? activator,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var infoKind = stack[start];
        stack.RemoveRange(start, argCount);
        result = AcsPlayerInventory.ArmorInfo(activator, infoKind, globalStrings);
        return true;
    }

    private static bool TryDropInventory(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var typeStringId = stack[start + 1];
        stack.RemoveRange(start, argCount);

        var typeName = typeStringId >= 0 && typeStringId < stringTable.Length ? stringTable[typeStringId] : null;
        if (string.IsNullOrEmpty(typeName))
            return true;

        if (tid == 0)
        {
            AcsPlayerInventory.Drop(activator, typeName);
            return true;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid).ToArray())
            AcsPlayerInventory.Drop(actor, typeName);
        return true;
    }

    private const int PickAfForceTid = 1;
    private const int PickAfReturnTid = 2;

    private static bool TryIsPointerEqual(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var pointer1 = stack[start];
        var pointer2 = stack[start + 1];
        var tid1 = argCount > 2 ? stack[start + 2] : 0;
        var tid2 = argCount > 3 ? stack[start + 3] : 0;
        stack.RemoveRange(start, argCount);

        var actor1 = ResolveActor(sim, tid1, activator);
        var actor2 = tid2 == tid1 ? actor1 : ResolveActor(sim, tid2, activator);
        var left = AcsActorPointer.Resolve(sim, actor1, pointer1);
        var right = AcsActorPointer.Resolve(sim, actor2, pointer2);
        result = AcsActorPointer.PointersEqual(left, right) ? 1 : 0;
        return true;
    }

    private static bool TryCanRaiseActor(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        result = ActorRaise.CanRaiseAll(sim, tid, activator) ? 1 : 0;
        return true;
    }

    private static bool TryPickActor(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 5 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var angleRaw = unchecked((uint)stack[start + 1]);
        var pitchRaw = unchecked((uint)stack[start + 2]);
        var distance = AcsToDouble(stack[start + 3]);
        var assignTid = stack[start + 4];
        var actorMask = argCount > 5 ? stack[start + 5] : 0x00000004;
        var wallMask = argCount > 6
            ? stack[start + 6]
            : LevelLine.BlockEverythingFlag | LevelLine.BlockHitscanFlag;
        var pickFlags = argCount > 7 ? stack[start + 7] : 0;
        stack.RemoveRange(start, argCount);

        var source = ResolveActor(sim, tid, activator);
        if (source is null || source.Destroyed)
            return true;

        var picked = CombatTrace.PickActor(
            sim,
            source,
            new BamAngle(unchecked(angleRaw << 16)),
            new BamAngle(unchecked(pitchRaw << 16)),
            distance,
            actorMask,
            wallMask);
        if (picked is null)
            return true;

        if ((pickFlags & PickAfForceTid) == 0 && assignTid == 0 && picked.ThingId == 0)
            return true;

        if (picked.ThingId == 0 || (pickFlags & PickAfForceTid) != 0)
            picked.ThingId = assignTid;

        result = (pickFlags & PickAfReturnTid) != 0 ? picked.ThingId : 1;
        return true;
    }

    private static bool TryGetActorFloorTexture(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = globalStrings.Add(string.Empty);
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count)
            return true;

        result = globalStrings.Add(sim.Level.Sectors[actor.SectorIndex].FloorPic);
        return true;
    }

    private static bool TryGetActorFloorTerrain(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = globalStrings.Add(string.Empty);
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count)
            return true;

        result = globalStrings.Add(sim.Level.Sectors[actor.SectorIndex].FloorTerrain);
        return true;
    }

    private static bool TryFixedQuantize(List<int> stack, int function, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var fixedValue = stack[start];
        stack.RemoveRange(start, argCount);

        const int fractionalMask = 0xFFFF;
        const int fractionalRoundBias = 0x8000;
        var integral = fixedValue & ~fractionalMask;
        result = function switch
        {
            Floor => integral,
            Round => (fixedValue + fractionalRoundBias) & ~fractionalMask,
            Ceil => integral + 0x10000,
            _ => 0,
        };
        return true;
    }

    private static bool TryCheckClass(List<int> stack, string[] stringTable, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var stringId = stack[start];
        stack.RemoveRange(start, argCount);

        var className = stringId >= 0 && stringId < stringTable.Length ? stringTable[stringId] : null;
        result = DoomActorCatalog.TryEditorNumberForClassName(className, out _) ? 1 : 0;
        return true;
    }

    private static Actor? ResolveActor(AuthoritySimulation sim, int tid, Actor? activator)
    {
        if (tid == 0)
            return activator is { Destroyed: false } ? activator : null;
        return AcsActorTid.SingleFromTid(sim, tid);
    }

    private static bool TrySetActivator(
        AuthoritySimulation sim,
        List<int> stack,
        AcsActivatorBinding activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var pointer = argCount > 1 ? stack[start + 1] : AcsActorPointer.Default;
        stack.RemoveRange(start, argCount);

        Actor? actor;
        if (argCount > 1 && pointer != AcsActorPointer.Default)
            actor = AcsActorPointer.Resolve(sim, ResolveActor(sim, tid, activator.Value), pointer);
        else
            actor = AcsActorTid.SingleFromTid(sim, tid);

        if (actor is { Destroyed: false })
        {
            activator.Value = actor;
            result = 1;
        }
        return true;
    }

    private static bool TrySetActivatorToTarget(
        AuthoritySimulation sim,
        List<int> stack,
        AcsActivatorBinding activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);

        var actor = ResolveActor(sim, tid, activator.Value);
        if (actor is null || actor.Destroyed)
            return true;

        Actor? target;
        if (actor is PlayerPawn player && !player.IsDead)
            target = CombatTrace.FindTarget(sim, player, HitscanCombat.HitscanRange);
        else if (actor.Brain?.TargetId is { } targetId)
            target = sim.Actors.FirstOrDefault(candidate => candidate.Id == targetId && !candidate.Destroyed);
        else
            target = null;

        if (target is null || target.Destroyed)
            return true;

        activator.Value = target;
        result = 1;
        return true;
    }

    private static bool TryGetNetId(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var tid = stack[start];
        var index = stack[start + 1];
        var pointer = argCount > 2 ? stack[start + 2] : AcsActorPointer.Default;
        stack.RemoveRange(start, argCount);

        result = AcsNetId.GetNetId(sim, tid, index, activator, pointer);
        return true;
    }

    private static bool TrySetActivatorByNetId(
        AuthoritySimulation sim,
        List<int> stack,
        AcsActivatorBinding activator,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var netId = unchecked((uint)stack[start]);
        var pointer = argCount > 1 ? stack[start + 1] : AcsActorPointer.Default;
        stack.RemoveRange(start, argCount);

        var actor = AcsNetId.ActorFromNetId(sim, netId);
        if (pointer != AcsActorPointer.Default)
            actor = AcsActorPointer.Resolve(sim, actor, pointer);
        if (actor is null || actor.Destroyed)
            return true;

        activator.Value = actor;
        result = 1;
        return true;
    }
}
