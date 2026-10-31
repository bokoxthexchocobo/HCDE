namespace HCDE.Playsim;

/// <summary>
/// Proximity and warp from <c>p_things.cpp</c> (<b>566–835</b>). Chunk <b>1158</b>.
/// <c>P_Thing_CheckInputNum</c> occupies <b>536–565</b>. The file ends at <b>835</b>.
/// </summary>
public static class VmPThingsWarp1158Slice
{
    public const int Ancestor = 1 << 0;
    public const int LessOrEqual = 1 << 1;
    public const int NoZ = 1 << 2;
    public const int CountDead = 1 << 3;
    public const int DeadOnly = 1 << 4;
    public const int Exact = 1 << 5;
    public const int SetTarget = 1 << 6;
    public const int SetMaster = 1 << 7;
    public const int SetTracer = 1 << 8;
    public const int Farthest = 1 << 9;
    public const int Closest = 1 << 10;
    public const int SetOnPtr = 1 << 11;
    public const int CheckSight = 1 << 12;

    public const int AbsoluteOffset = 0x1;
    public const int AbsoluteAngle = 0x2;
    public const int UseCallerAngle = 0x4;
    public const int NoCheckPosition = 0x8;
    public const int Interpolate = 0x10;
    public const int WarpInterpolation = 0x20;
    public const int CopyInterpolation = 0x40;
    public const int Stop = 0x80;
    public const int ToFloor = 0x100;
    public const int TestOnly = 0x200;
    public const int AbsolutePosition = 0x400;
    public const int Bob = 0x800;
    public const int MovePtr = 0x1000;
    public const int CopyVelocity = 0x4000;
    public const int CopyPitch = 0x8000;
    public const int FloatBob = 0x00080000;

    public readonly record struct Neighbor(
        int Id,
        bool Unmorphed,
        bool KindOf,
        bool ExactClass,
        bool MapActor,
        double Distance,
        double Z,
        double Height,
        bool Killed,
        bool InSight);

    public readonly record struct Proximity(
        int Value,
        int Seen,
        int SelfTarget,
        int SelfMaster,
        int SelfTracer,
        int RefTarget,
        int RefMaster,
        int RefTracer,
        int ChosenId,
        bool Wrote);

    public readonly record struct Body(
        int Id,
        double X,
        double Y,
        double Z,
        double VelX,
        double VelY,
        double VelZ,
        double Yaw,
        double Pitch,
        double Height,
        double Radius,
        double PrevX,
        double PrevY,
        double PrevZ,
        double PrevYaw,
        double PrevPitch,
        int PortalGroup,
        int PrevPortalGroup,
        bool Player,
        double MinPitch,
        double MaxPitch,
        int Flags2,
        double BobPhase,
        double BobFactor,
        double BobStrength);

    public readonly record struct WarpResult(bool Warped, Body Caller, Body Reference, bool TriedMove);

    public static Proximity CheckProximity(
        bool hasRef,
        bool hasClass,
        double distance,
        int count,
        int flags,
        bool counting,
        int refId,
        double refZ,
        double refHeight,
        int selfTarget,
        int selfMaster,
        int selfTracer,
        int refTarget,
        int refMaster,
        int refTracer,
        IReadOnlyList<Neighbor> neighbors)
    {
        if (!hasRef || !hasClass || distance <= 0)
        {
            return new Proximity(
                0, 0, selfTarget, selfMaster, selfTracer, refTarget, refMaster, refTracer, -1, false);
        }

        int counter = 0;
        int result = 0;
        double closer = distance;
        double farther = 0;
        int chosen = -1;
        bool ptrWillChange = (flags & (SetTarget | SetMaster | SetTracer)) != 0;
        bool ptrDistPref = (flags & (Closest | Farthest)) != 0;
        double refTop = refZ + refHeight;

        foreach (Neighbor mo in neighbors)
        {
            if (mo.Id == refId)
                continue;
            if (mo.Unmorphed)
                continue;
            if ((flags & Ancestor) != 0)
            {
                if (!mo.KindOf)
                    continue;
            }
            else if (!mo.ExactClass)
            {
                continue;
            }

            if (!mo.MapActor)
                continue;

            double moTop = mo.Z + mo.Height;
            bool inSlice = mo.Distance < distance
                && ((flags & NoZ) != 0
                    || (refZ > mo.Z && refZ - moTop < distance)
                    || (refZ <= mo.Z && mo.Z - refTop < distance));
            if (!inSlice)
                continue;

            if ((flags & CheckSight) != 0 && !mo.InSight)
                continue;

            if (mo.Killed)
            {
                if ((flags & (CountDead | DeadOnly)) == 0)
                    continue;
            }
            else if ((flags & DeadOnly) != 0)
            {
                continue;
            }

            if (ptrWillChange)
            {
                double current = mo.Distance;
                if ((flags & Closest) != 0 && current < closer)
                {
                    chosen = mo.Id;
                    closer = current;
                }
                else if ((flags & Farthest) != 0 && current > farther)
                {
                    chosen = mo.Id;
                    farther = current;
                }
                else if (chosen < 0)
                {
                    chosen = mo.Id;
                }
            }

            counter++;
            if (!counting && counter > count)
            {
                result = (flags & (LessOrEqual | Exact)) != 0 ? 0 : 1;
                if (!(ptrWillChange && ptrDistPref))
                    break;
            }
        }

        int nextSelfTarget = selfTarget;
        int nextSelfMaster = selfMaster;
        int nextSelfTracer = selfTracer;
        int nextRefTarget = refTarget;
        int nextRefMaster = refMaster;
        int nextRefTracer = refTracer;
        bool wrote = false;
        if (ptrWillChange && chosen >= 0)
        {
            wrote = true;
            if ((flags & SetOnPtr) != 0)
            {
                if ((flags & SetTarget) != 0)
                    nextRefTarget = chosen;
                if ((flags & SetMaster) != 0)
                    nextRefMaster = chosen;
                if ((flags & SetTracer) != 0)
                    nextRefTracer = chosen;
            }
            else
            {
                if ((flags & SetTarget) != 0)
                    nextSelfTarget = chosen;
                if ((flags & SetMaster) != 0)
                    nextSelfMaster = chosen;
                if ((flags & SetTracer) != 0)
                    nextSelfTracer = chosen;
            }
        }

        if (!counting)
        {
            if (counter == count)
                result = 1;
            else if (counter < count)
                result = (flags & LessOrEqual) != 0 && (flags & Exact) == 0 ? 1 : 0;
        }

        int value = counting ? counter : result;
        return new Proximity(
            value, counter, nextSelfTarget, nextSelfMaster, nextSelfTracer,
            nextRefTarget, nextRefMaster, nextRefTracer, chosen, wrote);
    }

    public static WarpResult Warp(
        bool hasCaller,
        bool hasReference,
        Body caller,
        Body reference,
        double xofs,
        double yofs,
        double zofs,
        double angle,
        int flags,
        double heightOffset,
        double radiusOffset,
        double pitch,
        bool fits,
        double portalShiftX,
        double portalShiftY,
        double floorZ,
        int movedPortalGroup,
        double offsetOldToNewX,
        double offsetOldToNewY,
        double offsetRefPrevX,
        double offsetRefPrevY,
        double offsetRefGroupX,
        double offsetRefGroupY,
        double mapTime,
        bool applyTryMove,
        double tryX,
        double tryY)
    {
        if (!hasCaller || !hasReference)
            return new WarpResult(false, caller, reference, false);

        if ((flags & MovePtr) != 0)
        {
            Body temp = reference;
            reference = caller;
            caller = temp;
        }

        double oldX = caller.X;
        double oldY = caller.Y;
        double oldZ = caller.Z;
        int oldGroup = caller.PortalGroup;
        double shiftedZ = zofs + (reference.Height * heightOffset);
        double ang = angle;
        if ((flags & AbsoluteAngle) == 0)
            ang += (flags & UseCallerAngle) != 0 ? caller.Yaw : reference.Yaw;

        double rad = radiusOffset * reference.Radius;
        double sine = SinDeg(ang);
        double cosine = CosDeg(ang);
        double offsetX = xofs;
        double offsetY = yofs;
        double nextX;
        double nextY;
        double nextZ;
        if ((flags & AbsolutePosition) == 0)
        {
            if ((flags & AbsoluteOffset) == 0)
            {
                offsetX = (xofs * cosine) + (yofs * sine);
                offsetY = (xofs * sine) - (yofs * cosine);
            }

            double dx = offsetX + (rad * cosine);
            double dy = offsetY + (rad * sine);
            nextX = reference.X + dx + portalShiftX;
            nextY = reference.Y + dy + portalShiftY;
            nextZ = (flags & ToFloor) != 0 ? reference.Z : reference.Z + shiftedZ;
        }
        else
        {
            nextX = xofs + (rad * cosine);
            nextY = yofs + (rad * sine);
            nextZ = shiftedZ;
        }

        caller = caller with { X = nextX, Y = nextY, Z = nextZ, PortalGroup = movedPortalGroup };
        if ((flags & ToFloor) != 0)
            caller = caller with { Z = floorZ + shiftedZ };

        bool placed = (flags & NoCheckPosition) != 0 || fits;
        if (!placed)
        {
            caller = caller with { X = oldX, Y = oldY, Z = oldZ, PortalGroup = oldGroup };
            return new WarpResult(false, caller, reference, false);
        }

        if ((flags & TestOnly) != 0)
        {
            caller = caller with { X = oldX, Y = oldY, Z = oldZ, PortalGroup = oldGroup };
            return new WarpResult(true, caller, reference, false);
        }

        caller = caller with { Yaw = ang };
        double pitchNow = caller.Pitch;
        if ((flags & CopyPitch) != 0)
            pitchNow = ApplyPitch(caller, reference.Pitch);
        if (pitch != 0)
            pitchNow = ApplyPitch(caller, pitchNow + pitch);
        caller = caller with { Pitch = pitchNow };

        double velX = caller.VelX;
        double velY = caller.VelY;
        double velZ = caller.VelZ;
        if ((flags & CopyVelocity) != 0)
        {
            velX = reference.VelX;
            velY = reference.VelY;
            velZ = reference.VelZ;
        }

        if ((flags & Stop) != 0)
        {
            velX = 0;
            velY = 0;
            velZ = 0;
        }

        caller = caller with { VelX = velX, VelY = velY, VelZ = velZ };
        if ((flags & WarpInterpolation) != 0)
        {
            double displacedX = oldX + offsetOldToNewX;
            double displacedY = oldY + offsetOldToNewY;
            double displacedZ = oldZ;
            caller = caller with
            {
                PrevX = caller.PrevX + caller.X - displacedX,
                PrevY = caller.PrevY + caller.Y - displacedY,
                PrevZ = caller.PrevZ + caller.Z - displacedZ,
                PrevPortalGroup = caller.PortalGroup,
            };
        }
        else if ((flags & CopyInterpolation) != 0)
        {
            double fromPrevX = oldX + offsetRefPrevX;
            double fromPrevY = oldY + offsetRefPrevY;
            double fromGroupX = oldX + offsetRefGroupX;
            double fromGroupY = oldY + offsetRefGroupY;
            caller = caller with
            {
                PrevX = caller.X + fromPrevX - fromGroupX,
                PrevY = caller.Y + fromPrevY - fromGroupY,
                PrevZ = caller.Z,
                PrevPortalGroup = caller.PortalGroup,
            };
        }
        else if ((flags & Interpolate) == 0)
        {
            caller = caller with
            {
                PrevX = caller.X,
                PrevY = caller.Y,
                PrevZ = caller.Z,
                PrevYaw = caller.Yaw,
                PrevPitch = caller.Pitch,
                PrevPortalGroup = caller.PortalGroup,
            };
        }

        if ((flags & Bob) != 0 && (reference.Flags2 & FloatBob) != 0)
        {
            double bob = BobOffset(reference.BobPhase, mapTime, reference.BobFactor, reference.BobStrength);
            caller = caller with { Z = caller.Z + bob };
        }

        if (applyTryMove)
            caller = caller with { X = tryX, Y = tryY };

        return new WarpResult(true, caller, reference, true);
    }

    private static double ApplyPitch(Body caller, double pitch)
    {
        if (!caller.Player)
            return pitch;
        if (pitch < caller.MinPitch)
            return caller.MinPitch;
        if (pitch > caller.MaxPitch)
            return caller.MaxPitch;
        return pitch;
    }

    private static double BobOffset(double phase, double mapTime, double factor, double strength)
    {
        double bob = Math.Sin((phase + (mapTime * factor)) * (180d / 32d) * (Math.PI / 180d)) * 8d;
        return bob * strength;
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
