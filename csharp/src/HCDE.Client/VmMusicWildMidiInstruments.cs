/*
** music_wildmidi_mididevice.cpp
** Provides access to WildMidi as a generic MIDI device.
**
**---------------------------------------------------------------------------
** Copyright 2015 Marisa Heit
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

public interface IVmMusicWildMidiInstruments
{
    int LoadConfiguration(string? name);
}

// Access is serialized by the host. Created instruments retain their reader dependencies.
public sealed class VmMusicWildMidiInstrumentCache<TReader, TInstruments>
    where TReader : class where TInstruments : class, IVmMusicWildMidiInstruments
{
    public TReader? PendingReader { get; private set; }
    public string ReaderName { get; private set; } = "";
    public string LoadedConfiguration { get; private set; } = "";
    public TInstruments? Instruments { get; private set; }

    public bool SetupConfiguration(string argument, string defaultConfiguration,
        Func<string, TReader?> openClient, Func<string, bool> fileExists,
        Func<string, byte[]?> readPrefix, Func<string, TReader?> openValidZip,
        Func<string, TReader?> openFileSystem)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ArgumentNullException.ThrowIfNull(defaultConfiguration);
        ArgumentNullException.ThrowIfNull(openClient);
        ArgumentNullException.ThrowIfNull(fileExists);
        ArgumentNullException.ThrowIfNull(readPrefix);
        ArgumentNullException.ThrowIfNull(openValidZip);
        ArgumentNullException.ThrowIfNull(openFileSystem);
        var name = argument.Length == 0 ? defaultConfiguration : argument;
        if (LoadedConfiguration.Equals(name, StringComparison.OrdinalIgnoreCase)) return false;
        var reader = openClient(name);
        if (reader is null && fileExists(name))
        {
            var prefix = readPrefix(name);
            if (prefix is { Length: >= 4 } && prefix.AsSpan(0, 4).SequenceEqual("PK\u0003\u0004"u8))
                reader = openValidZip(name);
            reader ??= openFileSystem(name);
        }
        if (reader is null)
            throw new InvalidOperationException($"WildMidi: {name}: Unable to load sound font\n");
        SetReader(name, reader);
        return true;
    }

    public void SetReader(string name, TReader reader)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(reader);
        ReaderName = name;
        PendingReader = reader;
    }

    public TInstruments Load(int sampleRate, Func<TReader, int, TInstruments> create)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentNullException.ThrowIfNull(create);
        if (PendingReader is { } reader)
        {
            LoadedConfiguration = ReaderName;
            Instruments = create(reader, sampleRate)
                ?? throw new InvalidOperationException("WildMIDI instrument factory returned no instruments.");
            PendingReader = null;
        }
        var instruments = Instruments
            ?? throw new InvalidOperationException("No instruments set for WildMidi device");
        if (instruments.LoadConfiguration(null) < 0)
        {
            Instruments = null;
            LoadedConfiguration = "";
            throw new InvalidOperationException("Unable to initialize instruments for WildMidi device");
        }
        return instruments;
    }
}
