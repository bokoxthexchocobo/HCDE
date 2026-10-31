namespace HCDE.Playsim;

/// <summary>
/// Game cvars from <c>src/g_cvars.cpp</c> lines 1-244, through end of file.
/// <c>r_dynlights</c> is defined in <c>r_draw.cpp</c> and starts true. <c>I_UpdateWindowTitle</c> is only a declaration.
/// </summary>
public sealed class VmGameCvars1362Slice
{
    public const int CvarArchive = 1;
    public const int CvarServerInfo = 1 << 2;
    public const int CvarNoInitCall = 1 << 9;
    public const int CvarGlobalConfig = 1 << 10;
    public const int GsLevel = 0;
    public const int GsStartup = 6;
    public const int GsTitleLevel = 7;
    public const int StatAutoDecal = 2;
    public const int BodyQueSize = 32;

    public const int SpreadDecalsFlags = CvarArchive;
    public const int PushersFlags = CvarServerInfo;
    public const int CacheNodesFlags = CvarArchive | CvarGlobalConfig;
    public const int CacheTimeFlags = CvarArchive | CvarGlobalConfig;
    public const int AlwaysApplyDmFlagsFlags = CvarServerInfo;
    public const int FrictionFlags = CvarServerInfo;
    public const int TurnSpeedFlags = CvarArchive | CvarGlobalConfig;
    public const int GlLightsFlags = CvarArchive | CvarGlobalConfig | CvarNoInitCall;
    public const int CorpseFlags = CvarArchive | CvarServerInfo | CvarNoInitCall;
    public const int MaxDecalsFlags = CvarArchive | CvarNoInitCall;
    public const int MaxParticlesFlags = CvarArchive | CvarNoInitCall;
    public const int TeamDamageFlags = CvarServerInfo | CvarNoInitCall;
    public const int ScaleWeaponFovFlags = CvarArchive | CvarGlobalConfig;

    public sealed class Level
    {
        public List<object?> CorpseQueue { get; } = [];
        public object?[] BodyQue { get; } = new object?[BodyQueSize];
        public int BodyQueSlot { get; set; }
        public int ImpactDecalCount { get; set; }
        public float TeamDamage { get; set; }
        public List<object> AutoDecals { get; } = [];
    }

    readonly Dictionary<string, int> _depth = new();
    bool _spreadDecals = true;
    bool _pushers = true;
    bool _cacheNodes = true;
    float _cacheTime = 0.6f;
    bool _alwaysApplyDmFlags;
    bool _friction = true;
    int _turnWalkFast = 640;
    int _turnSprintFast = 1280;
    int _turnWalkSlow = 320;
    int _turnSprintSlow = 320;
    bool _glLights = true;
    int _corpseFilter = 1;
    int _corpseQueueSize = 64;
    int _maxDecals = 1024;
    int _maxParticles = 4000;
    float _teamDamage;
    float _scaleWeaponFov = 1f;

    public VmGameCvars1362Slice()
    {
        SendServerInfoChange = name =>
        {
            if (NetGame && !LocalCanControlSettings)
            {
                Messages.Add($"Only setting controllers or server/RCON console can change server CVAR {name}\n");
                return true;
            }

            return GameState != GsStartup && !DemoPlayback && !SaveGameRestore;
        };
    }

    public bool UseCallback { get; set; }
    public bool CallbacksInstalled { get; set; } = true;
    public bool NetGame { get; set; }
    public bool LocalCanControlSettings { get; set; } = true;
    public int GameTic { get; set; }
    public int GameState { get; set; } = GsStartup;
    public bool DemoPlayback { get; set; }
    public bool SaveGameRestore { get; set; }
    public bool SyncingDynamicLights { get; private set; }
    public bool RDynLights { get; set; } = true;
    public Level? PrimaryLevel { get; set; }
    public List<Level?> Levels { get; } = [];
    public List<string> Messages { get; } = [];
    public List<string> SwallowedSets { get; } = [];
    public int ApplyEntered { get; private set; }
    public Func<string, bool>? SendServerInfoChange { get; set; }
    public Action<object>? DestroyActor { get; set; }
    public Action<Level>? RecreateAllAttachedLights { get; set; }
    public Action<Level>? DeleteAllAttachedLights { get; set; }
    public Action<Level?>? InitParticles { get; set; }
    public Func<Level, int, object?>? FirstThinker { get; set; }

    public bool SpreadDecals => _spreadDecals;
    public bool Pushers => _pushers;
    public bool CacheNodes => _cacheNodes;
    public float CacheTime => _cacheTime;
    public bool AlwaysApplyDmFlags => _alwaysApplyDmFlags;
    public bool Friction => _friction;
    public int TurnWalkFast => _turnWalkFast;
    public int TurnSprintFast => _turnSprintFast;
    public int TurnWalkSlow => _turnWalkSlow;
    public int TurnSprintSlow => _turnSprintSlow;
    public bool GlLights => _glLights;
    public int CorpseFilter => _corpseFilter;
    public int CorpseQueueSize => _corpseQueueSize;
    public int MaxDecals => _maxDecals;
    public int MaxParticles => _maxParticles;
    public float TeamDamage => _teamDamage;
    public float ScaleWeaponFov => _scaleWeaponFov;

    public void SetSpreadDecals(bool value) => StoreBool(ref _spreadDecals, SpreadDecalsFlags, "cl_spreaddecals", value, null);
    public void SetPushers(bool value) => StoreBool(ref _pushers, PushersFlags, "var_pushers", value, null);
    public void SetCacheNodes(bool value) => StoreBool(ref _cacheNodes, CacheNodesFlags, "gl_cachenodes", value, null);
    public void SetCacheTime(float value) => StoreFloat(ref _cacheTime, CacheTimeFlags, "gl_cachetime", value, null);
    public void SetAlwaysApplyDmFlags(bool value) => StoreBool(ref _alwaysApplyDmFlags, AlwaysApplyDmFlagsFlags, "alwaysapplydmflags", value, null);
    public void SetFriction(bool value) => StoreBool(ref _friction, FrictionFlags, "var_friction", value, null);
    public void SetScaleWeaponFov(float value) => StoreFloat(ref _scaleWeaponFov, ScaleWeaponFovFlags, "cl_scaleweaponfov", value, null);
    public void SetTurnWalkFast(int value) => StoreInt(ref _turnWalkFast, TurnSpeedFlags, "turnspeedwalkfast", value, OnWalkFast);
    public void SetTurnSprintFast(int value) => StoreInt(ref _turnSprintFast, TurnSpeedFlags, "turnspeedsprintfast", value, OnSprintFast);
    public void SetTurnWalkSlow(int value) => StoreInt(ref _turnWalkSlow, TurnSpeedFlags, "turnspeedwalkslow", value, OnWalkSlow);
    public void SetTurnSprintSlow(int value) => StoreInt(ref _turnSprintSlow, TurnSpeedFlags, "turnspeedsprintslow", value, OnSprintSlow);
    public void SetGlLights(bool value) => StoreBool(ref _glLights, GlLightsFlags, "gl_lights", value, OnGlLights);
    public void SetCorpseFilter(int value) => StoreInt(ref _corpseFilter, CorpseFlags, "sv_corpsefilter", value, OnCorpseFilter);
    public void ForceCorpseFilter(int value) => ForceInt(ref _corpseFilter, "sv_corpsefilter", value, OnCorpseFilter);
    public void SetCorpseQueueSize(int value) => StoreInt(ref _corpseQueueSize, CorpseFlags, "sv_corpsequeuesize", value, OnCorpseQueueSize);
    public void ForceCorpseQueueSize(int value) => ForceInt(ref _corpseQueueSize, "sv_corpsequeuesize", value, OnCorpseQueueSize);
    public void SetMaxDecals(int value) => StoreInt(ref _maxDecals, MaxDecalsFlags, "cl_maxdecals", value, OnMaxDecals);
    public void SetMaxParticles(int value) => StoreInt(ref _maxParticles, MaxParticlesFlags, "r_maxparticles", value, OnMaxParticles);
    public void SetTeamDamage(float value) => StoreFloat(ref _teamDamage, TeamDamageFlags, "teamdamage", value, OnTeamDamage);
    public void ForceTeamDamage(float value) => ForceFloat(ref _teamDamage, "teamdamage", value, OnTeamDamage);

    public void EnableCallbacks()
    {
        UseCallback = true;
        MaybeInit(TurnSpeedFlags, OnWalkFast);
        MaybeInit(TurnSpeedFlags, OnSprintFast);
        MaybeInit(TurnSpeedFlags, OnWalkSlow);
        MaybeInit(TurnSpeedFlags, OnSprintSlow);
        MaybeInit(GlLightsFlags, OnGlLights);
        MaybeInit(CorpseFlags, OnCorpseFilter);
        MaybeInit(CorpseFlags, OnCorpseQueueSize);
        MaybeInit(MaxDecalsFlags, OnMaxDecals);
        MaybeInit(MaxParticlesFlags, OnMaxParticles);
        MaybeInit(TeamDamageFlags, OnTeamDamage);
    }

    public void SyncDynamicLights(bool enabled)
    {
        if (SyncingDynamicLights)
            return;
        var wasSyncing = SyncingDynamicLights;
        SyncingDynamicLights = true;
        if (RDynLights != enabled)
            RDynLights = enabled;
        if (_glLights != enabled)
            StoreBool(ref _glLights, GlLightsFlags, "gl_lights", enabled, OnGlLights);
        SyncingDynamicLights = wasSyncing;
    }

    public void TrimMonsterCorpseQueue(Level level, int limit)
    {
        var queue = level.CorpseQueue;
        while ((uint)queue.Count > (uint)limit)
        {
            var corpse = queue[0];
            if (corpse != null)
                Destroy(corpse);
            queue.RemoveAt(0);
        }
    }

    public void TrimPlayerBodyQueue(Level level, int limit)
    {
        limit = Math.Clamp(limit, 0, BodyQueSize);
        for (var i = limit; i < BodyQueSize; i++)
        {
            if (level.BodyQue[i] != null)
            {
                Destroy(level.BodyQue[i]!);
                level.BodyQue[i] = null;
            }
        }

        if (limit <= 0)
            level.BodyQueSlot = 0;
        else if (level.BodyQueSlot >= limit)
            level.BodyQueSlot %= limit;
    }

    public void ApplyCorpseFilter()
    {
        ApplyEntered++;
        if (NetGame && GameTic == 0)
            return;
        if (GameState != GsLevel && GameState != GsTitleLevel)
            return;
        if (PrimaryLevel == null)
            return;

        var limit = Math.Max(_corpseQueueSize, 0);
        var filter = Math.Clamp(_corpseFilter, 0, 3);
        foreach (var level in Levels)
        {
            if (level == null)
                continue;
            if ((filter & 1) != 0)
                TrimMonsterCorpseQueue(level, limit);
            if ((filter & 2) != 0)
                TrimPlayerBodyQueue(level, limit);
        }
    }

    void MaybeInit(int flags, Action callback)
    {
        if ((flags & CvarNoInitCall) != 0)
            return;
        callback();
    }

    void OnWalkFast() => ClampTurn(ref _turnWalkFast, "turnspeedwalkfast", OnWalkFast);
    void OnSprintFast() => ClampTurn(ref _turnSprintFast, "turnspeedsprintfast", OnSprintFast);
    void OnWalkSlow() => ClampTurn(ref _turnWalkSlow, "turnspeedwalkslow", OnWalkSlow);
    void OnSprintSlow() => ClampTurn(ref _turnSprintSlow, "turnspeedsprintslow", OnSprintSlow);

    void ClampTurn(ref int slot, string name, Action again)
    {
        if (slot <= 0)
            StoreInt(ref slot, TurnSpeedFlags, name, 1, again);
    }

    void OnGlLights()
    {
        SyncDynamicLights(_glLights);
        foreach (var level in Levels)
        {
            if (level == null)
                throw new NullReferenceException();
            if (_glLights)
            {
                if (RecreateAllAttachedLights == null)
                    throw new InvalidOperationException("RecreateAllAttachedLights");
                RecreateAllAttachedLights(level);
            }
            else
            {
                if (DeleteAllAttachedLights == null)
                    throw new InvalidOperationException("DeleteAllAttachedLights");
                DeleteAllAttachedLights(level);
            }
        }
    }

    void OnCorpseFilter()
    {
        StoreInt(ref _corpseFilter, CorpseFlags, "sv_corpsefilter", Math.Clamp(_corpseFilter, 0, 3), OnCorpseFilter);
        if (!(NetGame && GameTic == 0))
            ApplyCorpseFilter();
    }

    void OnCorpseQueueSize()
    {
        if (_corpseQueueSize < 0)
            StoreInt(ref _corpseQueueSize, CorpseFlags, "sv_corpsequeuesize", 0, OnCorpseQueueSize);
        if (!(NetGame && GameTic == 0))
            ApplyCorpseFilter();
    }

    void OnMaxDecals()
    {
        if (_maxDecals < 0)
            StoreInt(ref _maxDecals, MaxDecalsFlags, "cl_maxdecals", 0, OnMaxDecals);
        else
        {
            foreach (var level in Levels)
            {
                if (level == null)
                    throw new NullReferenceException();
                while (level.ImpactDecalCount > _maxDecals)
                {
                    object? thinker = FirstThinker != null
                        ? FirstThinker(level, StatAutoDecal)
                        : level.AutoDecals.Count == 0 ? null : level.AutoDecals[0];
                    if (thinker != null)
                    {
                        Destroy(thinker);
                        level.AutoDecals.Remove(thinker);
                        level.ImpactDecalCount--;
                    }
                    else
                        throw new InvalidOperationException("STAT_AUTODECAL");
                }
            }
        }
    }

    void OnMaxParticles()
    {
        if (_maxParticles == 0)
            StoreInt(ref _maxParticles, MaxParticlesFlags, "r_maxparticles", 4000, OnMaxParticles);
        else if (_maxParticles > 65535)
            StoreInt(ref _maxParticles, MaxParticlesFlags, "r_maxparticles", 65535, OnMaxParticles);
        else if (_maxParticles < 100)
            StoreInt(ref _maxParticles, MaxParticlesFlags, "r_maxparticles", 100, OnMaxParticles);
        if (GameState != GsStartup)
        {
            foreach (var level in Levels)
            {
                if (InitParticles == null)
                    throw new InvalidOperationException("P_InitParticles");
                InitParticles(level);
            }
        }
    }

    void OnTeamDamage()
    {
        foreach (var level in Levels)
        {
            if (level == null)
                throw new NullReferenceException();
            level.TeamDamage = _teamDamage;
        }
    }

    bool Swallow(int flags, string name)
    {
        if ((flags & CvarServerInfo) == 0 || !CallbacksInstalled || SendServerInfoChange == null)
            return false;
        if (!SendServerInfoChange(name))
            return false;
        SwallowedSets.Add(name);
        return true;
    }

    void StoreBool(ref bool slot, int flags, string name, bool value, Action? callback)
    {
        if (Swallow(flags, name))
            return;
        ForceBool(ref slot, name, value, callback);
    }

    void ForceBool(ref bool slot, string name, bool value, Action? callback)
    {
        var unchanged = slot == value;
        slot = value;
        RunCallback(name, unchanged, callback);
    }

    void StoreInt(ref int slot, int flags, string name, int value, Action? callback)
    {
        if (Swallow(flags, name))
            return;
        ForceInt(ref slot, name, value, callback);
    }

    void ForceInt(ref int slot, string name, int value, Action? callback)
    {
        var unchanged = slot == value;
        slot = value;
        RunCallback(name, unchanged, callback);
    }

    void StoreFloat(ref float slot, int flags, string name, float value, Action? callback)
    {
        if (Swallow(flags, name))
            return;
        ForceFloat(ref slot, name, value, callback);
    }

    void ForceFloat(ref float slot, string name, float value, Action? callback)
    {
        var unchanged = slot == value;
        slot = value;
        RunCallback(name, unchanged, callback);
    }

    void RunCallback(string name, bool unchanged, Action? callback)
    {
        if (!UseCallback || callback == null)
            return;
        _depth.TryGetValue(name, out var depth);
        if (depth > 0 && unchanged)
            throw new InvalidOperationException(name);
        _depth[name] = depth + 1;
        try
        {
            callback();
        }
        finally
        {
            _depth[name] = depth;
        }
    }

    void Destroy(object actor)
    {
        if (DestroyActor == null)
            throw new InvalidOperationException("Destroy");
        DestroyActor(actor);
    }
}
