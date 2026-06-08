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
        public string Id;
        /// <summary>
        /// Method to unlock this achievement.
        /// </summary>
        /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
        /// <param name="doStorageUpdate">Indicates whether to update the storage again. Used to make CountAchievements properly update.</param>
        public void Unlock(bool showOnUI = true, bool doStorageUpdate = true)
        {
            if (showOnUI && !Unlocked) AchievementToast.ShowAndDeleteToast(this);
            if (Unlocked) return; //Won't unlock an already unlocked achievement
            Unlocked = true;
            if (doStorageUpdate) AchievementStorage.AchievementStorageUpdate(this, true);
            
        }

        public BaseAchievement(string name, string description, string iconPath)
        {
            Name = name;
            Description = description;
            IconPath = iconPath;
            Assembly = Assembly.GetCallingAssembly();
            Id = Assembly.GetName().Name + "_" + Name;
        }
    }