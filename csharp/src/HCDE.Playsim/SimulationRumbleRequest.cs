namespace HCDE.Playsim;

public sealed record SimulationRumbleRequest(uint ActorId, string Pattern, bool LocalPlayerOrCameraOnly = false,
    uint? SuppressIfLocalActorId = null);
