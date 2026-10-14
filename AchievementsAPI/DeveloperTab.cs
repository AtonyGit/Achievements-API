using AchievementsAPI.API;

namespace AchievementsAPI;

/// <summary>
/// Tab for developer-encounter achievements. Unlocking is driven by
/// <see cref="DeveloperPresence"/>: a developer-only plugin announces itself over
/// RPC, which unlocks <see cref="PlayedWithDeveloper"/> for everyone else. The tab
/// stays completely out of the menu and stats until the achievement is unlocked.
/// </summary>
public class DeveloperTab : AchievementsTab
{
    private const string TrophyIcon = "AchievementsAPI.Resources.AchievementsIcon.png";

    public override string Name => "Developer Tab";
    public override bool IsSelectable => PlayedWithDeveloper.Unlocked;
    public override string IconPath => TrophyIcon;

    public BaseAchievement PlayedWithDeveloper { get; set; } = new BaseAchievement(
        "Among Legends", "Play a game with a developer.", TrophyIcon, rarity: 3, hidden: true);
}
