using AchievementsAPI;
using UnityEngine;

namespace AchievementsAPI.MainMenu;

public class AchievementsMenuOpen
{
    public static void OpenMenu()
    {

        var menu = Object.Instantiate(Assets.achievementPrefab);
        menu.SetActive(true);
    }
}