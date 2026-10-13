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

using System.Buffers.Binary;

namespace HCDE.Client;

public sealed record VmMusicHmiTrack(ReadOnlyMemory<byte> Events, IReadOnlyList<ushort> Designations);

public sealed class VmMusicHmiDocument
{
    public bool IsHmp { get; }
    public ushort Division { get; }
    public uint InitialTempo => IsHmp ? 1000000u : 4000000u;
    public IReadOnlyList<VmMusicHmiTrack> Tracks { get; }

    private VmMusicHmiDocument(bool isHmp, ushort division, List<VmMusicHmiTrack> tracks)
    {
        IsHmp = isHmp;
        Division = division;
        Tracks = tracks.AsReadOnly();
    }

    public static VmMusicHmiDocument Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 0x100) throw new InvalidDataException("Truncated HMI/HMP header.");
        var hmp = data.StartsWith("HMIMIDIP"u8);
        if (!hmp && !data.StartsWith("HMI-MIDISONG061595\0"u8))
            throw new InvalidDataException("Invalid HMI/HMP signature.");
        var division = hmp ? UInt32(data, 0x38) : (uint)UInt16(data, 0xd4) * 4;
        if (division is 0 or > ushort.MaxValue) throw new InvalidDataException("Invalid HMI/HMP division.");
        var tracks = new List<VmMusicHmiTrack>();
        if (hmp)
        {
            var position = data[8] == 0 ? 0x308 : data.Slice(8, 7).SequenceEqual("013195\0"u8) ? 0x388
                : throw new InvalidDataException("Unknown HMP version.");
            var count = UInt32(data, 0x30);
            if (count == 0 || position > data.Length || count > (data.Length - position) / 12)
                throw new InvalidDataException("Invalid HMP track count.");
            for (var i = 0u; i < count; i++)
            {
                Require(data, position, 12);
                var length = UInt32(data, position + 4);
                if (length < 12 || length > data.Length - position)
                    throw new InvalidDataException("Invalid HMP track length.");
                if (length > 12)
                    tracks.Add(new(data.Slice(position + 12, (int)length - 12).ToArray(),
                        Array.AsReadOnly<ushort>([0xa000, 0xa00a, 0xa002, 0, 0, 0, 0, 0])));
                position += (int)length;
            }
        }
        else
        {
            var count = UInt16(data, 0xe4);
            var directory = UInt32(data, 0xe8);
            if (count == 0 || directory > data.Length || (uint)count * 4 > data.Length - directory)
                throw new InvalidDataException("Invalid HMI track directory.");
            for (var i = 0; i < count; i++)
            {
                var start = UInt32(data, (int)directory + i * 4);
                var end = i + 1 == count ? (uint)data.Length : UInt32(data, (int)directory + (i + 1) * 4);
                if (start > data.Length || end > data.Length || end <= start || end - start < 0xa9)
                    throw new InvalidDataException("Invalid HMI track boundaries.");
                var track = data.Slice((int)start, (int)(end - start));
                if (!track.StartsWith("HMI-MIDITRACK"u8)) continue;
                var eventStart = UInt32(track, 0x57);
                if (eventStart < 0xa9 || eventStart > track.Length)
                    throw new InvalidDataException("Invalid HMI event offset.");
                if (eventStart == track.Length) continue;
                var designations = new ushort[8];
                for (var j = 0; j < designations.Length; j++) designations[j] = UInt16(track, 0x99 + j * 2);
                tracks.Add(new(track[(int)eventStart..].ToArray(), Array.AsReadOnly(designations)));
            }
        }
        if (tracks.Count == 0) throw new InvalidDataException("HMI/HMP contains no event tracks.");
        return new(hmp, (ushort)division, tracks);
    }

    private static void Require(ReadOnlySpan<byte> data, int offset, int length)
    {
        if (offset < 0 || offset > data.Length || length > data.Length - offset)
            throw new InvalidDataException("Truncated HMI/HMP field.");
    }
    private static ushort UInt16(ReadOnlySpan<byte> data, int offset)
    {
        Require(data, offset, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
    }
    private static uint UInt32(ReadOnlySpan<byte> data, int offset)
    {
        Require(data, offset, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4));
    }
}
