using AchievementsAPI.API;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built detail popup for a single achievement, shown when an item
/// in the achievements menu is clicked or a toast asks to view it.
/// </summary>
public static class AchievementDetailPanel
{
    public static GameObject Create(Transform parent, BaseAchievement achievement)
    {
        var revealed = !achievement.Hidden || achievement.Unlocked;
        var showRarity = revealed || !achievement.HideRarity;

        var bg = CreateCard(parent, "AchievementDetail");
        var panel = bg.gameObject;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 400f);

        // Left: rarity disc with the achievement icon
        var discColor = showRarity ? RarityColor(achievement.Rarity) : (Color) new Color32(60, 60, 60, 255);
        var disc = CreateImage(rect, "Disc", ToastSprites.Circle, discColor);
        disc.raycastTarget = false;
        var discRect = disc.rectTransform;
        discRect.anchorMin = discRect.anchorMax = new Vector2(0.5f, 0.5f);
        discRect.anchoredPosition = new Vector2(-260f, 20f);
        discRect.sizeDelta = new Vector2(160f, 160f);

        var iconSprite = revealed ? achievement.Icon
            : SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", typeof(AchievementDetailPanel).Assembly, 100);
        var iconColor = achievement.Unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        var icon = CreateImage(discRect, "Icon", ToastSprites.TrimTransparent(iconSprite), iconColor);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var iconRect = icon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(112f, 112f);

        // Right column: name, rarity, description, progress, status
        var name = CreateText(rect, "Name", revealed ? achievement.Name : "Hidden Achievement",
            32f, Color.white, FontStyles.Bold);
        Outline(name);
        var nameRect = name.rectTransform;
        nameRect.anchorMin = nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0f, 1f);
        nameRect.anchoredPosition = new Vector2(-150f, -40f);
        nameRect.sizeDelta = new Vector2(490f, 40f);

        var nextY = -90f;
        if (showRarity)
        {
            var rarity = CreateText(rect, "Rarity", RarityName(achievement.Rarity),
                22f, RarityColor(achievement.Rarity));
            var rarityRect = rarity.rectTransform;
            rarityRect.anchorMin = rarityRect.anchorMax = new Vector2(0.5f, 1f);
            rarityRect.pivot = new Vector2(0f, 1f);
            rarityRect.anchoredPosition = new Vector2(-150f, nextY);
            rarityRect.sizeDelta = new Vector2(490f, 28f);
            nextY -= 34f;
        }

        var desc = CreateText(rect, "Description", revealed ? achievement.Description : "Hidden Achievement",
            22f, ValueColor);
        desc.enableWordWrapping = true;
        var descRect = desc.rectTransform;
        descRect.anchorMin = descRect.anchorMax = new Vector2(0.5f, 1f);
        descRect.pivot = new Vector2(0f, 1f);
        descRect.anchoredPosition = new Vector2(-150f, nextY);
        descRect.sizeDelta = new Vector2(490f, 90f);

        if (achievement is CountAchievement count && count.RequiredValue > 0 && !(count.HideProgress && count.Hidden))
        {
            var progress = CreateText(rect, "Progress", $"{count.CurrentValue} / {count.RequiredValue}",
                22f, ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
            var progressRect = progress.rectTransform;
            progressRect.anchorMin = progressRect.anchorMax = new Vector2(0.5f, 0f);
            progressRect.pivot = new Vector2(1f, 1f);
            progressRect.anchoredPosition = new Vector2(340f, 116f);
            progressRect.sizeDelta = new Vector2(490f, 28f);

            var bar = CreateProgressBar(rect, RarityColor(achievement.Rarity), out var fill);
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.anchoredPosition = new Vector2(95f, 96f);
            bar.sizeDelta = new Vector2(480f, 10f);
            SetProgress(fill, (float) count.CurrentValue / count.RequiredValue);
        }

        var status = CreateText(rect, "Status", achievement.Unlocked ? "Unlocked" : "Locked",
            22f, achievement.Unlocked ? (Color) AccentHover : ValueColor, FontStyles.Bold);
        var statusRect = status.rectTransform;
        statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0f);
        statusRect.pivot = new Vector2(0f, 0f);
        statusRect.anchoredPosition = new Vector2(-150f, 40f);
        statusRect.sizeDelta = new Vector2(490f, 28f);

        var close = CreateButton(rect, "Close", new Vector2(0f, 40f));
        close.onClick.AddListener((UnityAction)(() => panel.Destroy()));

        return panel;
    }
}
