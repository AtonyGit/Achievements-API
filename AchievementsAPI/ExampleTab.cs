using System.Reflection;
using AchievementsAPI.API;
using UnityEngine;

namespace AchievementsAPI;

public class ExampleTab : AchievementsTab
{
    public override string Name => "Example Tab OwO";
    public override Color GetTabColor()
    {
        return Color.red;
    }

    public BaseAchievement achievement { get; set; } = new BaseAchievement("Double Double Trouble", "uwu", "AchievementsAPI.Resources.ExampleIcon.png");
    public CountAchievement achievement2 { get; set; } = new CountAchievement("Skibidi", ":3", "AchievementsAPI.Resources.ExampleIcon.png", 15, 20, true);
    public CountAchievement achievement3 { get; set; } = new CountAchievement("Skibidi 2 Electric Boogaloo", ":3", "AchievementsAPI.Resources.ExampleIcon.png", 0, 100, true);
    public BaseAchievement baseachievement3 { get; set; } = new BaseAchievement("Skibidi 3 Electric Boogaloo", ":3", "AchievementsAPI.Resources.ExampleIcon.png");
    public override Sprite GetIcon()
    {
        return SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", Assembly.GetCallingAssembly(), 100);
    }
}