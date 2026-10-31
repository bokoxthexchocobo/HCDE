namespace HCDE.Playsim.Tests;

public class VmDebugServer1385SliceTests
{
    [Fact]
    public void ListenSkipsPortZeroAndCloseOrDisconnectWakesTheRestartThread()
    {
        var slice = new VmDebugServer1385Slice();
        Assert.Equal(["mark|DAP debug server object created"], slice.Trace);
        Assert.False(slice.Listen(0));
        Assert.False(slice.PortChosen);
        Assert.Null(slice.Server);
        Assert.False(slice.RestartWaiting);
        Assert.Equal(
            [
                "mark|DAP debug server object created",
                "warning|DAP debug server listen skipped: port is 0",
            ],
            slice.Trace);

        Assert.True(slice.Listen(4600));
        Assert.Equal(4600, slice.Port);
        Assert.True(slice.RestartWaiting);
        Assert.False(slice.Stopped);
        Assert.False(slice.TerminateFlag);
        Assert.Equal(1, slice.SubscribeCount);
        Assert.True(slice.EnabledHandler);
        Assert.True(slice.Server!.Running);
        Assert.Equal(4600, slice.Server.StartedPort);

        slice.OnClientConnected(null);
        slice.OnClientConnected(new VmDebugServer1385Slice.Connection { Open = false });
        Assert.Equal(0, slice.Script.StartCount);

        slice.Server.OnConnect!(new VmDebugServer1385Slice.Connection());
        var session = slice.Script.Active!;
        Assert.Equal(1, slice.Script.StartCount);
        Assert.False(slice.Closed);
        Assert.NotNull(session.OnClosed);
        Assert.NotNull(session.OnDisconnectSent);

        slice.Script.Ending = true;
        session.OnClosed!();
        Assert.False(slice.TerminateFlag);
        slice.Script.Ending = false;

        slice.HoldGate();
        session.OnClosed!();
        Assert.False(slice.TerminateFlag);
        Assert.False(slice.Closed);
        slice.ReleaseGate();

        session.OnClosed!();
        Assert.True(slice.TerminateFlag);
        Assert.True(slice.Closed);
        slice.RunRestartThread();
        Assert.Equal(1, slice.Script.EndCount);
        Assert.True(slice.Script.LastClosed);
        Assert.False(slice.TerminateFlag);
        Assert.True(slice.RestartWaiting);
        Assert.Equal(0, slice.Server.StopCount);

        slice.Script.Quitting = true;
        session.OnDisconnectSent!();
        var exited = Assert.Throws<VmDebugServer1385Slice.ExitEvent>(() => slice.RunRestartThread());
        Assert.Equal(0, exited.Reason);
        Assert.Equal("The game wants to exit", exited.Message);
        Assert.True(slice.Quitting);
        Assert.Equal(2, slice.Script.EndCount);
        Assert.False(slice.RestartWaiting);
        Assert.Equal(0, slice.Server.StopCount);
        Assert.Equal(
            [
                "DAP debugging server started on port 4600\n",
                "\u001cGClient connected but connection is not open\n",
                "\u001cGClient connected but connection is not open\n",
                "DAP client connected.\n",
                "DAP connection closed.\n",
                "DAP connection closed.\n",
                "DAP connection closed.\n",
                "\u001cGDAP connection closed without terminating session.\n",
                "Debugger disconnecting...\n",
            ],
            slice.Logs);
        Assert.DoesNotContain("mark|DAP debug server stopped", slice.Trace);
        Assert.Contains("mark|DAP client connected", slice.Trace);

        var joined = new VmDebugServer1385Slice();
        Assert.True(joined.Listen(1));
        joined.Script.Quitting = true;
        var stop = Assert.Throws<VmDebugServer1385Slice.ExitEvent>(() => joined.Stop());
        Assert.Equal(0, stop.Reason);
        Assert.Equal(1, joined.Server!.StopCount);
        Assert.Equal(1, joined.Script.EndCount);
        Assert.False(joined.Script.LastClosed);
        Assert.False(joined.EnabledHandler);
        Assert.Contains("mark|DAP debug server stop requested", joined.Trace);
        Assert.DoesNotContain("mark|DAP debug server stopped", joined.Trace);
    }

    [Fact]
    public void ErrorRestartsUntilStartFailsAndRelistenLeavesStopped()
    {
        var dead = new VmDebugServer1385Slice();
        dead.Destroy();
        Assert.Equal(
            [
                "mark|DAP debug server object created",
                "mark|DAP debug server stop requested",
                "mark|DAP debug server stopped",
            ],
            dead.Trace);
        Assert.True(dead.Stopped);
        Assert.True(dead.TerminateFlag);
        Assert.Equal(1, dead.UnsubscribeCount);
        Assert.Null(dead.Server);

        var failed = new VmDebugServer1385Slice
        {
            CreateServer = () => new VmDebugServer1385Slice.NetServer { AcceptStart = false },
        };
        Assert.False(failed.Listen(9));
        Assert.False(failed.Stopped);
        Assert.False(failed.EnabledHandler);
        Assert.Equal(0, failed.SubscribeCount);
        Assert.True(failed.RestartWaiting);
        Assert.Equal(9, failed.Server!.StartedPort);
        Assert.Contains("error|DAP debug server failed to start port=9", failed.Trace);
        Assert.Contains("\u001cGDAP debugging server failed to start on port 9, debugging will be unavailable!\n", failed.Logs);
        failed.Stop();
        Assert.Equal(1, failed.Server.StopCount);
        Assert.Equal(1, failed.Script.EndCount);
        Assert.False(failed.Script.LastClosed);
        Assert.Contains("mark|DAP debug server stopped", failed.Trace);
        Assert.False(failed.RestartWaiting);

        var births = 0;
        var slice = new VmDebugServer1385Slice
        {
            CreateServer = () =>
            {
                births++;
                return new VmDebugServer1385Slice.NetServer
                {
                    AcceptStart = births < 3,
                    ErrorDuringStart = births == 3 ? "mid" : null,
                };
            },
        };
        Assert.True(slice.Listen(7));
        slice.Server!.OnError!("boom");
        slice.RunRestartThread();
        Assert.False(slice.RestartPending);
        Assert.True(slice.RestartWaiting);
        Assert.Equal(2, slice.Created.Count);
        Assert.Equal(1, slice.Created[0].StopCount);
        Assert.True(slice.Server.Running);
        Assert.Equal(2, slice.SubscribeCount);
        Assert.Equal(7, slice.Port);

        slice.Server.OnError!(null);
        slice.RunRestartThread();
        Assert.True(slice.Stopped);
        Assert.True(slice.RestartPending);
        Assert.False(slice.RestartWaiting);
        Assert.False(slice.EnabledHandler);
        Assert.Equal(1, slice.UnsubscribeCount);
        Assert.Equal(3, slice.Created.Count);
        Assert.Equal(1, slice.Created[1].StopCount);
        Assert.Equal(0, slice.Created[2].StopCount);
        Assert.Same(slice.Created[2], slice.Server);
        Assert.Equal(
            [
                "mark|DAP debug server object created",
                "mark|DAP debug server listen requested port=7",
                "mark|DAP debug server started port=7",
                "warning|DAP server error: boom",
                "mark|DAP debug server started port=7",
                "warning|DAP server error: unknown",
                "warning|DAP server error: mid",
                "error|DAP debug server failed to start port=7",
                "error|DAP debug server restart failed",
            ],
            slice.Trace);
        Assert.Equal(
            [
                "DAP debugging server started on port 7\n",
                "\u001cGServer error: boom\n\n",
                "\u001cGRestarting server...\n",
                "DAP debugging server started on port 7\n",
                "\u001cGServer error: (null)\n\n",
                "\u001cGRestarting server...\n",
                "\u001cGServer error: mid\n\n",
                "\u001cGRestart failed! Stopping server...\n",
                "\u001cGDAP debugging server failed to start on port 7, debugging will be unavailable!\n",
            ],
            slice.Logs);

        var again = new VmDebugServer1385Slice();
        Assert.True(again.Listen(2));
        Assert.True(again.Listen(3));
        Assert.True(again.Stopped);
        Assert.False(again.TerminateFlag);
        Assert.False(again.Closed);
        Assert.Equal(3, again.Port);
        Assert.Single(again.Created);
        Assert.Equal(1, again.Server!.StopCount);
        Assert.True(again.Server.Running);
        Assert.Equal(3, again.Server.StartedPort);
        Assert.Equal(1, again.Script.EndCount);
        Assert.False(again.Script.LastClosed);
        Assert.True(again.RestartWaiting);
        Assert.Equal(2, again.SubscribeCount);
        Assert.Equal(1, again.UnsubscribeCount);
        Assert.True(again.EnabledHandler);
        again.Server.OnError!("later");
        Assert.True(again.RestartPending);
        Assert.True(again.Closed);
        again.RunRestartThread();
        Assert.True(again.Stopped);
        Assert.True(again.RestartPending);
        Assert.False(again.RestartWaiting);
        Assert.Equal(2, again.Script.EndCount);
        Assert.True(again.Script.LastClosed);
        Assert.Equal(1, again.Server.StopCount);
        Assert.True(again.Server.Running);
        Assert.Single(again.Created);
        Assert.Contains("mark|DAP debug server stopped", again.Trace);
        Assert.Contains("mark|DAP debug server listen requested port=3", again.Trace);
        Assert.DoesNotContain("error|DAP debug server restart failed", again.Trace);
        Assert.Contains("\u001cGServer error: later\n\n", again.Logs);
        Assert.Contains("\u001cGRestarting server...\n", again.Logs);

        var quitting = new VmDebugServer1385Slice();
        Assert.True(quitting.Listen(4));
        quitting.Script.Quitting = true;
        var ex = Assert.Throws<VmDebugServer1385Slice.ExitEvent>(() => quitting.Listen(5));
        Assert.Equal(0, ex.Reason);
        Assert.Equal(5, quitting.Port);
        Assert.Single(quitting.Created);
        Assert.Equal(1, quitting.Server!.StopCount);
        Assert.Equal(1, quitting.Script.EndCount);
        Assert.False(quitting.Script.LastClosed);
        Assert.True(quitting.Quitting);
        Assert.False(quitting.RestartWaiting);
        Assert.Contains("mark|DAP debug server stop requested", quitting.Trace);
        Assert.DoesNotContain("mark|DAP debug server listen requested port=5", quitting.Trace);
        Assert.DoesNotContain("mark|DAP debug server stopped", quitting.Trace);
    }
}
