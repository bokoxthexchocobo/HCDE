namespace HCDE.Playsim;

/// <summary>Cajun bot lifecycle, CCMD bodies (<see cref="VmBBotCcmd"/>), and BOTSUPP init from <c>b_bot.cpp</c>.</summary>
public static class VmBBot
{
    public const int BotInuseNo = 0;
    public const int BotInuseWaiting = 1;
    public const int BotInuseYes = 2;
    public const int TeamNone = 255;
    public const int DemKillbots = 52;
    public const int DemGenericcheat = 53;
    public const int ChtFreeze = 14;
    public const int GsLevel = 4;
    public const int Mf3Warnbot = 1 << 20;

    public const int BifBotReactionSkillThing = 1;
    public const int BifBotExplosive = 2;
    public const int BifBotBfg = 4;

    public sealed class BotSkill
    {
        public int Aiming { get; set; }
        public int Perfection { get; set; }
        public int Reaction { get; set; }
        public int Isp { get; set; }
    }

    public sealed class BotInfoNode
    {
        public BotInfoNode? Next { get; set; }
        public string Name { get; set; } = "";
        public int InUse { get; set; }
        public int LastTeam { get; set; }
    }

    public sealed class BotWeaponData
    {
        public int MoveCombatDist { get; set; }
        public int Flags { get; set; }
        public string? ProjectileTypeName { get; set; }
    }

    public sealed class BotState
    {
        public object? Player { get; set; }
        public double Angle { get; set; }
        public Actor? Dest { get; set; }
        public Actor? Prev { get; set; }
        public Actor? Enemy { get; set; }
        public Actor? Missile { get; set; }
        public Actor? Mate { get; set; }
        public Actor? LastMate { get; set; }
        public BotSkill Skill { get; } = new();
        public int TActive { get; set; }
        public int TRespawn { get; set; }
        public int TStrafe { get; set; }
        public int TReact { get; set; }
        public int TFight { get; set; }
        public int TRoam { get; set; }
        public int TRocket { get; set; }
        public bool FirstShot { get; set; } = true;
        public bool Sleft { get; set; }
        public bool Allround { get; set; }
        public bool Increase { get; set; }
        public (double X, double Y) Old { get; set; }
    }

    public sealed class PlayerSlot
    {
        public Actor? Mo { get; set; }
        public BotState? Bot { get; set; }
        public string UserName { get; set; } = "";
        public int Team { get; set; }
        public bool InGame { get; set; }
    }

    public sealed class BotTickHost
    {
        public Actor? PlayerMo { get; set; }
        public bool LevelFrozen { get; set; }
        public bool ThinkInvoked { get; private set; }
        public bool ClockInvoked { get; private set; }
        public bool UnclockInvoked { get; private set; }

        public void BotThinkClock() => ClockInvoked = true;

        public void BotThinkUnclock() => UnclockInvoked = true;

        public void Think(BotState bot) => ThinkInvoked = true;
    }

    public sealed class ClearPlayerHost
    {
        public List<Actor> DestroyedMos { get; } = new();
        public List<BotState> DestroyedBots { get; } = new();
        public bool PlayerReset { get; private set; }

        public void DestroyMo(Actor mo) => DestroyedMos.Add(mo);

        public void DestroyBot(BotState bot) => DestroyedBots.Add(bot);

        public void ResetPlayerSlot() => PlayerReset = true;
    }

    public sealed class InitBotHost
    {
        public bool IsWeaponDescendant(string typeName) => WeaponCheck(typeName);

        public string? ResolveActorClass(string token) => ClassResolve(token);

        public Func<string, bool> WeaponCheck { get; init; } = _ => false;

        public Func<string, string?> ClassResolve { get; init; } = _ => null;
    }

    public static void Clear(BotState bot)
    {
        bot.Player = null;
        bot.Angle = 0;
        bot.Dest = null;
        bot.Prev = null;
        bot.Enemy = null;
        bot.Missile = null;
        bot.Mate = null;
        bot.LastMate = null;
        bot.Skill.Aiming = 0;
        bot.Skill.Perfection = 0;
        bot.Skill.Reaction = 0;
        bot.Skill.Isp = 0;
        bot.TActive = 0;
        bot.TRespawn = 0;
        bot.TStrafe = 0;
        bot.TReact = 0;
        bot.TFight = 0;
        bot.TRoam = 0;
        bot.TRocket = 0;
        bot.FirstShot = true;
        bot.Sleft = false;
        bot.Allround = false;
        bot.Increase = false;
        bot.Old = (0, 0);
    }

    public static void WriteSkillSnapshot(BotSkill skill, IList<string> keys, IList<int> values)
    {
        keys.Add("aiming");
        values.Add(skill.Aiming);
        keys.Add("perfection");
        values.Add(skill.Perfection);
        keys.Add("reaction");
        values.Add(skill.Reaction);
        keys.Add("isp");
        values.Add(skill.Isp);
    }

    public static void WriteBotSnapshot(BotState bot, IList<string> keys, IList<object?> values)
    {
        keys.Add("player");
        values.Add(bot.Player);
        keys.Add("angle");
        values.Add(bot.Angle);
        keys.Add("dest");
        values.Add(bot.Dest);
        keys.Add("prev");
        values.Add(bot.Prev);
        keys.Add("enemy");
        values.Add(bot.Enemy);
        keys.Add("missile");
        values.Add(bot.Missile);
        keys.Add("mate");
        values.Add(bot.Mate);
        keys.Add("lastmate");
        values.Add(bot.LastMate);
        keys.Add("skill");
        values.Add(bot.Skill);
        keys.Add("active");
        values.Add(bot.TActive);
        keys.Add("respawn");
        values.Add(bot.TRespawn);
        keys.Add("strafe");
        values.Add(bot.TStrafe);
        keys.Add("react");
        values.Add(bot.TReact);
        keys.Add("fight");
        values.Add(bot.TFight);
        keys.Add("roam");
        values.Add(bot.TRoam);
        keys.Add("rocket");
        values.Add(bot.TRocket);
        keys.Add("firstshot");
        values.Add(bot.FirstShot);
        keys.Add("sleft");
        values.Add(bot.Sleft);
        keys.Add("allround");
        values.Add(bot.Allround);
        keys.Add("increase");
        values.Add(bot.Increase);
        keys.Add("old");
        values.Add(bot.Old);
    }

    public static void Tick(BotState bot, BotTickHost host)
    {
        if (host.PlayerMo == null || host.LevelFrozen)
            return;

        host.BotThinkClock();
        host.Think(bot);
        host.BotThinkUnclock();
    }

    public static void ClearPlayer(
        PlayerSlot slot,
        BotInfoNode? botListHead,
        bool keepTeam,
        ClearPlayerHost host)
    {
        if (slot.Mo != null)
        {
            host.DestroyMo(slot.Mo);
            slot.Mo = null;
        }

        var bot = botListHead;
        while (bot != null && !string.Equals(slot.UserName, bot.Name, StringComparison.OrdinalIgnoreCase))
            bot = bot.Next;

        if (bot != null)
        {
            bot.InUse = BotInuseNo;
            bot.LastTeam = keepTeam ? slot.Team : TeamNone;
        }

        if (slot.Bot != null)
        {
            host.DestroyBot(slot.Bot);
            slot.Bot = null;
        }

        host.ResetPlayerSlot();
        slot.InGame = false;
    }

    public static void ParseBotSuppWeaponLine(
        string weaponTypeName,
        IReadOnlyList<string> tokens,
        Dictionary<string, BotWeaponData> botInfo,
        InitBotHost initHost)
    {
        if (!initHost.IsWeaponDescendant(weaponTypeName))
            throw new InvalidOperationException($"{weaponTypeName} is not a weapon type");

        var bi = new BotWeaponData();
        var i = 1;
        if (i < tokens.Count && tokens[i] == ",")
            i++;
        if (i >= tokens.Count)
            throw new InvalidOperationException("expected combat distance after weapon type");
        bi.MoveCombatDist = int.Parse(tokens[i++]);
        for (; i < tokens.Count; i++)
        {
            var tok = tokens[i];
            if (tok == ",")
                continue;
            if (tok == "BOT_REACTION_SKILL_THING")
                bi.Flags |= BifBotReactionSkillThing;
            else if (tok == "BOT_EXPLOSIVE")
                bi.Flags |= BifBotExplosive;
            else if (tok == "BOT_BFG")
                bi.Flags |= BifBotBfg;
            else
            {
                var cls = initHost.ResolveActorClass(tok);
                if (cls == null)
                    throw new InvalidOperationException($"Unknown token {tok}");
                bi.ProjectileTypeName = cls;
            }
        }

        botInfo[weaponTypeName] = bi;
    }

    public static void ApplyWarnBotMissileDefaults(IEnumerable<string> classNames, Dictionary<string, int> actorFlags3)
    {
        foreach (var name in classNames)
        {
            if (actorFlags3.ContainsKey(name))
                actorFlags3[name] |= Mf3Warnbot;
        }
    }

    public static void InitBotStuffFromLumps(
        IEnumerable<IReadOnlyList<string>> weaponLines,
        Dictionary<string, BotWeaponData> botInfo,
        InitBotHost initHost,
        Dictionary<string, int> actorFlags3)
    {
        botInfo.Clear();
        foreach (var line in weaponLines)
        {
            if (line.Count == 0)
                continue;
            ParseBotSuppWeaponLine(line[0], line, botInfo, initHost);
        }

        ApplyWarnBotMissileDefaults(["PlasmaBall", "Ripper", "HornRodFX1"], actorFlags3);
    }
}
