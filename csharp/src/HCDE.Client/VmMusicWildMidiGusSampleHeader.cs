/*
 * gus_pat.c -- Midi Wavetable Processing library
 *
 * Copyright (C) WildMIDI Developers 2001-2015
 *
 * This file is part of WildMIDI.
 *
 * WildMIDI is free software: you can redistribute and/or modify the player
 * under the terms of the GNU General Public License and you can redistribute
 * and/or modify the library under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation, either version 3 of
 * the licenses, or(at your option) any later version.
 *
 * WildMIDI is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License and
 * the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License and the
 * GNU Lesser General Public License along with WildMIDI.  If not,  see
 * <http://www.gnu.org/licenses/>.
 */

using System.Buffers.Binary;

namespace HCDE.Client;

public readonly record struct VmMusicWildMidiGusSampleHeader(uint ByteLength, uint LoopStart,
    uint LoopEnd, ushort Rate, uint FrequencyLow, uint FrequencyHigh, uint FrequencyRoot,
    uint IncrementDivisor, byte Modes, byte LoopFraction)
{
    public const int Size = 96;

    public static VmMusicWildMidiGusSampleHeader Parse(ReadOnlySpan<byte> header)
    {
        if (header.Length < Size) throw new ArgumentException("A GUS sample header requires 96 bytes.", nameof(header));
        var length = BinaryPrimitives.ReadUInt32LittleEndian(header[8..]);
        var start = BinaryPrimitives.ReadUInt32LittleEndian(header[12..]);
        var end = BinaryPrimitives.ReadUInt32LittleEndian(header[16..]);
        var rate = BinaryPrimitives.ReadUInt16LittleEndian(header[20..]);
        var root = BinaryPrimitives.ReadUInt32LittleEndian(header[30..]);
        if (rate == 0) throw new ArgumentException("A GUS sample rate must be nonzero.", nameof(header));
        var fraction = header[7];
        if (start > end)
        {
            (start, end) = (end, start);
            fraction = (byte)(((fraction & 15) << 4) | (fraction >> 4));
        }
        var divisor = unchecked(unchecked(root * 512U) / rate * 2U);
        if (divisor == 0)
            throw new ArgumentException("The GUS root frequency produces a zero pitch divisor.", nameof(header));
        return new(length, start, end, rate,
            BinaryPrimitives.ReadUInt32LittleEndian(header[22..]),
            BinaryPrimitives.ReadUInt32LittleEndian(header[26..]), root, divisor, header[55], fraction);
    }

    public VmMusicWildMidiDecodedPcm Decode(ReadOnlySpan<byte> payload)
    {
        if (ByteLength > (uint)payload.Length)
            throw new ArgumentException("The GUS sample payload is truncated.", nameof(payload));
        return VmMusicWildMidiPcmDecoder.Decode(payload[..(int)ByteLength], LoopStart, LoopEnd, Modes, LoopFraction);
    }
}
