# thingdef damage property and damagetype block — chunk 33 audit

**Native:** `thingdef_properties.cpp` `DEFINE_PROPERTY(damage, X, Actor)` 678–692; `thingdef_parse.cpp` `ParseDamageDefinition` 1225–1262.

**Managed:** `DecorateActorDamagePropertyHandler.cs`, `DecorateDamageTypeDefinitionParser.cs`, router `damage` hook, `DecorateDamagePropertyHandlerTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
