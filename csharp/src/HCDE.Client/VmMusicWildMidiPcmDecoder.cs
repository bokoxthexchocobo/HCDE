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

namespace HCDE.Client;

public readonly record struct VmMusicWildMidiDecodedPcm(short[] Data, uint DataLength,
    uint LoopStart, uint LoopEnd, byte Modes)
{
    public byte LoopFraction { get; init; }
}

public static class VmMusicWildMidiPcmDecoder
{
    public static VmMusicWildMidiSampleData ToMixerSample(VmMusicWildMidiDecodedPcm decoded)
    {
        ArgumentNullException.ThrowIfNull(decoded.Data);
        if (decoded.DataLength == 0 || decoded.DataLength > uint.MaxValue >> 10)
            throw new ArgumentException("Decoded sample length must fit the native fixed-point position.", nameof(decoded));
        if ((ulong)decoded.Data.Length < (ulong)decoded.DataLength + 2)
            throw new ArgumentException("Decoded samples must include two interpolation guard samples.", nameof(decoded));
        if (decoded.LoopStart > decoded.LoopEnd || decoded.LoopEnd > decoded.DataLength)
            throw new ArgumentException("Decoded loop bounds must lie within the sample.", nameof(decoded));
        var start = (decoded.LoopStart << 10) | (uint)((decoded.LoopFraction & 15) << 6);
        var end = (decoded.LoopEnd << 10) | (uint)((decoded.LoopFraction >> 4) << 6);
        if ((decoded.Modes & 4) != 0 && (end <= start || end >= decoded.DataLength << 10))
            throw new ArgumentException("Looping samples require a positive loop and an endpoint inside the sample.", nameof(decoded));
        return new(decoded.Data, new(decoded.DataLength << 10, start, end, unchecked(end - start)));
    }

    public static VmMusicWildMidiDecodedPcm DecodeForward(ReadOnlySpan<byte> bytes,
        uint loopStart, uint loopEnd, byte modes)
    {
        if ((modes & (8 | 16)) != 0)
            throw new ArgumentException("Forward PCM decoding does not support ping-pong or reverse modes.", nameof(modes));
        return Decode(bytes, loopStart, loopEnd, modes);
    }

    public static VmMusicWildMidiDecodedPcm Decode(ReadOnlySpan<byte> bytes,
        uint loopStart, uint loopEnd, byte modes, byte loopFraction = 0)
    {
        if ((modes & 8) != 0) return DecodePingPong(bytes, loopStart, loopEnd, modes, loopFraction);
        var sixteenBit = (modes & 1) != 0;
        var unsigned = (modes & 2) != 0;
        var reverse = (modes & 16) != 0;
        if (bytes.IsEmpty || (sixteenBit && (bytes.Length & 1) != 0))
            throw new ArgumentException("PCM data must contain complete, nonempty samples.", nameof(bytes));
        if (loopStart > loopEnd || loopEnd > (uint)bytes.Length)
            throw new ArgumentException("Loop bounds must lie within the encoded sample.");
        var count = sixteenBit ? bytes.Length / 2 : bytes.Length;
        var data = AllocateSamples(count);
        for (var i = 0; i < count; i++)
        {
            var value = sixteenBit ? bytes[i * 2] | (bytes[i * 2 + 1] << 8) : bytes[i] << 8;
            if (unsigned) value ^= 0x8000;
            data[reverse ? count - 1 - i : i] = unchecked((short)value);
        }
        if (reverse)
        {
            var previousEnd = loopEnd;
            loopEnd = (uint)bytes.Length - loopStart;
            loopStart = (uint)bytes.Length - previousEnd;
            loopFraction = (byte)(((loopFraction & 15) << 4) | (loopFraction >> 4));
        }
        return new(data, (uint)count, sixteenBit ? loopStart >> 1 : loopStart,
            sixteenBit ? loopEnd >> 1 : loopEnd, (byte)(modes & ~(2 | 16))) { LoopFraction = loopFraction };
    }

    private static VmMusicWildMidiDecodedPcm DecodePingPong(ReadOnlySpan<byte> bytes,
        uint loopStart, uint loopEnd, byte modes, byte loopFraction)
    {
        if ((modes & 16) != 0)
            return DecodeReversePingPong(bytes, loopStart, loopEnd, modes, loopFraction);
        var sixteenBit = (modes & 1) != 0;
        if (sixteenBit && ((loopStart | loopEnd) & 1) != 0)
            throw new ArgumentException("Sixteen-bit ping-pong loops must have sample-aligned byte offsets.");
        var decoded = Decode(bytes, loopStart, loopEnd, (byte)(modes & ~8), loopFraction);
        var start = checked((int)decoded.LoopStart);
        var end = checked((int)decoded.LoopEnd);
        var length = end - start;
        // Native do/while conversion requires a prefix, an interior loop and its endpoint sample.
        if (start == 0 || length < 2 || end >= decoded.DataLength)
            throw new ArgumentException("Ping-pong conversion requires a nonempty prefix, at least two loop samples and an endpoint sample.");
        var count = checked((int)decoded.DataLength + length * 2);
        var expanded = AllocateSamples(count);
        for (var i = 0; i < count; i++)
        {
            var source = i <= end ? i : i <= end + length ? end * 2 - i : i - length * 2;
            expanded[i] = decoded.Data[source];
        }
        // convert_16up normalizes PCM but retains SAMPLE_UNSIGNED in its metadata.
        var outputModes = sixteenBit ? (byte)(decoded.Modes | (modes & 2)) : decoded.Modes;
        return new(expanded, (uint)count, (uint)(start + length), (uint)(end + length * 2), outputModes)
            { LoopFraction = loopFraction };
    }

    private static VmMusicWildMidiDecodedPcm DecodeReversePingPong(ReadOnlySpan<byte> bytes,
        uint loopStart, uint loopEnd, byte modes, byte loopFraction)
    {
        var sixteenBit = (modes & 1) != 0;
        if (sixteenBit && ((loopStart | loopEnd) & 1) != 0)
            throw new ArgumentException("Sixteen-bit reverse ping-pong loops must have sample-aligned byte offsets.");
        var decoded = Decode(bytes, loopStart, loopEnd, (byte)(modes & ~(8 | 16)), loopFraction);
        var start = checked((int)decoded.LoopStart);
        var end = checked((int)decoded.LoopEnd);
        var length = end - start;
        if (start == 0 || length < 2 || end >= decoded.DataLength - 1)
            throw new ArgumentException("Reverse ping-pong conversion requires a prefix, two loop samples and two endpoint samples.");
        var count = checked((int)decoded.DataLength + length * 2);
        var expanded = AllocateSamples(count);
        // Sample indices repair inverted native 16-bit pointer comparisons and byte-unit lengths.
        var read = checked((int)decoded.DataLength - 1);
        var write = 0;
        while (read > end) expanded[write++] = decoded.Data[read--];
        expanded[write] = decoded.Data[read--];
        var mirror = write + length * 2;
        expanded[mirror--] = expanded[write++];
        var tail = write + length * 2;
        while (read > start)
        {
            var value = decoded.Data[read--];
            expanded[write++] = value;
            expanded[mirror--] = value;
            expanded[tail++] = value;
        }
        expanded[write] = decoded.Data[read--];
        expanded[tail++] = expanded[write];
        while (read >= 0)
        {
            if (tail >= expanded.Length)
                throw new ArgumentException("The native reverse ping-pong tail exceeds its allocated sample buffer.");
            expanded[tail++] = decoded.Data[read--];
            // convert_8srp advances twice in its tail; convert_8urp advances once.
            if (!sixteenBit && (modes & 2) == 0) tail++;
        }
        return new(expanded, (uint)count, (uint)(start + length), (uint)(end + length * 2),
            (byte)(modes & ~(8 | 16 | 2))) { LoopFraction = loopFraction };
    }

    private static short[] AllocateSamples(int count)
    {
        if ((uint)count > uint.MaxValue >> 10)
            throw new ArgumentException("Decoded sample length exceeds the native fixed-point position range.", nameof(count));
        return new short[checked(count + 2)];
    }
}
