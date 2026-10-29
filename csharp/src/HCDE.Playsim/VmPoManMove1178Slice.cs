namespace HCDE.Playsim;

/// <summary>
/// Move and move-to poly thinkers from <c>po_man.cpp</c> (<b>369–526</b>). Chunk <b>1178</b>.
/// <c>DPolyDoor::Tick</c> at 527 stays chunk 671. Types live on chunk 1177.
/// </summary>
public static class VmPoManMove1178Slice
{
    public static (double X, double Y) ToVector(double degrees, double length)
    {
        var radians = degrees * Math.PI / 180d;
        return (Math.Cos(radians) * length, Math.Sin(radians) * length);
    }

    public static double MakeUnit(ref double x, ref double y)
    {
        var len = Math.Sqrt((x * x) + (y * y));
        var ilen = len;
        if (ilen != 0d)
        {
            ilen = 1d / ilen;
        }

        x *= ilen;
        y *= ilen;
        return len;
    }

    public static void MoveTick(VmPoManRotate1177Slice.Level level, VmPoManRotate1177Slice.MovePoly action)
    {
        var poly = action.Poly;
        if (poly == null)
        {
            return;
        }

        var moved = false;
        if (poly.Move != null)
        {
            moved = poly.Move(action.VelX, action.VelY);
        }

        if (!moved)
        {
            return;
        }

        var absSpeed = Math.Abs(action.Speed);
        action.Dist -= absSpeed;
        if (action.Dist <= 0d)
        {
            level.StopSequence(poly);
            action.Destroy();
        }
        else if (action.Dist < absSpeed)
        {
            action.Speed = action.Dist * (action.Speed < 0d ? -1d : 1d);
            var step = ToVector(action.Angle, action.Speed);
            action.VelX = step.X;
            action.VelY = step.Y;
        }

        poly.UpdateLinks();
    }

    public static bool MovePolyEvent(
        VmPoManRotate1177Slice.Level level,
        int polyNum,
        double speed,
        double angle,
        double dist,
        bool overRide)
    {
        VmPoManRotate1177Slice.MovePoly? pe = null;
        var poly = level.GetPoly(polyNum);
        if (poly == null)
        {
            level.Prints.Add("EV_MovePoly: Invalid polyobj num: " + polyNum.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n");
            return false;
        }

        var mirrors = level.MirrorsOf(poly);
        for (var i = 0; i < mirrors.Count; i++)
        {
            poly = mirrors[i];
            if ((poly.SpecialData != null || poly.Blocked) && !overRide)
            {
                break;
            }

            var created = new VmPoManRotate1177Slice.MovePoly();
            created.Construct(poly);
            level.Thinkers.Add(created);
            poly.SpecialData = created;
            poly.Blocked = false;
            created.Dist = dist;
            created.Speed = speed;
            created.Angle = angle;
            var step = ToVector(angle, speed);
            created.VelX = step.X;
            created.VelY = step.Y;
            level.StartSequence(poly, poly.SeqType, VmPoManRotate1177Slice.SeqDoor, 0);
            if (dist / speed <= 2d)
            {
                created.StopInterpolation();
            }

            angle += 180d;
            pe = created;
        }

        return pe != null;
    }

    public static void MoveToTick(VmPoManRotate1177Slice.Level level, VmPoManRotate1177Slice.MovePolyTo action)
    {
        var poly = action.Poly;
        if (poly == null)
        {
            return;
        }

        var moved = false;
        if (poly.Move != null)
        {
            moved = poly.Move(action.VelX, action.VelY);
        }

        if (!moved)
        {
            return;
        }

        var absSpeed = Math.Abs(action.Speed);
        action.Dist -= absSpeed;
        if (action.Dist <= 0d)
        {
            level.StopSequence(poly);
            action.Destroy();
        }
        else if (action.Dist < absSpeed)
        {
            action.Speed = action.Dist * (action.Speed < 0d ? -1d : 1d);
            action.VelX = action.TargetX - poly.StartX;
            action.VelY = action.TargetY - poly.StartY;
        }

        poly.UpdateLinks();
    }

    public static bool MovePolyToEvent(
        VmPoManRotate1177Slice.Level level,
        int polyNum,
        double speed,
        double targetX,
        double targetY,
        bool overRide)
    {
        VmPoManRotate1177Slice.MovePolyTo? pe = null;
        var origin = level.GetPoly(polyNum);
        if (origin == null)
        {
            level.Prints.Add("EV_MovePolyTo: Invalid polyobj num: " + polyNum.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n");
            return false;
        }

        var distX = targetX - origin.StartX;
        var distY = targetY - origin.StartY;
        var distLen = MakeUnit(ref distX, ref distY);
        var mirrors = level.MirrorsOf(origin);
        for (var i = 0; i < mirrors.Count; i++)
        {
            var poly = mirrors[i];
            if ((poly.SpecialData != null || poly.Blocked) && !overRide)
            {
                break;
            }

            var created = new VmPoManRotate1177Slice.MovePolyTo();
            created.Construct(poly);
            level.Thinkers.Add(created);
            poly.SpecialData = created;
            poly.Blocked = false;
            created.Dist = distLen;
            created.Speed = speed;
            created.VelX = distX * speed;
            created.VelY = distY * speed;
            created.TargetX = poly.StartX + (distX * distLen);
            created.TargetY = poly.StartY + (distY * distLen);
            level.StartSequence(poly, poly.SeqType, VmPoManRotate1177Slice.SeqDoor, 0);
            if ((created.Dist / created.Speed) <= 2d)
            {
                created.StopInterpolation();
            }

            distX = -distX;
            distY = -distY;
            pe = created;
        }

        return pe != null;
    }
}
