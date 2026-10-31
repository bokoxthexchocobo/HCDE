namespace HCDE.Playsim.Tests;

public class VmGlDebug1305SliceTests
{
    [Fact]
    public void Update_DrainsQueriesAndArmsTheBreakpointOnce()
    {
        VmGlDebug1305.ResetProcess();
        var host = new VmGlDebug1305.Host();
        VmGlDebug1305.OnGlDebugLevelChanged(host);
        Assert.Equal("No OpenGL debug support detected.\n", Assert.Single(host.Prints));
        host.Flags = VmGlDebug1305.DebugFlag;
        VmGlDebug1305.OnGlDebugLevelChanged(host);
        Assert.Single(host.Prints);
        host.Flags = 0;

        host.GpuStatActive = true;
        VmGlDebug1305.PushGroup(host, "a");
        VmGlDebug1305.PushGroup(host, "b");
        VmGlDebug1305.PopGroup(host);
        VmGlDebug1305.PopGroup(host);
        Assert.Equal(new[] { "gen:1", "begin:" + VmGlDebug1305.GlTimeElapsed + ":1", "gen:2", "begin:" + VmGlDebug1305.GlTimeElapsed + ":2", "end:" + VmGlDebug1305.GlTimeElapsed, "end:" + VmGlDebug1305.GlTimeElapsed }, host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("push:", StringComparison.Ordinal));
        host.Results[1] = 0;
        host.Results[2] = 1_500_000;
        host.KeepGpuStatActive = true;
        var debug = new VmGlDebug1305(host);
        debug.Update();
        Assert.Equal("a=0.00 ms\nb=1.50 ms\n", host.GpuStatOutput);
        Assert.Empty(VmGlDebug1305.TimeQueries);
        Assert.True(host.GpuStatActive);
        Assert.False(host.KeepGpuStatActive);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("control:", StringComparison.Ordinal));
        Assert.Equal(new[] { "result:1:" + VmGlDebug1305.GlQueryResult, "delete:1", "result:2:" + VmGlDebug1305.GlQueryResult, "delete:2" }, host.Steps.TakeLast(4));

        host.Flags = VmGlDebug1305.DebugFlag;
        host.DebugLevel = -1;
        host.Breakpoint = true;
        host.MaxDebugMessageLength = 64;
        host.Pending.Add(new VmGlDebug1305.DebugMsg(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 7, VmGlDebug1305.GlSeverityHigh, 6, "secret"));
        int prints = host.Prints.Count;
        debug.Update();
        Assert.Equal(prints, host.Prints.Count);
        Assert.Empty(host.Pending);
        Assert.Equal(uint.MaxValue, debug.CurrentLevel);
        Assert.True(debug.BreakpointMode);
        Assert.True(host.Callback);
        Assert.Contains("callback:this", host.Steps);
        Assert.Contains("enable:" + VmGlDebug1305.GlDebugOutputSynchronous, host.Steps);
        Assert.Contains(Control(VmGlDebug1305.GlSeverityHigh, true), host.Steps);
        Assert.Contains(Control(VmGlDebug1305.GlSeverityMedium, true), host.Steps);
        Assert.Contains(Control(VmGlDebug1305.GlSeverityLow, true), host.Steps);
        Assert.Contains(Control(VmGlDebug1305.GlSeverityNotification, true), host.Steps);
        Assert.Equal("", host.GpuStatOutput);

        int steps = host.Steps.Count;
        debug.Update();
        Assert.Equal(steps + 2, host.Steps.Count);
        Assert.Equal("get-int:" + VmGlDebug1305.GlMaxDebugMessageLength, host.Steps[^2]);
        Assert.StartsWith("get-log:", host.Steps[^1]);

        host.DebugLevel = 0;
        host.Breakpoint = false;
        host.Pending.Add(new VmGlDebug1305.DebugMsg(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 1, VmGlDebug1305.GlSeverityHigh, 4, "stay"));
        debug.Update();
        Assert.False(host.Callback);
        Assert.False(debug.BreakpointMode);
        Assert.Equal(0u, debug.CurrentLevel);
        Assert.Contains("callback:null", host.Steps);
        Assert.Contains("disable:" + VmGlDebug1305.GlDebugOutputSynchronous, host.Steps);
        Assert.Contains(Control(VmGlDebug1305.GlSeverityHigh, false), host.Steps);
        Assert.Equal("stay", Assert.Single(host.Pending).Text);

        host.Steps.Clear();
        VmGlDebug1305.LabelObject(host, VmGlDebug1305.GlTypeError, 9, "tex");
        host.Flags = 0;
        host.DebugLevel = 2;
        VmGlDebug1305.LabelObject(host, VmGlDebug1305.GlTypeError, 9, "tex");
        VmGlDebug1305.LabelObjectPtr(host, 3, "ptr");
        host.Flags = VmGlDebug1305.DebugFlag;
        host.DebugLevel = 0;
        VmGlDebug1305.LabelObject(host, VmGlDebug1305.GlTypeError, 9, "tex");
        host.DebugLevel = 2;
        VmGlDebug1305.LabelObject(host, VmGlDebug1305.GlTypeError, 9, "tex");
        VmGlDebug1305.LabelObjectPtr(host, 3, "ptr");
        Assert.Equal(new[] { "label:" + VmGlDebug1305.GlTypeError + ":9:-1:tex", "ptr:3:-1:ptr" }, host.Steps);

        host.GpuStatActive = false;
        VmGlDebug1305.PushGroup(host, "pass");
        VmGlDebug1305.PopGroup(host);
        Assert.Contains("push:" + VmGlDebug1305.GlSourceApplication + ":0:4:pass", host.Steps);
        Assert.Contains("pop", host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("gen:", StringComparison.Ordinal));
        Assert.Empty(VmGlDebug1305.TimeQueries);

        host.Pending.Clear();
        host.Pending.Add(new VmGlDebug1305.DebugMsg(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 1, VmGlDebug1305.GlSeverityHigh, 4, "wait"));
        host.DebugLevel = 4;
        var fresh = new VmGlDebug1305(host);
        int before = host.Steps.Count;
        fresh.OutputMessageLog();
        Assert.Equal(before, host.Steps.Count);
        fresh.UpdateLoggingLevel();
        host.MaxDebugMessageLength = 0;
        fresh.OutputMessageLog();
        Assert.Equal("wait", Assert.Single(host.Pending).Text);
        Assert.Contains("get-log:50:0", host.Steps);
        Assert.Equal(4u, fresh.CurrentLevel);
    }

    [Fact]
    public void Log_StopsAtFiftyAndTheCallbackAsserts()
    {
        VmGlDebug1305.ResetProcess();
        var host = new VmGlDebug1305.Host { Flags = VmGlDebug1305.DebugFlag };
        var debug = new VmGlDebug1305(host);
        debug.DebugCallback(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 1, VmGlDebug1305.GlSeverityHigh, 6, "hidden");
        Assert.Empty(host.Prints);

        host.DebugLevel = 1;
        debug.DebugCallback(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeOther, 2, VmGlDebug1305.GlSeverityNotification, 4, "note");
        Assert.Empty(host.Prints);
        host.DebugLevel = 4;
        debug.DebugCallback(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeOther, 2, VmGlDebug1305.GlSeverityNotification, 4, "note");
        Assert.Equal("[api] notification: note\n", Assert.Single(host.Prints));
        debug.DebugCallback(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeOther, 2, VmGlDebug1305.GlSeverityNotification, 4, "note");
        Assert.Single(host.Prints);
        var failed = Assert.Throws<InvalidOperationException>(() =>
            debug.DebugCallback(VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 3, VmGlDebug1305.GlSeverityHigh, 4, "boom"));
        Assert.Equal("assert severity == GL_DEBUG_SEVERITY_NOTIFICATION", failed.Message);
        Assert.Equal("[api] high severity, error: boom\n", host.Prints[1]);

        VmGlDebug1305.ResetProcess();
        host.Prints.Clear();
        host.Steps.Clear();
        host.DebugLevel = 4;
        host.MaxDebugMessageLength = 32;
        host.Pending.Add(Msg("ab\0cd", VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));
        host.Pending.Add(Msg("ab", VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));
        host.Pending.Add(Msg("push-group", VmGlDebug1305.GlSourceApplication, VmGlDebug1305.GlTypePushGroup, VmGlDebug1305.GlSeverityNotification));
        host.Pending.Add(Msg("pop-group", VmGlDebug1305.GlSourceApplication, VmGlDebug1305.GlTypePopGroup, VmGlDebug1305.GlSeverityNotification));
        host.Pending.Add(Msg("beta", VmGlDebug1305.GlSourceApplication, VmGlDebug1305.GlTypeOther, VmGlDebug1305.GlSeverityNotification));
        host.Pending.Add(Msg("gamma", 99, VmGlDebug1305.GlTypePerformance, 7));
        for (int i = 0; i < 46; i++)
            host.Pending.Add(Msg("n" + i, VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));
        host.Pending.Add(Msg("cap", VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));
        host.Pending.Add(Msg("overflow", VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));
        host.Pending.Add(Msg("past", VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, VmGlDebug1305.GlSeverityHigh));

        var other = new VmGlDebug1305(host);
        debug.Update();
        Assert.Equal(50, host.Prints.Count);
        Assert.Equal("[api] high severity, error: ab\n", host.Prints[0]);
        Assert.Equal("[application] notification: beta\n", host.Prints[1]);
        Assert.Equal("[99] 7, performance: gamma\n", host.Prints[2]);
        Assert.Equal("[api] high severity, error: n0\n", host.Prints[3]);
        Assert.Equal("[api] high severity, error: n45\n", host.Prints[48]);
        Assert.Equal("Max OpenGL debug messages reached. Suppressing further output.\n", host.Prints[49]);
        Assert.Equal(51, VmGlDebug1305.MessagesPrinted);
        Assert.Contains("cap", VmGlDebug1305.Seen);
        Assert.Contains("overflow", VmGlDebug1305.Seen);
        Assert.DoesNotContain("past", VmGlDebug1305.Seen);
        Assert.DoesNotContain("push-group", VmGlDebug1305.Seen);
        Assert.Empty(host.Pending);
        Assert.Contains("get-log:50:1600", host.Steps);
        Assert.Equal(3, host.Steps.Count(step => step.StartsWith("get-log:", StringComparison.Ordinal)));
        VmGlDebug1305.PrintMessage(host, VmGlDebug1305.GlSourceApi, VmGlDebug1305.GlTypeError, 1, VmGlDebug1305.GlSeverityHigh, 4, "late");
        Assert.Equal(51, VmGlDebug1305.MessagesPrinted);
        Assert.DoesNotContain("late", VmGlDebug1305.Seen);
        Assert.Equal(0u, other.CurrentLevel);
        Assert.Equal(4u, debug.CurrentLevel);
        Assert.Equal(VmGlDebug1305.GlDontCare, 0x1100);
        Assert.Equal("window system", VmGlDebug1305.SourceToString(VmGlDebug1305.GlSourceWindowSystem));
        Assert.Equal("shader compiler", VmGlDebug1305.SourceToString(VmGlDebug1305.GlSourceShaderCompiler));
        Assert.Equal("third party", VmGlDebug1305.SourceToString(VmGlDebug1305.GlSourceThirdParty));
        Assert.Equal("other", VmGlDebug1305.SourceToString(VmGlDebug1305.GlSourceOther));
        Assert.Equal("deprecated", VmGlDebug1305.TypeToString(VmGlDebug1305.GlTypeDeprecated));
        Assert.Equal("undefined", VmGlDebug1305.TypeToString(VmGlDebug1305.GlTypeUndefined));
        Assert.Equal("portability", VmGlDebug1305.TypeToString(VmGlDebug1305.GlTypePortability));
        Assert.Equal("marker", VmGlDebug1305.TypeToString(VmGlDebug1305.GlTypeMarker));
        Assert.Equal("low severity", VmGlDebug1305.SeverityToString(VmGlDebug1305.GlSeverityLow));
        Assert.Equal("medium severity", VmGlDebug1305.SeverityToString(VmGlDebug1305.GlSeverityMedium));
        Assert.Equal("8", VmGlDebug1305.TypeToString(8));
    }

    static VmGlDebug1305.DebugMsg Msg(string text, int source, int type, int severity)
    {
        return new VmGlDebug1305.DebugMsg(source, type, 1, severity, text.Length, text);
    }

    static string Control(int severity, bool enabled)
    {
        return "control:" + VmGlDebug1305.GlDontCare + ":" + VmGlDebug1305.GlDontCare + ":" + severity + ":0:" + (enabled ? "1" : "0");
    }
}
