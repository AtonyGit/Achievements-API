using System.Linq;

namespace AchievementsAPI.API;

public class AchievementsTabSingleton<T> where T : AchievementsTab
{
    public static T Instance => AchievementsManager.Tabs.First(x => x.GetType() == typeof(T)) as T;
}