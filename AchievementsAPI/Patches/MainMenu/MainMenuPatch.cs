using HarmonyLib;
using System.Collections;
using AchievementsAPI.API;
using AchievementsAPI.MainMenu;

namespace AchievementsAPI.MainMenu;
[HarmonyPatch]
public class MainMenuPatches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPatch(Priority.Last)]
    [HarmonyPostfix]
    public static void OnMainMenuAwakePostfix(MainMenuManager __instance)
    {
        MainMenuButtons.SetUp(__instance);
        AchievementLoadFix.Load();
    }
}