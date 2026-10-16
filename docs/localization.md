# Localization

Tab names, achievement names and descriptions, and category names are stored as Reactor `StringNames`, not raw strings. The menu and toasts resolve them through the game's translation system (patched by Reactor's `LocalizationManager`) every time they render, so translations kick in automatically without touching achievements at runtime when the language changes.

## Supplying names

Any of these work everywhere a name is expected:

- a built-in game `StringNames`, e.g. `StringNames.Crewmate`;
- `CustomStringName.CreateAndRegister("...")`, which registers the given text as the hard-coded English fallback;
- a `StringNames` backed by a `Reactor.Localization.LocalizationProvider` (your own, or one from an API like MiraAPI) for real translations in every language.

Plain `string` constructors are still available and behave like `CreateAndRegister`: the text becomes a hard-coded English-only string. Fine for quick mods, but use `StringNames` if you want translation packs to cover your achievements.

## Translating the API's own UI

`AchievementsStrings` holds every piece of player-facing text the API renders itself: menu chrome, filters, rarity names, category names, toast titles ("Achievement Unlocked!" / "Achievement Progressed!"), the settings labels. All of it is `StringNames` with English registered as the hard-coded fallback, and a localization provider with higher priority (like a translation pack) wins over the fallback. One provider can translate the whole achievements UI without the API knowing about it.

Resolve any `StringNames` in code with:

```csharp
var text = AchievementsStrings.GetText(stringName);         // menu/system text
var name = achievement.GetName();                           // achievement name in current language
var desc = achievement.GetDescription();
```

`GetText` goes through the game's `TranslationController`; before the game has booted far enough for that to exist (e.g. very early plugin load) it returns an empty string.

`AchievementsStrings.RarityName(rarity)` maps 0-3 to the Common/Rare/Epic/Legendary strings.

## Ids are language-independent

The id used for save files is `{AssemblyName}_{Name}`, but the "name" half is not display text:

- string constructors keep the raw string they were given (so existing saves keep matching);
- `StringNames` constructors use the enum value, which is whatever `CreateAndRegister` (or the game's enum) assigned.

Two consequences:

1. **Rename freely.** Editing an achievement's display text never breaks save files, as long as each name keeps its own `StringNames` value.
2. **Register `StringNames` deterministically.** `CustomStringName` values are assigned at runtime in registration order, so creating the same strings in the same order on every launch (static readonly fields at plugin load, like the [getting started](getting-started.md) example) keeps ids stable across runs and across versions. Registering achievements conditionally, or in an order that can change between builds, will reshuffle saves.
