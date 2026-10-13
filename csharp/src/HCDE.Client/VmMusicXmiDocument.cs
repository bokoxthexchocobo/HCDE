/*
** music_xmi_midiout.cpp
** Code to let ZDoom play XMIDI music through the MIDI streaming API.
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

using System.Buffers.Binary;

namespace HCDE.Client;

public sealed record VmMusicXmiSong(ReadOnlyMemory<byte> Events, ReadOnlyMemory<byte> Timbres);

public sealed class VmMusicXmiDocument
{
    public ushort Division => 60;
    public uint InitialTempo => 500000;
    public IReadOnlyList<VmMusicXmiSong> Songs { get; }

    private VmMusicXmiDocument(List<VmMusicXmiSong> songs) => Songs = songs.AsReadOnly();

    public static VmMusicXmiDocument Parse(ReadOnlySpan<byte> data)
    {
        var snapshot = data.ToArray();
        var songs = new List<VmMusicXmiSong>();
        var pending = new Stack<(int Position, int End)>();
        pending.Push((0, snapshot.Length));
        while (pending.TryPop(out var range))
        {
            var position = range.Position;
            while (position < range.End)
            {
                var chunk = ReadChunk(snapshot, position, range.End);
                var id = snapshot.AsSpan(position, 4);
                position = chunk.Next;
                if (!id.SequenceEqual("FORM"u8) && !id.SequenceEqual("CAT "u8)) continue;
                if (chunk.Length < 4) throw new InvalidDataException("Missing XMI container type.");
                if (id.SequenceEqual("CAT "u8))
                {
                    // Resume the parent after its children to retain native song order.
                    pending.Push((position, range.End));
                    pending.Push((chunk.Start + 4, chunk.Start + chunk.Length));
                    break;
                }
                if (snapshot.AsSpan(chunk.Start, 4).SequenceEqual("XMID"u8))
                    songs.Add(ReadSong(snapshot, chunk.Start + 4, chunk.Start + chunk.Length));
            }
        }
        if (songs.Count == 0) throw new InvalidDataException("XMI contains no XMID songs.");
        return new(songs);
    }

    private static VmMusicXmiSong ReadSong(byte[] data, int position, int end)
    {
        ReadOnlyMemory<byte> timbres = ReadOnlyMemory<byte>.Empty;
        while (position < end)
        {
            var chunk = ReadChunk(data, position, end);
            var id = data.AsSpan(position, 4);
            if (id.SequenceEqual("TIMB"u8)) timbres = data.AsMemory(chunk.Start, chunk.Length);
            if (id.SequenceEqual("EVNT"u8)) return new(data.AsMemory(chunk.Start, chunk.Length), timbres);
            position = chunk.Next;
        }
        throw new InvalidDataException("XMID song contains no EVNT chunk.");
    }

    private static (int Start, int Length, int Next) ReadChunk(byte[] data, int position, int end)
    {
        if (end - position < 8) throw new InvalidDataException("Truncated XMI chunk header.");
        var length = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position + 4, 4));
        var start = position + 8;
        if ((ulong)length + (length & 1) > (ulong)(end - start))
            throw new InvalidDataException("Truncated XMI chunk payload or padding.");
        return (start, (int)length, start + (int)length + (int)(length & 1));
    }
}
