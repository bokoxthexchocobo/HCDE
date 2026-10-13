/*
** midisource_mids.cpp
** Code to let ZMusic play MIDS MIDI music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 2020 Cacodemon345
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

using System.Buffers.Binary;

namespace HCDE.Client;

public sealed class VmMusicMidsSource : IVmMusicSmfSource
{
    private readonly uint[] events;
    private readonly int stride;
    private int position;
    private bool disposed;
    public ushort Division { get; }
    public uint InitialTempo { get; private set; } = 500000;
    public int LoopLimit { get; set; }

    public VmMusicMidsSource(ReadOnlySpan<byte> data)
    {
        if (data.Length <= 52 || data.Length % 4 != 0 || !data.StartsWith("RIFF"u8)
            || !data.Slice(8, 4).SequenceEqual("MIDS"u8)
            || !data.Slice(12, 4).SequenceEqual("fmt "u8) || Read(data, 16) != 12
            || !data.Slice(32, 4).SequenceEqual("data"u8))
            throw new InvalidDataException("Invalid MIDS header.");
        var division = Read(data, 20);
        if (division is 0 or > 65535) throw new InvalidDataException("Invalid MIDS division.");
        Division = (ushort)division;
        stride = Read(data, 28) == 0 ? 3 : 2;
        var size = Read(data, 36);
        if (size < 4 || size > data.Length - 40) throw new InvalidDataException("Invalid MIDS data chunk length.");
        var end = 40 + (int)size;
        var blocks = Read(data, 40);
        var offset = 44;
        var words = new List<uint>();
        for (uint block = 0; block < blocks; block++)
        {
            if (end - offset < 8) throw new InvalidDataException("Truncated MIDS block header.");
            // Native ignores tkStart and uses each event's delta time.
            var length = Read(data, offset + 4);
            offset += 8;
            if (length % (stride * 4) != 0 || length > end - offset)
                throw new InvalidDataException("Invalid MIDS block length.");
            var blockEnd = offset + (int)length;
            while (offset < blockEnd) { words.Add(Read(data, offset)); offset += 4; }
        }
        events = words.ToArray();
        for (var i = stride - 1; i < events.Length; i += stride)
            if ((events[i] >> 24) >= 128)
                throw new InvalidDataException("Long MIDS events are not supported by the fixed-record decoder.");
    }

    private static uint Read(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4));
    public void CheckCapabilities(int technology) => ObjectDisposedException.ThrowIf(disposed, this);
    public void Restart()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        position = 0;
        InitialTempo = events.Length != 0 && (events[stride - 1] >> 24) == 1
            ? events[stride - 1] & 0xffffff : 500000;
        if (InitialTempo == 0) throw new InvalidDataException("MIDS tempo must be positive.");
    }
    public void StartPlayback(bool looped, int loopLimit)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        LoopLimit = loopLimit;
    }
    public bool CheckDone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return position >= events.Length;
    }
    public int MakeEvents(Span<uint> buffer, uint maximumTime)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (buffer.Length < 3) throw new ArgumentException("MIDS buffer must hold an event.", nameof(buffer));
        var maximumTicks = (ulong)maximumTime * Division / InitialTempo;
        ulong total = 0;
        var written = 0;
        while (position < events.Length && written <= buffer.Length - 3 && total <= maximumTicks)
        {
            var delay = events[position++];
            buffer[written++] = delay;
            buffer[written++] = stride == 2 ? 0 : events[position++];
            buffer[written++] = events[position++];
            total += delay;
        }
        return written;
    }
    public void Dispose() => disposed = true;
}
