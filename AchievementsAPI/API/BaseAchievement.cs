using System.Reflection;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// Base Achievement class, used to define achievements.
/// </summary>
public class BaseAchievement
{
    public string Name;
    public string Description;
    public string IconPath;
    public bool Unlocked;
    public Assembly Assembly;
    /// <summary>
    /// Method to unlock this achievement.
    /// </summary>
    /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
    /// <param name="doStorageUpdate">Indicates whether to update the storage again. Used to make CountAchievements properly update.</param>
    public void Unlock(bool showOnUI = true, bool doStorageUpdate = true)
    {
        Unlocked = true; //TODO Maybe remove this and make Unlocked redirect to a check
        if (doStorageUpdate) AchievementStorage.AchievementStorageUpdateBase(this, true);
    }

    public BaseAchievement(string name, string description, string iconPath)
    {
        Name = name;
        Description = description;
        IconPath = iconPath;
        
        Assembly = Assembly.GetCallingAssembly();
    }
}