using UnityEngine.SocialPlatforms.Impl;

namespace AchievementsAPI.API;

public class CountAchievement : BaseAchievement
{
    public int CurrentValue;
    public int RequiredValue;
    public bool ProgressPersists;
    public CountAchievement(string name, string description, string iconPath, int currentValue, int requiredValue, bool progressPersists) : base(name, description, iconPath)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
    }

    public void Increment(int count, bool showOnUI = true)
    {
        SetValue(count + CurrentValue, showOnUI);
    }

    public void SetValue(int value, bool showOnUI = true)
    {
        CurrentValue = value;
        if (CurrentValue >= RequiredValue) Unlock(showOnUI);
    }
}