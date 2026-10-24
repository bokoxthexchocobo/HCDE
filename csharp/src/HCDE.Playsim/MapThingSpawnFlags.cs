namespace HCDE.Playsim;

/// <summary>Native <c>mapthing_t::flags</c> game-mode visibility bits from <c>doomdata.h</c>.</summary>
[Flags]
public enum MapThingSpawnFlags : ushort
{
    None = 0,
    SinglePlayer = 0x0100,
    Cooperative = 0x0200,
    Deathmatch = 0x0400,
}

public static class MapThingSpawnFlagMask
{
    public static MapThingSpawnFlags MaskForGameMode(MapThingSpawnGameMode mode, bool spawnMulti, bool spawnMultiCoopOnly)
    {
        return mode switch
        {
            MapThingSpawnGameMode.Deathmatch => MapThingSpawnFlags.Deathmatch,
            MapThingSpawnGameMode.CooperativeMultiplayer => MapThingSpawnFlags.Cooperative,
            MapThingSpawnGameMode.MultiplayerWithSpawnMulti => spawnMultiCoopOnly
                ? MapThingSpawnFlags.Cooperative
                : MapThingSpawnFlags.Cooperative | MapThingSpawnFlags.SinglePlayer,
            _ => MapThingSpawnFlags.SinglePlayer,
        };
    }
}

public enum MapThingSpawnGameMode
{
    SinglePlayer,
    CooperativeMultiplayer,
    Deathmatch,
    MultiplayerWithSpawnMulti,
}
