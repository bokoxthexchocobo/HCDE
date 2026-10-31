namespace HCDE.Playsim;

/// <summary>
/// Player poison and the kill/remove cheats from <c>p_interaction.cpp</c>
/// (<b>1834–2052</b>). Chunk <b>1159</b>. The file ends at <b>2052</b>.
/// </summary>
public static class VmPInteractionPoison1159Slice
{
    public const int God = 1 << 1;
    public const int God2 = 1 << 25;
    public const int Invulnerable = 0x08000000;
    public const int NoDamage = 0x00000040;
    public const int LaxTelefrag = 0x00100000;
    public const int TelefragDamage = 1000000;
    public const int NoSuicide = 1 << 22;
    public const int GenericCheat = 10;
    public const int Suicide = 15;
    public const int KillClass = 50;
    public const int Remove = 68;
    public const int Massacre = 19;
    public const int Massacre2 = 53;
    public const string None = "None";
    public const string Fire = "Fire";

    public readonly record struct Dose(
        bool Applied,
        int Count,
        int PoisonerId,
        string Type,
        string PainType);

    public readonly record struct Hit(
        bool Ignored,
        int PlayerHealth,
        int ActorHealth,
        int Special,
        string DamageType,
        bool Died,
        bool Saved,
        bool UsedHealth,
        int AutoRequest,
        bool UsedStrife,
        bool PlayedPain,
        int AttackerId);

    public readonly record struct Packet(int Command, int Extra, string Text, bool HasExtra, bool HasText);

    public readonly record struct ConsoleResult(bool Hidden, bool Usage, IReadOnlyList<Packet> Net);

    public static Dose Poison(
        int cheats,
        int flags2,
        int flags5,
        bool hasSource,
        bool sourceIsThisPlayer,
        bool teammate,
        double teamDamage,
        int poison,
        int count,
        int sourceId,
        bool hasPoisoner,
        string damageType,
        string painType)
    {
        if ((cheats & God) != 0
            || (flags2 & Invulnerable) != 0
            || (cheats & God2) != 0
            || (flags5 & NoDamage) != 0)
        {
            return new Dose(false, count, -1, None, None);
        }

        if (hasSource && !sourceIsThisPlayer && teammate)
            poison = (int)(poison * teamDamage);

        int poisonerId = -1;
        string type = None;
        string pain = None;
        if (poison > 0)
        {
            count += poison;
            poisonerId = hasSource ? sourceId : -1;
            if (!hasPoisoner)
            {
                type = None;
                pain = None;
            }
            else
            {
                type = damageType;
                pain = IsNone(painType) ? damageType : painType;
            }

            if (count > 100)
                count = 100;
        }

        return new Dose(true, count, poisonerId, type, pain);
    }

    public static Hit Damage(
        bool hasPlayer,
        int actorHealth,
        int playerHealth,
        int rawDamage,
        int cheats,
        int flags2,
        int flags5,
        int flags7,
        double skillFactor,
        double playerFactor,
        bool replaceDamage,
        int replacedDamage,
        bool autoUse,
        bool deathmatch,
        bool alternative,
        int healthAfterAuto,
        int healthAfterStrife,
        int buddha,
        string poisonType,
        string actorDamageType,
        int sourceId,
        int levelTime,
        bool playPainSound,
        bool hasPainState)
    {
        if (!hasPlayer || actorHealth <= 0)
        {
            return Ignored(playerHealth, actorHealth, actorDamageType);
        }

        bool telefrag = rawDamage >= TelefragDamage && (flags7 & LaxTelefrag) == 0;
        if ((cheats & God2) != 0
            || (flags5 & NoDamage) != 0
            || (!telefrag && ((flags2 & Invulnerable) != 0 || (cheats & God) != 0)))
        {
            return Ignored(playerHealth, actorHealth, actorDamageType);
        }

        int damage = (int)(rawDamage * skillFactor * playerFactor);
        if (replaceDamage)
            damage = replacedDamage;
        if (damage <= 0)
            return Ignored(playerHealth, actorHealth, actorDamageType);

        int health = playerHealth;
        bool usedHealth = false;
        int autoRequest = 0;
        if (damage >= health && (autoUse || deathmatch) && !alternative)
        {
            usedHealth = true;
            autoRequest = damage - health + 1;
            health = healthAfterAuto;
        }

        health -= damage;
        bool usedStrife = false;
        if (health < 50 && !deathmatch)
        {
            usedStrife = true;
            health = healthAfterStrife;
        }

        if (health < 0)
            health = 0;

        int nextActor = actorHealth - damage;
        if (nextActor <= 0)
        {
            int save = buddha;
            if (telefrag && save == 1)
                save = 0;
            if (save != 0)
            {
                return new Hit(
                    false, 1, 1, 0, actorDamageType, false, true,
                    usedHealth, autoRequest, usedStrife, false, sourceId);
            }

            string typed = actorDamageType;
            if (!alternative)
            {
                if (poisonType == Fire && nextActor > -50 && damage > 25)
                    typed = Fire;
                else
                    typed = poisonType;
            }

            return new Hit(
                false, health, nextActor, damage, typed, true, false,
                usedHealth, autoRequest, usedStrife, false, sourceId);
        }

        bool played = (levelTime & 63) == 0 && playPainSound && hasPainState;
        return new Hit(
            false, health, nextActor, 0, actorDamageType, false, false,
            usedHealth, autoRequest, usedStrife, played, sourceId);
    }

    public static ConsoleResult Console(string command, IReadOnlyList<string> args, bool cheatBlocked, bool noSuicide)
    {
        if (string.Equals(command, "kill", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Count > 0)
            {
                if (cheatBlocked)
                    return new ConsoleResult(false, false, Array.Empty<Packet>());
                if (string.Equals(args[0], "monsters", StringComparison.OrdinalIgnoreCase))
                {
                    return Sent(new Packet(GenericCheat, Massacre, "", true, false));
                }

                if (string.Equals(args[0], "baddies", StringComparison.OrdinalIgnoreCase))
                {
                    return Sent(new Packet(GenericCheat, Massacre2, "", true, false));
                }

                return Sent(new Packet(KillClass, 0, args[0], false, true));
            }

            if (noSuicide)
                return new ConsoleResult(false, false, Array.Empty<Packet>());
            return Sent(new Packet(Suicide, 0, "", false, false));
        }

        if (args.Count == 1)
        {
            if (cheatBlocked)
                return new ConsoleResult(false, false, Array.Empty<Packet>());
            return Sent(new Packet(Remove, 0, args[0], false, true));
        }

        return new ConsoleResult(false, true, Array.Empty<Packet>());
    }

    private static ConsoleResult Sent(Packet packet)
    {
        return new ConsoleResult(true, false, new[] { packet });
    }

    private static Hit Ignored(int playerHealth, int actorHealth, string damageType)
    {
        return new Hit(true, playerHealth, actorHealth, 0, damageType, false, false, false, 0, false, false, -1);
    }

    private static bool IsNone(string name)
    {
        return string.IsNullOrEmpty(name) || name == None;
    }
}
