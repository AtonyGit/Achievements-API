<div align="center">

# Achievements API

An Among Us API for registering mod achievements, tracking progress, and displaying in-game achievement menus and toast notifications.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 6](https://img.shields.io/badge/.NET-6.0-512BD4)](https://dotnet.microsoft.com)
[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?logo=discord&logoColor=white)](https://discord.gg/RJyNm9UaT7)

</div>

## Features

- Property-based or runtime achievement registration
- Progress counters with persistent state
- Hidden achievements, rarity tiers, and Skeld Points
- In-game menu with categories, filters, details, and toast settings
- Animated toast notifications with sound and swipe-to-dismiss
- Mouse, keyboard, and controller support
- Encrypted, signed save data

## Screenshots

<div align="center">
  <img width="80%" alt="Achievements menu with categories, progress bars, and Skeld Points" src="Images/Menu.png" />
  <br />
  <sub>Achievement menu</sub>
  <br /><br />
  <table>
    <tr>
      <td align="center">
        <img alt="Achievement progress toast" src="Images/ExampleAchievement.png" /><br />
        <sub>Progress notification</sub>
      </td>
      <td align="center">
        <img alt="Toast settings page with preview" src="Images/Settings.png" /><br />
        <sub>Toast settings and preview</sub>
      </td>
    </tr>
  </table>
</div>

## Getting started

Choose a **hard dependency** if your mod requires Achievements API. Use a **soft dependency** if your mod must also run without it.

### Hard dependency

Reference `AchievementsAPI` and derive a tab from `AchievementsTab`. Public achievement properties are discovered automatically.

```csharp
using Reactor.Localization.Utilities;

[BepInAutoPlugin]
[BepInDependency(AchievementsAPIPlugin.Id)]
public class MyPlugin : BasePlugin
{
    public override void Load() { }
}

public class MyTab : AchievementsTab
{
    private static readonly StringNames TabName =
        CustomStringName.CreateAndRegister("My Mod");
    private static readonly StringNames FirstWin =
        CustomStringName.CreateAndRegister("First Win");
    private static readonly StringNames FirstWinDesc =
        CustomStringName.CreateAndRegister("Win a game.");
    private static readonly StringNames BusyBee =
        CustomStringName.CreateAndRegister("Busy Bee");
    private static readonly StringNames BusyBeeDesc =
        CustomStringName.CreateAndRegister("Complete 10 tasks.");

    public override StringNames Name => TabName;
    public override string IconPath => "MyMod.Resources.Icon.png";

    public BaseAchievement FirstWin { get; set; } =
        new(FirstWin, FirstWinDesc, "MyMod.Resources.FirstWin.png", rarity: 1);

    public CountAchievement TenTasks { get; set; } =
        new(BusyBee, BusyBeeDesc, "MyMod.Resources.Tasks.png", 0, 10);
}
```

- Call `achievement.Unlock()` to unlock an achievement.
- For `CountAchievement`, call `Increment(amount)` or `SetValue(value)`. It unlocks when `RequiredValue` is reached.
- Icons can be embedded resource paths or `Sprite` instances.
- Rarity values: `0` common, `1` rare, `2` epic, `3` legendary.
- Other options include `hidden`, `hideRarity`, `hideProgress`, and `AchPersistence`.

### Soft dependency

Use runtime registration if players should be able to run your mod without Achievements API. This approach is used by [Town of Us Mira](https://github.com/AtonyGit/TOU-Mira) and [Mira Unleashed](https://github.com/AtonyGit/TownOfUsMiraUnleashed).

Check that the API is loaded before calling it:

```csharp
using BepInEx.Unity.IL2CPP;

[BepInAutoPlugin]
[BepInDependency("AchievementsAPI", BepInDependency.DependencyFlags.SoftDependency)]
public class MyPlugin : BasePlugin
{
    public override void Load()
    {
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey("AchievementsAPI"))
            return;

        RegisterAchievements();
    }

    private void RegisterAchievements()
    {
        var tab = new SoftAchievementsTab("My Mod", categoryId: "general");
        AchievementsManager.RegisterTab(tab);

        tab.RegisterAchievement(new RuntimeAchievement(
            "First Win", "Win a game.", rarity: 1,
            assembly: typeof(MyPlugin).Assembly));

        tab.RegisterAchievement(new RuntimeCountAchievement(
            "Busy Bee", "Complete 10 tasks.", 0, 10,
            AchPersistence.ThroughoutRounds, rarity: 1,
            assembly: typeof(MyPlugin).Assembly));
    }
}
```

**Important:** Do not derive from `AchievementsTab` in a soft-dependent mod. BepInEx/MiraAPI assembly scanning can touch those types before the API is loaded. Use `SoftAchievementsTab`, `RuntimeAchievement`, and `RuntimeCountAchievement` instead.

Pass your mod's assembly to runtime achievements to keep IDs stable (`{AssemblyName}_{AchievementName}`). Keep registration code separate from gameplay logic. For an existing helper, see `TownOfUs.Achievements.AchievementsAPIHelper` in [TOU-Mira](https://github.com/AtonyGit/TOU-Mira).

## Registration and localization

### Localization

Names and descriptions use Reactor `StringNames`, which the menu and toasts resolve through the game's localization system when rendered.

You can use:
- Built-in values such as `StringNames.Crewmate`
- `CustomStringName.CreateAndRegister("...")` for an English fallback
- A `Reactor.Localization.LocalizationProvider`, including providers from MiraAPI

String-based constructors remain available and register English fallback text. The API's own UI strings are exposed through `AchievementsStrings`.

Use `AchievementsStrings.GetText(stringName)`, `GetName()`, or `GetDescription()` to resolve text. Achievement IDs do not depend on the displayed language. For `StringNames`-based IDs, create names deterministically (for example, during plugin load) to keep save data stable.

### Runtime registration

Register achievements without declaring properties on a tab:

```csharp
var tab = new SoftAchievementsTab(
    CustomStringName.CreateAndRegister("My Tab"));

AchievementsManager.RegisterTab(tab);
tab.RegisterAchievement(myAchievement);
```

Use `AchievementsManager.GetTab(stringName)` to retrieve a tab and `tab.GetAchievements()` to enumerate its achievements.

### Categories

Tabs use the `general` category by default. The built-in categories are Crewmate, Impostor, and Neutral. Create additional categories with:

```csharp
AchievementsManager.GetOrCreateCategory(id, name, icon, order);
```

Set an achievement's `Category` to override its tab's category.

### Sub-icons

Achievements can display a secondary sprite, such as a role icon:

```csharp
tab.RegisterAchievement(new RuntimeAchievement(
    "Hitman", "Prophesy your target's death.", sprite)
{
    SubIcon = roleIconSprite,
    SubIconOffset = new Vector3(-40f, -70f),
    SubIconScale = new Vector3(0.6f, 0.6f, 1f)
});
```

`SubIcon`, `SubIconOffset`, and `SubIconScale` are settable fields on `BaseAchievement`. Subclass-based mods can override `MenuSubIcon`, `MenuSubIconOffset`, and `MenuSubIconScale` for dynamic behavior, such as loading bundle assets on demand.

### Hidden achievements

Set `Hidden` to conceal an achievement until it is unlocked. Hidden achievements display as **Hidden Achievement** in the menu and progress toasts. `HideRarity` hides the rarity tint; `HideProgress` hides the progress bar. Hidden achievements award double Skeld Points.

### Skeld Points

Points per achievement are based on rarity:

| Rarity | Points |
| --- | ---: |
| Common | 10 |
| Rare | 25 |
| Epic | 50 |
| Legendary | 100 |

Override `BasePoints` or `PointsMultiplier` to customize scoring. The menu shows total and earned points, completion by rarity, and each achievement's point value.

### Reset triggers

Set `ResetTrigger` to reset an achievement automatically:

- `AchResetTrigger.Never` (default)
- `AchResetTrigger.OnGameEnd`
- `AchResetTrigger.OnMeetingStart`
- `AchResetTrigger.OnMeetingEnd`
- `AchResetTrigger.OnRoundStart`

Resetting clears `Unlocked` and, for `CountAchievement`, `CurrentValue`, then saves the change. You can also call `achievement.Reset()` directly.

### Deferred toasts

Set `ToastTiming` to choose when a notification appears:

- `AchToastTiming.Immediate` (default)
- `AchToastTiming.OnRoundStart`
- `AchToastTiming.OnMeetingStart`
- `AchToastTiming.OnMeetingEnd`
- `AchToastTiming.OnGameEnd`

Deferred notifications are queued and displayed one at a time at the selected game event.

## In-game menu

Open the achievements menu from the main menu. Achievements are grouped by mod and include:

- Filters for All, Completed, In Progress, and Locked
- Category navigation with paging
- Achievement details and unlock timestamps
- Toast settings
- Skeld Points and completion statistics

The menu and toasts capture pointer input while active, preventing clicks from reaching the game UI behind them.

### Keyboard and controller

The menu supports full keyboard and controller navigation.

| Action | Controller | Keyboard |
| --- | --- | --- |
| Move | Left stick / D-pad | Arrow keys |
| Select | A | Enter / Space |
| Back / close | B | Escape |
| Previous / next category | LB / RB | Q / E |
| Previous / next filter | LT / RT | Shift+Tab / Tab |

Moving the mouse switches back to pointer mode.

## Toast notifications

Toasts slide in from the configured screen position and play a chime. Rare and higher-rarity achievements use a different sound. Swipe a toast sideways to dismiss it.

- **Open the menu:** middle-click a toast.
- **Controller:** hold Y while a toast is visible.
- **Trackpad:** enable `TrackpadMode` to open the menu with a normal click.

### Configuration

Settings are available in the plugin config under **Toasts** and in the menu's Settings page.

| Setting | Default | Description |
| --- | --- | --- |
| `ToastsEnabled` | `true` | Enable toast notifications |
| `ToastDuration` | `4.4` | Visible duration in seconds |
| `RareToastDuration` | `6.0` | Visible duration for rarity above common |
| `ToastSoundEnabled` | `true` | Play the unlock sound |
| `ToastVolume` | `0.5` | Unlock sound volume |
| `Position` | `BottomCenter` | Toast position on screen |
| `TrackpadMode` | `false` | Open the menu with a normal click |

## Storage

Save data is stored at `AchievementsAPIData/achievements.json` in the game's persistent data directory. Data is encrypted and signed to prevent manual editing. Unlocks are saved immediately; progress increments are batched to avoid stutter.

## Build

Requires the .NET 6 SDK.

```sh
dotnet build AchievementsAPI.sln
```

CI runs `build.cake` and uploads `AchievementsAPI.dll` as a build artifact.

## Credits

- **satire ([rewalo](https://github.com/rewalo))**: UI and toast design/implementation, animations, and optimizations
- **[atony](https://github.com/AtonyGit)**: UI design and achievement-system improvements
- **[pix](https://github.com/wanderingpix)**: Additional help

## Support

Join the [Discord server](https://discord.gg/RJyNm9UaT7) for support and discussion.

> [!NOTE]
> This mod is not affiliated with or endorsed by Innersloth LLC. Portions of the materials are property of Innersloth LLC. © Innersloth LLC.
