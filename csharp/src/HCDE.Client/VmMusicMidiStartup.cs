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

public interface IVmMusicMidiStartupDevice : IVmMusicMidiBufferDevice, IVmMusicMidiPauseDevice
{
    void PrecacheInstruments(IReadOnlyList<ushort> instruments);
    int SetTimeDivision(ushort division);
    int SetTempo(uint tempo);
    void InitializePlayback();
    int StartStream();
}

// The caller opens the device, selects source capabilities, and closes it after failure or completion.
public sealed class VmMusicMidiStartup
{
    private readonly IVmMusicSmfSource source;
    private readonly IVmMusicMidiStartupDevice device;
    private readonly VmMusicMidiPlaybackBuffer playback;
    private readonly Action? streamStarting;
    private bool started;
    public VmMusicMidiLifecycle Lifecycle { get; }
    public VmMusicMidiBufferService Buffers { get; }

    public VmMusicMidiStartup(IVmMusicSmfSource source, IVmMusicMidiStartupDevice device) : this(source, device, null) { }

    internal VmMusicMidiStartup(IVmMusicSmfSource source, IVmMusicMidiStartupDevice device, Action? streamStarting)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(device);
        this.source = source;
        this.device = device;
        this.streamStarting = streamStarting;
        playback = new(source);
        Lifecycle = new(playback, device);
        Buffers = new(playback, device);
    }

    public bool Start(uint maximumTime, bool canHandleSysex,
        float musicVolume = 1, float relativeVolume = 1, float masterVolume = 1, bool liveLoops = false)
    {
        if (started) throw new InvalidOperationException("MIDI startup has already been attempted.");
        started = true;
        var ready = false;
        try
        {
            device.PrecacheInstruments(VmMusicMidiPrecache.Collect(source));
            source.StartPlayback(liveLoops, liveLoops ? 0 : source.LoopLimit);
            Check(device.SetTimeDivision(source.Division), "Setting MIDI time division failed");
            Check(device.SetTempo(source.InitialTempo), "Setting MIDI tempo failed");
            Lifecycle.MusicVolumeChanged(musicVolume, relativeVolume, masterVolume);
            playback.VolumeUpdates.OutputVolume(playback.Channels.MasterVolume, device.FakeVolume);
            device.InitializePlayback();
            var result = Buffers.Prime(maximumTime, canHandleSysex);
            if (result.Status == VmMusicMidiPrimeStatus.ShortSong) return false;
            Check(result.Error, "Initial MIDI stream output failed");
            streamStarting?.Invoke();
            Check(device.StartStream(), "Starting MIDI playback failed");
            Lifecycle.MarkPlaying();
            ready = true;
            return true;
        }
        finally
        {
            if (!ready) Lifecycle.MarkStopped();
        }
    }

    private static void Check(int error, string message)
    {
        if (error != 0) throw new InvalidOperationException($"{message}: {error}.");
    }
}
