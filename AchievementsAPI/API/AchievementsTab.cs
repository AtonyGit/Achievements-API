using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// Abstract class for implementing an achievements tab.
/// </summary>
public abstract class AchievementsTab
{
    private readonly List<BaseAchievement> _softAchievements = new();
    private List<BaseAchievement>? _propertyAchievements;

    /// <summary>
    /// Registers an achievement on this tab at runtime, without needing a property on the tab class.
    /// Loads the achievement's stored state. Does nothing if an achievement with the same Id is already present.
    /// </summary>
    /// <param name="achievement">The achievement to register.</param>
    public void RegisterAchievement(BaseAchievement achievement)
    {
        if (GetAchievements().Any(x => x.Id == achievement.Id)) return;
        _softAchievements.Add(achievement);
        AchievementStorage.AchievementStorageGet(achievement);
    }

    /// <summary>
    /// Gets all achievements on this tab: the ones declared as properties, followed by
    /// soft-registered ones, de-duplicated by Id.
    /// </summary>
    public IReadOnlyList<BaseAchievement> GetAchievements()
    {
        _propertyAchievements ??= GetType().GetProperties()
            .Where(x => x.PropertyType.IsAssignableTo(typeof(BaseAchievement)))
            .Select(x => x.GetValue(this) as BaseAchievement)
            .Where(x => x != null)
            .Cast<BaseAchievement>()
            .ToList();

        return _propertyAchievements
            .Concat(_softAchievements)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();
    }

    /// <summary>
    /// The name of the Achievements Tab, displayed in UI.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Whether the tab should be selectable in the Achievements Menu. Handy for custom UI's or internal achievements.
    /// </summary>
    public virtual bool IsSelectable => true;

    /// <summary>
    /// A method which gets the color of the Achievements Menu background when switching to this tab.
    /// </summary>
    public virtual Color GetTabColor()
    {
        return new Color32(255, 255, 150, 255);
    }
    /// <summary>
    /// The path to the tabs icon, for if you don't have it as a Sprite
    /// </summary>
    public virtual string IconPath => "AchievementsAPI.Resources.ExampleIcon.png";
    
    
    /// <summary>
    /// A Method which gets the icon of the tab, used for its icon in the Achievements Menu. 
    /// </summary>
    /// <returns></returns>
    public virtual Sprite GetIcon()
    {
        return SpriteTools.LoadSpriteFromPath(IconPath, Assembly.GetCallingAssembly(), 100);
    }
}