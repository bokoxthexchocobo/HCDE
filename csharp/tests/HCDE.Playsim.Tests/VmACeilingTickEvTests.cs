namespace HCDE.Playsim.Tests;

public class VmACeilingTickEvTests
{
    [Fact]
    public void PlayCeilingSound_DefaultPicksCeilingNormal()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        string? snd = null;
        VmACeilingTickEv.PlayCeilingSoundSector(sec, 0, new VmACeilingTickEv.SoundContext
        {
            StartSequenceSound = (_, _, s, _) => snd = s,
        });
        Assert.Equal("CeilingNormal", snd);
    }

    [Fact]
    public void Tick_DownCrushSlowdown_ReducesSpeed()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var ceiling = new VmACeilingTickEv.CeilingTickThinker
        {
            Sector = sec,
            Type = VmACeilingCreate.CeilCrushAndRaise,
            Direction = -1,
            CrushMode = VmACeilingCreate.CrushSlowdown,
            Speed = 4,
        };
        VmACeilingTickEv.Tick(ceiling, new VmACeilingTickEv.TickContext
        {
            MoveCeilingDown = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.Crushed,
        });
        Assert.Equal(1.0 / 8, ceiling.Speed);
    }

    [Fact]
    public void EvDoCeiling_ManualTag_CallsCreate()
    {
        var sec = new VmACeilingCreate.CeilingSector { SectorNum = 2 };
        var line = new VmACeilingCreate.CeilingLine { FrontSector = sec };
        var created = false;
        VmACeilingTickEv.EvDoCeiling(new VmACeilingTickEv.EvCeilingContext
        {
            ManualBackSector = _ => sec,
            Create = new VmACeilingCreate.CreateCeilingContext
            {
                CreateThinker = (s, sp1, sp2, sil) =>
                {
                    created = true;
                    return new VmACeilingCreate.CeilingThinker { Sector = s, Speed = sp1, Speed1 = sp1, Speed2 = sp2 };
                },
                PlayCeilingSound = _ => { },
            },
        }, VmACeilingCreate.CeilRaiseByValue, line, 0, 4, 0, 8, 0, 0, 0, 0);
        Assert.True(created);
    }

    [Fact]
    public void Tick_UpPastDest_DestroyThinker()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var ceiling = new VmACeilingTickEv.CeilingTickThinker
        {
            Sector = sec,
            Type = VmACeilingCreate.CeilRaiseByValue,
            Direction = 1,
            TopHeight = 200,
            Speed = 4,
        };
        var destroyed = false;
        VmACeilingTickEv.Tick(ceiling, new VmACeilingTickEv.TickContext
        {
            MoveCeilingUp = (_, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            Destroy = _ => destroyed = true,
        });
        Assert.True(destroyed);
        Assert.True(ceiling.Destroyed);
    }

    [Fact]
    public void ActivateInStasisCeiling_RestoresDirection()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var thinker = new VmACeilingTickEv.CeilingTickThinker
        {
            Sector = sec,
            Tag = 7,
            Direction = 0,
            OldDirection = -1,
        };
        var ok = VmACeilingTickEv.ActivateInStasisCeiling(new VmACeilingTickEv.EvCeilingContext
        {
            CeilingThinkerIterator = () => new[] { thinker },
        }, 7);
        Assert.True(ok);
        Assert.Equal(-1, thinker.Direction);
    }

    [Fact]
    public void EvStopCeiling_DestroyThinkerOnTaggedSector()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var thinker = new VmACeilingTickEv.CeilingTickThinker { Sector = sec };
        sec.CeilingData = thinker;
        var destroyed = false;
        VmACeilingTickEv.EvStopCeiling(new VmACeilingTickEv.EvCeilingContext
        {
            SectorTagIteratorWithLine = (_, _) => new[] { 0 },
            SectorByIndex = _ => sec,
            GetCeilingThinker = _ => thinker,
            DestroyThinker = _ => destroyed = true,
        }, 1, null);
        Assert.True(destroyed);
    }

    [Fact]
    public void EvCeilingCrushStop_PutsCrusherInStasis()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var thinker = new VmACeilingTickEv.CeilingTickThinker
        {
            Sector = sec,
            Tag = 3,
            Direction = -1,
        };
        var stopped = false;
        VmACeilingTickEv.EvCeilingCrushStop(new VmACeilingTickEv.EvCeilingContext
        {
            CeilingThinkerIterator = () => new[] { thinker },
            StopSequence = _ => stopped = true,
        }, 3, false);
        Assert.Equal(0, thinker.Direction);
        Assert.Equal(-1, thinker.OldDirection);
        Assert.True(stopped);
    }
}
