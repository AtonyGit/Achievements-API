# Tabs and categories

The menu lists achievements grouped by the mod they came from, inside shared categories. Tabs are the unit a mod contributes; categories are the shared buckets mods drop achievements into.

## AchievementsTab

A tab is an `AchievementsTab` subclass. Every non-abstract subclass in a loaded BepInEx plugin's assembly is discovered and registered automatically when that plugin loads, so on the hard-dependency path you never call `RegisterTab` yourself.

```csharp
public class MyTab : AchievementsTab
{
    public override StringNames Name => tabName;
    public override string IconPath => "MyMod.Resources.Icon.png";

    public BaseAchievement FirstWin { get; set; } = ...;
}
```

The overridable surface:

| Member | Default | Effect |
| --- | --- | --- |
| `Name` | abstract | Tab name, shown when its mod-group header is collapsed. A translatable `StringNames` |
| `IconPath` / `GetIcon()` | API example icon | The tab/mod icon. Override `GetIcon()` if you have a `Sprite` rather than an embedded resource |
| `IsSelectable` | `true` | False keeps the whole tab out of the menu and stats. Handy for internal achievements or custom UIs (the API's own `DeveloperTab` uses this to stay invisible until earned) |
| `CategoryId` | `general` | The shared category this tab's achievements default to |
| `ModName` | BepInEx plugin name | Group header when a category mixes mods. Tabs from one mod should return the same value so they collapse into one group |
| `ModIcon` | `GetIcon()` | Icon next to the group header |
| `GetTabColor()` | yellow-ish | Unused by the current menu; kept for compatibility |

Achievements are collected two ways and combined with de-duplication by id:

1. properties of type `BaseAchievement` (or a subclass) on the tab, discovered by reflection on first use;
2. anything passed to `tab.RegisterAchievement(...)` at runtime.

`RegisterAchievement` loads stored state immediately, so late registrations start with the right unlock/progress values. Registering the same id twice is a no-op.

`AchievementsManager.Tabs` holds every registered tab; `GetTab(stringName)` finds one by name. `RegisterTab` ignores a second registration of the same tab type (except `SoftAchievementsTab`, which is always allowed, since many mods share that type).

## SoftAchievementsTab

The concrete, no-subclass tab for runtime registration:

```csharp
var tab = new SoftAchievementsTab(
    "My Mod",                       // or a StringNames
    categoryId: AchievementCategories.Crewmate,
    icon: mySprite,
    modName: "My Mod",              // override the plugin-name grouping
    modIcon: () => MyAssets.Logo);  // lazy
AchievementsManager.RegisterTab(tab);
```

Every constructor argument is optional; null falls back to the base behavior described in the table above.

## Categories

Categories are owned by the API, not by any one mod; any mod can put achievements into any category, and the menu groups entries inside a category by the mod they came from. Four are built in (`AchievementCategories`):

| Id | Display name | Sort order |
| --- | --- | --- |
| `general` | General | 0 |
| `crewmate` | Crewmate | 10 |
| `impostor` | Impostor | 20 |
| `neutral` | Neutral | 30 |

A tab picks its category with `CategoryId`; an individual achievement can break out of its tab's category by setting `Category` on itself. If neither names anything, `general` is used.

Add your own with `AchievementsManager.GetOrCreateCategory`:

```csharp
AchievementsManager.GetOrCreateCategory(
    id: "mymod-events",
    name: CustomStringName.CreateAndRegister("Seasonal"),
    icon: () => MyAssets.EventsIcon,
    order: 40);
```

A few details worth knowing:

- `GetOrCreateCategory` returns the existing entry untouched if the id is taken, so several mods can safely declare the same category. Prefer it over `new AchievementCategory(...)`.
- An id nobody registered still works: the menu auto-creates a category on the fly with the id as its display name and the API trophy as its icon. Register it properly to give it a real name, icon and order.
- `SetIcon` replaces a category's icon; `SetDefaultIcon` only fills it in if none is set. Use the latter to supply art for the built-in categories without fighting other mods over it.
- `order` sorts the left column; built-ins sit at 0-30 and the default for new categories is 100.
