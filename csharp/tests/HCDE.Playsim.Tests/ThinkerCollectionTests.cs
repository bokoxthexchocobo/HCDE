namespace HCDE.Playsim.Tests;

public class ThinkerCollectionTests
{
    [Fact]
    public void Run_TicksLowerStatsBeforeHigherStats()
    {
        var log = new List<string>();
        var world = new ThinkerCollection();
        world.Add(new LoggingThinker("late", log), ThinkerStat.Default);
        world.Add(new LoggingThinker("early", log), ThinkerStat.FirstThinking);

        world.Run();

        Assert.Equal(new[] { "early", "late" }, log);
        Assert.Equal(1, world.Clock.Tic);
    }

    [Fact]
    public void Run_PostBeginsFreshThinkersBeforeTheirFirstTick()
    {
        var world = new ThinkerCollection();
        var thinker = new Thinker();
        world.Add(thinker);

        world.Run();

        Assert.Equal(1, thinker.PostBeginCount);
        Assert.Equal(1, thinker.TickCount);
        Assert.False(thinker.JustSpawned);
        Assert.Single(world.ThinkersIn(ThinkerStat.Default));
    }

    [Fact]
    public void Run_TicksThinkersSpawnedDuringTheSamePass()
    {
        var log = new List<string>();
        var world = new ThinkerCollection();
        world.Add(new SpawningThinker("parent", log, world, ThinkerStat.Default));

        world.Run();

        Assert.Equal(new[] { "parent", "child" }, log);
        Assert.Equal(2, world.ThinkersIn(ThinkerStat.Default).Count);
    }

    [Fact]
    public void Run_SkipsDestroyedThinkersAndNonThinkingStats()
    {
        var world = new ThinkerCollection();
        var dead = new DestroyOnBeginThinker();
        var info = new Thinker();
        world.Add(dead);
        world.Add(info, ThinkerStat.Info);

        world.Run();

        Assert.Equal(1, dead.PostBeginCount);
        Assert.Equal(0, dead.TickCount);
        Assert.Equal(0, info.TickCount);
    }

    private sealed class LoggingThinker : Thinker
    {
        private readonly string _name;
        private readonly List<string> _log;

        public LoggingThinker(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Tick()
        {
            base.Tick();
            _log.Add(_name);
        }
    }

    private sealed class SpawningThinker : Thinker
    {
        private readonly string _name;
        private readonly List<string> _log;
        private readonly ThinkerCollection _world;
        private readonly int _childStat;

        public SpawningThinker(string name, List<string> log, ThinkerCollection world, int childStat)
        {
            _name = name;
            _log = log;
            _world = world;
            _childStat = childStat;
        }

        public override void Tick()
        {
            base.Tick();
            _log.Add(_name);
            if (TickCount == 1)
                _world.Add(new LoggingThinker("child", _log), _childStat);
        }
    }

    private sealed class DestroyOnBeginThinker : Thinker
    {
        public override void PostBeginPlay()
        {
            base.PostBeginPlay();
            Destroy();
        }
    }
}
