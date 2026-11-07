using S = HCDE.Playsim.VmNetWindow1394Slice;

namespace HCDE.Playsim.Tests;

public class VmNetWindow1394SliceTests
{
    [Fact]
    public void ErrorInitMessageAndLoopFollowTheDedicatedBranch()
    {
        var session = new S.Session();
        var ex = Assert.Throws<InvalidOperationException>(() => S.NetError(null, session, false));
        Assert.Equal(S.UnknownError, ex.Message);
        Assert.Equal("\r" + ("Fatal network error: " + S.UnknownError).PadRight(40) + "\n", Assert.Single(session.Logs));
        Assert.Equal("fatal network error: " + S.UnknownError, Assert.Single(session.Warnings));
        Assert.Equal(1, session.Closes);

        session = new S.Session { Dedicated = true };
        ex = Assert.Throws<InvalidOperationException>(() => S.NetError("boom", session, true));
        Assert.Equal("boom", ex.Message);
        Assert.Equal("Fatal network error: boom\n", Assert.Single(session.Logs));
        Assert.Equal(0, session.Closes);

        session = new S.Session();
        Assert.Throws<InvalidOperationException>(() => S.NetError("x", session, false));
        Assert.Equal("\r" + "Fatal network error: x".PadRight(40) + "\n", Assert.Single(session.Logs));

        session = new S.Session { Silent = true };
        Assert.Throws<InvalidOperationException>(() => S.NetError("quiet", session, true));
        Assert.Equal(0, session.Closes);
        Assert.Equal("Fatal network error: quiet\n", Assert.Single(session.Logs));

        session = new S.Session { Dedicated = true, WindowsDedicatedBuild = true };
        S.NetInit("listening", true, session);
        Assert.Equal("NetServer:: listening\n", Assert.Single(session.Prints));
        Assert.Equal("listening", Assert.Single(session.Statuses));
        Assert.Empty(session.Guards);
        Assert.Equal(0, session.Inits);

        session = new S.Session();
        S.NetInit("listening", true, session);
        S.NetMessage("wait", session);
        Assert.Equal(S.GuardWindow, session.Guards[0]);
        Assert.Equal(S.GuardMessage, session.Guards[1]);
        Assert.Equal("listening", session.InitMessage);
        Assert.True(session.InitHost);
        Assert.Equal(1, session.Inits);
        Assert.Equal("wait", Assert.Single(session.WindowMessages));
        Assert.Equal("NetSession:: wait\n", session.Prints[1]);

        session = new S.Session { Silent = true };
        S.NetMessage("quiet", session);
        Assert.Empty(session.Guards);
        Assert.Empty(session.WindowMessages);

        int calls = 0;
        session = new S.Session { Dedicated = true };
        Assert.True(S.NetLoop(() => ++calls == 2, session));
        Assert.Equal(2, session.RconPolls);
        Assert.Equal(1, session.Sleeps);
        Assert.Equal(0, session.Pumps);

        calls = 0;
        session = new S.Session { Dedicated = true, WindowsDedicatedBuild = true, AbortRequested = true };
        Assert.False(S.NetLoop(() => ++calls == 2, session));
        Assert.Equal(1, session.RconPolls);
        Assert.Equal(1, session.Pumps);
        Assert.Equal(0, session.Sleeps);

        session = new S.Session { Silent = true, AbortRequested = true };
        Assert.False(S.NetLoop(() => true, session));
        Assert.Equal(0, session.RconPolls);
        Assert.Equal(0, session.Sleeps);

        session = new S.Session();
        Assert.True(S.NetLoop(() => throw new InvalidOperationException("callback"), session, true));
        Assert.True(session.WindowLoop);
        Assert.Equal(2u, S.CflHost);
        Assert.Equal(1u, S.CflConsolePlayer);
        Assert.Equal(0u, S.CflNone);
    }

    [Fact]
    public void ClientFlowKickBanAndDoneUseTheWindowCallees()
    {
        var session = new S.Session { ConsolePlayer = 2 };
        string Name(int client, uint limit) => limit == 0 ? "AlicePlayer" : "Ali";
        S.ClientConnected(2, session, Name, _ => false, _ => true, 7, 3);
        var connect = Assert.Single(session.Connects);
        Assert.Equal("Ali", connect.Name);
        Assert.Equal(S.CflHost | S.CflConsolePlayer, connect.Flags);
        Assert.Equal(7, connect.Status);
        Assert.Equal("NetSession:: Client 'AlicePlayer' connected.\n", Assert.Single(session.Prints));

        session = new S.Session { Dedicated = true };
        S.ClientConnected(0, session, Name, client => client == 0, _ => true, 1);
        Assert.Empty(session.Connects);
        Assert.Equal("NetServer:: Dedicated server authority slot ready.\n", Assert.Single(session.Prints));

        session = new S.Session();
        S.ClientUpdated(4, 3, session);
        Assert.Equal(4, session.Updates[0].Client);
        Assert.Equal(3, session.Updates[0].Status);

        S.ClientDisconnected(4, session, (client, _) => "Bob", "");
        Assert.Equal("NetSession:: Client 'Bob' disconnected.\n", session.Prints[0]);
        Assert.Empty(session.Warnings);
        S.ClientDisconnected(4, session, (client, _) => "Bob", "timeout");
        Assert.Equal("NetSession:: Client 'Bob' disconnected (timeout).\n", session.Prints[1]);
        Assert.Equal("client disconnected client=4 name=Bob reason=timeout", Assert.Single(session.Warnings));
        Assert.Equal(4, session.Disconnects[0]);
        Assert.Equal(4, session.Disconnects[1]);

        session.UsesDedicatedSlot = true;
        S.UpdatePlayers(1, 4, session);
        S.UpdatePlayers(0, 0, session);
        session.UsesDedicatedSlot = false;
        S.UpdatePlayers(2, 5, session);
        Assert.Equal(0, session.Progresses[0].Current);
        Assert.Equal(3, session.Progresses[0].Limit);
        Assert.Equal(0, session.Progresses[1].Current);
        Assert.Equal(0, session.Progresses[1].Limit);
        Assert.Equal(2, session.Progresses[2].Current);
        Assert.Equal(5, session.Progresses[2].Limit);

        Assert.True(S.ShouldStart(new S.Session { Dedicated = true, StartRequested = true }, null));
        Assert.False(S.ShouldStart(new S.Session { Dedicated = true }, null));
        Assert.True(S.ShouldStart(session, () => true));

        var kicks = new List<int> { 8 };
        int[] queue = { 4, 9, -1 };
        int i = 0;
        S.KickClients(kicks, () => queue[i++]);
        Assert.Equal(4, kicks[0]);
        Assert.Equal(9, kicks[1]);
        Assert.Equal(2, kicks.ToArray().Length);

        var bans = new List<int>();
        S.BanClients(bans, () => -1);
        Assert.Empty(bans);
        S.NetDone(session);
        Assert.Equal(1, session.Dones);
    }
}
