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

public interface IVmMusicMidiBufferDevice
{
    int Unprepare(int bufferNumber);
    int Prepare(int bufferNumber, ReadOnlyMemory<uint> events);
    int StreamOut(int bufferNumber);
    int StreamOutSync(int bufferNumber);
}

public enum VmMusicMidiPrimeStatus { Ready, ShortSong, DeviceError }
public sealed record VmMusicMidiPrimeResult(VmMusicMidiPrimeStatus Status, int Error = 0);

public enum VmMusicMidiEndState { Playing, StopPending, StopQueued }

public sealed class VmMusicMidiBufferService
{
    private readonly VmMusicMidiPlaybackBuffer playback;
    private readonly IVmMusicMidiBufferDevice device;
    private readonly uint[][] buffers;
    private bool started;
    private bool validateLoopDuration;
    private bool loopAdvancedTime;
    public int BufferNumber { get; private set; }
    public VmMusicMidiEndState EndState { get; private set; }

    public VmMusicMidiBufferService(VmMusicMidiPlaybackBuffer playback, IVmMusicMidiBufferDevice device,
        int bufferWords = VmMusicSmfSource.BufferWords)
    {
        ArgumentNullException.ThrowIfNull(playback);
        ArgumentNullException.ThrowIfNull(device);
        if (bufferWords < 169) throw new ArgumentOutOfRangeException(nameof(bufferWords));
        this.playback = playback;
        this.device = device;
        buffers = [new uint[bufferWords], new uint[bufferWords]];
    }

    public int Service(uint maximumTime, bool paused, bool looping, bool canHandleSysex)
    {
        started = true;
        if (EndState == VmMusicMidiEndState.StopQueued) return 0;
        var error = device.Unprepare(BufferNumber);
        if (error != 0) return error;
        int written;
        var stopping = EndState == VmMusicMidiEndState.StopPending;
        if (stopping) written = VmMusicMidiControlBuffers.WriteStop(buffers[BufferNumber]);
        else
        {
            if (!playback.RestartPending && playback.SourceDone)
            {
                if (!looping)
                {
                    EndState = VmMusicMidiEndState.StopPending;
                    return 0;
                }
                playback.RequestRestart();
            }
            var restarted = playback.RestartPending;
            written = playback.Fill(buffers[BufferNumber], maximumTime, paused, canHandleSysex);
            if (validateLoopDuration && looping && !paused)
                ObserveLoopDuration(buffers[BufferNumber].AsSpan(0, written), restarted);
        }
        error = device.Prepare(BufferNumber, buffers[BufferNumber].AsMemory(0, written));
        if (error != 0) return error;
        if (stopping) EndState = VmMusicMidiEndState.StopQueued;
        error = device.StreamOut(BufferNumber);
        if (error == 0) BufferNumber ^= 1;
        return error;
    }

    public VmMusicMidiPrimeResult Prime(uint maximumTime, bool canHandleSysex, bool allowShortSong = false,
        bool looping = false)
    {
        if (started) throw new InvalidOperationException("MIDI service has already started.");
        started = true;
        validateLoopDuration = allowShortSong && looping;
        playback.RequestRestart();
        for (var i = 0; i < 2; i++)
        {
            var stopping = !playback.RestartPending && playback.SourceDone;
            if (stopping && !allowShortSong)
                return new(VmMusicMidiPrimeStatus.ShortSong);
            if (stopping && looping)
            {
                playback.RequestRestart();
                stopping = false;
            }
            var restarted = playback.RestartPending;
            var written = stopping ? VmMusicMidiControlBuffers.WriteStop(buffers[BufferNumber])
                : playback.Fill(buffers[BufferNumber], maximumTime, false, canHandleSysex);
            if (validateLoopDuration)
                ObserveLoopDuration(buffers[BufferNumber].AsSpan(0, written), restarted);
            var error = device.Prepare(BufferNumber, buffers[BufferNumber].AsMemory(0, written));
            if (error != 0) return new(VmMusicMidiPrimeStatus.DeviceError, error);
            error = device.StreamOutSync(BufferNumber);
            if (error != 0) return new(VmMusicMidiPrimeStatus.DeviceError, error);
            if (stopping) EndState = VmMusicMidiEndState.StopQueued;
            BufferNumber ^= 1;
        }
        return new(VmMusicMidiPrimeStatus.Ready);
    }

    private void ObserveLoopDuration(ReadOnlySpan<uint> events, bool restarted)
    {
        if (restarted) loopAdvancedTime = false;
        for (var offset = 0; offset < events.Length;)
        {
            if (events[offset] != 0) { loopAdvancedTime = true; break; }
            var word = events[offset + 2];
            offset += 3 + (word >= 0x80000000 ? (int)(((word & 0xffffff) + 3) / 4) : 0);
        }
        if (playback.SourceDone && !loopAdvancedTime)
            throw new InvalidDataException("A repeating MIDI song must advance playback time.");
    }
}
