using UnityEngine.SocialPlatforms.Impl;

namespace AchievementsAPI.API;

/// <summary>
/// Achievement class for achievements that can increment.
/// </summary>
public class CountAchievement : BaseAchievement
{
    /// <summary>
    /// The current progress for this achievement
    /// </summary>
    public int CurrentValue;
    /// <summary>
    /// The required progress to unlock this achievement.
    /// </summary>
    public int RequiredValue;
    /// <summary>
    /// Defines if the progress persists between games.
    /// </summary>
    public bool ProgressPersists;
    public CountAchievement(string name, string description, string iconPath, int currentValue, int requiredValue, bool progressPersists) : base(name, description, iconPath)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
    }
    /// <summary>
    /// Method to increment the progress of this achievement.
    /// </summary>
    /// <param name="count">The amount to increment by.</param>
    /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
    public void Increment(int count, bool showOnUI = true)
    {
        SetValue(count + CurrentValue, showOnUI);
    }
    /// <summary>
    /// Method to set the progress of this achievement.
    /// </summary>
    /// <param name="value">The progress value.</param>
    /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
    public void SetValue(int value, bool showOnUI = true)
    {
        CurrentValue = value;
        if (CurrentValue >= RequiredValue)
        {
            Unlock(false, false);
            AchievementStorage.AchievementStorageUpdate(this, value, true);
            if (showOnUI) AchievementToast.ShowAndDeleteToast(this, true);
            return;
        }
        AchievementStorage.AchievementStorageUpdate(this, value, Unlocked);
        if (showOnUI && !Unlocked) AchievementToast.ShowAndDeleteToast(this, Unlocked);
    }
}