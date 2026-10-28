namespace HCDE.Playsim;

/// <summary>Sector effect thinkers and <c>sector_t::MoveFloor</c> / <c>MoveCeiling</c> from <c>dsectoreffect.cpp</c>.</summary>
public static class VmDSectorEffect
{
    public const int CompatFloorMove = 1 << 0;

    public sealed class SectorEffectLinks
    {
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public object? LightingData { get; set; }
    }

    public static void SectorEffectOnDestroy(SectorEffectLinks sector, object effect, Action<object> superOnDestroy)
    {
        if (sector.FloorData == effect)
            sector.FloorData = null;
        if (sector.CeilingData == effect)
            sector.CeilingData = null;
        if (sector.LightingData == effect)
            sector.LightingData = null;
        superOnDestroy(effect);
    }

    public static void SectorEffectConstruct(Action<object, object?> bindSector, object effect, object? sector) =>
        bindSector(effect, sector);

    public static void MoverConstruct(Action<object, object?> superConstruct, object effect, object? sector)
    {
        superConstruct(effect, sector);
    }

    public static void MoverOnDestroy(object? interpolation, Action<bool> delRef, Action superOnDestroy)
    {
        StopInterpolation(interpolation, delRef, false);
        superOnDestroy();
    }

    public static void StopInterpolation(object? interpolation, Action<bool> delRef, bool force)
    {
        if (interpolation != null)
        {
            delRef(force);
            interpolation = null;
        }
    }

    public static void MovingFloorConstruct(
        SectorEffectLinks sector,
        object effect,
        Action<object, object?> superConstruct,
        object? boundSector,
        Action<object> setInterpolation)
    {
        superConstruct(effect, boundSector);
        sector.FloorData = effect;
        setInterpolation(effect);
    }

    public static void MovingCeilingConstruct(
        SectorEffectLinks sector,
        object effect,
        bool interpolate,
        Action<object, object?> superConstruct,
        object? boundSector,
        Action<object> setInterpolation)
    {
        superConstruct(effect, boundSector);
        sector.CeilingData = effect;
        if (interpolate)
            setInterpolation(effect);
    }

    public sealed class CrushScratch
    {
        public bool Crushed { get; set; }
    }

    public sealed class SectorPlanes
    {
        public double FloorD { get; set; }
        public double CeilingD { get; set; }
        public bool FloorSlope { get; set; }
        public bool CeilingSlope { get; set; }
        public bool PortalLinkedFloor { get; set; }
        public bool PortalLinkedCeiling { get; set; }
        public int CompatFlags2 { get; set; }
        public Func<double, double>? FloorGetChangedHeight { get; set; }
        public Func<double, double, double>? FloorHeightDiff { get; set; }
        public Func<double, double>? CeilingGetChangedHeight { get; set; }
        public Func<double, double, double>? CeilingHeightDiff { get; set; }
        public Action<double> SetFloorD { get; set; } = _ => { };
        public Action<double> SetCeilingD { get; set; } = _ => { };
    }

    public static bool MoveAttached(
        int crush,
        double move,
        int floorOrCeiling,
        bool resetFailed,
        bool instant,
        ref bool crushed,
        Func<int, double, int, bool, bool, bool> scroll3dMidtex,
        Func<int, double, int, bool, bool, bool> moveLinkedSectors)
    {
        crushed = false;
        if (!scroll3dMidtex(crush, move, floorOrCeiling, instant, false))
        {
            crushed = true;
            if (resetFailed)
            {
                scroll3dMidtex(crush, -move, floorOrCeiling, instant, false);
                return false;
            }
        }

        if (!moveLinkedSectors(crush, move, floorOrCeiling, instant, false))
        {
            crushed = true;
            if (resetFailed)
            {
                moveLinkedSectors(crush, -move, floorOrCeiling, instant, false);
                scroll3dMidtex(crush, -move, floorOrCeiling, instant, false);
                return false;
            }
        }

        return true;
    }

    public static VmADoorTickSound.MoveResult MoveFloor(
        SectorPlanes planes,
        double speed,
        double dest,
        int crush,
        int direction,
        bool hexenCrush,
        bool instant,
        Func<int, double, int, bool, bool, CrushScratch, bool> moveAttached,
        Func<int, double, int, bool, bool, bool> changeSector,
        Action<double> changeFloorTexZ,
        Action adjustFloorClip)
    {
        var scratch = new CrushScratch();
        var lastPos = planes.FloorD;
        switch (direction)
        {
            case -1:
            {
                var movedest = planes.FloorGetChangedHeight?.Invoke(-speed) ?? (lastPos - speed);
                if (movedest >= dest)
                {
                    var move = planes.FloorHeightDiff?.Invoke(lastPos, dest) ?? (dest - lastPos);
                    if (!moveAttached(crush, move, 0, true, instant, scratch))
                        return VmADoorTickSound.MoveResult.PastDest;
                    planes.SetFloorD(dest);
                    if (changeSector(crush, move, 0, false, instant))
                    {
                        planes.SetFloorD(lastPos);
                        changeSector(crush, -move, 0, true, instant);
                        moveAttached(crush, -move, 0, false, instant, scratch);
                    }
                    else
                    {
                        changeFloorTexZ(move);
                        adjustFloorClip();
                    }

                    return VmADoorTickSound.MoveResult.PastDest;
                }

                if (!moveAttached(crush, -speed, 0, true, instant, scratch))
                    return VmADoorTickSound.MoveResult.Crushed;
                planes.SetFloorD(movedest);
                if (changeSector(crush, -speed, 0, false, instant))
                {
                    planes.SetFloorD(lastPos);
                    changeSector(crush, speed, 0, true, instant);
                    moveAttached(crush, speed, 0, false, instant, scratch);
                    return VmADoorTickSound.MoveResult.Crushed;
                }

                changeFloorTexZ(planes.FloorHeightDiff?.Invoke(lastPos, planes.FloorD) ?? speed);
                adjustFloorClip();
                if (scratch.Crushed)
                    return VmADoorTickSound.MoveResult.Crushed;
                break;
            }
            case 1:
            {
                if (!planes.CeilingSlope && !planes.FloorSlope && !planes.PortalLinkedCeiling &&
                    (planes.CompatFlags2 & CompatFloorMove) == 0 && -dest > planes.CeilingD)
                    dest = -planes.CeilingD;

                var stopMoving = crush < 0 || hexenCrush;
                var movedest = planes.FloorGetChangedHeight?.Invoke(speed) ?? (lastPos + speed);
                if (movedest <= dest)
                {
                    stopMoving = stopMoving || movedest != dest;
                    var move = planes.FloorHeightDiff?.Invoke(lastPos, dest) ?? (dest - lastPos);
                    if (!moveAttached(crush, move, 0, stopMoving, instant, scratch))
                        return VmADoorTickSound.MoveResult.PastDest;
                    planes.SetFloorD(dest);
                    if (changeSector(crush, move, 0, false, instant) && stopMoving)
                    {
                        planes.SetFloorD(lastPos);
                        changeSector(crush, -move, 0, true, instant);
                        moveAttached(crush, -move, 0, false, instant, scratch);
                    }
                    else
                    {
                        changeFloorTexZ(move);
                        adjustFloorClip();
                    }

                    return VmADoorTickSound.MoveResult.PastDest;
                }

                if (!moveAttached(crush, speed, 0, stopMoving, instant, scratch))
                    return VmADoorTickSound.MoveResult.Crushed;
                planes.SetFloorD(movedest);
                if (changeSector(crush, speed, 0, false, instant))
                {
                    if (!stopMoving)
                    {
                        changeFloorTexZ(planes.FloorHeightDiff?.Invoke(lastPos, planes.FloorD) ?? speed);
                        adjustFloorClip();
                        return VmADoorTickSound.MoveResult.Crushed;
                    }

                    planes.SetFloorD(lastPos);
                    changeSector(crush, -speed, 0, true, instant);
                    moveAttached(crush, -speed, 0, false, instant, scratch);
                    return VmADoorTickSound.MoveResult.Crushed;
                }

                changeFloorTexZ(planes.FloorHeightDiff?.Invoke(lastPos, planes.FloorD) ?? speed);
                adjustFloorClip();
                if (scratch.Crushed)
                    return VmADoorTickSound.MoveResult.Crushed;
                break;
            }
        }

        return VmADoorTickSound.MoveResult.Ok;
    }

    public static VmADoorTickSound.MoveResult MoveCeiling(
        SectorPlanes planes,
        double speed,
        double dest,
        int crush,
        int direction,
        bool hexenCrush,
        Func<int, double, int, bool, bool, CrushScratch, bool> moveAttached,
        Func<int, double, int, bool, bool, bool> changeSector,
        Action<double> changeCeilingTexZ)
    {
        var scratch = new CrushScratch();
        var lastPos = planes.CeilingD;
        switch (direction)
        {
            case -1:
            {
                if (!planes.CeilingSlope && !planes.FloorSlope && !planes.PortalLinkedFloor &&
                    (planes.CompatFlags2 & CompatFloorMove) == 0 && dest < -planes.FloorD)
                    dest = -planes.FloorD;

                var stopMoving = crush < 0 || hexenCrush;
                var movedest = planes.CeilingGetChangedHeight?.Invoke(-speed) ?? (lastPos - speed);
                if (movedest <= dest)
                {
                    stopMoving = stopMoving || movedest != dest;
                    var move = planes.CeilingHeightDiff?.Invoke(lastPos, dest) ?? (dest - lastPos);
                    if (!moveAttached(crush, move, 1, stopMoving, false, scratch))
                        return VmADoorTickSound.MoveResult.PastDest;
                    planes.SetCeilingD(dest);
                    if (changeSector(crush, move, 1, false, false) && stopMoving)
                    {
                        planes.SetCeilingD(lastPos);
                        changeSector(crush, -move, 1, true, false);
                        moveAttached(crush, -move, 1, false, false, scratch);
                    }
                    else
                        changeCeilingTexZ(move);
                    return VmADoorTickSound.MoveResult.PastDest;
                }

                if (!moveAttached(crush, -speed, 1, stopMoving, false, scratch))
                    return VmADoorTickSound.MoveResult.Crushed;
                planes.SetCeilingD(movedest);
                if (changeSector(crush, -speed, 1, false, false))
                {
                    if (!stopMoving)
                    {
                        changeCeilingTexZ(planes.CeilingHeightDiff?.Invoke(lastPos, planes.CeilingD) ?? -speed);
                        return VmADoorTickSound.MoveResult.Crushed;
                    }

                    planes.SetCeilingD(lastPos);
                    changeSector(crush, speed, 1, true, false);
                    moveAttached(crush, speed, 1, false, false, scratch);
                    return VmADoorTickSound.MoveResult.Crushed;
                }

                changeCeilingTexZ(planes.CeilingHeightDiff?.Invoke(lastPos, planes.CeilingD) ?? -speed);
                if (scratch.Crushed)
                    return VmADoorTickSound.MoveResult.Crushed;
                break;
            }
            case 1:
            {
                var movedest = planes.CeilingGetChangedHeight?.Invoke(speed) ?? (lastPos + speed);
                if (movedest >= dest)
                {
                    var move = planes.CeilingHeightDiff?.Invoke(lastPos, dest) ?? (dest - lastPos);
                    if (!moveAttached(crush, move, 1, true, false, scratch))
                        return VmADoorTickSound.MoveResult.PastDest;
                    planes.SetCeilingD(dest);
                    if (changeSector(crush, move, 1, false, false))
                    {
                        planes.SetCeilingD(lastPos);
                        changeSector(crush, -move, 1, true, false);
                        moveAttached(crush, -move, 1, false, false, scratch);
                    }
                    else
                        changeCeilingTexZ(move);
                    return VmADoorTickSound.MoveResult.PastDest;
                }

                if (!moveAttached(crush, speed, 1, true, false, scratch))
                    return VmADoorTickSound.MoveResult.Crushed;
                planes.SetCeilingD(movedest);
                if (changeSector(crush, speed, 1, false, false))
                {
                    planes.SetCeilingD(lastPos);
                    changeSector(crush, -speed, 1, true, false);
                    moveAttached(crush, -speed, 1, false, false, scratch);
                    return VmADoorTickSound.MoveResult.Crushed;
                }

                changeCeilingTexZ(planes.CeilingHeightDiff?.Invoke(lastPos, planes.CeilingD) ?? speed);
                if (scratch.Crushed)
                    return VmADoorTickSound.MoveResult.Crushed;
                break;
            }
        }

        return VmADoorTickSound.MoveResult.Ok;
    }
}
