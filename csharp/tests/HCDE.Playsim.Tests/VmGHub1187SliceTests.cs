namespace HCDE.Playsim.Tests;

public class VmGHub1187SliceTests
{
    static void Reset()
    {
        VmGHub1187Slice.ClearHubInfo();
        VmGHub1187Slice.Multiplayer = false;
        VmGHub1187Slice.Deathmatch = 0;
        VmGHub1187Slice.Strings.Clear();
    }

    static VmGHub1187Slice.Level Map(int num, int killed, int items, int secrets)
    {
        return new VmGHub1187Slice.Level
        {
            Levelnum = num,
            KilledMonsters = killed,
            FoundItems = items,
            FoundSecrets = secrets,
        };
    }

    static VmGHub1187Slice.Start Blank()
    {
        return new VmGHub1187Slice.Start();
    }

    [Fact]
    public void LeavingHub_SumsEveryMapThenClearsWhenTheHubEnds()
    {
        Reset();
        var prior = new VmGHub1187Slice.HubInfo
        {
            Levelnum = 3,
            MaxKills = 4,
            MaxItems = 1,
            MaxSecret = 8,
        };
        prior.Players[0].Skills = 2;
        prior.Players[1].Sitems = 5;
        VmGHub1187Slice.HubData.Add(prior);

        var wbs = Blank();
        wbs.FinishedEp = 99;
        wbs.MaxKills = 10;
        wbs.MaxItems = 3;
        wbs.MaxSecret = 1;
        wbs.MaxFrags = 6;
        wbs.Players[0].Skills = 50;
        wbs.Players[0].Stime = 7;
        wbs.Players[1].Sitems = 4;
        wbs.ThisAuthor = "keep";
        VmGHub1187Slice.Strings["ENTRY"] = "The Hub";
        var cluster = new VmGHub1187Slice.Cluster
        {
            Flags = VmGHub1187Slice.ClusterHub | VmGHub1187Slice.ClusterLookupName,
            ClusterName = "ENTRY",
        };

        VmGHub1187Slice.LeavingHub(Map(9, 40, 12, 2), VmGHub1187Slice.FinishNextHub, cluster, wbs);

        Assert.Empty(VmGHub1187Slice.HubData);
        Assert.Equal(40, wbs.TotalKills);
        Assert.Equal(14, wbs.MaxKills);
        Assert.Equal(4, wbs.MaxItems);
        Assert.Equal(9, wbs.MaxSecret);
        Assert.Equal(6, wbs.MaxFrags);
        Assert.Equal(42, wbs.Players[0].Skills);
        Assert.Equal(12, wbs.Players[0].Sitems);
        Assert.Equal(2, wbs.Players[0].Ssecret);
        Assert.Equal(7, wbs.Players[0].Stime);
        Assert.Equal(9, wbs.Players[1].Sitems);
        Assert.Equal("The Hub", wbs.ThisName);
        Assert.Equal("keep", wbs.ThisAuthor);
        Assert.Equal(VmGHub1187Slice.InvalidTexture, wbs.LName0);
        Assert.Equal(99, wbs.FinishedEp);
    }

    [Fact]
    public void SameHub_KeepsRowsAndDeathmatchKeepsThePassedCounters()
    {
        Reset();
        var cluster = new VmGHub1187Slice.Cluster
        {
            Flags = VmGHub1187Slice.ClusterHub | VmGHub1187Slice.ClusterLookupName,
            ClusterName = "ENTRY",
        };
        var wbs = Blank();
        wbs.FinishedEp = 99;
        wbs.MaxKills = 8;
        wbs.Players[0].Skills = 1;
        wbs.Players[0].Stime = 6;
        wbs.ThisName = "stay";

        VmGHub1187Slice.LeavingHub(Map(1, 3, 4, 5), VmGHub1187Slice.FinishSameHub, cluster, wbs);

        Assert.Single(VmGHub1187Slice.HubData);
        Assert.Equal(1, VmGHub1187Slice.HubData[0].Levelnum);
        Assert.Equal(3, VmGHub1187Slice.HubData[0].Players[0].Skills);
        Assert.Equal(4, VmGHub1187Slice.HubData[0].Players[0].Sitems);
        Assert.Equal(5, VmGHub1187Slice.HubData[0].Players[0].Ssecret);
        Assert.Equal(6, VmGHub1187Slice.HubData[0].Players[0].Stime);
        Assert.Equal(8, VmGHub1187Slice.HubData[0].MaxKills);
        Assert.Equal(1, wbs.Players[0].Skills);
        Assert.Equal("stay", wbs.ThisName);
        Assert.Equal(0, wbs.LName0);

        wbs.Players[0].Skills = 1;
        VmGHub1187Slice.LeavingHub(Map(1, 9, 1, 1), VmGHub1187Slice.FinishSameHub, cluster, wbs);
        Assert.Single(VmGHub1187Slice.HubData);
        Assert.Equal(9, VmGHub1187Slice.HubData[0].Players[0].Skills);

        VmGHub1187Slice.LeavingHub(Map(2, 1, 1, 1), VmGHub1187Slice.FinishSameHub, cluster, wbs);
        Assert.Equal(2, VmGHub1187Slice.HubData.Count);

        VmGHub1187Slice.Deathmatch = 2;
        wbs.Players[0].Skills = 15;
        VmGHub1187Slice.LeavingHub(Map(2, 100, 100, 100), VmGHub1187Slice.FinishSameHub, cluster, wbs);
        Assert.Equal(15, VmGHub1187Slice.HubData[1].Players[0].Skills);

        VmGHub1187Slice.Deathmatch = 0;
        VmGHub1187Slice.Multiplayer = true;
        wbs.Players[0].Skills = 16;
        VmGHub1187Slice.LeavingHub(Map(4, 70, 1, 1), VmGHub1187Slice.FinishSameHub, cluster, wbs);
        var coop = VmGHub1187Slice.HubData[^1];
        Assert.Equal(4, coop.Levelnum);
        Assert.Equal(16, coop.Players[0].Skills);

        var done = Blank();
        done.ThisAuthor = "old";
        done.MaxKills = 3;
        var raw = new VmGHub1187Slice.Cluster
        {
            Flags = VmGHub1187Slice.ClusterHub,
            ClusterName = "Cluster 7",
        };
        VmGHub1187Slice.LeavingHub(Map(8, 1, 0, 0), VmGHub1187Slice.FinishNoHub, raw, done);
        Assert.Empty(VmGHub1187Slice.HubData);
        Assert.Equal("Cluster 7", done.ThisName);
        Assert.Equal("", done.ThisAuthor);
        Assert.Equal(VmGHub1187Slice.InvalidTexture, done.LName0);

        VmGHub1187Slice.HubData.Add(new VmGHub1187Slice.HubInfo { Levelnum = 1 });
        var outside = Blank();
        outside.ThisName = "kept";
        VmGHub1187Slice.LeavingHub(
            Map(1, 9, 9, 9),
            VmGHub1187Slice.FinishNextHub,
            new VmGHub1187Slice.Cluster(),
            outside);
        Assert.Empty(VmGHub1187Slice.HubData);
        Assert.Equal("kept", outside.ThisName);
        Assert.Equal(0, outside.TotalKills);
    }

    [Fact]
    public void SerializeHub_WritesPlayerFieldsAndDropsAMissingRead()
    {
        Reset();
        var empty = new VmGHub1187Slice.Archive();
        VmGHub1187Slice.SerializeHub(empty);
        Assert.Empty(empty.Trace);

        var hub = new VmGHub1187Slice.HubInfo
        {
            Levelnum = 9,
            TotalKills = 3,
            MaxKills = 4,
            MaxItems = 5,
            MaxSecret = 6,
            MaxFrags = 7,
        };
        hub.Players[0].Skills = 11;
        hub.Players[0].Sitems = 12;
        hub.Players[0].Ssecret = 13;
        hub.Players[0].Stime = 14;
        hub.Players[0].Fragcount = 15;
        hub.Players[0].Frags[0] = 4;
        VmGHub1187Slice.HubData.Add(hub);

        var closed = new VmGHub1187Slice.Archive { OpenObjects = false };
        VmGHub1187Slice.SerializeHub(closed);
        Assert.Contains("array hubinfo", closed.Trace);
        Assert.DoesNotContain("levelnum=9", closed.Trace);
        Assert.Single(VmGHub1187Slice.HubData);

        var arc = new VmGHub1187Slice.Archive();
        VmGHub1187Slice.SerializeHub(arc);
        Assert.Equal("array hubinfo", arc.Trace[0]);
        var levelAt = arc.Trace.IndexOf("levelnum=9");
        var playersAt = arc.Trace.IndexOf("array players");
        var killsAt = arc.Trace.IndexOf("kills=11");
        Assert.True(levelAt > 0);
        Assert.True(playersAt > levelAt);
        Assert.True(killsAt > playersAt);
        Assert.Contains("items=12", arc.Trace);
        Assert.Contains("secrets=13", arc.Trace);
        Assert.Contains("time=14", arc.Trace);
        Assert.Contains("fragcount=15", arc.Trace);
        Assert.Contains("maxfrags=7", arc.Trace);
        Assert.Contains("#4", arc.Trace);
        Assert.Equal(64, arc.Trace.FindAll(line => line == "array frags").Count);

        var reading = new VmGHub1187Slice.Archive { Writing = false, ArraysOpen = false };
        VmGHub1187Slice.SerializeHub(reading);
        Assert.Empty(VmGHub1187Slice.HubData);
    }
}
