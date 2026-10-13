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

public sealed class VmMusicMidiVolumeUpdates
{
    private readonly VmMusicMidiChannelVolume channels;
    private ushort pendingVolume;
    public bool HasPendingUpdate { get; private set; }
    public const int BufferWords = 16 * 3;

    public VmMusicMidiVolumeUpdates(VmMusicMidiChannelVolume channels)
    {
        ArgumentNullException.ThrowIfNull(channels);
        this.channels = channels;
    }

    public void OutputVolume(ushort volume, bool fakeVolume)
    {
        if (!fakeVolume) return;
        pendingVolume = volume;
        HasPendingUpdate = true;
    }

    public void Clear() => HasPendingUpdate = false;

    public int RequiredWords(bool paused) => HasPendingUpdate && (!paused || pendingVolume == 0) ? BufferWords : 0;

    public int WritePending(Span<uint> buffer, bool paused)
    {
        if (!HasPendingUpdate || (paused && pendingVolume != 0)) return 0;
        if (buffer.Length < BufferWords)
            throw new ArgumentException("MIDI volume update buffer must hold all sixteen channel messages.", nameof(buffer));
        var events = channels.CreateVolumeChanges(pendingVolume);
        var position = 0;
        foreach (var item in events)
        {
            buffer[position++] = 0;
            buffer[position++] = 0;
            buffer[position++] = item.Event;
        }
        HasPendingUpdate = false;
        return position;
    }
}
