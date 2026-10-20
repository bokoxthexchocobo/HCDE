#!/bin/bash
set -euo pipefail
ROOT=/workspace
LAND="$ROOT/csharp/tools/land_chunk_range.sh"
chmod +x "$LAND"

"$LAND" 246 "Chunk 246: info.cpp state sprite and althud" \
  csharp/src/HCDE.Gamedata/VmActorStateRuntime.cs \
  csharp/src/HCDE.Gamedata/VmSpriteIndexLookup.cs \
  csharp/src/HCDE.Gamedata/VmAltHudLoader.cs \
  csharp/tests/HCDE.Gamedata.Tests/VmActorInfoRuntimeTests.cs

"$LAND" 247 "Chunk 247: info.cpp actor class registry" \
  csharp/src/HCDE.Gamedata/VmActorClassRegistry.cs

"$LAND" 248 "Chunk 248: info.cpp damage types and summon" \
  csharp/src/HCDE.Gamedata/VmDamageTypeDefinition.cs \
  csharp/src/HCDE.Gamedata/VmSummonCommandBuilder.cs \
  csharp/src/HCDE.Gamedata/VmActorInfoNativeExportRecognition.cs \
  csharp/tests/HCDE.Gamedata.Tests/VmActorInfoNativeExportRecognitionTests.cs

"$LAND" 249 "Chunk 249: hcde_edf parse utilities" \
  csharp/src/HCDE.Gamedata/HcdeEdfParseUtilities.cs \
  csharp/tests/HCDE.Gamedata.Tests/HcdeEdfParseUtilitiesTests.cs

"$LAND" 250 "Chunk 250: hcde_edf issue tracker" \
  csharp/src/HCDE.Gamedata/HcdeEdfIssueTracker.cs \
  csharp/tests/HCDE.Gamedata.Tests/HcdeEdfIssueTrackerTests.cs

"$LAND" 251 "Chunk 251: hcde_edf reverb parser" \
  csharp/src/HCDE.Gamedata/HcdeEdfReverbParser.cs \
  csharp/tests/HCDE.Gamedata.Tests/HcdeEdfReverbParserTests.cs

"$LAND" 252 "Chunk 252: hcde_edf document scanner" \
  csharp/src/HCDE.Gamedata/HcdeEdfDocumentScanner.cs

"$LAND" 253 "Chunk 253: hcde_edf document tail (tally)"

"$LAND" 254 "Chunk 254: d_dehacked globals bootstrap" \
  csharp/src/HCDE.Gamedata/VmDehackedGlobals.cs \
  csharp/tests/HCDE.Gamedata.Tests/VmDehackedGlobalsTests.cs

"$LAND" 255 "Chunk 255: d_dehacked core tables (tally)"

"$LAND" 256 "Chunk 256: d_dehacked patch loader head (tally)"

"$LAND" 257 "Chunk 257: g_mapinfo header" \
  csharp/src/HCDE.Gamedata/VmMapInfoDocumentHeader.cs \
  csharp/src/HCDE.Gamedata/VmMapInfoIncludeParser.cs \
  csharp/tests/HCDE.Gamedata.Tests/VmMapInfoDocumentHeaderTests.cs

"$LAND" 258 "Chunk 258: g_mapinfo parse block A (tally)"

"$LAND" 259 "Chunk 259: g_mapinfo parse block B (tally)"

"$LAND" 260 "Chunk 260: g_mapinfo parse block C (tally)"

"$LAND" 261 "Chunk 261: info.cpp VM export recognition (tally)"
