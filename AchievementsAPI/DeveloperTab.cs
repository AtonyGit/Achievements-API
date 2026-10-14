using System;
using System.Linq;
using AchievementsAPI.API;

namespace AchievementsAPI;

/// <summary>
/// Tab for developer-encounter achievements. It stays completely out of the menu
/// and stats until its achievement is unlocked, at which point it becomes visible.
/// </summary>
public class DeveloperTab : AchievementsTab
{
    private const string TrophyIcon = "AchievementsAPI.Resources.AchievementsIcon.png";

    internal static readonly string[] DeveloperFriendCodes = { "fancyswell#8073" };

    public override string Name => "Developer Tab";
    public override bool IsSelectable => PlayedWithDeveloper.Unlocked;
    public override string IconPath => TrophyIcon;

    public BaseAchievement PlayedWithDeveloper { get; set; } = new BaseAchievement(
        "Among Legends", "Play a game with a developer.", TrophyIcon, rarity: 3, hidden: true);

    internal static bool IsDeveloper(string? friendCode) =>
        !string.IsNullOrEmpty(friendCode) &&
        DeveloperFriendCodes.Any(c => c.Equals(friendCode, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Unlocks <see cref="PlayedWithDeveloper"/> if any other player in the
    /// current game has a developer friend code.
    /// </summary>
    internal static void CheckForDevelopers()
    {
        try
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.AmOwner || p.Data == null) continue;
                if (!IsDeveloper(p.Data.FriendCode)) continue;
                AchievementsManager.Tabs.OfType<DeveloperTab>().FirstOrDefault()?.PlayedWithDeveloper.Unlock();
                return;
            }
        }
        catch (Exception e)
        {
            AchievementsAPIPlugin.Instance.Log.LogWarning($"Developer check failed: {e}");
        }
    }
}
