    using System;
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
        /// Gets or sets the achievement's icon
        /// </summary>
        public virtual Sprite Icon => _sprite;

        /// <summary>
        /// Gets the background sprite for the achievement in the achievements menu.
        /// </summary>
        public virtual Sprite? MenuBgSprite => null;

        /// <summary>
        /// Gets the background sprite for the achievement in the toast pop up.
        /// No longer used by the current toast; kept for compatibility.
        /// </summary>
        public virtual Sprite? ToastBgSprite => null;

        /// <summary>
        /// Gets the offset for the achievement's icon in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuIconOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's title in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuTitleOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's description in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuDescOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's sub-icon, if any.
        /// </summary>
        public virtual Vector3 MenuSubIconOffset => new(0, 0);

        /// <summary>
        /// Gets the scale for the achievement's sub-icon, if any.
        /// </summary>
        public virtual Vector3 MenuSubIconScale => new(0.5f, 0.5f, 1);

        /// <summary>
        /// Gets the achievement's sub-icon, if any.
        /// </summary>
        public virtual Sprite? MenuSubIcon => null;

        /// <summary>
        /// Gets the offset for the achievement's "Obtained" text in the toast pop up.
        /// </summary>
        public virtual Vector3 ToastObtainedOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's title in the toast pop up.
        /// </summary>
        public virtual Vector3 ToastTitleOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's icon in the toast pop up.
        /// </summary>
        public virtual Vector3 ToastIconOffset => new(0, 0);

        /// <summary>
        /// Whether the achievement's background becomes colored.
        /// </summary>
        public bool RarityOnBgSprite = true;

        private Sprite _sprite;
        public bool Unlocked;

        /// <summary>
        /// When this achievement was unlocked (UTC), or null if locked or unlocked before this was tracked.
        /// </summary>
        public DateTime? UnlockedAt => Unlocked ? AchievementStorage.GetData(this).UnlockedAt : null;
        /// <summary>
        /// When the achievement is automatically reset back to its locked/unprogressed state.
        /// </summary>
        public AchResetTrigger ResetTrigger = AchResetTrigger.Never;
        /// <summary>
        /// When the achievement's toast popup is shown relative to game events.
        /// </summary>
        public AchToastTiming ToastTiming = AchToastTiming.Immediate;
        /// <summary>
        /// The achievement's rarity:
        /// 0 = default (common)
        /// 1 = rare (blue)
        /// 2 = epic (purple)
        /// 3 = legendary (yellow)
        /// </summary>
        public int Rarity;
        /// <summary>
        /// Points awarded when this achievement is unlocked, shown as "Skeld Score"
        /// (with the ringed S emblem) in the achievements menu. Defaults from
        /// <see cref="Rarity"/>: 10 (common), 25 (rare), 50 (epic), 100 (legendary).
        /// <see cref="Hidden"/> achievements are worth double. Override to customize.
        /// </summary>
        public virtual int Points => (Rarity switch { 1 => 25, 2 => 50, 3 => 100, _ => 10 }) * (Hidden ? 2 : 1);
        /// <summary>
        /// Whether the achievement is hidden or not (hidden achievements get the default icon and have their name and description set to "Hidden Achievement" until unlocked)
        /// </summary>
        public bool Hidden;
        /// <summary>
        /// Id of the shared category this achievement is listed under in the Achievements Menu
        /// (see <see cref="AchievementCategories"/>). Null uses the owning tab's <see cref="AchievementsTab.CategoryId"/>.
        /// Set via object initializer: <c>new MyAchievement(...) { Category = AchievementCategories.Impostor }</c>.
        /// </summary>
        public string? Category;
        /// <summary>
        /// Whether to hide the achievement's rarity (if the achievement is hidden)
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
            if (showOnUI && !Unlocked) AchievementToastQueue.Enqueue(this, true);
            if (Unlocked) return; //Won't unlock an already unlocked achievement
            Unlocked = true;
            if (doStorageUpdate) AchievementStorage.AchievementStorageUpdate(this, true);

        }
        /// <summary>
        /// Resets this achievement back to its locked state and writes through to storage.
        /// </summary>
        /// <param name="save">Whether to save to storage immediately. Pass false when batching resets.</param>
        public virtual void Reset(bool save = true)
        {
            Unlocked = false;
            AchievementStorage.AchievementDataReset(this, false, save);
        }
        public BaseAchievement(string name, string description, string iconPath, int rarity = 0, bool hidden = false, bool hideRarity = true, Assembly? assembly = null)
        {
            Name = name;
            Description = description;
            IconPath = iconPath;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            _sprite = SpriteTools.LoadSpriteFromPath(IconPath, Assembly, 100);
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
            _sprite = icon;
            Id = Assembly.GetName().Name + "_" + Name;
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
        }
        public BaseAchievement(string name, string description, int rarity = 0, bool hidden = false, bool hideRarity = true, Assembly? assembly = null)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            Id = Assembly.GetName().Name + "_" + Name;
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
        }
    }