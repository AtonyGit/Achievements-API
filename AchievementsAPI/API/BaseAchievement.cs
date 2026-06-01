using UnityEngine;

namespace AchievementsAPI.API;

public class BaseAchievement
{
    public string Name;
    public string Description;
    public string IconPath;
    public bool Unlocked;
    public void Unlock(bool showOnUI = true)
    {
        
    }

    public BaseAchievement(string name, string description, string iconPath)
    {
        Name = name;
        Description = description;
        IconPath = iconPath;
    }
}