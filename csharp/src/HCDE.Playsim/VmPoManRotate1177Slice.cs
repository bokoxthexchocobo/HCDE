using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Poly thinker registration, lifecycle, and <c>EV_RotatePoly</c> from <c>po_man.cpp</c> (<b>1–368</b>). Chunk <b>1177</b>.
/// Mirror order is a host list: <c>FPolyMirrorIterator::NextMirror</c> at 1805 is outside this band.
/// </summary>
public static class VmPoManRotate1177Slice
{
    public const int SeqDoor = 1;
    public const double ByteAngleScale = 90d / 64d;
    public const double SpeedScale = 90d / 512d;

    public static object? FreePolyNodes;

    public static readonly string[] PrivatePrototypes =
    [
        "UnLinkPolyobj",
        "LinkPolyobj",
        "CheckMobjBlocking",
        "SpawnPolyobj",
        "DoMovePolyobj",
        "NewPolyNode",
        "FreePolyNode",
        "ReleaseAllPolyNodes",
    ];

    public readonly record struct ClassRecord(string Name, bool IsAbstract, bool Pointers);

    public static List<ClassRecord> Registered { get; } = new();

    public static List<string> PointerFields { get; } = new();

    static VmPoManRotate1177Slice()
    {
        Registered.Add(new ClassRecord("DPolyAction", false, true));
        Registered.Add(new ClassRecord("DRotatePoly", false, false));
        Registered.Add(new ClassRecord("DMovePoly", false, false));
        Registered.Add(new ClassRecord("DMovePolyTo", false, false));
        Registered.Add(new ClassRecord("DPolyDoor", false, false));
        PointerFields.Add("m_Interpolation");
    }

    public static string Format(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    public sealed class Archive
    {
        public List<(string Key, string Value)> Entries { get; } = new();

        public void Add(string key, string value)
        {
            Entries.Add((key, value));
        }
    }

    public sealed class Interpolation
    {
        public int Refs = 1;

        public void DelRef()
        {
            Refs--;
        }
    }

    public sealed class SequenceStart
    {
        public PolyObj Poly;
        public int SeqType;
        public int Kind;
        public int Mode;

        public SequenceStart(PolyObj poly, int seqType, int kind, int mode)
        {
            Poly = poly;
            SeqType = seqType;
            Kind = kind;
            Mode = mode;
        }
    }

    public sealed class PolyObj
    {
        public int Id;
        public object? SpecialData;
        public bool Blocked;
        public int HasPortals;
        public int SeqType;
        public double StartX;
        public double StartY;
        public int LinkUpdates;
        public Interpolation? LiveInterpolation;
        public Func<double, bool>? Rotate;
        public Func<double, double, bool>? Move;

        public Interpolation AcquireInterpolation()
        {
            var token = new Interpolation();
            LiveInterpolation = token;
            return token;
        }

        public void UpdateLinks()
        {
            LinkUpdates++;
        }
    }

    public sealed class Level
    {
        public Dictionary<int, PolyObj> Polys { get; } = new();
        public List<string> Prints { get; } = new();
        public List<string> Log { get; } = new();
        public List<PolyAction> Thinkers { get; } = new();
        public List<SequenceStart> Sequences { get; } = new();
        public List<PolyObj>? MirrorOrder;

        public PolyObj? GetPoly(int polyNum)
        {
            if (Polys.TryGetValue(polyNum, out var poly))
            {
                return poly;
            }

            return null;
        }

        public List<PolyObj> MirrorsOf(PolyObj poly)
        {
            if (MirrorOrder != null)
            {
                return MirrorOrder;
            }

            return new List<PolyObj> { poly };
        }

        public void StartSequence(PolyObj poly, int seqType, int kind, int mode)
        {
            Sequences.Add(new SequenceStart(poly, seqType, kind, mode));
        }

        public void StopSequence(PolyObj poly)
        {
            Log.Add("stop:" + poly.Id.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class PolyAction
    {
        public PolyObj? Poly;
        public double Speed;
        public double Dist;
        public Interpolation? Interpolation;
        public bool Destroyed;
        public bool SuperDestroyed;

        public virtual void Serialize(Archive arc)
        {
            arc.Add("polyobj", Poly == null ? "null" : Poly.Id.ToString(CultureInfo.InvariantCulture));
            arc.Add("speed", Format(Speed));
            arc.Add("dist", Format(Dist));
            arc.Add("interpolation", Interpolation == null ? "null" : "set");
        }

        public virtual void Construct(PolyObj poly)
        {
            Poly = poly;
            Speed = 0;
            Dist = 0;
            SetInterpolation();
        }

        public virtual void OnDestroy()
        {
            var poly = Poly;
            if (poly == null)
            {
                throw new InvalidOperationException("DPolyAction::OnDestroy requires a polyobj.");
            }

            if (ReferenceEquals(poly.SpecialData, this))
            {
                poly.SpecialData = null;
            }

            StopInterpolation();
            SuperDestroyed = true;
        }

        public void Stop(Level level)
        {
            var poly = Poly;
            if (poly == null)
            {
                throw new InvalidOperationException("DPolyAction::Stop requires a polyobj.");
            }

            level.StopSequence(poly);
            Destroy();
        }

        public void Destroy()
        {
            OnDestroy();
            Destroyed = true;
        }

        public void SetInterpolation()
        {
            var poly = Poly;
            if (poly == null)
            {
                throw new InvalidOperationException("DPolyAction::SetInterpolation requires a polyobj.");
            }

            Interpolation = poly.AcquireInterpolation();
        }

        public void StopInterpolation()
        {
            if (Interpolation != null)
            {
                Interpolation.DelRef();
                Interpolation = null;
            }
        }
    }

    public sealed class RotatePoly : PolyAction
    {
        public override void Construct(PolyObj poly)
        {
            base.Construct(poly);
        }

        public void Tick(Level level)
        {
            var poly = Poly;
            if (poly == null)
            {
                return;
            }

            if (Dist != -1d && Dist < Math.Abs(Speed))
            {
                Speed = Speed < 0d ? -Dist : Dist;
            }

            var rotated = false;
            if (poly.Rotate != null)
            {
                rotated = poly.Rotate(Speed);
            }

            if (!rotated)
            {
                return;
            }

            if (Dist == -1d)
            {
                return;
            }

            Dist -= Math.Abs(Speed);
            if (Dist == 0d)
            {
                level.StopSequence(poly);
                Destroy();
            }
        }
    }

    public class MovePoly : PolyAction
    {
        public double Angle;
        public double VelX;
        public double VelY;

        public override void Serialize(Archive arc)
        {
            base.Serialize(arc);
            arc.Add("angle", Format(Angle));
            arc.Add("speedv", Format(VelX) + "," + Format(VelY));
        }

        public override void Construct(PolyObj poly)
        {
            base.Construct(poly);
            Angle = 0;
            VelX = 0;
            VelY = 0;
        }
    }

    public sealed class MovePolyTo : PolyAction
    {
        public double VelX;
        public double VelY;
        public double TargetX;
        public double TargetY;

        public override void Serialize(Archive arc)
        {
            base.Serialize(arc);
            arc.Add("speedv", Format(VelX) + "," + Format(VelY));
            arc.Add("target", Format(TargetX) + "," + Format(TargetY));
        }

        public override void Construct(PolyObj poly)
        {
            base.Construct(poly);
            VelX = 0;
            VelY = 0;
            TargetX = 0;
            TargetY = 0;
        }
    }

    public sealed class PolyDoor : MovePoly
    {
        public int Type;
        public double Direction;
        public double TotalDist;
        public int Tics;
        public int WaitTics;
        public bool Close;

        public override void Serialize(Archive arc)
        {
            base.Serialize(arc);
            arc.Add("type", Type.ToString(CultureInfo.InvariantCulture));
            arc.Add("direction", Format(Direction));
            arc.Add("totaldist", Format(TotalDist));
            arc.Add("tics", Tics.ToString(CultureInfo.InvariantCulture));
            arc.Add("waittics", WaitTics.ToString(CultureInfo.InvariantCulture));
            arc.Add("close", Close ? "true" : "false");
        }

        public void Construct(PolyObj poly, int type)
        {
            base.Construct(poly);
            Type = type;
            Direction = 0;
            TotalDist = 0;
            Tics = 0;
            WaitTics = 0;
            Close = false;
        }
    }

    public static bool RotatePolyEvent(Level level, int polyNum, int speed, int byteAngle, int direction, bool overRide)
    {
        RotatePoly? pe = null;
        var poly = level.GetPoly(polyNum);
        if (poly == null)
        {
            level.Prints.Add("EV_RotatePoly: Invalid polyobj num: " + polyNum.ToString(CultureInfo.InvariantCulture) + "\n");
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

            if (poly.HasPortals == 2)
            {
                break;
            }

            var created = new RotatePoly();
            created.Construct(poly);
            level.Thinkers.Add(created);
            poly.SpecialData = created;
            poly.Blocked = false;
            if (byteAngle != 0)
            {
                if (byteAngle == 255)
                {
                    created.Dist = -1d;
                }
                else
                {
                    created.Dist = byteAngle * ByteAngleScale;
                }
            }
            else
            {
                created.Dist = 360d;
            }

            created.Speed = speed * direction * SpeedScale;
            level.StartSequence(poly, poly.SeqType, SeqDoor, 0);
            direction = -direction;
            pe = created;
        }

        return pe != null;
    }
}
