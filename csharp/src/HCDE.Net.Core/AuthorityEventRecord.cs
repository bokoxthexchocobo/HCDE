namespace HCDE.Net.Core;

public enum ReplicatedActorCategory : byte
{
    Unknown = 0,
    Player = 1,
    Monster = 2,
    Projectile = 3,
    Pickup = 4,
    Map = 5,
    Script = 6,
    Visual = 7,
}

public enum ReplicatedActorSource : byte
{
    Shared = 0,
    Invasion = 1,
    Coop = 2,
    Dm = 3,
}

public enum AuthorityEventType : byte
{
    Spawn = 1,
    Despawn = 2,
    Damage = 3,
    CosmeticSpawn = 4,
}

public readonly struct AuthorityEventRecord
{
    public AuthorityEventRecord(
        AuthorityEventType eventType,
        ReplicatedActorSource source,
        ReplicatedActorCategory category,
        byte actorFlags,
        uint actorId,
        uint eventTic,
        ushort classId,
        short health,
        ushort wave,
        ReadOnlySpan<byte> className,
        double posX,
        double posY,
        double posZ,
        double velX,
        double velY,
        double velZ,
        uint yaw,
        uint pitch)
    {
        EventType = eventType;
        Source = source;
        Category = category;
        ActorFlags = actorFlags;
        ActorId = actorId;
        EventTic = eventTic;
        ClassId = classId;
        Health = health;
        Wave = wave;
        ClassName = className.Length == 0 ? Array.Empty<byte>() : className.ToArray();
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        VelX = velX;
        VelY = velY;
        VelZ = velZ;
        Yaw = yaw;
        Pitch = pitch;
    }

    public AuthorityEventType EventType { get; }
    public ReplicatedActorSource Source { get; }
    public ReplicatedActorCategory Category { get; }
    public byte ActorFlags { get; }
    public uint ActorId { get; }
    public uint EventTic { get; }
    public ushort ClassId { get; }
    public short Health { get; }
    public ushort Wave { get; }
    public byte[] ClassName { get; }
    public double PosX { get; }
    public double PosY { get; }
    public double PosZ { get; }
    public double VelX { get; }
    public double VelY { get; }
    public double VelZ { get; }
    public uint Yaw { get; }
    public uint Pitch { get; }

    public static int MinRecordSize(ReadOnlySpan<byte> className) =>
        LiveConstants.AuthorityEventRecordPrefixSize
        + className.Length
        + LiveConstants.AuthorityEventRecordSuffixSize;

    public bool IsValid(out string? rejectReason)
    {
        rejectReason = null;
        if (EventType is not (AuthorityEventType.Spawn
            or AuthorityEventType.Despawn
            or AuthorityEventType.Damage
            or AuthorityEventType.CosmeticSpawn))
        {
            rejectReason = "authority-event-type-invalid";
            return false;
        }

        if ((byte)Source > (byte)ReplicatedActorSource.Dm)
        {
            rejectReason = "authority-event-source-invalid";
            return false;
        }

        if ((byte)Category > (byte)ReplicatedActorCategory.Visual)
        {
            rejectReason = "authority-event-category-invalid";
            return false;
        }

        if ((ActorFlags & ~LiveConstants.ActorDeltaFlagLive) != 0)
        {
            rejectReason = "authority-event-flags-invalid";
            return false;
        }

        if ((EventType is AuthorityEventType.Spawn or AuthorityEventType.CosmeticSpawn) && ClassName.Length == 0)
        {
            rejectReason = "authority-event-missing-class-name";
            return false;
        }

        if (ClassName.Length > byte.MaxValue)
        {
            rejectReason = "authority-event-class-name-too-long";
            return false;
        }

        return true;
    }
}
