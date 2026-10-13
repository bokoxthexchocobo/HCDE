/*
** music_midi_midiout.cpp
** Code to let ZDoom play SMF MIDI music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 1998-2008 Marisa Heit
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
** This file also supports the Apogee Sound System's EMIDI files. That
** basically means you can play the Duke3D songs without any editing and
** have them sound right.
*/

using System.Buffers.Binary;

namespace HCDE.Client;

public sealed class VmMusicSmfDocument
{
    public ushort Format { get; }
    public ushort Division { get; }
    public IReadOnlyList<ReadOnlyMemory<byte>> Tracks { get; }

    private VmMusicSmfDocument(ushort format, ushort division, ReadOnlyMemory<byte>[] tracks)
    {
        Format = format;
        Division = division;
        Tracks = Array.AsReadOnly(tracks);
    }

    public static VmMusicSmfDocument Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 14 || !data.StartsWith("MThd"u8)
            || BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4)) != 6)
            throw new InvalidDataException("Invalid Standard MIDI File header.");
        var format = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));
        if (format > 2) throw new InvalidDataException("Invalid Standard MIDI File format.");
        var declaredTracks = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));
        // Native format zero always loads one track, regardless of the declared count.
        var count = format == 0 ? 1 : declaredTracks;
        if (count == 0) throw new InvalidDataException("Standard MIDI File contains no tracks.");
        var division = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(12, 2));
        if (division == 0) throw new InvalidDataException("Standard MIDI File division must be nonzero.");
        var tracks = new ReadOnlyMemory<byte>[count];
        var position = 14;
        for (var i = 0; i < count; i++)
        {
            if (data.Length - position < 8) throw new InvalidDataException("Truncated MIDI track header.");
            if (!data.Slice(position, 4).SequenceEqual("MTrk"u8))
                throw new InvalidDataException("Expected MIDI track chunk.");
            var length = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(position + 4, 4));
            position += 8;
            if (length > data.Length - position) throw new InvalidDataException("Truncated MIDI track data.");
            tracks[i] = data.Slice(position, (int)length).ToArray();
            position += (int)length;
        }
        return new(format, division, tracks);
    }
}
