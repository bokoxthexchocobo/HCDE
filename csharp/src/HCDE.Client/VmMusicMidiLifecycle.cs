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

public interface IVmMusicMidiPauseDevice
{
    bool FakeVolume { get; }
    bool Pause(bool paused);
}

public enum VmMusicMidiPlaybackState { Stopped, Playing, Paused }

public sealed class VmMusicMidiLifecycle
{
    private readonly VmMusicMidiPlaybackBuffer playback;
    private readonly IVmMusicMidiPauseDevice device;
    public VmMusicMidiPlaybackState State { get; private set; }

    public VmMusicMidiLifecycle(VmMusicMidiPlaybackBuffer playback, IVmMusicMidiPauseDevice device)
    {
        ArgumentNullException.ThrowIfNull(playback);
        ArgumentNullException.ThrowIfNull(device);
        this.playback = playback;
        this.device = device;
    }

    public void MarkPlaying() => State = VmMusicMidiPlaybackState.Playing;

    public void MarkStopped()
    {
        State = VmMusicMidiPlaybackState.Stopped;
        playback.VolumeUpdates.Clear();
    }

    public void Pause()
    {
        if (State != VmMusicMidiPlaybackState.Playing) return;
        State = VmMusicMidiPlaybackState.Paused;
        if (!device.Pause(true)) playback.VolumeUpdates.OutputVolume(0, device.FakeVolume);
    }

    public void Resume()
    {
        if (State != VmMusicMidiPlaybackState.Paused) return;
        if (!device.Pause(false))
            playback.VolumeUpdates.OutputVolume(playback.Channels.MasterVolume, device.FakeVolume);
        State = VmMusicMidiPlaybackState.Playing;
    }

    public void MusicVolumeChanged(float musicVolume, float relativeVolume, float masterVolume)
    {
        ushort volume = ushort.MaxValue;
        if (device.FakeVolume)
        {
            var realVolume = musicVolume * relativeVolume * masterVolume;
            if (!float.IsFinite(realVolume)) throw new ArgumentOutOfRangeException(nameof(musicVolume), "MIDI volume product must be finite.");
            if (realVolume < 0 || realVolume > 1) realVolume = 1;
            volume = (ushort)(realVolume * 65535f);
        }
        playback.Channels.MasterVolume = volume;
        if (State == VmMusicMidiPlaybackState.Playing)
            playback.VolumeUpdates.OutputVolume(volume, device.FakeVolume);
    }

    public int Service(VmMusicMidiBufferService service, uint maximumTime, bool looping, bool canHandleSysex)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (State == VmMusicMidiPlaybackState.Stopped) return 0;
        return service.Service(maximumTime, State == VmMusicMidiPlaybackState.Paused, looping, canHandleSysex);
    }
}
