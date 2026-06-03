using AchievementsAPI;
using AchievementsAPI.API;
using UnityEngine;

namespace AchievementsAPI.MainMenu;

public class AchievementsMenuOpen
{
    public static void OpenMenu(MainMenuManager mainMenuManager)
    {
        var menu = Object.Instantiate(Assets.achievementPrefab).GetComponent<AchievementsMenu>();
        menu.mainMenuManager = mainMenuManager;
        menu.gameObject.SetActive(true);
        AchievementsTabSingleton<ExampleTab>.Instance.baseachievement3.Unlock();
        AchievementsTabSingleton<ExampleTab>.Instance.achievement3.Increment(1);
    }
}