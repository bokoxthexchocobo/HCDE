# thingdef translation property — chunk 32 audit

**Native:** `thingdef_properties.cpp` `DEFINE_PROPERTY(translation, L, Actor)` 815–858.

**Managed:** `DecorateTranslationPropertyHandler.cs`, router hook, `DecorateTranslationPropertyHandlerTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
