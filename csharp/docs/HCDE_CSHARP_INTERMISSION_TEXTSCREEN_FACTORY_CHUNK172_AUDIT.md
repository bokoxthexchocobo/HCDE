# Chunk 172 — textscreen + intermission action factory

`VmIntermissionActionTextscreen` keys and `VmIntermissionDescriptorParser` (`TryAddAction`, `TrySetLink`).

**Not credited:** `TextLump` lump load, `GotoTitle` / `Link` MAPINFO tokens in full parser, `ParseIntermission` descriptor blocks.

Tests: `VmIntermissionActionParserTests.Textscreen_MultilineAndLookup`, `Descriptor_FactoryRejectsUnknownType`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
