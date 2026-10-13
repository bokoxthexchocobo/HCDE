/*
** music_midistream.cpp
** Implements base class for MIDI and MUS streaming.
**
**---------------------------------------------------------------------------
** Copyright 2008 Marisa Heit
** All rights reserved.
**
** Redistribution and use in source and binary forms, with or without
** modification, are permitted provided that the following conditions
** are met:
**
** 1. Redistributions of source code must retain the above copyright
**    notice, this list of conditions and the following disclaimer.
** 2. Redistributions in binary form must reproduce the above copyright
**    notice, this list of conditions and the following disclaimer in the
**    documentation and/or other materials provided with the distribution.
** 3. The name of the author may not be used to endorse or promote products
**    derived from this software without specific prior written permission.
**
** THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
** IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
** OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
** IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
** NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
** THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**---------------------------------------------------------------------------
**
*/

namespace HCDE.Client;

public interface IVmMusicMidiSessionDevice : IVmMusicMidiStartupDevice
{
    bool IsOpen { get; }
    int Technology { get; }
    bool CanHandleSysex { get; }
    int Open();
    // Deliver completions in queue order, serialized with session access. StartStream may complete inline;
    // other queue operations must return before their completion callbacks run.
    // Managed adapters must handle callback exceptions and nonzero results at their backend boundary.
    void SetCompletionCallback(Func<int>? callback);
    bool Update();
    void Stop();
    void Close();
}

// Owns the device lifetime, including IDisposable devices, but borrows the source. Callers serialize access.
public sealed class VmMusicMidiSession : IDisposable
{
    private readonly IVmMusicSmfSource source;
    private readonly IVmMusicMidiSessionDevice device;
    private bool attempted;
    private bool closed;
    private bool? originalSkipSysex;
    private uint maximumTime;
    private bool callbacksEnabled;
    private bool startingStream;
    private int startupCompletions;
    private bool repeatSong;
    public int LastServiceError { get; private set; }
    public Exception? LastServiceException { get; private set; }
    public VmMusicMidiStartup Startup { get; }

    public VmMusicMidiSession(IVmMusicSmfSource source, IVmMusicMidiSessionDevice device)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(device);
        this.source = source;
        this.device = device;
        Startup = new(source, device, () => startingStream = true);
    }

    public bool Start(uint maximumTime, float musicVolume = 1, float relativeVolume = 1, float masterVolume = 1,
        int subsong = 0, bool repeatSong = false, bool liveLoops = false)
    {
        ObjectDisposedException.ThrowIf(closed, this);
        if (attempted) throw new InvalidOperationException("MIDI session startup has already been attempted.");
        if (device.IsOpen) throw new InvalidOperationException("MIDI session requires a closed device.");
        if (!source.SetSubsong(subsong))
            throw new ArgumentOutOfRangeException(nameof(subsong), subsong, "MIDI subsong does not exist.");
        attempted = true;
        this.maximumTime = maximumTime;
        this.repeatSong = repeatSong;
        bool ready;
        try
        {
            device.SetCompletionCallback(ServiceCompletion);
            var error = device.Open();
            if (error != 0) throw new InvalidOperationException($"Could not open MIDI out device: {error}.");
            source.CheckCapabilities(device.Technology);
            if (source is IVmMusicMidiSysexPolicy policy)
            {
                originalSkipSysex = policy.SkipSysex;
                policy.SkipSysex = !device.CanHandleSysex;
            }
            else if (!device.CanHandleSysex && source is not VmMusicMusSource && source is not VmMusicMidsSource)
                throw new NotSupportedException("MIDI source does not support SysEx suppression.");
            ready = Startup.Start(maximumTime, device.CanHandleSysex, musicVolume, relativeVolume, masterVolume, liveLoops,
                repeatSong: repeatSong);
            startingStream = false;
            callbacksEnabled = ready;
            while (ready && startupCompletions > 0)
            {
                startupCompletions--;
                var completionError = ServiceCompletion();
                if (completionError != 0)
                    throw new InvalidOperationException($"Initial MIDI completion failed: {completionError}.");
            }
        }
        catch (Exception startupError)
        {
            var errors = CloseCore();
            if (errors.Count != 0)
                throw new AggregateException("MIDI startup and cleanup failed.", new[] { startupError }.Concat(errors));
            throw;
        }
        if (!ready) Dispose();
        return ready;
    }

    public int ServiceCompletion()
    {
        if (!closed && startingStream)
        {
            startupCompletions = checked(startupCompletions + 1);
            return 0;
        }
        if (closed || !callbacksEnabled || LastServiceError != 0) return LastServiceError;
        try
        {
            LastServiceError = Startup.Lifecycle.Service(Startup.Buffers, maximumTime, repeatSong, device.CanHandleSysex);
        }
        catch (Exception serviceError)
        {
            LastServiceException = serviceError;
            LastServiceError = -1;
            Startup.Lifecycle.MarkStopped();
            throw;
        }
        return LastServiceError;
    }

    public void Update()
    {
        if (closed || !callbacksEnabled) return;
        bool alive;
        try
        {
            alive = device.Update();
        }
        catch (Exception updateError)
        {
            var errors = CloseCore();
            if (errors.Count != 0)
                throw new AggregateException("MIDI update and cleanup failed.", new[] { updateError }.Concat(errors));
            throw;
        }
        if (!alive) Dispose();
    }

    // Like native IsPlaying, the host poll closes once end-of-song or a device failure is observed.
    public bool IsPlaying()
    {
        if (closed || !callbacksEnabled) return false;
        if (LastServiceError != 0 || Startup.Buffers.EndState != VmMusicMidiEndState.Playing || !device.IsOpen)
        {
            var errors = CloseCore();
            if (errors.Count != 0)
            {
                if (LastServiceException is not null) errors.Insert(0, LastServiceException);
                throw new AggregateException("MIDI playback cleanup failed.", errors);
            }
            return false;
        }
        return Startup.Lifecycle.State != VmMusicMidiPlaybackState.Stopped;
    }

    public void Dispose()
    {
        var errors = CloseCore();
        if (errors.Count != 0) throw new AggregateException("MIDI device cleanup failed.", errors);
    }

    private List<Exception> CloseCore()
    {
        List<Exception> errors = [];
        if (closed) return errors;
        closed = true;
        callbacksEnabled = false;
        startingStream = false;
        startupCompletions = 0;
        Startup.Lifecycle.MarkStopped();
        // Cleanup attempts every operation and reports all failures, including the original startup error.
        void Attempt(Action action)
        {
            try { action(); }
            catch (Exception error) { errors.Add(error); }
        }
        if (!attempted)
        {
            if (device is IDisposable unusedDevice) Attempt(unusedDevice.Dispose);
            return errors;
        }
        Attempt(() => device.SetCompletionCallback(null));
        Attempt(() =>
        {
            if (!device.IsOpen) return;
            Attempt(device.Stop);
            for (var number = 0; number < 2; number++)
            {
                var slot = number;
                Attempt(() =>
                {
                    var error = device.Unprepare(slot);
                    if (error != 0) throw new InvalidOperationException($"Unpreparing MIDI buffer {slot} failed: {error}.");
                });
            }
        });
        Attempt(device.Close);
        if (device is IDisposable disposableDevice) Attempt(disposableDevice.Dispose);
        if (originalSkipSysex is bool original)
            Attempt(() => ((IVmMusicMidiSysexPolicy)source).SkipSysex = original);
        return errors;
    }
}
