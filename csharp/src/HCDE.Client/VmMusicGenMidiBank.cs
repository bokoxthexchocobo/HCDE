/*
** music_timidity_mididevice.cpp
** Provides access to TiMidity as a generic MIDI device.
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

/*
 * Wohlstand's OPL3 Bank File - a bank format to store OPL3 timbre data and setup
 *
 * Copyright (c) 2015-2025 Vitaly Novichkov <admin@wohlnet.ru>
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using System.Buffers.Binary;

namespace HCDE.Client;

public static class VmMusicGenMidiBank
{
    public const int InstrumentCount = 175;
    public const int InstrumentSize = 36;
    private const int WoplHeaderSize = 19 + 34 * 2;
    private const int WoplInstrumentSize = 66;

    public static byte[] FromLump(ReadOnlySpan<byte> lump)
    {
        if (!lump.StartsWith("#OPL_II#"u8)) throw new InvalidDataException("Invalid GENMIDI lump signature.");
        return ConvertToWopl(lump[8..]);
    }

    public static byte[] ConvertToWopl(ReadOnlySpan<byte> instruments)
    {
        if (instruments.Length < InstrumentCount * InstrumentSize)
            throw new InvalidDataException("Truncated GENMIDI instrument bank.");
        var bank = new byte[WoplHeaderSize + 256 * WoplInstrumentSize];
        "WOPL3-BANK\0"u8.CopyTo(bank);
        BinaryPrimitives.WriteUInt16LittleEndian(bank.AsSpan(11), 3);
        BinaryPrimitives.WriteUInt16BigEndian(bank.AsSpan(13), 1);
        BinaryPrimitives.WriteUInt16BigEndian(bank.AsSpan(15), 1);
        bank[18] = 2; // Native DMX volume model, with its API enum offset removed.
        for (var i = 0; i < 256; i++) bank[WoplHeaderSize + i * WoplInstrumentSize + 39] = 4;
        for (var i = 0; i < InstrumentCount; i++)
        {
            var drum = i >= 128;
            var slot = drum ? i + 35 : i;
            WriteInstrument(instruments.Slice(i * InstrumentSize, InstrumentSize),
                bank.AsSpan(WoplHeaderSize + slot * WoplInstrumentSize, WoplInstrumentSize), drum);
        }
        return bank;
    }

    private static void WriteInstrument(ReadOnlySpan<byte> source, Span<byte> target, bool drum)
    {
        var flags = BinaryPrimitives.ReadUInt16LittleEndian(source);
        var note = drum && (flags & 1) == 0 ? 60 : (int)source[3];
        var offset1 = drum ? 12 : BinaryPrimitives.ReadInt16LittleEndian(source[18..]) + 12;
        var offset2 = drum ? 12 : BinaryPrimitives.ReadInt16LittleEndian(source[34..]) + 12;
        while (note != 0 && note < 20) { note += 12; offset1 -= 12; offset2 -= 12; }
        BinaryPrimitives.WriteInt16BigEndian(target[32..], unchecked((short)offset1));
        BinaryPrimitives.WriteInt16BigEndian(target[34..], unchecked((short)offset2));
        target[37] = unchecked((byte)(source[2] - 128));
        target[38] = (byte)note;
        target[39] = (byte)((flags & 4) != 0 ? 3 : 0);
        target[40] = source[10];
        target[41] = source[26];
        WriteOperator(source[11..], target[42..]); // Carrier then modulator, for each voice.
        WriteOperator(source[4..], target[47..]);
        WriteOperator(source[27..], target[52..]);
        WriteOperator(source[20..], target[57..]);
        BinaryPrimitives.WriteUInt16BigEndian(target[62..], 5000);
        BinaryPrimitives.WriteUInt16BigEndian(target[64..], 5000);
    }

    private static void WriteOperator(ReadOnlySpan<byte> source, Span<byte> target)
    {
        target[0] = source[0];
        target[1] = (byte)(source[4] | source[5]);
        target[2] = source[1];
        target[3] = source[2];
        target[4] = source[3];
    }
}
