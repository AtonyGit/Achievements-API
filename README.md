An Among Us API that allows mods to add their own achievements!

# Features:
- Easy implementation
- Progressable achievements
- An achievement menu
- Xbox-style toast notifications
- Lots of customization (coming soon)
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/672fa262-1b7c-416a-9ba4-e26f4c82a10b" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/fc0b6b90-e3d1-4753-92b3-a348b8add78f" />

# New in this version

## Reset triggers
Set `ResetTrigger` on any achievement (field or the last optional constructor parameter) to automatically reset it:
- `AchResetTrigger.Never` (default), `OnGameEnd`, `OnMeetingStart`, `OnMeetingEnd`, `OnRoundStart`

Resetting clears `Unlocked` (and `CurrentValue` for `CountAchievement`) and writes through to storage. You can also call `achievement.Reset()` manually.

## Deferred toasts
Set `ToastTiming` to control when an achievement's toast appears:
- `AchToastTiming.Immediate` (default), `OnRoundStart`, `OnMeetingStart`, `OnMeetingEnd`, `OnGameEnd`

Deferred toasts are queued and flushed at the matching game event, shown one at a time. They can be swiped away sideways.

## Soft-registered achievements
Register achievements at runtime without declaring properties on a tab:
```csharp
var tab = new SoftAchievementsTab("My Tab");
AchievementsManager.RegisterTab(tab);
tab.RegisterAchievement(myAchievement);
```
Use `AchievementsManager.GetTab(name)` to look up a tab, and `tab.GetAchievements()` to enumerate all achievements (property-declared + soft-registered).

## Skeld Score
Each achievement is worth Skeld Score based on rarity (10 common / 25 rare / 50 epic / 100 legendary); override `Points` to customize. The menu shows your earned/total Skeld Score next to a Gamerscore-style emblem, plus per-rarity completion, and each row shows the achievement's Skeld Score in a pill.

## Configuration
Toast settings live in the "Toasts" section of the plugin config (`ToastsEnabled`, `ToastDuration`, `ToastSoundEnabled`, `ToastVolume`) and can also be changed in-game via the Settings button in the achievements menu.

For support, or just chatting, join the [discord](https://discord.gg/RJyNm9UaT7)!

>[!NOTE]
>**This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.**

> Will probably hopefully be used in:
> - Stargazer
> - NewMod

> Thanks [pix](https://github.com/wanderingpix) for the help!
