using System.Linq;

namespace AchievementsAPI.API;
/// <summary>
/// Singleton for accessing an <see cref="AchievementsTab"/>.
/// </summary>
/// <typeparam name="T">The type of the <see cref="AchievementsTab"/></typeparam>
public class AchievementsTabSingleton<T> where T : AchievementsTab
{
    /// <summary>
    /// The Instance of the <see cref="AchievementsTab"/>
    /// </summary>
    public static T Instance => AchievementsManager.Tabs.First(x => x.GetType() == typeof(T)) as T;
}