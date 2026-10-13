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

public static class VmMusicWildMidiFactory
{
    public static Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> CreateCachedResourceWaveFactory(
        VmMusicWildMidiResourceCache cache, string configFile, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrEmpty(configFile);
        return request =>
        {
            if (request.Device is not (-1 or 6))
                throw new NotSupportedException("This WAVE factory supports WildMIDI only.");
            var selected = string.IsNullOrEmpty(request.Soundfont) ? configFile : request.Soundfont;
            return cache.CreateRenderer(selected, request.SampleRate, enhancedResampling, reverb);
        };
    }

    public static Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> CreateCachedFileWaveFactory(
        VmMusicWildMidiCatalogCache cache, string configFile, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrEmpty(configFile);
        return request =>
        {
            if (request.Device is not (-1 or 6))
                throw new NotSupportedException("This WAVE factory supports WildMIDI only.");
            var selected = string.IsNullOrEmpty(request.Soundfont) ? configFile : request.Soundfont;
            var catalog = cache.Load(selected, VmMusicWildMidiConfigLoader.LoadFile);
            return CreateConfigured(catalog, request.SampleRate, enhancedResampling, reverb);
        };
    }

    public static VmMusicWildMidiManagedRenderer CreateFromResource(string fileName, int requestedSampleRate,
        bool enhancedResampling, bool reverb, IReadOnlyList<ushort>? instruments = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        return IsArchive(fileName) ? CreateFromArchive(fileName, requestedSampleRate, enhancedResampling, reverb, instruments)
            : CreateFromFile(fileName, requestedSampleRate, enhancedResampling, reverb, instruments);
    }

    internal static bool IsArchive(string fileName)
    {
        using (var input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Span<byte> signature = stackalloc byte[4];
            var count = (int)Math.Min(input.Length, signature.Length);
            input.ReadExactly(signature[..count]);
            return count == 4 && signature.SequenceEqual("PK\x03\x04"u8);
        }
    }

    public static VmMusicWildMidiManagedRenderer CreateFromArchive(string archiveFile, int requestedSampleRate,
        bool enhancedResampling, bool reverb, IReadOnlyList<ushort>? instruments = null)
    {
        var reader = new VmGusZipMusicReader(archiveFile, allowDiskFallback: false);
        try
        {
            var catalog = VmMusicWildMidiConfigLoader.LoadBank(reader, OperatingSystem.IsWindows());
            var renderer = CreateConfigured(catalog, requestedSampleRate, enhancedResampling, reverb, instruments);
            renderer.OwnBankReader(reader);
            return renderer;
        }
        catch (Exception initializationError)
        {
            try { reader.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI archive initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }

    public static Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> CreateArchiveWaveFactory(
        string archiveFile, bool enhancedResampling, bool reverb)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveFile);
        return request =>
        {
            if (request.Device is not (-1 or 6))
                throw new NotSupportedException("This WAVE factory supports WildMIDI only.");
            var selectedArchive = string.IsNullOrEmpty(request.Soundfont) ? archiveFile : request.Soundfont;
            return CreateFromArchive(selectedArchive, request.SampleRate, enhancedResampling, reverb);
        };
    }

    public static Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> CreateWaveFactory(
        string configFile, bool enhancedResampling, bool reverb)
    {
        ArgumentException.ThrowIfNullOrEmpty(configFile);
        return request =>
        {
            if (request.Device is not (-1 or 6))
                throw new NotSupportedException("This WAVE factory supports WildMIDI only.");
            var selectedConfig = string.IsNullOrEmpty(request.Soundfont) ? configFile : request.Soundfont;
            return CreateFromResource(selectedConfig, request.SampleRate, enhancedResampling, reverb);
        };
    }

    public static VmMusicWildMidiManagedRenderer CreateForSource(VmMusicWildMidiInstrumentCatalog catalog,
        IVmMusicSmfSource source, int requestedSampleRate, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(source);
        var instruments = VmMusicMidiPrecache.Collect(source);
        return CreateConfigured(catalog, requestedSampleRate, enhancedResampling, reverb, instruments);
    }

    public static VmMusicWildMidiManagedRenderer CreateFileForSource(string configFile, IVmMusicSmfSource source,
        int requestedSampleRate, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(source);
        return CreateForSource(VmMusicWildMidiConfigLoader.LoadFile(configFile), source, requestedSampleRate, enhancedResampling, reverb);
    }

    public static VmMusicWildMidiManagedRenderer CreateResourceForSource(string fileName, IVmMusicSmfSource source,
        int requestedSampleRate, bool enhancedResampling, bool reverb)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(source);
        var instruments = VmMusicMidiPrecache.Collect(source);
        return CreateFromResource(fileName, requestedSampleRate, enhancedResampling, reverb, instruments);
    }

    public static VmMusicWildMidiManagedRenderer CreateConfigured(VmMusicWildMidiInstrumentCatalog catalog,
        int requestedSampleRate, bool enhancedResampling, bool reverb, IReadOnlyList<ushort>? instruments = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var sampleRate = requestedSampleRate is >= 11025 and <= 65535 ? requestedSampleRate : 44100;
        var renderer = new VmMusicWildMidiManagedRenderer(sampleRate, catalog.FindExact, catalog.LoadSamples, catalog.Reverb);
        try
        {
            VmMusicWildMidiSettings.Initialize(renderer, enhancedResampling, reverb);
            if (instruments is not null)
                VmMusicWildMidiSettings.PrecacheInstruments(renderer, instruments.ToArray());
            return renderer;
        }
        catch (Exception initializationError)
        {
            try { renderer.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }

    public static VmMusicWildMidiManagedRenderer CreateFromFile(string configFile, int requestedSampleRate,
        bool enhancedResampling, bool reverb, IReadOnlyList<ushort>? instruments = null) =>
        CreateConfigured(VmMusicWildMidiConfigLoader.LoadFile(configFile), requestedSampleRate, enhancedResampling, reverb, instruments);

    // The caller owns the returned renderer; its implementation retains instruments.
    public static TRenderer Create<TReader, TInstruments, TRenderer>(
        VmMusicWildMidiInstrumentCache<TReader, TInstruments> cache, int requestedSampleRate,
        Func<TReader, int, TInstruments> createInstruments, Func<TInstruments, TRenderer?> allocateRenderer,
        bool enhancedResampling, bool reverb)
        where TReader : class where TInstruments : class, IVmMusicWildMidiInstruments
        where TRenderer : class, IVmMusicWildMidiRenderer, IDisposable
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(createInstruments);
        ArgumentNullException.ThrowIfNull(allocateRenderer);
        var sampleRate = requestedSampleRate is >= 11025 and <= 65535 ? requestedSampleRate : 44100;
        var instruments = cache.Load(sampleRate, createInstruments);
        var renderer = allocateRenderer(instruments)
            ?? throw new VmMusicMidiDeviceUnavailableException("Unable to create WildMIDI renderer.");
        try
        {
            VmMusicWildMidiSettings.Initialize(renderer, enhancedResampling, reverb);
            return renderer;
        }
        catch (Exception initializationError)
        {
            try { renderer.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }
}
