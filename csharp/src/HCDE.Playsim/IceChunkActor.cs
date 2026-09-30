namespace HCDE.Playsim;

/// <summary>Native <c>IceChunk</c> debris from <c>A_FreezeDeathChunks</c>. Ice chunk heads are absent.</summary>
public sealed class IceChunkActor : Actor
{
    public const int DoomEdNumMarker = 65540;

    private int _remainingTics;
    public int RemainingTics => _remainingTics;

    internal IceChunkActor(int remainingTics)
    {
        _remainingTics = remainingTics;
        DoomEdNum = DoomEdNumMarker;
        Solid = false;
        Shootable = false;
        AllowDropOff = true;
        Radius = Fixed.FromInt(3);
        Height = Fixed.FromInt(4);
        Mass = 5;
    }

    public override void Tick()
    {
        RememberPosition();
        if (--_remainingTics <= 0)
        {
            Destroy();
            return;
        }

        if (Simulation != null)
            ActorPhysics.Step(Simulation, this);
        base.Tick();
    }
}
