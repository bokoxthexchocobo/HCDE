namespace HCDE.Playsim;

/// <summary><c>DFraggleThinker::InitFunctions</c> from <c>fragglescript/t_func.cpp</c> (4017–4231).</summary>
public static class VmFScriptInitFunctions
{
    public static readonly string[] ActorNamesInit =
    {
        "DoomPlayer", "ZombieMan", "ShotgunGuy", "Archvile", "ArchvileFire", "Revenant", "RevenantTracer",
        "RevenantTracerSmoke", "Fatso", "FatShot", "ChaingunGuy", "DoomImp", "Demon", "Spectre", "Cacodemon",
        "BaronOfHell", "BaronBall", "HellKnight", "LostSoul", "SpiderMastermind", "Arachnotron", "Cyberdemon",
        "PainElemental", "WolfensteinSS", "CommanderKeen", "BossBrain", "BossEye", "BossTarget", "SpawnShot",
        "SpawnFire", "ExplosiveBarrel", "DoomImpBall", "CacodemonBall", "Rocket", "PlasmaBall", "BFGBall",
        "ArachnotronPlasma", "BulletPuff", "Blood", "TeleportFog", "ItemFog", "TeleportDest", "BFGExtra",
        "GreenArmor", "BlueArmor", "HealthBonus", "ArmorBonus", "BlueCard", "RedCard", "YellowCard",
        "YellowSkull", "RedSkull", "BlueSkull", "Stimpack", "Medikit", "Soulsphere", "InvulnerabilitySphere",
        "Berserk", "BlurSphere", "RadSuit", "Allmap", "Infrared", "Megasphere", "Clip", "ClipBox", "RocketAmmo",
        "RocketBox", "Cell", "CellPack", "Shell", "ShellBox", "Backpack", "BFG9000", "Chaingun", "Chainsaw",
        "RocketLauncher", "PlasmaRifle", "Shotgun", "SuperShotgun", "TechLamp", "TechLamp2", "Column",
        "TallGreenColumn", "ShortGreenColumn", "TallRedColumn", "ShortRedColumn", "SkullColumn", "HeartColumn",
        "EvilEye", "FloatingSkull", "TorchTree", "BlueTorch", "GreenTorch", "RedTorch", "ShortBlueTorch",
        "ShortGreenTorch", "ShortRedTorch", "Slalagtite", "TechPillar", "CandleStick", "Candelabra",
        "BloodyTwitch", "Meat2", "Meat3", "Meat4", "Meat5", "NonsolidMeat2", "NonsolidMeat4", "NonsolidMeat3",
        "NonsolidMeat5", "NonsolidTwitch", "DeadCacodemon", "DeadMarine", "DeadZombieMan", "DeadDemon",
        "DeadLostSoul", "DeadDoomImp", "DeadShotgunGuy", "GibbedMarine", "GibbedMarineExtra", "HeadsOnAStick",
        "Gibs", "HeadOnAStick", "HeadCandles", "DeadStick", "LiveStick", "BigTree", "BurningBarrel",
        "HangNoGuts", "HangBNoBrain", "HangTLookingDown", "HangTSkull", "HangTLookingUp", "HangTNoBrain",
        "ColonGibs", "SmallBloodPool", "BrainStem", "PointPusher", "PointPuller",
    };

    public sealed class GlobalInitState
    {
        public object?[] ActorTypes { get; } = new object?[ActorNamesInit.Length];
        public Dictionary<string, Action<VmFScriptParser.ParserContext>> Functions { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, VmFScriptParser.ParserContext.FraggleLineSpec> LineSpecConstants { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public int ConsolePlayer { get; set; }
        public int Zoom { get; set; }
        public object? TriggerObj { get; set; }
    }

    public readonly record struct LineSpecialMeta(string Name, int MaxArgs);

    public static IReadOnlyDictionary<string, Action<VmFScriptParser.ParserContext>> FraggleFunctions { get; } =
        BuildTable();

    public static void RegisterFraggleFunctions(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        foreach (var (name, action) in FraggleFunctions)
            target[name] = action;
    }

    public static GlobalInitState InitFunctions(
        Func<string, object?> findActor,
        Func<int, LineSpecialMeta?> lineSpecialInfo,
        int maxLineSpecial)
    {
        var state = new GlobalInitState();
        for (var i = 0; i < ActorNamesInit.Length; i++)
            state.ActorTypes[i] = findActor(ActorNamesInit[i]);

        for (var i = 0; i <= maxLineSpecial; i++)
        {
            var ls = lineSpecialInfo(i);
            if (ls is { MaxArgs: >= 0 })
                state.LineSpecConstants[ls.Value.Name] = new VmFScriptParser.ParserContext.FraggleLineSpec(i, ls.Value.MaxArgs);
        }

        RegisterFraggleFunctions(state.Functions);
        return state;
    }

    static Dictionary<string, Action<VmFScriptParser.ParserContext>> BuildTable()
    {
        var t = new Dictionary<string, Action<VmFScriptParser.ParserContext>>(StringComparer.OrdinalIgnoreCase)
        {
            ["break"] = VmFScriptParser.SfBreak,
            ["continue"] = VmFScriptParser.SfContinue,
            ["print"] = VmFScriptParser.SfPrint,
            ["rnd"] = VmFScriptParser.SfRnd,
            ["prnd"] = VmFScriptParser.SfRnd,
            ["wait"] = VmFScriptParser.SfWait,
            ["tagwait"] = VmFScriptParser.SfTagWait,
            ["scriptwait"] = VmFScriptParser.SfScriptWait,
            ["scriptwaitpre"] = VmFScriptParser.SfScriptWaitPre,
            ["startscript"] = VmFScriptParser.SfStartScript,
            ["scriptrunning"] = VmFScriptParser.SfScriptRunning,
            ["gameskill"] = VmFScriptParser.SfGameskill,
            ["gamemode"] = VmFScriptParser.SfGamemode,
            ["playername"] = VmFScriptParser.SfPlayerName,
            ["playeraddfrag"] = VmFScriptParser.SfPlayerAddFrag,
            ["playerobj"] = VmFScriptParser.SfPlayerObj,
            ["isplayerobj"] = VmFScriptParser.SfIsPlayerObj,
            ["isobjplayer"] = VmFScriptParser.SfIsPlayerObj,
            ["skincolor"] = VmFScriptParser.SfSkinColor,
            ["playerkeys"] = VmFScriptParser.SfPlayerKeys,
            ["playerammo"] = VmFScriptParser.SfPlayerAmmo,
            ["maxplayerammo"] = VmFScriptParser.SfMaxPlayerAmmo,
            ["playerweapon"] = VmFScriptParser.SfPlayerWeapon,
            ["playerselwep"] = VmFScriptParser.SfPlayerSelectedWeapon,
            ["spawn"] = VmFScriptParser.SfSpawn,
            ["kill"] = VmFScriptParser.SfKillObj,
            ["removeobj"] = VmFScriptParser.SfRemoveObj,
            ["objx"] = VmFScriptParser.SfObjX,
            ["objy"] = VmFScriptParser.SfObjY,
            ["objz"] = VmFScriptParser.SfObjZ,
            ["teleport"] = VmFScriptParser.SfTeleport,
            ["silentteleport"] = VmFScriptParser.SfSilentTeleport,
            ["damageobj"] = VmFScriptParser.SfDamageObj,
            ["player"] = VmFScriptParser.SfPlayer,
            ["objsector"] = VmFScriptParser.SfObjSector,
            ["objflag"] = VmFScriptParser.SfObjFlag,
            ["pushobj"] = VmFScriptParser.SfPushThing,
            ["pushthing"] = VmFScriptParser.SfPushThing,
            ["objangle"] = VmFScriptParser.SfObjAngle,
            ["objhealth"] = VmFScriptParser.SfObjHealth,
            ["objdead"] = VmFScriptParser.SfObjDead,
            ["reactiontime"] = VmFScriptParser.SfReactionTime,
            ["objreactiontime"] = VmFScriptParser.SfReactionTime,
            ["objtarget"] = VmFScriptParser.SfMobjTarget,
            ["objmomx"] = VmFScriptParser.SfMobjMomx,
            ["objmomy"] = VmFScriptParser.SfMobjMomy,
            ["objmomz"] = VmFScriptParser.SfMobjMomz,
            ["spawnmissile"] = VmFScriptParser.SfSpawnMissile,
            ["mapthings"] = VmFScriptParser.SfMapThings,
            ["objtype"] = VmFScriptParser.SfObjType,
            ["mapthingnumexist"] = VmFScriptParser.SfMapThingNumExist,
            ["objstate"] = VmFScriptParser.SfObjState,
            ["resurrect"] = VmFScriptParser.SfResurrect,
            ["lineattack"] = VmFScriptParser.SfLineAttack,
            ["setobjposition"] = VmFScriptParser.SfSetObjPosition,
            ["objawaken"] = VmFScriptParser.SfObjAwaken,
            ["ambientsound"] = VmFScriptParser.SfAmbientSound,
            ["startambiantsound"] = VmFScriptParser.SfAmbientSound,
            ["exitsecret"] = VmFScriptParser.SfExitSecret,
            ["mobjvalue"] = VmFScriptParser.SfMobjValue,
            ["stringvalue"] = VmFScriptParser.SfStringValue,
            ["intvalue"] = VmFScriptParser.SfIntValue,
            ["fixedvalue"] = VmFScriptParser.SfFixedValue,
            ["spawnshot2"] = VmFScriptParser.SfSpawnShot2,
            ["setcolor"] = VmFScriptParser.SfSetColor,
            ["objradius"] = VmFScriptParser.SfMobjRadius,
            ["objheight"] = VmFScriptParser.SfMobjHeight,
            ["thingcount"] = VmFScriptParser.SfThingCount,
            ["killinsector"] = VmFScriptParser.SfKillInSector,
            ["levelnum"] = VmFScriptParser.SfLevelNum,
            ["giveinventory"] = VmFScriptParser.SfGiveInventory,
            ["takeinventory"] = VmFScriptParser.SfTakeInventory,
            ["checkinventory"] = VmFScriptParser.SfCheckInventory,
            ["setweapon"] = VmFScriptParser.SfSetWeapon,
            ["movecamera"] = VmFScriptParser.SfMoveCamera,
            ["pointtoangle"] = VmFScriptParser.SfPointToAngle,
            ["pointtodist"] = VmFScriptParser.SfPointToDist,
            ["sin"] = VmFScriptParser.SfSin,
            ["asin"] = VmFScriptParser.SfAsin,
            ["cos"] = VmFScriptParser.SfCos,
            ["acos"] = VmFScriptParser.SfAcos,
            ["tan"] = VmFScriptParser.SfTan,
            ["atan"] = VmFScriptParser.SfAtan,
            ["exp"] = VmFScriptParser.SfExp,
            ["log"] = VmFScriptParser.SfLog,
            ["sqrt"] = VmFScriptParser.SfSqrt,
            ["floor"] = VmFScriptParser.SfFloor,
            ["pow"] = VmFScriptParser.SfPow,
            ["lineflag"] = VmFScriptParser.SfLineFlag,
            ["setlinetrigger"] = VmFScriptParser.SfSetLineTrigger,
            ["checkcvar"] = VmFScriptParser.SfCheckCVar,
            ["playdemo"] = VmFScriptParser.SfPlayDemo,
            ["max"] = VmFScriptParser.SfMax,
            ["min"] = VmFScriptParser.SfMin,
            ["abs"] = VmFScriptParser.SfAbs,
            ["setcorona"] = VmFScriptParser.SfSetCorona,
        };
        return t;
    }
}
