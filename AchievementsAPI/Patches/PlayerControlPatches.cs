using System.Linq;
using AchievementsAPI.API;
using HarmonyLib;

namespace AchievementsAPI.Patches;

[HarmonyPatch]
public class PlayerControlPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnGameEnd))]
    [HarmonyPrefix]
    public static void PlayerControl_OnGameEnd_Prefix(PlayerControl __instance)
    {
        foreach (var tab in AchievementsManager.Tabs)
        {
            foreach (var propInfo in tab.GetType().GetProperties().Where(x => x.PropertyType == typeof(CountAchievement)))
            {
                var achievement = (CountAchievement) propInfo.GetValue(tab);
                if (achievement == null) continue;
                if (!achievement.ProgressPersists) achievement.CurrentValue = 0;
            }
        }
    }
}