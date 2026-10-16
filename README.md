<div align="center">

# Achievements API

An Among Us modding API that lets any mod register custom achievements, with a full in-game menu and Xbox-style toast notifications.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 6](https://img.shields.io/badge/.NET-6.0-512BD4)](https://dotnet.microsoft.com)
[![Discord](https://img.shields.io/badge/Chat-on%20Discord-5865F2?logo=discord&logoColor=white)](https://discord.gg/RJyNm9UaT7)

</div>

## Features

- Easy implementation: declare achievements as properties on a tab and they are picked up automatically
- Progressable achievements with persistent counters
- Hidden achievements, rarities and Skeld Points
- An in-game menu with categories, details pages and toast settings
- Xbox-style toast notifications with sound, animations and swipe-away support
- Full mouse, keyboard and controller support
- Encrypted save data that survives updates

## Screenshots

<div align="center">
  <img width="80%" alt="The achievements menu, with the Skeld Points card, categories and achievement rows" src="Images/Menu.png" />
  <br />
  <sub>The main achievements menu, with categories, progress bars and the Skeld Points card.</sub>
  <br /><br />
  <table>
    <tr>
      <td align="center">
        <img alt="An achievement progress toast shown in game" src="Images/ExampleAchievement.png" /><br />
        <sub>A toast popping up mid-game as progress is made.</sub>
      </td>
      <td align="center">
        <img alt="The toast settings page with a preview toast" src="Images/Settings.png" /><br />
        <sub>Toast settings, editable in game with a live preview.</sub>
      </td>
    </tr>
  </table>
</div>

## Quick start

Reference `AchievementsAPI` from your mod and subclass `AchievementsTab`. Achievements are declared as properties and discovered automatically when your plugin loads:

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
    private static readonly StringNames tabName = CustomStringName.CreateAndRegister("My Mod");

    public override StringNames Name => tabName;
    public override string IconPath => "MyMod.Resources.Icon.png";

    public BaseAchievement FirstWin { get; set; } =
        new("First Win", "Win a game.", "MyMod.Resources.FirstWin.png", rarity: 1);

    public CountAchievement TenTasks { get; set; } =
        new("Busy Bee", "Complete 10 tasks.", "MyMod.Resources.Tasks.png", 0, 10);
}
```

Unlock with `achievement.Unlock()`; count achievements unlock on their own via `Increment(amount)` once the goal is reached. Keeping the API optional (soft dependency) uses a different pattern; both are covered in the docs.

## Documentation

**[docs/](docs/README.md)** has the full developer documentation:

- [Getting started](docs/getting-started.md): hard vs soft dependencies, first tab
- [Defining achievements](docs/achievements.md): rarity, Skeld Points, hidden achievements, sub-icons, art overrides
- [Tabs and categories](docs/tabs-and-categories.md): discovery, mod grouping, shared categories
- [Localization](docs/localization.md): `StringNames` and translation packs
- [Game events](docs/game-events.md): reset triggers, progress persistence, deferred toasts
- [Menu and toasts](docs/menu-and-toasts.md): the player UI and its configuration
- [Storage](docs/storage.md): save file format and encryption
- [Building and internals](docs/building.md): project layout, CI, the developer plugin

## Building

The solution targets .NET 6: `dotnet build AchievementsAPI.sln`. CI runs `build.cake` and uploads `AchievementsAPI.dll` as an artifact.

## Credits

- **satire ([rewalo](https://github.com/rewalo))**: UI design, UI code, toast design, toast code and animations, optimizations and much more. The mod would not look or feel the way it does without him.
- **[atony](https://github.com/AtonyGit)**: UI design and achievements system improvements.
- **[pix](https://github.com/wanderingpix)**: for the help along the way.

## Support

For support, or just chatting, join the [Discord](https://discord.gg/RJyNm9UaT7)!

> [!NOTE]
> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
