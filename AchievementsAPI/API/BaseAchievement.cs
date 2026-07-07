    using System.Reflection;
    using UnityEngine;

    namespace AchievementsAPI.API;

    /// <summary>
    /// Base Achievement class, used to define achievements.
    /// </summary>
    public class BaseAchievement
    {
        /// <summary>
        /// The achievement's name
        /// </summary>
        public string Name;
        /// <summary>
        /// The achievement's description
        /// </summary>
        public string Description;
        /// <summary>
        /// The achievement's icon's path
        /// </summary>
        public string IconPath;
        /// <summary>
        /// The achievement's icon
        /// </summary>
        public Sprite Icon;
        public bool Unlocked;
        /// <summary>
        /// The achievement's rarity:
        /// 0 = default (common)
        /// 1 = rare (blue)
        /// 2 = epic (purple)
        /// 3 = legendary (yellow)
        /// </summary>
        public int Rarity;
        /// <summary>
        /// Wether the achievement is hidden or not (hidden achievements get the default icon and have their name and description set to "Hidden Achievement" until unlocked)
        /// </summary>
        public bool Hidden;
        /// <summary>
        /// Wether to hide the achievement's rarity (if the achievement is hidden)
        /// </summary>
        public bool HideRarity;
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
        
        
        public BaseAchievement(string name, string description, string iconPath, int rarity = 0, bool hidden = false, bool hideRarity = true, Assembly? assembly = null)
        {
            Name = name;
            Description = description;
            IconPath = iconPath;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            Icon = SpriteTools.LoadSpriteFromPath(IconPath, Assembly, 100);
            Id = Assembly.GetName().Name + "_" + Name;
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
        }
        public BaseAchievement(string name, string description, Sprite icon, int rarity = 0, bool hidden = false, bool hideRarity = true, Assembly? assembly = null)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            Icon = icon;
            Id = Assembly.GetName().Name + "_" + Name;
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
        }
    }