# Received poison tick and kill-monsters sector — chunk 57 audit

**Native:** `p_interaction.cpp` `P_PoisonMobj` 1660–1713; `p_mobj.cpp` `AActor::Tick` received-poison cadence 5098–5108 and `SECF_KILLMONSTERS` 5111–5117; `maploader/specials.cpp` MBF21 kill-monsters bit 456–458.

**Managed:** `ActorPoisonReceived.cs`, `ActorSectorKillMonsters.cs`, actor received-poison fields, `SectorDamage.Initialize` / `SectorEnvironmentLineSpecials.KillMonstersFlag`; `ActorPoisonReceivedTests`, `ActorSectorKillMonstersTests`.

Player `CF_PREDICTING` early-out, net mirror short-circuit, and full `P_ZMovement` tail (5006–5096) remain outside this slice (largely recognition in `ActorPhysics`).

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
