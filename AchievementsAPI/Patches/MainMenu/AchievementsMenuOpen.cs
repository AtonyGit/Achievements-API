using UnityEngine;
using UnityEngine.UI;

namespace AchievementsAPI.Patches.MainMenu;

public static class AchievementsMenuOpen
{
    private static AchievementsMenu Create()
    {
        var go = new GameObject("AchievementsMenu");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 31000; // below the toast canvas (32000)
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        MenuUi.EnsureEventSystem(go);
        return go.AddComponent<AchievementsMenu>();
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
        var existing = UnityEngine.Object.FindObjectOfType<AchievementsMenu>();
        if (existing)
        {
            existing.FocusAchievement(focusAchievementId);
            return;
        }

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
