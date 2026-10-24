namespace HCDE.Playsim;

/// <summary>Native <c>gamestate_t</c> subset for headless authority (no title, demo, or console states).</summary>
public enum HeadlessGameState
{
    Startup,
    Level,
    Intermission,
}

/// <summary>Native <c>gameaction_t</c> subset processed on the authority tick.</summary>
public enum HeadlessGameAction
{
    Nothing,
    NewGame,
    Completed,
    WorldDone,
    LoadLevel,
}

/// <summary>Native <c>PST_*</c> player lifecycle for co-op travel and respawn.</summary>
public enum HeadlessPlayerState
{
    Live,
    Dead,
    Enter,
    Reborn,
}
