// This code is mostly from Pix (wanderingpix), so thanks Pix!
// Pix also made a lot of assets: thanks for that too.
using HarmonyLib;
using System.Collections;
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
    }
}