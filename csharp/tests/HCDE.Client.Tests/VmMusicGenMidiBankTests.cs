using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicGenMidiBankTests
{
    private const int Header = 87;
    private const int Record = 66;
    private static byte[] Instruments() => new byte[175 * 36];

    [Fact]
    public void SerializedBankHasNativeHeaderCountsAndBlankPercussionSlots()
    {
        var bank = VmMusicGenMidiBank.ConvertToWopl(Instruments());
        Assert.Equal(16983, bank.Length);
        Assert.Equal(Convert.FromHexString("574F504C332D42414E4B000300000100010002"), bank[..19]);
        Assert.Equal(VmMusicBankFormat.Wopl, VmMusicBankCatalog.Identify(bank));
        Assert.All(bank[19..Header], value => Assert.Equal(0, value));
        for (var slot = 0; slot < 256; slot++)
        {
            var instrument = bank.AsSpan(Header + slot * Record, Record).ToArray();
            Assert.Equal(slot >= 128 && (slot < 163 || slot >= 210) ? 4 : 0, instrument[39]);
        }
    }

    [Fact]
    public void VoiceRegistersAndSignedTuningMatchNativeWoplLayout()
    {
        var data = Instruments();
        data[0] = 4; data[2] = 5; data[3] = 0;
        new byte[] { 1, 2, 3, 4, 0x40, 5 }.CopyTo(data, 4);
        data[10] = 11;
        new byte[] { 6, 7, 8, 9, 0x80, 10 }.CopyTo(data, 11);
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(18), -14);
        new byte[] { 12, 13, 14, 15, 0xc0, 16 }.CopyTo(data, 20);
        data[26] = 22;
        new byte[] { 17, 18, 19, 20, 0x40, 21 }.CopyTo(data, 27);
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(34), 27);
        var bank = VmMusicGenMidiBank.ConvertToWopl(data);
        var instrument = bank.AsSpan(Header, Record);
        Assert.Equal(new byte[] { 0xff, 0xfe, 0, 39, 0, 133, 0, 3, 11, 22 }, instrument.Slice(32, 10).ToArray());
        Assert.Equal(new byte[] { 6, 0x8a, 7, 8, 9, 1, 0x45, 2, 3, 4,
            17, 0x55, 18, 19, 20, 12, 0xd0, 13, 14, 15 }, instrument.Slice(42, 20).ToArray());
        Assert.Equal(new byte[] { 0x13, 0x88, 0x13, 0x88 }, instrument[62..].ToArray());
    }

    [Fact]
    public void EveryInstrumentMapsToItsMelodicOrPercussionSlotAndOutputOwnsItsBytes()
    {
        var data = Instruments();
        for (var i = 0; i < 175; i++) data[i * 36 + 2] = unchecked((byte)(i + 128));
        var bank = VmMusicGenMidiBank.ConvertToWopl(data);
        Array.Clear(data);
        for (var i = 0; i < 175; i++)
        {
            var slot = i < 128 ? i : i + 35;
            Assert.Equal((byte)i, bank[Header + slot * Record + 37]);
        }
        bank[Header + 37] = 99;
        Assert.Equal(128, VmMusicGenMidiBank.ConvertToWopl(data)[Header + 37]);
    }

    [Theory]
    [InlineData(false, 5, 60, 12)]
    [InlineData(true, 0, 0, 12)]
    [InlineData(true, 5, 29, -12)]
    [InlineData(true, 19, 31, 0)]
    [InlineData(true, 20, 20, 12)]
    public void PercussionFixedPitchTransposesLowNotes(bool fixedPitch, byte note, byte expectedNote, short offset)
    {
        var data = Instruments();
        var input = data.AsSpan(128 * 36, 36);
        input[0] = (byte)(fixedPitch ? 1 : 0); input[3] = note;
        BinaryPrimitives.WriteInt16LittleEndian(input[18..], 200);
        BinaryPrimitives.WriteInt16LittleEndian(input[34..], -200);
        var bank = VmMusicGenMidiBank.ConvertToWopl(data);
        var instrument = bank.AsSpan(Header + 163 * Record, Record);
        Assert.Equal(expectedNote, instrument[38]);
        Assert.Equal(offset, BinaryPrimitives.ReadInt16BigEndian(instrument[32..]));
        Assert.Equal(offset, BinaryPrimitives.ReadInt16BigEndian(instrument[34..]));
    }

    [Fact]
    public void LumpConversionValidatesSignatureAndIgnoresTrailingNames()
    {
        var lump = new byte[8 + 175 * 36 + 175 * 32];
        "#OPL_II#"u8.CopyTo(lump);
        lump[8 + 174 * 36 + 2] = 128;
        var expected = VmMusicGenMidiBank.ConvertToWopl(lump.AsSpan(8, 175 * 36));
        Assert.Equal(expected, VmMusicGenMidiBank.FromLump(lump));
        lump[0] = 0;
        Assert.Throws<InvalidDataException>(() => VmMusicGenMidiBank.FromLump(lump));
        Assert.Throws<InvalidDataException>(() => VmMusicGenMidiBank.FromLump("#OPL_II#"u8));
        Assert.Throws<InvalidDataException>(() => VmMusicGenMidiBank.ConvertToWopl(new byte[175 * 36 - 1]));
        Assert.Throws<InvalidDataException>(() => VmMusicGenMidiBank.FromLump([]));
    }
}
