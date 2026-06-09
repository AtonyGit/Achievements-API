using AchievementsAPI.API;
using HarmonyLib;

namespace AchievementsAPI.Patches.MainMenu;
[HarmonyPatch]
public class MainMenuPatches
{
    private static bool _storageInitialized = false;
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPatch(Priority.Last)]
    [HarmonyPostfix]
    public static void OnMainMenuAwakePostfix(MainMenuManager __instance)
    {
        MainMenuButtons.SetUp(__instance);
        if (!_storageInitialized)
        {
            AchievementStorage.Load();
            _storageInitialized = true;
        }
        AchievementsTabSingleton<ExampleTab>.Instance.achievement.Unlock();
    }
}