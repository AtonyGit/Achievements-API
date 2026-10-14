An Among Us API that allows mods to add their own achievements!

# Features:
- Easy implementation
- Progressable achievements
- Hidden achievements, rarities and Skeld Score
- An achievement menu with categories, details and in-game settings
- Xbox-style toast notifications with sound
- Full mouse, keyboard and controller support
- Encrypted save data

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/672fa262-1b7c-416a-9ba4-e26f4c82a10b" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/fc0b6b90-e3d1-4753-92b3-a348b8add78f" />

# Getting started

Create a class that extends `AchievementsTab` and declare your achievements as properties. Tabs are picked up automatically when your plugin loads.

```csharp
public class MyTab : AchievementsTab
{
    public override string Name => "My Mod";
    public override string IconPath => "MyMod.Resources.Icon.png";

    public BaseAchievement FirstWin { get; set; } =
        new("First Win", "Win a game.", "MyMod.Resources.FirstWin.png", rarity: 1);

    public CountAchievement TenTasks { get; set; } =
        new("Busy Bee", "Complete 10 tasks.", "MyMod.Resources.Tasks.png", 0, 10);
}
```

Unlock an achievement with `achievement.Unlock()`. For a `CountAchievement`, use `Increment(amount)` or `SetValue(value)`; it unlocks on its own once `RequiredValue` is reached.

The icon can be an embedded resource path or a `Sprite`. Constructor options cover rarity (0 common, 1 rare, 2 epic, 3 legendary), `hidden`, `hideRarity`, and for count achievements `hideProgress` and how progress persists (`AchPersistence`).

## Soft-registered achievements
Register achievements at runtime without declaring properties on a tab:
```csharp
var tab = new SoftAchievementsTab("My Tab");
AchievementsManager.RegisterTab(tab);
tab.RegisterAchievement(myAchievement);
```
Use `AchievementsManager.GetTab(name)` to look up a tab and `tab.GetAchievements()` to enumerate everything on it.

## Categories
Every tab belongs to a category (`CategoryId`, defaulting to General). Crewmate, Impostor and Neutral are built in, and mods can add their own with `AchievementsManager.GetOrCreateCategory(id, name, icon, order)`. Individual achievements can override their tab's category through `Category`.

## Hidden achievements
Hidden achievements show as "Hidden Achievement" in the menu and in progress toasts until they are unlocked. Set `HideRarity` to also hide the rarity tint and `HideProgress` to hide the progress bar. Hidden achievements are worth double Skeld Score.

## Skeld Score
Each achievement is worth Skeld Score based on rarity (10 common / 25 rare / 50 epic / 100 legendary); override `BasePoints` or `PointsMultiplier` to customize. The menu shows earned and total score next to a Gamerscore-style emblem, plus per-rarity completion, and each row shows its value in a pill.

## Reset triggers
Set `ResetTrigger` on any achievement to automatically reset it:
- `AchResetTrigger.Never` (default), `OnGameEnd`, `OnMeetingStart`, `OnMeetingEnd`, `OnRoundStart`

Resetting clears `Unlocked` (and `CurrentValue` for `CountAchievement`) and writes through to storage. You can also call `achievement.Reset()` manually.

## Deferred toasts
Set `ToastTiming` to control when an achievement's toast appears:
- `AchToastTiming.Immediate` (default), `OnRoundStart`, `OnMeetingStart`, `OnMeetingEnd`, `OnGameEnd`

Deferred toasts are queued and flushed at the matching game event, shown one at a time.

# The menu

The achievements button on the main menu opens a full-screen menu that lists every registered achievement grouped by mod. It has:
- Filters for All, Completed, In Progress and Locked
- A category column with paging for mods that register many categories
- A details page for each achievement with its unlock time
- A settings page for toasts
- A stats card with Skeld Score and per-rarity completion

While the menu or a toast has the pointer, clicks do not reach the game UI behind it.

## Controller and keyboard
The menu can be driven entirely without a mouse. A focus ring shows the current selection and the list scrolls to keep it visible.

| Action | Controller | Keyboard |
| --- | --- | --- |
| Move | Left stick / D-pad | Arrow keys |
| Select | A | Enter / Space |
| Back / Close | B | Escape |
| Previous / next category | LB / RB | Q / E |
| Previous / next filter | LT / RT | Shift+Tab / Tab |

Moving the mouse switches back to pointer mode.

# Toasts

Toasts slide in from the configured screen edge and play a chime (a different one for rare and above). They can be swiped away sideways. Middle-clicking a toast opens the achievements menu; on a controller hold Y while the toast is visible. Trackpad users can turn on `TrackpadMode` to open the menu with a normal click instead.

## Configuration
Toast settings live in the "Toasts" section of the plugin config and can also be changed from the Settings page in the menu:

| Setting | Default | Description |
| --- | --- | --- |
| `ToastsEnabled` | `true` | Show toast popups |
| `ToastDuration` | `4.4` | Seconds a toast stays fully visible |
| `RareToastDuration` | `6.0` | Same, for rarity above common |
| `ToastSoundEnabled` | `true` | Play the unlock chime |
| `ToastVolume` | `0.5` | Chime volume |
| `Position` | `BottomCenter` | Screen corner or edge the toast uses |
| `TrackpadMode` | `false` | Open the menu from a toast with a normal click |

# Storage

Progress is saved to `AchievementsAPIData/achievements.json` in the game's persistent data folder, encrypted and signed so it cannot be edited by hand. Unlocks are written immediately; progress updates are batched so frequent increments do not cause stutter.

# Building

The solution targets .NET 6 and builds with `dotnet build AchievementsAPI.sln`. CI runs `build.cake` and uploads `AchievementsAPI.dll` as an artifact.

For support, or just chatting, join the [discord](https://discord.gg/RJyNm9UaT7)!

>[!NOTE]
>**This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.**

> Will probably hopefully be used in:
> - Stargazer
> - NewMod

> Thanks [pix](https://github.com/wanderingpix) for the help!
