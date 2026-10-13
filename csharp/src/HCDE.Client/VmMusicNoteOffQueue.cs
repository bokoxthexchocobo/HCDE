/*
** music_hmi_midiout.cpp
** Code to let ZDoom play HMI MIDI music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 2010 Marisa Heit
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

internal sealed class VmMusicNoteOffQueue
{
    private readonly List<(uint Word, ulong Time)> heap = [];

    internal void Enqueue(uint word, ulong time)
    {
        var index = heap.Count;
        heap.Add(default);
        while (index > 0 && heap[(index - 1) / 2].Time > time)
        {
            heap[index] = heap[(index - 1) / 2];
            index = (index - 1) / 2;
        }
        heap[index] = (word, time);
    }

    internal bool TryPeek(out uint word, out ulong time)
    {
        if (heap.Count == 0) { word = 0; time = 0; return false; }
        (word, time) = heap[0];
        return true;
    }

    internal bool TryDequeue(out uint word, out ulong time)
    {
        if (!TryPeek(out word, out time)) return false;
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        var index = 0;
        while (index < heap.Count)
        {
            var left = (long)index * 2 + 1;
            var right = left + 1;
            var smallest = index;
            if (left < heap.Count && heap[(int)left].Time < heap[index].Time) smallest = (int)left;
            if (right < heap.Count && heap[(int)right].Time < heap[smallest].Time) smallest = (int)right;
            if (smallest == index) break;
            (heap[index], heap[smallest]) = (heap[smallest], heap[index]);
            index = smallest;
        }
        return true;
    }

    internal uint Dequeue()
    {
        if (!TryDequeue(out var word, out _)) throw new InvalidOperationException("Note-off queue is empty.");
        return word;
    }
}
