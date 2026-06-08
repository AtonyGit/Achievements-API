namespace AchievementsAPI.API;

public static class AchievementLoadFix
{
    public static void Load()
    {
        AchievementStorage.Load();
        foreach (var tab in AchievementsManager.Tabs)
        {
            AchievementStorage.AchievementStorageGet(tab);
        }
    }
}