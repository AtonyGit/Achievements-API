using System.Collections.Generic;
using AchievementsAPI.API;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities;

namespace AchievementsAPI;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class AchievementsAPIPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        AchievementsManager.Initialize();
        Harmony.PatchAll();
    }
}