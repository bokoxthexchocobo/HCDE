namespace HCDE.Playsim;

/// <summary>
/// Successful <c>P_TryMove</c> link and the push-line label from <c>p_map.cpp</c> (<b>2727–2898</b>). Chunk <b>1174</b>.
/// Crossing checks are chunk <b>1173</b>.
/// </summary>
public static class VmPMapTryMove1174Slice
{
    public const int Mf2FloorClip = 0x00000020;
    public const int Mf2MCross = 0x00800000;
    public const int Mf2PCross = 0x01000000;
    public const int Mf6NoTrigger = 0x00010000;
    public const int SpacCross = 1 << 0;
    public const int SpacMCross = 1 << 2;
    public const int SpacPCross = 1 << 5;
    public const int SpacAnyCross = 1 << 7;
    public const int Teleport = 70;
    public const int TeleportNoFog = 71;
    public const int TeleportLine = 215;
    public const int SecEyesDive = 1 << 6;
    public const int SecEyesSurface = 1 << 7;
    public const int SecEyesBelowC = 1 << 8;
    public const int SecEyesAboveC = 1 << 9;
    public const int SecMfFakeFloorOnly = 2;

    public sealed class SpecHit
    {
        public int Id;
        public int Special;
        public int Side;
        public int OldSide;
    }

    public sealed class LinkHost
    {
        public string LinkedFromSector = "";
        public double LastX;
        public double LastY;
        public bool NewHasHeightSec;
        public bool NewHasAction;
        public double NewFakeFloorZ;
        public double NewFakeCeilingZ;
        public int HeightMoreFlags;
        public double RelX;
        public double RelY;
        public double RelZ;
        public string RelSector = "";
        public int RelGroup;
        public int PrevPortalGroup;
        public List<string> Events { get; } = new();
        public List<SpecHit> Specs { get; } = new();
    }

    public static bool TryMove(
        VmPMapTryMove1173Slice.Actor thing,
        double destX,
        double destY,
        int dropoff,
        bool onFloor,
        double onFloorZ,
        bool missileCheck,
        VmPMapTryMove1173Slice.CrossingHost crossingHost,
        LinkHost link)
    {
        var tm = new VmPMapTryMove1173Slice.Trace();
        return TryMove(thing, destX, destY, dropoff, onFloor, onFloorZ, tm, missileCheck, crossingHost, link);
    }

    public static bool TryMove(
        VmPMapTryMove1173Slice.Actor thing,
        double destX,
        double destY,
        int dropoff,
        bool onFloor,
        double onFloorZ,
        VmPMapTryMove1173Slice.Trace tm,
        bool missileCheck,
        VmPMapTryMove1173Slice.CrossingHost crossingHost,
        LinkHost link)
    {
        var crossing = VmPMapTryMove1173Slice.CrossingChecks(
            thing, destX, destY, dropoff, onFloor, onFloorZ, tm, missileCheck, crossingHost);
        if (crossing.Kind == VmPMapTryMove1173Slice.CrossingKind.Fail)
        {
            return false;
        }

        if (crossing.Kind == VmPMapTryMove1173Slice.CrossingKind.PushLine)
        {
            return PushLine(thing, crossing, link);
        }

        if (!crossing.PortalCrossed)
        {
            link.Events.Add("unlink");
            link.LinkedFromSector = thing.Sector;
            thing.FloorZ = tm.FloorZ;
            thing.CeilingZ = tm.CeilingZ;
            thing.DropoffZ = tm.DropoffZ;
            thing.FloorPic = tm.FloorPic;
            thing.FloorTerrain = tm.FloorTerrain;
            thing.FloorSector = tm.FloorSector;
            thing.CeilingPic = tm.CeilingPic;
            thing.CeilingSector = tm.CeilingSector;
            thing.X = destX;
            thing.Y = destY;
            link.Events.Add("link");
        }

        if ((thing.Flags2 & Mf2FloorClip) != 0)
        {
            link.Events.Add("floorclip");
        }

        if ((thing.Flags & (VmPMapTryMove1173Slice.MfTeleport | VmPMapTryMove1173Slice.MfNoClip)) == 0)
        {
            link.LastX = thing.X;
            link.LastY = thing.Y;
            while (link.Specs.Count > 0)
            {
                var spec = link.Specs[link.Specs.Count - 1];
                link.Specs.RemoveAt(link.Specs.Count - 1);
                if (spec.Side != spec.OldSide && spec.Special != 0 && (thing.Flags6 & Mf6NoTrigger) == 0)
                {
                    link.Events.Add(Activation(thing, spec) + ":" + spec.Id);
                }
            }
        }

        if (thing.IsPlayer && (thing.Cheats & VmPMapTryMove1173Slice.CfPredicting) != 0)
        {
            thing.Flags6 &= ~VmPMapTryMove1173Slice.Mf6InTryMove;
            return true;
        }

        if (link.NewHasHeightSec && crossing.OldHasHeightSec && link.NewHasAction)
        {
            var eyeZ = thing.Z + crossing.ViewHeight;
            if (!crossing.OldAboveFakeFloor && eyeZ > link.NewFakeFloorZ)
            {
                link.Events.Add("eyes:" + SecEyesSurface);
            }
            else if (crossing.OldAboveFakeFloor && eyeZ <= link.NewFakeFloorZ)
            {
                link.Events.Add("eyes:" + SecEyesDive);
            }

            if ((link.HeightMoreFlags & SecMfFakeFloorOnly) == 0)
            {
                if (!crossing.OldAboveFakeCeiling && eyeZ > link.NewFakeCeilingZ)
                {
                    link.Events.Add("eyes:" + SecEyesAboveC);
                }
                else if (crossing.OldAboveFakeCeiling && eyeZ <= link.NewFakeCeilingZ)
                {
                    link.Events.Add("eyes:" + SecEyesBelowC);
                }
            }
        }

        if (tm.PortalStep)
        {
            link.Events.Add("unlink");
            var oldX = thing.X;
            var oldY = thing.Y;
            var oldZ = thing.Z;
            thing.X = link.RelX;
            thing.Y = link.RelY;
            thing.Z = link.RelZ;
            thing.PrevX += thing.X - oldX;
            thing.PrevY += thing.Y - oldY;
            thing.PrevZ += thing.Z - oldZ;
            thing.Sector = link.RelSector;
            link.PrevPortalGroup = link.RelGroup;
            link.Events.Add("link");
            link.Events.Add("floorceil");
        }

        link.Events.Add("transition:" + crossing.OldSector);
        thing.Flags6 &= ~VmPMapTryMove1173Slice.Mf6InTryMove;
        return true;
    }

    static string Activation(VmPMapTryMove1173Slice.Actor thing, SpecHit spec)
    {
        if (thing.IsPlayer && (thing.Cheats & VmPMapTryMove1173Slice.CfPredicting) != 0)
        {
            return "predict:" + SpacCross;
        }

        if (thing.IsPlayer)
        {
            return "cross:" + SpacCross;
        }

        if ((thing.Flags2 & Mf2MCross) != 0)
        {
            return "mcross:" + SpacMCross;
        }

        if ((thing.Flags2 & Mf2PCross) != 0)
        {
            return "pcross:" + SpacPCross;
        }

        if (spec.Special == Teleport || spec.Special == TeleportNoFog || spec.Special == TeleportLine)
        {
            return "mcross:" + SpacMCross;
        }

        return "any:" + SpacAnyCross;
    }

    static bool PushLine(
        VmPMapTryMove1173Slice.Actor thing,
        VmPMapTryMove1173Slice.CrossingResult crossing,
        LinkHost link)
    {
        thing.Flags6 &= ~VmPMapTryMove1173Slice.Mf6InTryMove;
        thing.Z = crossing.OldZ;
        if (thing.IsPlayer && (thing.Cheats & VmPMapTryMove1173Slice.CfPredicting) != 0)
        {
            return false;
        }

        if ((thing.Flags & (VmPMapTryMove1173Slice.MfTeleport | VmPMapTryMove1173Slice.MfNoClip)) == 0)
        {
            if ((thing.Flags2 & VmPMapTryMove1173Slice.Mf2Blasted) != 0)
            {
                link.Events.Add("damage:" + (thing.Mass >> 5) + ":melee");
            }

            for (var i = link.Specs.Count - 1; i >= 0; i--)
            {
                var spec = link.Specs[i];
                link.Events.Add("push:" + spec.Id + ":" + spec.Side);
            }
        }

        return false;
    }
}
