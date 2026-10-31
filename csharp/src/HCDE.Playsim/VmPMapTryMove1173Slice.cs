namespace HCDE.Playsim;

/// <summary>
/// Crossing checks of <c>P_TryMove</c> from <c>p_map.cpp</c> (<b>2359–2726</b>). Chunk <b>1173</b>.
/// The push-line label and the successful link are <b>2727–2898</b>.
/// </summary>
public static class VmPMapTryMove1173Slice
{
    public const int MfShootable = 0x00000004;
    public const int MfNoGravity = 0x00000200;
    public const int MfDropoff = 0x00000400;
    public const int MfNoClip = 0x00001000;
    public const int MfFloat = 0x00004000;
    public const int MfTeleport = 0x00008000;
    public const int MfMissile = 0x00010000;
    public const int Mf2Blasted = 0x00000008;
    public const int Mf2Fly = 0x00000010;
    public const int Mf2OnMobj = 0x00000800;
    public const int Mf2PassMobj = 0x00001000;
    public const int Mf2CantLeaveFloorPic = 0x02000000;
    public const int Mf3FloorHugger = 0x00000001;
    public const int Mf3CeilingHugger = 0x00000002;
    public const int Mf4ScrollMove = 0x00080000;
    public const int Mf5NoDropoff = 0x00000004;
    public const int Mf5AvoidingDropoff = 0x00000020;
    public const int Mf6StepMissile = 0x00000080;
    public const int Mf6InTryMove = 0x10000000;
    public const int BounceFloors = 1 << 1;
    public const int BounceMbf = 1 << 12;
    public const int CfPredicting = 1 << 13;
    public const int CompatNoPassMobj = 1 << 4;
    public const int CompatCrossDropoff = 1 << 20;
    public const int PorttVisual = 0;
    public const int PorttTeleport = 1;
    public const int PorttInteractive = 2;
    public const int PorttLinked = 3;

    public enum CrossingKind
    {
        Continue,
        PushLine,
        Fail,
    }

    public sealed class Actor
    {
        public double X;
        public double Y;
        public double Z;
        public double PrevX;
        public double PrevY;
        public double PrevZ;
        public double VelX;
        public double VelY;
        public double VelZ;
        public double Yaw;
        public double Height = 56;
        public double MaxStepHeight = 24;
        public double MaxDropOffHeight = 24;
        public double FloorZ;
        public double CeilingZ;
        public double DropoffZ;
        public int FloorPic;
        public string FloorTerrain = "";
        public string FloorSector = "home";
        public int CeilingPic;
        public string CeilingSector = "home";
        public string Sector = "home";
        public int PortalGroup;
        public int Flags;
        public int Flags2;
        public int Flags3;
        public int Flags4;
        public int Flags5;
        public int Flags6;
        public int BounceFlags;
        public int Cheats;
        public bool IsPlayer;
        public bool HasBot;
        public bool BotPrevCopied;
        public bool BotHasDest = true;
        public bool Sentient;
        public bool HasTarget;
        public double TargetZ;
        public double ViewHeight = 41;
        public int Mass = 100;
        public bool CrossingPortal;
        public double Top => Z + Height;
    }

    public sealed class Trace
    {
        public double PosX;
        public double PosY;
        public double FloorZ;
        public double CeilingZ = 128;
        public double DropoffZ;
        public int FloorPic;
        public string FloorTerrain = "";
        public string FloorSector = "home";
        public int CeilingPic;
        public string CeilingSector = "home";
        public string Sector = "home";
        public bool FloatOk;
        public bool PortalStep;
        public int PortalGroup;
    }

    public sealed class PortalHit
    {
        public int Id;
        public int FrontGroup;
        public int OldSide;
        public int Side = 1;
        public double Frac;
        public int Type = PorttLinked;
        public double DispX;
        public double DispY;
        public double ShiftX;
        public double ShiftY;
        public double ShiftZ;
        public double VelShiftX;
        public double VelShiftY;
        public double AngleShift;
        public double OldX;
        public double OldY;
        public double RefX;
        public double RefY;
    }

    public sealed class CamPoint
    {
        public double X;
        public double Y;
        public double Z;
        public double Angle;
    }

    public sealed class CrossingHost
    {
        public bool PositionOk = true;
        public bool HasBlocking;
        public bool BlockingIsPlayer;
        public double BlockingTop;
        public double BlockingCeilingZ = 256;
        public bool BlockingPortalBlocks;
        public bool CheckSetsPortalStep;
        public bool PortalPositionOk = true;
        public bool PortalHasBlocking;
        public bool Dangerous;
        public bool StepTestOk = true;
        public bool Has3DFloor;
        public bool OldHasHeightSec;
        public double OldFakeFloorZ;
        public double OldFakeCeilingZ;
        public bool IsCamera;
        public bool HasConsolePlayer;
        public double ViewZ;
        public int Compat;
        public bool Probed;
        public double ProbeX;
        public double ProbeY;
        public double ProbeZ;
        public double ProbeCheckX;
        public double ProbeCheckY;
        public List<string> Events { get; } = new();
        public List<PortalHit> Portals { get; } = new();
        public List<CamPoint> Points { get; } = new();
    }

    public sealed class CrossingResult
    {
        public CrossingKind Kind;
        public bool PortalCrossed;
        public double OldZ;
        public string OldSector = "";
        public double ViewHeight;
        public bool OldHasHeightSec;
        public bool OldAboveFakeFloor;
        public bool OldAboveFakeCeiling;
    }

    /// <summary>
    /// Native <c>cheats &amp;&amp; CF_PREDICTING</c>. The flag is a nonzero constant, so any cheat word counts.
    /// </summary>
    static bool CheatsAndPredictingFlag(int cheats)
    {
        if (cheats == 0)
        {
            return false;
        }

        return CfPredicting != 0;
    }

    public static CrossingResult CrossingChecks(
        Actor thing,
        double destX,
        double destY,
        int dropoff,
        bool onFloor,
        double onFloorZ,
        Trace tm,
        bool missileCheck,
        CrossingHost host)
    {
        var oldZ = thing.Z;
        var oldSector = thing.Sector;
        var viewHeight = 0d;
        var oldHasHeight = host.OldHasHeightSec;
        var oldAboveFloor = false;
        var oldAboveCeiling = false;
        var portalCrossed = false;

        CrossingResult Done(CrossingKind kind)
        {
            return new CrossingResult
            {
                Kind = kind,
                PortalCrossed = portalCrossed,
                OldZ = oldZ,
                OldSector = oldSector,
                ViewHeight = viewHeight,
                OldHasHeightSec = oldHasHeight,
                OldAboveFakeFloor = oldAboveFloor,
                OldAboveFakeCeiling = oldAboveCeiling,
            };
        }

        tm.FloatOk = false;
        tm.PortalStep = false;
        if (onFloor)
        {
            thing.Z = onFloorZ;
        }

        thing.Flags6 |= Mf6InTryMove;
        tm.PosX = destX;
        tm.PosY = destY;
        if (host.CheckSetsPortalStep)
        {
            tm.PortalStep = true;
        }

        if (!host.PositionOk)
        {
            var predicting = (thing.Cheats & CfPredicting) != 0;
            if (host.HasBlocking && (!thing.IsPlayer || !predicting))
            {
                host.Events.Add("collide");
            }

            if (!host.HasBlocking || host.BlockingIsPlayer || !thing.IsPlayer)
            {
                return Done(CrossingKind.PushLine);
            }

            if (host.BlockingIsPlayer || !thing.IsPlayer)
            {
                return Done(CrossingKind.PushLine);
            }
            else if (host.BlockingTop - thing.Z > thing.MaxStepHeight
                || ((host.BlockingCeilingZ - host.BlockingTop < thing.Height) && host.BlockingPortalBlocks)
                || (tm.CeilingZ - host.BlockingTop < thing.Height))
            {
                return Done(CrossingKind.PushLine);
            }

            if ((thing.Flags2 & Mf2PassMobj) == 0 || (host.Compat & CompatNoPassMobj) != 0)
            {
                thing.Z = oldZ;
                thing.Flags6 &= ~Mf6InTryMove;
                return Done(CrossingKind.Fail);
            }
        }

        if ((thing.Flags3 & Mf3FloorHugger) != 0)
        {
            thing.Z = tm.FloorZ;
        }
        else if ((thing.Flags3 & Mf3CeilingHugger) != 0)
        {
            thing.Z = tm.CeilingZ - thing.Height;
        }

        if (onFloor && tm.FloorSector == thing.FloorSector)
        {
            thing.Z = tm.FloorZ;
        }

        if ((thing.Flags & MfNoClip) == 0)
        {
            if (tm.CeilingZ - tm.FloorZ < thing.Height)
            {
                return Done(CrossingKind.PushLine);
            }

            tm.FloatOk = true;
            if ((thing.Flags & MfTeleport) == 0
                && tm.CeilingZ < thing.Top
                && (thing.Flags3 & Mf3CeilingHugger) == 0
                && ((thing.Flags2 & Mf2Fly) == 0 || (thing.Flags & MfNoGravity) == 0))
            {
                return Done(CrossingKind.PushLine);
            }

            if ((thing.Flags2 & Mf2Fly) != 0 && (thing.Flags & MfNoGravity) != 0)
            {
                if (thing.Top > tm.CeilingZ)
                {
                    return Done(CrossingKind.PushLine);
                }
            }

            if ((thing.Flags & MfTeleport) == 0
                && ((thing.Flags3 & Mf3FloorHugger) == 0 || (thing.Flags5 & Mf5NoDropoff) != 0))
            {
                if ((thing.Flags & MfMissile) != 0
                    && (thing.Flags6 & Mf6StepMissile) == 0
                    && tm.FloorZ > thing.Z
                    && (thing.Flags3 & Mf3FloorHugger) == 0)
                {
                    return Done(CrossingKind.PushLine);
                }

                if (tm.FloorZ - thing.Z > thing.MaxStepHeight)
                {
                    return Done(CrossingKind.PushLine);
                }
                else if (thing.Z < tm.FloorZ)
                {
                    var savedZ = thing.Z;
                    thing.Z = tm.FloorZ;
                    var good = host.StepTestOk;
                    thing.Z = savedZ;
                    if (!good)
                    {
                        return Done(CrossingKind.PushLine);
                    }

                    if ((thing.Flags6 & Mf6StepMissile) != 0)
                    {
                        thing.Z = tm.FloorZ;
                        if (thing.VelZ < 0)
                        {
                            if ((thing.BounceFlags & BounceFloors) != 0)
                            {
                                host.Events.Add(host.Has3DFloor ? "bounce:3d" : "bounce:floor");
                            }
                            else
                            {
                                thing.VelZ = 0;
                            }
                        }
                    }
                }
            }

            if ((host.Compat & CompatCrossDropoff) != 0 && (thing.Flags4 & Mf4ScrollMove) == 0)
            {
                dropoff = 0;
            }

            if (dropoff == 2 && (tm.FloorZ - tm.DropoffZ > 128d || !thing.HasTarget || thing.TargetZ > tm.DropoffZ))
            {
                dropoff = 0;
            }

            if ((dropoff == 0 && (thing.Flags & (MfDropoff | MfFloat | MfMissile)) == 0) || (thing.Flags5 & Mf5NoDropoff) != 0)
            {
                if ((thing.Flags5 & Mf5AvoidingDropoff) == 0)
                {
                    var floorZ = tm.FloorZ;
                    if ((thing.Flags2 & Mf2OnMobj) != 0)
                    {
                        floorZ = Math.Max(thing.Z, tm.FloorZ);
                    }

                    if (floorZ - tm.DropoffZ > thing.MaxDropOffHeight && (thing.Flags2 & Mf2Blasted) == 0 && !missileCheck)
                    {
                        thing.Z = oldZ;
                        thing.Flags6 &= ~Mf6InTryMove;
                        return Done(CrossingKind.Fail);
                    }
                }
                else if (thing.FloorZ - tm.FloorZ > thing.MaxDropOffHeight || thing.DropoffZ - tm.DropoffZ > thing.MaxDropOffHeight)
                {
                    thing.Flags6 &= ~Mf6InTryMove;
                    return Done(CrossingKind.Fail);
                }
            }

            if ((thing.Flags2 & Mf2CantLeaveFloorPic) != 0
                && (tm.FloorPic != thing.FloorPic || tm.FloorZ - thing.Z != 0d))
            {
                thing.Z = oldZ;
                thing.Flags6 &= ~Mf6InTryMove;
                return Done(CrossingKind.Fail);
            }

            if (thing.IsPlayer && thing.HasBot && (thing.Flags & MfShootable) != 0)
            {
                if (tm.Sector != thing.Sector && host.Dangerous)
                {
                    thing.BotPrevCopied = true;
                    thing.BotHasDest = false;
                    thing.VelX = 0;
                    thing.VelY = 0;
                    thing.Z = oldZ;
                    thing.Flags6 &= ~Mf6InTryMove;
                    return Done(CrossingKind.Fail);
                }
            }
        }

        viewHeight = thing.IsPlayer ? thing.ViewHeight : thing.Height / 2d;
        if (host.OldHasHeightSec)
        {
            var eyeZ = oldZ + viewHeight;
            oldAboveFloor = eyeZ > host.OldFakeFloorZ;
            oldAboveCeiling = eyeZ > host.OldFakeCeilingZ;
        }

        if ((thing.BounceFlags & BounceMbf) != 0
            && (thing.Flags & (MfMissile | MfNoGravity)) == 0
            && !thing.Sentient
            && tm.FloorZ - thing.Z > 16d)
        {
            thing.Flags6 &= ~Mf6InTryMove;
            return Done(CrossingKind.Fail);
        }

        while (true)
        {
            var bestFrac = 1.1;
            var best = -1;
            for (var i = 0; i < host.Portals.Count; i++)
            {
                var spec = host.Portals[i];
                if (spec.FrontGroup != thing.PortalGroup)
                {
                    continue;
                }

                if (spec.OldSide == 0 && spec.Side == 1 && spec.Frac < bestFrac)
                {
                    bestFrac = spec.Frac;
                    best = i;
                }
            }

            if (bestFrac < 1.1)
            {
                var hit = host.Portals[best];
                host.Portals.RemoveAt(best);
                host.Events.Add("portal:" + hit.Id);
                if (hit.Type == PorttLinked)
                {
                    host.Events.Add("unlink");
                    thing.X = tm.PosX + hit.DispX;
                    thing.Y = tm.PosY + hit.DispY;
                    thing.PrevX += hit.DispX;
                    thing.PrevY += hit.DispY;
                    host.Events.Add("link");
                    host.Events.Add("floorceil");
                    portalCrossed = true;
                    tm.PortalStep = false;
                    tm.PosX += hit.DispX;
                    tm.PosY += hit.DispY;
                }
                else if (!portalCrossed)
                {
                    var oldX = thing.X;
                    var oldY = thing.Y;
                    var oldBodyZ = thing.Z;
                    var checkX = tm.PosX + hit.ShiftX;
                    var checkY = tm.PosY + hit.ShiftY;
                    var shiftedZ = thing.Z + hit.ShiftZ;
                    thing.X += hit.ShiftX;
                    thing.Y += hit.ShiftY;
                    thing.Z = shiftedZ;
                    host.Probed = true;
                    host.ProbeX = thing.X;
                    host.ProbeY = thing.Y;
                    host.ProbeZ = thing.Z;
                    host.ProbeCheckX = checkX;
                    host.ProbeCheckY = checkY;
                    if (!host.PortalPositionOk)
                    {
                        if (host.PortalHasBlocking && (!thing.IsPlayer || !CheatsAndPredictingFlag(thing.Cheats)))
                        {
                            host.Events.Add("collide-portal");
                        }

                        thing.X = oldX;
                        thing.Y = oldY;
                        thing.Z = oldBodyZ;
                        thing.Flags6 &= ~Mf6InTryMove;
                        return Done(CrossingKind.Fail);
                    }

                    host.Events.Add("unlink");
                    thing.X = checkX;
                    thing.Y = checkY;
                    thing.Z = shiftedZ;
                    thing.VelX += hit.VelShiftX;
                    thing.VelY += hit.VelShiftY;
                    thing.Yaw += hit.AngleShift;
                    host.Events.Add("link");
                    host.Events.Add("floorceil");
                    host.Events.Add("clear-interp");
                    portalCrossed = true;
                    tm.PortalStep = false;
                }

                if (host.IsCamera)
                {
                    var hitX = hit.OldX + (hit.RefX - hit.OldX) * bestFrac;
                    var hitY = hit.OldY + (hit.RefY - hit.OldY) * bestFrac;
                    var hitZ = 0d;
                    var hitAngle = 0d;
                    host.Points.Add(new CamPoint { X = hitX, Y = hitY, Z = hitZ, Angle = hitAngle });
                    if (hit.Type == PorttLinked)
                    {
                        hitX += hit.DispX;
                        hitY += hit.DispY;
                    }
                    else
                    {
                        hitX += hit.ShiftX;
                        hitY += hit.ShiftY;
                        hitZ += hit.ShiftZ;
                        if (host.HasConsolePlayer)
                        {
                            host.ViewZ += hitZ;
                        }

                        hitAngle += hit.AngleShift;
                        if (thing.IsPlayer && (hit.Type == PorttInteractive || hit.Type == PorttTeleport))
                        {
                            thing.CrossingPortal = true;
                        }
                    }

                    host.Points.Add(new CamPoint { X = hitX, Y = hitY, Z = hitZ, Angle = hitAngle });
                }

                if (hit.Type == PorttLinked)
                {
                    continue;
                }
            }

            break;
        }

        return Done(CrossingKind.Continue);
    }
}
