namespace HCDE.Playsim;

/// <summary>Native <c>IceChunk</c> debris from <c>A_FreezeDeathChunks</c>. Ice chunk heads are absent.</summary>
public sealed class IceChunkActor : Actor
{
    public const int DoomEdNumMarker = 65540;

    public int RemainingTics => Destroyed ? 0 : States.RemainingTics;
    internal override bool IsBlockmapActor => false;

    internal IceChunkActor(int remainingTics)
    {
        if (remainingTics <= 0) throw new ArgumentOutOfRangeException(nameof(remainingTics));
        DoomEdNum = DoomEdNumMarker;
        Solid = false;
        Shootable = false;
        AllowDropOff = true;
        CannotPush = true;
        NoTeleport = true;
        Radius = Fixed.FromInt(3);
        Height = Fixed.FromInt(4);
        Mass = 5;
        Gravity = Fixed.FromDouble(0.125);
        States.Configure(this, [new ActorFrame(remainingTics, 1, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, 2, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, 3, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, -1, _ => SetFrameDuration())], 0);
    }

    private void SetFrameDuration()
    {
        if (Simulation is { } sim)
            States.ForceRemainingTics(70 + (int)(sim.NextCombatRandom() % 64));
    }
}
