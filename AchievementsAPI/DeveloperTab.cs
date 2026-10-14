using System;
using System.Linq;
using AchievementsAPI.API;

namespace AchievementsAPI;

/// <summary>
/// Hidden tab for developer-only achievements. It is never selectable, so its
/// achievements never appear in the menu or stats; unlock toasts still show.
/// </summary>
public class DeveloperTab : AchievementsTab
{
    internal static readonly string[] DeveloperFriendCodes = { "fancyswell#8073" };

    public override string Name => "Developer Tab";
    public override bool IsSelectable => false;

    public BaseAchievement PlayedWithDeveloper { get; set; } = new BaseAchievement(
        "Among Legends", "Play a game with a developer.",
        "AchievementsAPI.Resources.ExampleIcon.png", rarity: 3, hidden: true);

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
