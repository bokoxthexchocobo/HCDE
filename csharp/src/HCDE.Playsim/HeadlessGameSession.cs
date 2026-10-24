using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Headless authority session: native <c>G_InitNew</c>, <c>G_DoLoadLevel</c>, <c>G_DoWorldDone</c>,
/// intermission handoff, and co-op travel without client UI.
/// </summary>
public sealed class HeadlessGameSession : IDisposable
{
    private readonly HeadlessGameSessionConfig _config;
    private AuthoritySimulation? _simulation;
    private List<HeadlessTravellerSnapshot> _travellers = [];
    private HeadlessGameAction _gameAction = HeadlessGameAction.Nothing;
    private string? _pendingMap;
    private int _pendingStartPos;
    private bool _pendingNewGame;
    private int _intermissionTicsRemaining;
    private ClusterTransitionText? _pendingIntermission;

    public VmBBotGame.CajunMasterState Cajun { get; } = new();

    public HeadlessGameSession(HeadlessGameSessionConfig config)
    {
        _config = config;
        State = HeadlessGameState.Startup;
    }

    public HeadlessGameState State { get; private set; }
    public HeadlessGameAction GameAction => _gameAction;
    public AuthoritySimulation? Simulation => _simulation;
    public string CurrentMapName { get; private set; } = "";
    public string? PendingNextMap { get; private set; }
    public ClusterTransitionText? ActiveIntermission => _pendingIntermission;
    public int IntermissionTicsRemaining => _intermissionTicsRemaining;
    public bool EndSequenceReached { get; private set; }

    public event Action<ClusterTransitionText?>? IntermissionStarted;
    public event Action<string>? MapLoaded;
    public event Action? EndSequence;

    public void InitNew(string mapName, int? rngSeed = null, bool newGame = true)
    {
        _gameAction = HeadlessGameAction.NewGame;
        _pendingMap = mapName;
        _pendingStartPos = 0;
        _pendingNewGame = newGame;
        if (rngSeed.HasValue)
            _config.RngSeed = rngSeed.Value;
        ProcessGameActions();
    }

    public void Tick()
    {
        ProcessGameActions();
        if (State == HeadlessGameState.Intermission)
        {
            if (_intermissionTicsRemaining > 0)
                _intermissionTicsRemaining--;
            if (_intermissionTicsRemaining <= 0)
            {
                _gameAction = HeadlessGameAction.WorldDone;
                State = HeadlessGameState.Level;
                ProcessGameActions();
            }
            return;
        }

        if (_simulation is not null)
        {
            VmBBotGame.Main(
                Cajun,
                _simulation,
                demoPlayback: false,
                gameStateLevel: VmBBot.GsLevel,
                localAuthority: true,
                levelFrozen: _simulation.LevelFrozen,
                print: _ => { },
                botSpawnRandom: _simulation.BotSpawnRandom);
            _simulation.DrainNetDemoCommands(Cajun, consolePlayer: 0, print: _ => { });
            HeadlessGameFlow.TryConsumeReloadRequest(this);
            _simulation.Tick();
        }
    }

    public void Dispose()
    {
        if (_simulation is not null)
        {
            _simulation.LevelExited -= OnLevelExited;
            _simulation = null;
        }
    }

    private void ProcessGameActions()
    {
        while (_gameAction != HeadlessGameAction.Nothing)
        {
            switch (_gameAction)
            {
                case HeadlessGameAction.NewGame:
                    DoNewGame();
                    break;
                case HeadlessGameAction.Completed:
                    DoCompleted();
                    break;
                case HeadlessGameAction.WorldDone:
                    DoWorldDone();
                    break;
                case HeadlessGameAction.LoadLevel:
                    DoLoadLevel(_pendingMap ?? CurrentMapName, _pendingStartPos, _pendingNewGame);
                    break;
                default:
                    _gameAction = HeadlessGameAction.Nothing;
                    break;
            }
        }
    }

    private void DoNewGame()
    {
        var map = _pendingMap ?? MapLoaderConstants.DefaultMapName;
        _pendingNewGame = true;
        DoLoadLevel(map, _pendingStartPos, newGame: true);
        _gameAction = HeadlessGameAction.Nothing;
    }

    private void DoLoadLevel(string mapName, int startPos, bool newGame)
    {
        if (_simulation is not null)
        {
            _simulation.LevelExited -= OnLevelExited;
            _simulation = null;
        }

        if (!HeadlessMapExit.TryLoadPlayLevel(_config.IwadBytes, mapName, _config.MapInfo,
                _config.ThingFlagFormat, out var level, out var error))
            throw new InvalidDataException(error ?? $"map-load-failed:{mapName}");

        _simulation = AuthoritySimulation.Start(level!, _config.RngSeed, _config.Dehacked,
            _config.Compat, _config.SpawnOptions, _config.NoExit);
        _simulation.CajunMaster = Cajun;
        VmBBotGame.Init(
            Cajun,
            ctf: false,
            teamplay: false,
            wantedBotNum: 0,
            getSpawnedCount: Cajun.GetSpawned.Count);
        _simulation.AllowSinglePlayerRespawn = _config.AllowSinglePlayerRespawn;
        _simulation.ForceRespawn = _config.ForceRespawn;
        _simulation.NoRespawn = _config.NoRespawn;
        _simulation.LevelExited += OnLevelExited;
        CurrentMapName = mapName;
        HeadlessLevelTravel.FinishTravel(_simulation, _travellers);
        _travellers = [];
        State = HeadlessGameState.Level;
        PendingNextMap = null;
        EndSequenceReached = false;
        _gameAction = HeadlessGameAction.Nothing;
        MapLoaded?.Invoke(mapName);
    }

    private void OnLevelExited()
    {
        if (_simulation is null || _gameAction == HeadlessGameAction.Completed)
            return;
        _gameAction = HeadlessGameAction.Completed;
        ProcessGameActions();
    }

    private void DoCompleted()
    {
        if (_simulation is null)
        {
            _gameAction = HeadlessGameAction.Nothing;
            return;
        }

        if (_config.SpawnOptions.Mode != SpawnGameMode.Deathmatch)
        {
            foreach (var player in _simulation.Players.Where(p => p.IsDead))
                _simulation.TryPlayerRespawn(player);
        }

        _travellers = HeadlessLevelTravel.StartTravel(_simulation);
        var secret = _simulation.SecretExit;
        if (!HeadlessMapExit.TryResolveNextMap(_config.MapInfo, CurrentMapName, secret,
                out var nextMap, out var endSequence))
        {
            _gameAction = HeadlessGameAction.Nothing;
            return;
        }

        if (endSequence || nextMap is null)
        {
            EndSequenceReached = true;
            EndSequence?.Invoke();
            _gameAction = HeadlessGameAction.Nothing;
            return;
        }

        PendingNextMap = nextMap;
        var deathmatch = _config.SpawnOptions.Mode == SpawnGameMode.Deathmatch;
        var useIntermission = HeadlessMapExit.ShouldDoIntermission(_config.MapInfo, CurrentMapName, nextMap,
            deathmatch, _config.AlwaysIntermission, _config.LevelNoIntermission);
        if (useIntermission && _config.IntermissionTics > 0)
        {
            ClusterTextResolver.TryResolveTransition(_config.IwadBytes, _config.MapInfo ?? new MapInfoSet(),
                CurrentMapName, nextMap, deathmatch, out _pendingIntermission, out _, secret);
            _intermissionTicsRemaining = _config.IntermissionTics;
            State = HeadlessGameState.Intermission;
            IntermissionStarted?.Invoke(_pendingIntermission);
            _pendingMap = nextMap;
            _pendingStartPos = 0;
            _pendingNewGame = false;
            _gameAction = HeadlessGameAction.Nothing;
            return;
        }

        _pendingMap = nextMap;
        _pendingStartPos = 0;
        _pendingNewGame = false;
        _gameAction = HeadlessGameAction.WorldDone;
    }

    private void DoWorldDone()
    {
        DoLoadLevel(_pendingMap ?? CurrentMapName, _pendingStartPos, _pendingNewGame);
        _gameAction = HeadlessGameAction.Nothing;
    }
}

public sealed class HeadlessGameSessionConfig
{
    public required byte[] IwadBytes { get; init; }
    public MapInfoSet? MapInfo { get; init; }
    public DehackedPatchResult? Dehacked { get; init; }
    public CompatSurface Compat { get; init; }
    public SpawnOptions SpawnOptions { get; init; } = new();
    public BinaryThingFlagFormat ThingFlagFormat { get; init; } = BinaryThingFlagFormat.Doom;
    public bool NoExit { get; init; }
    public int RngSeed { get; set; }
    public int IntermissionTics { get; init; } = GameTicClock.TicRate * 5;
    public bool AlwaysIntermission { get; init; }
    public bool LevelNoIntermission { get; init; }
    public bool AllowSinglePlayerRespawn { get; init; }
    public bool ForceRespawn { get; init; }
    public bool NoRespawn { get; init; }
}
