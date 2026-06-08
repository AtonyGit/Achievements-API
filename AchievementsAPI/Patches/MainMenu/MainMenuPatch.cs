using HarmonyLib;
using System.Collections;
using AchievementsAPI.API;
using AchievementsAPI.MainMenu;
using AchievementsAPI.Patches.MainMenu;

namespace AchievementsAPI.MainMenu;
[HarmonyPatch]
public class MainMenuPatches
{
    private static bool storageInitialized = false;
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPatch(Priority.Last)]
    [HarmonyPostfix]
    public static void OnMainMenuAwakePostfix(MainMenuManager __instance)
    {
        MainMenuButtons.SetUp(__instance);
        if (!storageInitialized)
        {
            AchievementStorage.Load();
            storageInitialized = true;
        }
        AchievementsTabSingleton<ExampleTab>.Instance.achievement.Unlock();
    }
}