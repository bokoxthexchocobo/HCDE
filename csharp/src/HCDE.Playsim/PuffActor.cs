namespace HCDE.Playsim;

/// <summary>Cosmetic hitscan puff from <c>P_LineAttack</c>. No collision or damage.</summary>
public sealed class PuffActor : Actor
{
    public const int DoomEdNumMarker = 65542;
    public Actor? DamageSource { get; internal set; }

    internal PuffActor(int firstFrameTics = 4)
    {
        if (firstFrameTics is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(firstFrameTics));
        DoomEdNum = DoomEdNumMarker;
        Solid = false;
        Shootable = false;
        NoGravity = true;
        VelocityZ = Fixed.FromInt(1);
        Radius = Fixed.FromInt(20);
        Height = Fixed.FromInt(16);
        Mass = 5;
        States.ConfigureSpawn(this, [new ActorFrame(firstFrameTics, 1, FullBright: true),
            new ActorFrame(4, 2, FullBright: false), new ActorFrame(4, 3, FullBright: false),
            new ActorFrame(4, -1, FullBright: false)], 0);
    }

    internal int RemainingTics => Destroyed ? 0 : (3 - States.Current) * 4 + States.RemainingTics;

    internal override bool IsBlockmapActor => false;

}
