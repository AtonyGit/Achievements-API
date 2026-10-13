using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// A concrete <see cref="AchievementsTab"/> that can be created at runtime without subclassing.
/// </summary>
public class SoftAchievementsTab : AchievementsTab
{
    private readonly string _name;
    private readonly Color? _tabColor;
    private readonly Sprite? _icon;

    /// <summary>
    /// Creates a new soft-registered achievements tab.
    /// </summary>
    /// <param name="name">The name of the tab, displayed in UI.</param>
    /// <param name="tabColor">The color of the Achievements Menu background for this tab.</param>
    /// <param name="icon">The icon of the tab in the Achievements Menu.</param>
    public SoftAchievementsTab(string name, Color? tabColor = null, Sprite? icon = null)
    {
        _name = name;
        _tabColor = tabColor;
        _icon = icon;
    }

    /// <inheritdoc />
    public override string Name => _name;

    /// <inheritdoc />
    public override Color GetTabColor()
    {
        return _tabColor ?? base.GetTabColor();
    }

    /// <inheritdoc />
    public override Sprite GetIcon()
    {
        return _icon ? _icon : base.GetIcon();
    }
}
