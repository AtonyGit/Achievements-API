namespace AchievementsAPI.Patches.MainMenu;

public static class AchievementsMenuOpen
{
    private static AchievementsMenu Create()
    {
        return UnityEngine.Object.Instantiate(Assets.achievementPrefab).GetComponent<AchievementsMenu>();
    }

    public static void OpenMenu(MainMenuManager mainMenuManager)
    {
        var menu = Create();
        menu.mainMenuManager = mainMenuManager;
        menu.gameObject.SetActive(true);
    }
    public static void OpenMenu(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var menu = Create();
        menu.OptionsMenuBehaviour = optionsMenuBehaviour;
        menu.gameObject.SetActive(true);
    }
    public static void OpenMenu(string focusAchievementId)
    {
        var menu = Create();
        var mainMenu = UnityEngine.Object.FindObjectOfType<MainMenuManager>();
        if (mainMenu)
        {
            menu.mainMenuManager = mainMenu;
        }
        menu.FocusAchievementId = focusAchievementId;
        menu.gameObject.SetActive(true);
    }
}
