namespace HCDE.Playsim;

/// <summary>
/// Chase dispatch, facing, monster rail, item drop, and pain sounds from
/// <c>p_enemy.cpp</c> (<b>2999–3270</b>). Chunk <b>1151</b>. Boss death begins at <b>3271</b>.
/// </summary>
public static class VmPEnemyPain1151Slice
{
    public const int FastChase = 1;
    public const int NoPlayActive = 2;
    public const int NightmareFast = 4;
    public const int Resurrect = 8;
    public const int DontMove = 16;
    public const int FlagMask = 0x3fffffff;
    public const int Stealth = 0x40000000;
    public const int Ambush = 0x00000020;
    public const int ShadowAimVertical = 0x00000008;
    public const int FaceBottom = 1;
    public const int FaceMiddle = 2;
    public const int FaceTop = 4;
    public const int MissileRange = 32 * 64;
    public const int AimWindowDegrees = 60;
    public const double DefaultMaxPitch = 270;
    public const int VoiceChannel = 2;
    public const int NoRumble = 0x80000;
    public const float NormalAttenuation = 1f;

    public readonly record struct ChaseOrder(bool Chased, bool Fast, bool PlayActive, bool Nightmare, bool DontMove, int PassedFlags);

    public readonly record struct Face(bool Turned, int Flags, int? VisDir, double Yaw, double Pitch, bool Pitched);

    public readonly record struct RailShot(bool Fired, int Flags, int? VisDir, double Yaw, double AttackPitch, double RestoredPitch, int Damage);

    public readonly record struct PainCue(bool Played, string Name, bool Rumble, int ChannelFlags);

    public static uint ToBams(double degrees)
    {
        double rounded = Math.Round(degrees * (0x40000000 / 90d), MidpointRounding.ToEven);
        return unchecked((uint)(long)rounded);
    }

    public static double Normalize360(double degrees)
    {
        return (90d / 0x40000000) * ToBams(degrees);
    }

    public static double Normalize180(double degrees)
    {
        return (90d / 0x40000000) * unchecked((int)ToBams(degrees));
    }

    public static ChaseOrder DoChase(int flags, bool resurrected)
    {
        if ((flags & Resurrect) != 0 && resurrected)
            return new ChaseOrder(false, false, false, false, false, 0);
        return new ChaseOrder(
            true,
            (flags & FastChase) != 0,
            (flags & NoPlayActive) == 0,
            (flags & NightmareFast) != 0,
            (flags & DontMove) != 0,
            flags & FlagMask);
    }

    public static Face Turn(
        bool hasOther,
        int flags,
        int flags9,
        double yaw,
        double pitch,
        double otherAngle,
        double maxTurn,
        double maxPitch,
        double angleOffset,
        double pitchOffset,
        int faceFlags,
        double zAdd,
        double selfZ,
        double selfBob,
        double selfTop,
        double selfCenter,
        double otherZ,
        double otherBob,
        double otherTop,
        double otherCenter,
        double deltaX,
        double deltaY,
        bool shadowed,
        double aimFactor,
        double penalty,
        int yawRoll,
        int pitchRoll)
    {
        if (!hasOther)
            return new Face(false, flags, null, yaw, pitch, false);

        int? visDir = (flags & Stealth) != 0 ? 1 : null;
        flags &= ~Ambush;
        double delta = -Normalize180(otherAngle - yaw);
        if (maxTurn != 0 && maxTurn < Math.Abs(delta))
        {
            if (delta > 0)
                yaw -= maxTurn + angleOffset;
            else
                yaw += maxTurn + angleOffset;
        }
        else
        {
            yaw = otherAngle + angleOffset;
        }

        bool pitched = maxPitch <= 180;
        if (pitched)
        {
            double sourceZ = selfZ + 32 + selfBob;
            double targetZ = otherZ + 32 + otherBob;
            if (targetZ >= otherTop)
                targetZ = otherCenter;
            if (sourceZ >= selfTop)
                sourceZ = selfCenter;
            if ((faceFlags & FaceBottom) != 0)
                targetZ = otherZ + otherBob;
            if ((faceFlags & FaceMiddle) != 0)
                targetZ = otherCenter + otherBob;
            if ((faceFlags & FaceTop) != 0)
                targetZ = otherTop + otherBob;
            targetZ += zAdd;

            double distZ = targetZ - sourceZ;
            double reach = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + distZ * distZ);
            double otherPitch = 0;
            if (reach != 0)
                otherPitch = Normalize180(-Math.Asin(distZ / reach) * (180d / Math.PI));
            if (maxPitch != 0)
            {
                if (pitch > otherPitch)
                    pitch -= Math.Min(maxPitch, Normalize360(pitch - otherPitch));
                else
                    pitch += Math.Min(maxPitch, Normalize360(otherPitch - pitch));
            }
            else
            {
                pitch = otherPitch;
            }

            pitch += pitchOffset;
            if ((flags9 & ShadowAimVertical) != 0 && maxPitch == 0 && pitch == otherPitch && shadowed)
                pitch += pitchRoll * (45d / 256d) * aimFactor * penalty;
        }

        if (maxTurn == 0 && yaw == otherAngle && shadowed)
            yaw += yawRoll * (45d / 256d) * aimFactor * penalty;
        return new Face(true, flags, visDir, yaw, pitch, pitched);
    }

    public static RailShot MonsterRail(
        bool hasTarget,
        int flags,
        int flags9,
        double pitch,
        double leadYaw,
        bool hasLineTarget,
        double aimPitch,
        double selfCenter,
        double targetCenter,
        double floorClip,
        double xyLength,
        bool shadowed,
        double aimFactor,
        double penalty,
        int yawRoll,
        int pitchRoll,
        int damage)
    {
        if (!hasTarget)
            return new RailShot(false, flags, null, leadYaw, pitch, pitch, damage);
        int? visDir = (flags & Stealth) != 0 ? 1 : null;
        flags &= ~Ambush;
        double attack = aimPitch;
        if (!hasLineTarget)
        {
            double zDiff = targetCenter - selfCenter - floorClip;
            attack = -Math.Atan2(zDiff, xyLength) * (180d / Math.PI);
        }

        if (shadowed)
        {
            leadYaw += yawRoll * (45d / 256d) * aimFactor * penalty;
            if ((flags9 & ShadowAimVertical) != 0)
                attack += pitchRoll * (45d / 256d) * aimFactor * penalty;
        }

        return new RailShot(true, flags, visDir, leadYaw, attack, pitch, damage);
    }

    public static bool DropItem(bool hasVm, bool vmReturnedActor)
    {
        if (hasVm && vmReturnedActor)
            return true;
        return false;
    }

    public static string PainAmount(int health)
    {
        if (health < 25)
            return "*pain25";
        if (health < 50)
            return "*pain50";
        if (health < 75)
            return "*pain75";
        return "*pain100";
    }

    public static PainCue Pain(
        bool isPlayer,
        bool hasAlternative,
        bool painSoundValid,
        string painSound,
        int health,
        bool hasDamageType,
        string damageType,
        Func<string, bool> hasSound)
    {
        if (isPlayer && !hasAlternative)
        {
            string amount = PainAmount(health);
            string name = amount;
            if (hasDamageType)
            {
                string specific = amount + "-" + damageType;
                if (hasSound(specific))
                    name = specific;
                else
                {
                    string generic = "*pain-" + damageType;
                    if (hasSound(generic))
                        name = generic;
                }
            }

            return new PainCue(true, name, true, NoRumble);
        }

        if (painSoundValid)
            return new PainCue(true, painSound, false, 0);
        return new PainCue(false, "", false, 0);
    }
}
