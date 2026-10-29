namespace HCDE.Playsim;

/// <summary>
/// Projectile spawn, thing damage, removal, raise, and velocity from <c>p_things.cpp</c>
/// (<b>249–535</b>). Chunk <b>1157</b>. <c>P_Thing_CheckInputNum</c> begins at <b>536</b>.
/// </summary>
public static class VmPThingsProjectile1157Slice
{
    public const int Special = 0x00000001;
    public const int Solid = 0x00000002;
    public const int Shootable = 0x00000004;
    public const int NoGravity = 0x00000200;
    public const int Missile = 0x00010000;
    public const int Dropped = 0x00020000;
    public const int Seeker = 0x40000000;
    public const int FloorHugger = 0x00000001;
    public const int CeilingHugger = 0x00000002;
    public const int IsMonster = 0x00002000;
    public const int NoMonsters = 1 << 12;
    public const int LevelNoMonsters = 0x00008000;
    public const int TransferFriend = 1;
    public const int NoCheckPosition = 2;
    public const int BodyChannel = 4;
    public const float IdleAttenuation = 1.001f;
    public const double OnFloorZ = -2147483648d;
    public const double OnCeilingZ = 2147483647d;
    public const double EighthGravity = 1d / 8d;

    public readonly record struct Origin(int Id, double X, double Y, double Z, double FloorClip, double Radius);

    public readonly record struct Aim(int Id);

    public readonly record struct Shot(
        bool Created,
        int Flags,
        int Flags2,
        int Flags3,
        double Gravity,
        double Speed,
        bool MissileFits,
        bool LocationOk,
        double InterceptX,
        double InterceptY,
        double InterceptZ,
        double InterceptYaw);

    public readonly record struct Fired(
        bool Alive,
        bool Destroyed,
        bool Cleared,
        int Tid,
        int Flags,
        int Flags3,
        double Gravity,
        double X,
        double Y,
        double Z,
        double VelX,
        double VelY,
        double VelZ,
        double Yaw,
        double Speed,
        int TargetId,
        int TracerId,
        bool Aimed,
        bool Lead,
        bool PlayedSpawn);

    public readonly record struct Volley(bool Any, IReadOnlyList<Fired> Actors);

    public readonly record struct Victim(bool Shootable, int Health, int SpawnHealth, bool Player, int PlayerHealth);

    public readonly record struct Hurt(int Index, int Health, int PlayerHealth, bool Counted, bool Dealt, bool Healed);

    public readonly record struct DamageTally(int Count, IReadOnlyList<Hurt> Hits);

    public readonly record struct Removal(bool Removed, bool Cleared);

    public readonly record struct Raised(
        bool Rose,
        double VelX,
        double VelY,
        double VelZ,
        int Flags,
        double Height,
        double Radius,
        bool Restored,
        bool CheckedResurrect,
        bool Played,
        int Channel,
        float Attenuation,
        bool Revived,
        bool CopiedFriend,
        bool ChangeTarget,
        bool ResetHealth,
        bool EnteredRaise);

    public readonly record struct RaiseCheck(bool Can, int Flags, double Height, double Radius);

    public readonly record struct Motion(
        double X,
        double Y,
        double Z,
        double PlayerX,
        double PlayerY,
        double PlayerZ);

    public static Volley Projectile(
        bool hasKind,
        int defaultFlags3,
        int dmFlags,
        int levelFlags2,
        int tid,
        int dest,
        double angle,
        double speed,
        double vspeed,
        int gravity,
        int newTid,
        bool leadTarget,
        bool hasSource,
        Origin source,
        IReadOnlyList<Origin> spots,
        bool hasForced,
        Aim forced,
        IReadOnlyList<Aim> aims,
        IReadOnlyList<Shot> shots)
    {
        if (!hasKind)
            return new Volley(false, Array.Empty<Fired>());
        if ((defaultFlags3 & IsMonster) != 0
            && ((dmFlags & NoMonsters) != 0 || (levelFlags2 & LevelNoMonsters) != 0))
        {
            return new Volley(false, Array.Empty<Fired>());
        }

        var origins = new List<Origin>();
        if (tid == 0)
        {
            if (hasSource)
                origins.Add(source);
        }
        else
        {
            origins.AddRange(spots);
        }

        int shotIndex = 0;
        bool any = false;
        var fired = new List<Fired>();
        foreach (Origin spot in origins)
        {
            if (dest == 0)
            {
                bool created = Launch(
                    spot, hasForced, forced, defaultFlags3, angle, speed, vspeed, gravity, newTid, leadTarget,
                    shots, ref shotIndex, fired);
                if (created)
                    any = true;
            }
            else if (aims.Count > 0)
            {
                foreach (Aim aim in aims)
                {
                    bool created = Launch(
                        spot, true, aim, defaultFlags3, angle, speed, vspeed, gravity, newTid, leadTarget,
                        shots, ref shotIndex, fired);
                    if (created)
                        any = true;
                }
            }
        }

        return new Volley(any, fired);
    }

    public static DamageTally Damage(int tid, int amount, IReadOnlyList<Victim> victims)
    {
        int count = 0;
        var hits = new List<Hurt>();
        int index = 0;
        while (index < victims.Count)
        {
            Victim actor = victims[index];
            int next = tid == 0 ? victims.Count : index + 1;
            if (actor.Shootable)
            {
                int health = actor.Health;
                int playerHealth = actor.PlayerHealth;
                bool dealt = false;
                bool healed = false;
                if (amount > 0)
                {
                    dealt = true;
                }
                else if (actor.Health < actor.SpawnHealth)
                {
                    health -= amount;
                    if (health > actor.SpawnHealth)
                        health = actor.SpawnHealth;
                    healed = true;
                    if (actor.Player)
                        playerHealth = health;
                }

                count++;
                hits.Add(new Hurt(index, health, playerHealth, true, dealt, healed));
            }

            index = next;
        }

        return new DamageTally(count, hits);
    }

    public static Removal Remove(bool hasPlayer, bool isPlayerBody, bool isMapActor)
    {
        if (hasPlayer && isPlayerBody)
            return new Removal(false, false);
        if (!isMapActor)
            return new Removal(false, false);
        return new Removal(true, true);
    }

    public static Raised Raise(
        bool hasThing,
        bool hasRaiseState,
        double velX,
        double velY,
        double velZ,
        int flags,
        double height,
        double radius,
        double defaultHeight,
        double defaultRadius,
        int raiseFlags,
        bool positionOk,
        bool canResurrect,
        bool hasRaiser)
    {
        if (!hasThing || !hasRaiseState)
        {
            return new Raised(
                false, velX, velY, velZ, flags, height, radius,
                false, false, false, 0, 0, false, false, false, false, false);
        }

        double nextX = 0;
        double nextY = 0;
        int nextFlags = flags | Solid;
        double nextHeight = defaultHeight;
        double nextRadius = defaultRadius;
        bool skipCheck = (raiseFlags & NoCheckPosition) != 0;
        if (!skipCheck && !positionOk)
        {
            return new Raised(
                false, nextX, nextY, velZ, flags, height, radius,
                true, false, false, 0, 0, false, false, false, false, false);
        }

        if (!canResurrect)
        {
            return new Raised(
                false, nextX, nextY, velZ, nextFlags, nextHeight, nextRadius,
                false, true, false, 0, 0, false, false, false, false, false);
        }

        bool copy = (raiseFlags & TransferFriend) != 0 && hasRaiser;
        return new Raised(
            true, nextX, nextY, velZ, nextFlags, nextHeight, nextRadius,
            false, true, true, BodyChannel, IdleAttenuation, true, copy, false, true, true);
    }

    public static RaiseCheck CanRaise(
        bool hasRaiseState,
        int flags,
        double height,
        double radius,
        double defaultHeight,
        double defaultRadius,
        bool positionOk)
    {
        if (!hasRaiseState)
            return new RaiseCheck(false, flags, height, radius);

        bool check = positionOk;
        return new RaiseCheck(check, flags, height, radius);
    }

    public static Motion SetVelocity(
        bool hasActor,
        bool hasPlayer,
        double velX,
        double velY,
        double velZ,
        double playerX,
        double playerY,
        double playerZ,
        double addX,
        double addY,
        double addZ,
        bool add,
        bool setBob)
    {
        if (!hasActor)
            return new Motion(velX, velY, velZ, playerX, playerY, playerZ);

        if (!add)
        {
            velX = 0;
            velY = 0;
            velZ = 0;
            if (hasPlayer)
            {
                playerX = 0;
                playerY = 0;
                playerZ = 0;
            }
        }

        velX += addX;
        velY += addY;
        velZ += addZ;
        if (setBob && hasPlayer)
        {
            playerX += addX;
            playerY += addY;
        }

        return new Motion(velX, velY, velZ, playerX, playerY, playerZ);
    }

    private static bool Launch(
        Origin spot,
        bool hasAim,
        Aim aim,
        int defaultFlags3,
        double angle,
        double speed,
        double vspeed,
        int gravity,
        int newTid,
        bool leadTarget,
        IReadOnlyList<Shot> shots,
        ref int shotIndex,
        List<Fired> fired)
    {
        Shot shot = shotIndex < shots.Count
            ? shots[shotIndex]
            : new Shot(false, 0, 0, 0, 0, 0, false, false, 0, 0, 0, 0);
        shotIndex++;
        if (!shot.Created)
            return false;

        double z = spot.Z;
        if ((defaultFlags3 & FloorHugger) != 0)
            z = OnFloorZ;
        else if ((defaultFlags3 & CeilingHugger) != 0)
            z = OnCeilingZ;
        else if (z != OnFloorZ)
            z -= spot.FloorClip;

        int flags = shot.Flags;
        double gravityValue = shot.Gravity;
        if (gravity != 0)
        {
            flags &= ~NoGravity;
            if ((shot.Flags3 & IsMonster) == 0 && gravity == 1)
                gravityValue = EighthGravity;
        }
        else
        {
            flags |= NoGravity;
        }

        double velX;
        double velY;
        double velZ;
        double yaw;
        int tracer = -1;
        bool aimed = hasAim;
        if (hasAim)
        {
            velX = shot.InterceptX;
            velY = shot.InterceptY;
            velZ = shot.InterceptZ;
            yaw = shot.InterceptYaw;
            if ((shot.Flags2 & Seeker) != 0)
                tracer = aim.Id;
        }
        else
        {
            yaw = angle;
            velX = speed * CosDeg(angle);
            velY = speed * SinDeg(angle);
            velZ = vspeed;
        }

        double actorSpeed = shot.Speed;
        if ((flags & Missile) != 0)
            actorSpeed = Math.Sqrt((velX * velX) + (velY * velY) + (velZ * velZ));
        if ((shot.Flags3 & (FloorHugger | CeilingHugger)) != 0)
            velZ = 0;
        if ((flags & Special) != 0)
            flags |= Dropped;

        bool alive = true;
        bool destroyed = false;
        bool cleared = false;
        bool kept = false;
        if ((flags & Missile) != 0)
        {
            if (shot.MissileFits)
                kept = true;
            else
            {
                alive = false;
                destroyed = true;
            }
        }
        else if (!shot.LocationOk)
        {
            alive = false;
            destroyed = true;
            cleared = true;
        }
        else
        {
            kept = true;
        }

        fired.Add(new Fired(
            alive, destroyed, cleared, newTid, flags, shot.Flags3, gravityValue,
            spot.X, spot.Y, z, velX, velY, velZ, yaw, actorSpeed, spot.Id, tracer,
            aimed, aimed && leadTarget, true));
        return kept;
    }

    private static double CosDeg(double degrees)
    {
        return Math.Cos(degrees * (Math.PI / 180d));
    }

    private static double SinDeg(double degrees)
    {
        return Math.Sin(degrees * (Math.PI / 180d));
    }
}
