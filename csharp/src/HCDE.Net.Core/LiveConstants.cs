namespace HCDE.Net.Core;

public enum LiveLane : byte
{
    Control = 0,
    Command,
    Authority,
    PlayerSnapshot,
    ActorDelta,
    QueryRegistry,
    PresentationEcho,
    Count,
}

public enum LiveMessageType : byte
{
    Control = 1,
    ClientCommands,
    ServerSnapshot,
}

public enum GameplayPayloadKind : byte
{
    ReservedLegacyClientCommands = 1,
    ReservedLegacyServerSnapshot,
    ClientInputs,
    ServerSnapshot,
}

[Flags]
public enum GameplayEnvelopeFlags : byte
{
    None = 0,
    ActorRepairRequest = 1 << 0,
}

[Flags]
public enum LiveControlCapabilityFlags : byte
{
    None = 0,
    SessionId = 1 << 0,
}

public static class LiveConstants
{
    public const int HeaderSize = 15;
    public const int GameplayHeaderSize = 12;
    public const int ControlBasePayloadSize = 6;
    public const int ControlCapabilitiesMinSize = 14;
    public const int ControlCapabilitiesFullSize = 18;
    public const int ControlMinPayloadSize = ControlBasePayloadSize + ControlCapabilitiesMinSize;
    public const int ControlFullPayloadSize = ControlBasePayloadSize + ControlCapabilitiesFullSize;

    public const byte ProtocolVersion = 1;
    public const byte GameplayProtocolVersion = 1;
    public const byte ControlCapabilitiesVersion = 1;

    public const ulong ControlIntervalMs = 1000;

    public static ReadOnlySpan<byte> LiveMagic => "HLIV"u8;
    public static ReadOnlySpan<byte> GameplayMagic => "HGPL"u8;
    public static ReadOnlySpan<byte> ControlCapabilitiesMagic => "HCAP"u8;
    public static ReadOnlySpan<byte> ClientInputMagic => "HCIN"u8;
    public static ReadOnlySpan<byte> ServerSnapshotMagic => "HCSN"u8;
    public static ReadOnlySpan<byte> ClientInputRecordsMagic => "HCIR"u8;
    public static ReadOnlySpan<byte> ServerSnapshotRecordsMagic => "HCSR"u8;

    public const int ClientInputHeaderSize = 29;
    public const int ServerSnapshotHeaderSize = 31;
    public const int ClientInputRecordsHeaderSize = 6;
    public const int ServerSnapshotRecordsHeaderSize = 6;
    public const int ExplicitUserCmdBytes = 16;

    public const byte ClientInputProtocolVersion = 5;
    public const byte ClientInputRecordsProtocolVersion = 4;
    public const byte ServerSnapshotProtocolVersion = 4;
    public const byte ServerSnapshotRecordsProtocolVersion = 2;

    public const int ServerWorldDeltaHeaderSize = 11;
    public const int ServerWorldDeltaPoseRecordV4Size = 38;
    public const int ServerWorldDeltaSectorRecordSize = 11;
    public const int ServerWorldDeltaSectorRecordMaxSize = 15;
    public const int AuthorityEventsHeaderSize = 8;
    public const int ActorDeltasHeaderSize = 8;
    public const byte ServerWorldDeltaProtocolVersion = 4;
    public const byte AuthorityEventsProtocolVersion = 1;
    public const byte ActorDeltasProtocolVersion = 2;

    public static ReadOnlySpan<byte> ServerWorldDeltaMagic => "HCDW"u8;
    public static ReadOnlySpan<byte> AuthorityEventsMagic => "HCAV"u8;
    public static ReadOnlySpan<byte> ActorDeltasMagic => "HCDA"u8;

    public const byte ServerWorldDeltaPoseHasActor = 1 << 0;
    public const byte ServerWorldDeltaPoseLive = 1 << 1;
    public const byte ServerWorldDeltaPoseOnGround = 1 << 2;
    public const byte ServerWorldDeltaSectorHasFloor = 1 << 0;
    public const byte ServerWorldDeltaSectorHasCeiling = 1 << 1;
    public const byte ServerWorldDeltaSectorHasLight = 1 << 2;
    public const byte ServerWorldDeltaSectorHasSpecial = 1 << 3;
    public const byte ServerWorldDeltaSectorKnownFlags =
        ServerWorldDeltaSectorHasFloor
        | ServerWorldDeltaSectorHasCeiling
        | ServerWorldDeltaSectorHasLight
        | ServerWorldDeltaSectorHasSpecial;
    public const byte ActorDeltasFlagComplete = 1 << 0;
    public const byte ActorDeltaFlagLive = 1 << 0;

    public const byte AuthorityEventSpawn = 1;
    public const byte AuthorityEventDespawn = 2;
    public const byte AuthorityEventDamage = 3;
    public const byte AuthorityEventCosmeticSpawn = 4;
    public const int AuthorityEventRecordPrefixSize = 19;
    public const int AuthorityEventRecordSuffixSize = 56;

    public const int SnapshotChecksumHistoryDepth = 64;
    public const byte SnapshotChecksumDefaultCategoryMask = 0x3F;

    public const ushort ActorDeltaFieldCategory = 1 << 0;
    public const ushort ActorDeltaFieldFlags = 1 << 1;
    public const ushort ActorDeltaFieldAction = 1 << 2;
    public const ushort ActorDeltaFieldHealth = 1 << 3;
    public const ushort ActorDeltaFieldPos = 1 << 4;
    public const ushort ActorDeltaFieldVel = 1 << 5;
    public const ushort ActorDeltaFieldAngles = 1 << 6;
    public const ushort ActorDeltaFieldCoopSpawnIndex = 1 << 7;
    public const ushort ActorDeltaFieldAll =
        ActorDeltaFieldCategory
        | ActorDeltaFieldFlags
        | ActorDeltaFieldAction
        | ActorDeltaFieldHealth
        | ActorDeltaFieldPos
        | ActorDeltaFieldVel
        | ActorDeltaFieldAngles
        | ActorDeltaFieldCoopSpawnIndex;

    public const double ActorDeltaPosScale = 16.0;
    public const double ActorDeltaVelScale = 32.0;

    public const int SnapshotChecksumBlockSize = 34;
    public const byte SnapshotChecksumProtocolVersion = 1;
    public const byte SnapshotChecksumCategoryCount = 6;

    public static ReadOnlySpan<byte> SnapshotChecksumMagic => "HCKS"u8;
    public static ReadOnlySpan<byte> CoopDeadSpawnsMagic => "HCDS"u8;
    public static ReadOnlySpan<byte> InvasionSnapshotMagic => "HCIV"u8;
    public static ReadOnlySpan<byte> PresentationEchoMagic => "ECHO"u8;

    public const int CoopDeadSpawnsHeaderSize = 8;
    public const byte CoopDeadSpawnsProtocolVersion = 1;

    public const int InvasionSnapshotHeaderV1Size = 36;
    public const int InvasionSnapshotHeaderV2Size = 52;
    public const byte InvasionSnapshotProtocolVersion = 2;
    public const int InvasionSnapshotPayloadBudgetBytes = 1200;

    public const byte InvasionSnapshotFlagBossWave = 1 << 0;
    public const byte InvasionSnapshotSpawnFlagUsingFallback = 1 << 0;

    public const byte InvasionStateDisabled = 0;
    public const byte InvasionStateWaiting = 1;
    public const byte InvasionStateCountdown = 2;
    public const byte InvasionStateSpawning = 3;
    public const byte InvasionStateCleanup = 4;
    public const byte InvasionStateIntermission = 5;
    public const byte InvasionStateVictory = 6;
    public const byte InvasionStateFailure = 7;

    public const byte InvasionSpawnSourceNone = 0;
    public const byte InvasionSpawnSourceClassic = 1;
    public const byte InvasionSpawnSourceMapSpot = 2;
    public const byte InvasionSpawnSourceDeathmatch = 3;
    public const byte InvasionSpawnSourcePlayerStart = 4;

    public const byte PresentationEchoProtocolVersion = 8;
    public const int PresentationEchoMinHeaderSize = 7;
    public const byte PresentationEchoInvalidInventorySlot = 0xFF;
    public const byte PresentationEchoInventoryFlagWeapon = 1 << 0;
    public const byte PresentationEchoInventoryFlagArmor = 1 << 1;
    public const int PresentationEchoPlayerFixedPrefixSize = 25;

    public const byte WeaponChangeReadyClass = 1 << 0;
    public const byte WeaponChangeForceReseat = 1 << 1;

    public const ushort WeaponStateReady = 1 << 0;

    public const int InputGapResyncTics = 2 * TicRate;
    public const int SnapshotGapResyncMs = 2000;
    public const int SnapshotGapImmediateTics = 3 * TicRate;
    public const int TicRate = 35;

    public const ulong CapControlV1 = 1UL << 0;
    public const ulong CapClientInputV5 = 1UL << 1;
    public const ulong CapServerSnapshotV4 = 1UL << 2;
    public const ulong CapServerWorldDeltaV2 = 1UL << 3;
    public const ulong CapInvasionSnapshotV2 = 1UL << 4;
    public const ulong CapPredatorSnapshotV1 = 1UL << 5;
    public const ulong CapActorRegistryV1 = 1UL << 16;
    public const ulong CapActorDeltaV2 = 1UL << 17;
    public const ulong CapLaneBudgetsV1 = 1UL << 18;
    public const ulong CapAuthorityEventsV1 = 1UL << 19;

    public const ulong KnownCapabilityMask =
        CapControlV1
        | CapClientInputV5
        | CapServerSnapshotV4
        | CapServerWorldDeltaV2
        | CapInvasionSnapshotV2
        | CapPredatorSnapshotV1
        | CapActorRegistryV1
        | CapActorDeltaV2
        | CapLaneBudgetsV1
        | CapAuthorityEventsV1;

    /// <summary>
    /// Capabilities advertised by a shipping HCDE build (matches <c>HCDELiveLocalCapabilities</c>).
    /// </summary>
    public const ulong DefaultLocalCapabilities =
        CapControlV1
        | CapClientInputV5
        | CapServerSnapshotV4
        | CapServerWorldDeltaV2
        | CapInvasionSnapshotV2
        | CapActorRegistryV1
        | CapActorDeltaV2
        | CapLaneBudgetsV1
        | CapAuthorityEventsV1;
}
