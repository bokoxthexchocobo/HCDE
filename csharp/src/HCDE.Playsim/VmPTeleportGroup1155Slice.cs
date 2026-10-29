namespace HCDE.Playsim;

/// <summary>
/// Tid, group, and sector teleports from <c>p_teleport.cpp</c> (<b>644–806</b>). Chunk <b>1155</b>.
/// </summary>
public static class VmPTeleportGroup1155Slice
{
    public const int DestFog = 1;
    public const int SourceFog = 2;
    public const int KeepOrientation = 4;
    public const double OnFloorZ = -2147483648d;

    public readonly record struct OtherMove(bool Did, int Flags, int Calls);

    public readonly record struct Placed(bool Did, double X, double Y, double Z, int Flags, double Yaw);

    public readonly record struct ActorSpot(double X, double Y, double Z, double Yaw, bool Teleported);

    public readonly record struct Touch(bool Present, int Tid, double X, double Y, double Z, double Yaw, bool Teleported);

    public readonly record struct GroupMove(
        bool Did,
        bool FellBack,
        int Flags,
        int Calls,
        bool FloorZ,
        IReadOnlyList<Placed> Actors,
        bool SourceMoved,
        double SourceX,
        double SourceY,
        double SourceZ,
        int SourceFlags,
        double SourceYaw);

    public static uint ToBams(double degrees)
    {
        double rounded = Math.Round(degrees * (0x40000000 / 90d), MidpointRounding.ToEven);
        return unchecked((uint)(long)rounded);
    }

    public static double Normalize360(double degrees)
    {
        return (90d / 0x40000000) * ToBams(degrees);
    }

    public static OtherMove TeleportOther(int otherTid, int destTid, bool fog, IReadOnlyList<bool> results)
    {
        if (otherTid == 0 || destTid == 0)
            return new OtherMove(false, 0, 0);

        int flags = KeepOrientation;
        if (fog)
            flags = DestFog | SourceFog;
        bool did = false;
        foreach (bool ok in results)
        {
            if (ok)
                did = true;
        }

        return new OtherMove(did, flags, results.Count);
    }

    public static Placed MoveOne(
        double victimX,
        double victimY,
        double victimZ,
        double victimYaw,
        double sourceX,
        double sourceY,
        double sourceZ,
        double sourceYaw,
        double destX,
        double destY,
        double destZ,
        double destYaw,
        bool floorZ,
        bool fog,
        bool teleported)
    {
        double turn = destYaw - sourceYaw;
        double offX = victimX - sourceX;
        double offY = victimY - sourceY;
        double cosine = CosDeg(turn);
        double sine = SinDeg(turn);
        double rotatedX = (offX * cosine) - (offY * sine);
        double rotatedY = (offX * sine) + (offY * cosine);
        double z = destZ + victimZ - sourceZ;
        if (floorZ)
            z = OnFloorZ;
        int flags = KeepOrientation;
        if (fog)
            flags = DestFog | SourceFog | KeepOrientation;
        double yaw = Normalize360((destYaw + victimYaw) - sourceYaw);
        return new Placed(teleported, destX + rotatedX, destY + rotatedY, z, flags, yaw);
    }

    public static GroupMove TeleportGroup(
        int groupTid,
        int destTid,
        bool hasSource,
        double sourceX,
        double sourceY,
        double sourceZ,
        double sourceYaw,
        bool hasDest,
        double destX,
        double destY,
        double destZ,
        double destYaw,
        bool destIsDest2,
        bool hasDirectVictim,
        ActorSpot direct,
        bool moveSource,
        bool fog,
        bool sourceOk,
        IReadOnlyList<ActorSpot> members)
    {
        if (!hasSource)
        {
            var oks = new List<bool>(members.Count);
            foreach (ActorSpot member in members)
                oks.Add(member.Teleported);
            OtherMove other = TeleportOther(groupTid, destTid, fog, oks);
            return new GroupMove(other.Did, true, other.Flags, other.Calls, false, Array.Empty<Placed>(), false, 0, 0, 0, 0, 0);
        }

        if (!hasDest)
            return Empty();

        bool floorZ = !destIsDest2;
        var placed = new List<Placed>();
        if (groupTid == 0 && hasDirectVictim)
        {
            placed.Add(MoveOne(
                direct.X, direct.Y, direct.Z, direct.Yaw,
                sourceX, sourceY, sourceZ, sourceYaw,
                destX, destY, destZ, destYaw,
                floorZ, fog, direct.Teleported));
        }
        else
        {
            foreach (ActorSpot member in members)
            {
                placed.Add(MoveOne(
                    member.X, member.Y, member.Z, member.Yaw,
                    sourceX, sourceY, sourceZ, sourceYaw,
                    destX, destY, destZ, destYaw,
                    floorZ, fog, member.Teleported));
            }
        }

        bool did = false;
        foreach (Placed actor in placed)
        {
            if (actor.Did)
                did = true;
        }

        bool sourceMoved = false;
        double movedX = 0;
        double movedY = 0;
        double movedZ = 0;
        int sourceFlags = 0;
        double movedYaw = 0;
        int calls = placed.Count;
        if (moveSource && did)
        {
            sourceMoved = true;
            movedX = destX;
            movedY = destY;
            movedZ = floorZ ? OnFloorZ : destZ;
            sourceFlags = KeepOrientation;
            movedYaw = destYaw;
            calls++;
            if (sourceOk)
                did = true;
        }

        return new GroupMove(did, false, 0, calls, floorZ, placed, sourceMoved, movedX, movedY, movedZ, sourceFlags, movedYaw);
    }

    public static GroupMove TeleportSector(
        bool hasSource,
        double sourceX,
        double sourceY,
        double sourceZ,
        double sourceYaw,
        bool hasDest,
        double destX,
        double destY,
        double destZ,
        double destYaw,
        bool destIsDest2,
        bool fog,
        int groupTid,
        IReadOnlyList<Touch> touching)
    {
        if (!hasSource || !hasDest)
            return Empty();

        bool floorZ = !destIsDest2;
        var placed = new List<Placed>();
        bool did = false;
        foreach (Touch touch in touching)
        {
            if (!touch.Present)
                continue;
            if (groupTid != 0 && touch.Tid != groupTid)
                continue;
            Placed moved = MoveOne(
                touch.X, touch.Y, touch.Z, touch.Yaw,
                sourceX, sourceY, sourceZ, sourceYaw,
                destX, destY, destZ, destYaw,
                floorZ, fog, touch.Teleported);
            placed.Add(moved);
            if (moved.Did)
                did = true;
        }

        return new GroupMove(did, false, 0, placed.Count, floorZ, placed, false, 0, 0, 0, 0, 0);
    }

    private static GroupMove Empty()
    {
        return new GroupMove(false, false, 0, 0, false, Array.Empty<Placed>(), false, 0, 0, 0, 0, 0);
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
