namespace HCDE.Playsim;

/// <summary><c>DAnimatedDoor</c> thinker and <c>FLevelLocals::EV_SlidingDoor</c> from <c>a_doors.cpp</c> (548–858).</summary>
public static class VmAAnimatedDoor
{
    public const int MlBlocking = 1 << 0;
    public const int AdOpenClose = 0;
    public const int AdClose = 1;
    public const int StatusOpening = 0;
    public const int StatusWaiting = 1;
    public const int StatusClosing = 2;
    public const int StatusDead = 3;

    public sealed class DoorAnim
    {
        public int NumTextureFrames { get; set; }
        public List<string> TextureFrames { get; } = new();
        public string CloseSound { get; set; } = "";
        public string OpenSound { get; set; } = "";
    }

    public sealed class AnimLine
    {
        public uint Flags { get; set; }
        public AnimSide? Side0 { get; set; }
        public AnimSide? Side1 { get; set; }
        public AnimSector? BackSector { get; set; }
        public string TopTexture { get; set; } = "";
    }

    public sealed class AnimSide
    {
        public string MidTexture { get; set; } = "";
        public string TopTexture { get; set; } = "";
    }

    public sealed class AnimSector
    {
        public double CeilingPlaneD { get; set; }
        public double CeilingPlaneC { get; set; }
        public object? CeilingData { get; set; }
        public object? TouchingThingList { get; set; }
        public List<AnimLine> Lines { get; } = new();
        public Func<double, double, int, int, bool, VmADoorTickSound.MoveResult>? MoveCeiling { get; set; }
    }

    public sealed class AnimatedDoorThinker
    {
        public AnimSector Sector { get; set; } = null!;
        public AnimLine? Line1 { get; set; }
        public AnimLine? Line2 { get; set; }
        public int Frame { get; set; }
        public DoorAnim? DoorAnim { get; set; }
        public int Timer { get; set; }
        public double BotDist { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int Speed { get; set; }
        public int Delay { get; set; }
        public bool SetBlocking1 { get; set; }
        public bool SetBlocking2 { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class AnimTickContext
    {
        public Action<AnimSector, int, string, int>? StartSequence { get; init; }
        public Action<AnimatedDoorThinker>? Destroy { get; init; }
    }

    public sealed class AnimConstructContext
    {
        public Func<string, double>? TextureDisplayHeight { get; init; }
        public Action<AnimSector, int, string, int>? StartSequence { get; init; }
    }

    public sealed class EvSlidingDoorContext
    {
        public Func<AnimLine, DoorAnim?>? FindAnimatedDoor { get; init; }
        public Func<AnimSector, AnimLine, int, int, DoorAnim, int, bool>? CreateAnimatedDoor { get; init; }
        public Func<int, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, AnimSector?>? SectorByIndex { get; init; }
        public Func<AnimatedDoorThinker, bool>? StartClosing { get; init; }
        public Func<object?, bool>? IsAnimatedDoor { get; init; }
        public Func<AnimSector, AnimatedDoorThinker?>? GetAnimatedDoor { get; init; }
    }

    public sealed class SerializeState
    {
        public AnimLine? Line1 { get; set; }
        public AnimLine? Line2 { get; set; }
        public int Frame { get; set; }
        public int Timer { get; set; }
        public double BotDist { get; set; }
        public int Status { get; set; }
        public int Speed { get; set; }
        public int Delay { get; set; }
        public DoorAnim? DoorAnim { get; set; }
        public bool SetBlocking1 { get; set; }
        public bool SetBlocking2 { get; set; }
        public int Type { get; set; }
    }

    public static void ConstructSectorOnly(AnimatedDoorThinker door, AnimSector sec) => door.Sector = sec;

    public static void Serialize(AnimatedDoorThinker door, SerializeState arc)
    {
        arc.Line1 = door.Line1;
        arc.Line2 = door.Line2;
        arc.Frame = door.Frame;
        arc.Timer = door.Timer;
        arc.BotDist = door.BotDist;
        arc.Status = door.Status;
        arc.Speed = door.Speed;
        arc.Delay = door.Delay;
        arc.DoorAnim = door.DoorAnim;
        arc.SetBlocking1 = door.SetBlocking1;
        arc.SetBlocking2 = door.SetBlocking2;
        arc.Type = door.Type;
    }

    public static bool StartClosing(AnimatedDoorThinker door, AnimTickContext ctx)
    {
        var sec = door.Sector;
        if (sec.TouchingThingList != null)
            return false;

        var topdist = sec.CeilingPlaneD;
        if (sec.MoveCeiling?.Invoke(2048, door.BotDist, 0, -1, false) == VmADoorTickSound.MoveResult.Crushed)
            return false;

        sec.MoveCeiling?.Invoke(2048, topdist, 1, 1, false);

        if (door.Line1 != null)
            door.Line1.Flags |= (uint)MlBlocking;
        if (door.Line2 != null)
            door.Line2.Flags |= (uint)MlBlocking;
        if (door.DoorAnim != null && door.DoorAnim.CloseSound.Length > 0)
            ctx.StartSequence?.Invoke(sec, 0, door.DoorAnim.CloseSound, 1);

        door.Status = StatusClosing;
        door.Timer = door.Speed;
        return true;
    }

    public static void Tick(AnimatedDoorThinker door, AnimTickContext ctx)
    {
        if (door.DoorAnim == null)
        {
            ctx.Destroy?.Invoke(door);
            return;
        }

        var sec = door.Sector;
        switch (door.Status)
        {
            case StatusDead:
                sec.CeilingData = null;
                ctx.Destroy?.Invoke(door);
                break;

            case StatusOpening:
                if (--door.Timer == 0)
                {
                    if (++door.Frame >= door.DoorAnim.NumTextureFrames)
                    {
                        if (door.Line1 != null)
                            door.Line1.Flags &= ~(uint)MlBlocking;
                        if (door.Line2 != null)
                            door.Line2.Flags &= ~(uint)MlBlocking;

                        if (door.Delay == 0)
                        {
                            sec.CeilingData = null;
                            ctx.Destroy?.Invoke(door);
                            break;
                        }

                        door.Timer = door.Delay;
                        door.Status = StatusWaiting;
                    }
                    else
                    {
                        door.Timer = door.Speed;
                        SetFrameTextures(door);
                    }
                }
                break;

            case StatusWaiting:
                if (door.Type == AdClose || --door.Timer == 0)
                {
                    if (!StartClosing(door, ctx))
                        door.Timer = door.Delay;
                }
                break;

            case StatusClosing:
                if (--door.Timer == 0)
                {
                    if (--door.Frame < 0)
                    {
                        sec.MoveCeiling?.Invoke(2048, door.BotDist, -1, -1, false);
                        sec.CeilingData = null;
                        ctx.Destroy?.Invoke(door);
                        if (door.Line1 != null && !door.SetBlocking1)
                            door.Line1.Flags &= ~(uint)MlBlocking;
                        if (door.Line2 != null && !door.SetBlocking2)
                            door.Line2.Flags &= ~(uint)MlBlocking;
                        break;
                    }

                    door.Timer = door.Speed;
                    SetFrameTextures(door);
                }
                break;
        }
    }

    static void SetFrameTextures(AnimatedDoorThinker door)
    {
        var tex = door.DoorAnim!.TextureFrames[door.Frame];
        foreach (var line in new[] { door.Line1, door.Line2 })
        {
            if (line?.Side0 != null)
                line.Side0.MidTexture = tex;
            if (line?.Side1 != null)
                line.Side1.MidTexture = tex;
        }
    }

    public static void Construct(
        AnimatedDoorThinker door,
        AnimSector sec,
        AnimLine line,
        int speed,
        int delay,
        DoorAnim anim,
        int type,
        AnimConstructContext ctx)
    {
        door.Sector = sec;
        door.DoorAnim = anim;
        door.Line1 = line;
        door.Line2 = line;

        foreach (var l in sec.Lines)
        {
            if (ReferenceEquals(l, line))
                continue;
            if (l.Side0?.TopTexture == line.Side0?.TopTexture)
            {
                door.Line2 = l;
                break;
            }
        }

        if (line.Side0 != null)
            line.Side0.MidTexture = line.Side0.TopTexture;
        if (door.Line2?.Side0 != null && door.Line2 != line)
            door.Line2.Side0.MidTexture = door.Line2.Side0.TopTexture;

        var picnum = line.Side0?.TopTexture ?? "";
        var texHeight = ctx.TextureDisplayHeight?.Invoke(picnum) ?? 64;
        var topdist = sec.CeilingPlaneD - texHeight * sec.CeilingPlaneC;

        door.Type = type;
        door.Status = type == AdClose ? StatusWaiting : StatusOpening;
        door.Speed = speed;
        door.Delay = delay;
        door.Timer = door.Speed;
        door.Frame = 0;
        door.SetBlocking1 = (line.Flags & MlBlocking) != 0;
        door.SetBlocking2 = (door.Line2?.Flags & MlBlocking) != 0;
        line.Flags |= (uint)MlBlocking;
        if (door.Line2 != null)
            door.Line2.Flags |= (uint)MlBlocking;
        door.BotDist = sec.CeilingPlaneD;
        sec.MoveCeiling?.Invoke(2048, topdist, 1, 1, false);

        if (type == AdOpenClose && anim.OpenSound.Length > 0)
            ctx.StartSequence?.Invoke(sec, 0, anim.OpenSound, 1);
    }

    public static bool EvSlidingDoor(
        EvSlidingDoorContext ctx,
        AnimLine line,
        object? actor,
        int tag,
        int speed,
        int delay,
        int type)
    {
        AnimSector? sec;
        var rtn = false;

        if (tag == 0)
        {
            sec = line.BackSector;
            if (sec == null)
                return false;

            if (sec.CeilingData != null)
            {
                if (actor == null)
                    return false;
                var player = actor as EvActor;
                if (player?.Player == null)
                    return false;

                var existing = ctx.GetAnimatedDoor?.Invoke(sec);
                if (existing != null && existing.Status == StatusWaiting)
                    return ctx.StartClosing?.Invoke(existing) == true;
                return false;
            }

            if (type == AdClose)
                return false;

            var anim = ctx.FindAnimatedDoor?.Invoke(line);
            if (anim != null)
            {
                ctx.CreateAnimatedDoor?.Invoke(sec, line, speed, delay, anim, type);
                return true;
            }

            return false;
        }

        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>())
        {
            sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;

            if (sec.CeilingData != null)
            {
                var existing = ctx.GetAnimatedDoor?.Invoke(sec);
                if (existing != null && existing.Status == StatusWaiting)
                    ctx.StartClosing?.Invoke(existing);
                continue;
            }

            if (type == AdClose)
                continue;

            foreach (var ln in sec.Lines)
            {
                if (ln.BackSector == null)
                    continue;
                var anim = ctx.FindAnimatedDoor?.Invoke(ln);
                if (anim != null)
                {
                    rtn = true;
                    ctx.CreateAnimatedDoor?.Invoke(sec, ln, speed, delay, anim, type);
                    break;
                }
            }
        }

        return rtn;
    }

    public sealed class EvActor
    {
        public object? Player { get; set; }
    }
}
