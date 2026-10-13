using AchievementsAPI.API;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built detail page for a single achievement, shown in place of
/// the achievement lists when an item is clicked or a toast asks to view it.
/// The menu positions the card (MenuUi.MatchRect); layout is anchor-based so it
/// works at any size.
/// </summary>
public static class AchievementDetailPanel
{
    public static GameObject Create(Transform parent, BaseAchievement achievement, System.Action onBack)
    {
        var revealed = !achievement.Hidden || achievement.Unlocked;
        var showRarity = revealed || !achievement.HideRarity;

        var bg = CreateRarityCard(parent, "AchievementDetail", showRarity ? achievement.Rarity : 0);
        var panel = bg.gameObject;
        var rect = bg.rectTransform;

        // Left: rarity-ringed disc with the achievement icon
        var discRect = CreateRarityDisc(rect, "Disc", 200f,
            showRarity ? RarityColor(achievement.Rarity) : RimColor, out var inner);
        discRect.anchorMin = discRect.anchorMax = new Vector2(0f, 0.5f);
        discRect.pivot = new Vector2(0.5f, 0.5f);
        discRect.anchoredPosition = new Vector2(150f, 10f);

        var iconSprite = revealed ? achievement.Icon
            : SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", typeof(AchievementDetailPanel).Assembly, 100);
        var iconColor = achievement.Unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        var icon = CreateImage(inner, "Icon", ToastSprites.TrimTransparent(iconSprite), iconColor);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var iconRect = icon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(136f, 136f);

        // Right column: name, rarity, description, progress, status
        var name = CreateText(rect, "Name", revealed ? achievement.Name : "Hidden Achievement",
            36f, Color.white, FontStyles.Bold);
        Outline(name);
        var nameRect = name.rectTransform;
        StretchRight(nameRect, -48f, 44f);

        var nextY = -100f;
        if (showRarity)
        {
            var rarity = CreateText(rect, "Rarity", RarityName(achievement.Rarity),
                22f, RarityColor(achievement.Rarity));
            StretchRight(rarity.rectTransform, nextY, 30f);
            nextY -= 34f;
        }

        var desc = CreateText(rect, "Description", revealed ? achievement.Description : "Hidden Achievement",
            24f, ValueColor);
        desc.enableWordWrapping = true;
        var descRect = desc.rectTransform;
        StretchRight(descRect, nextY - 6f, 120f);
        nextY -= 126f;

        if (achievement is CountAchievement count && count.RequiredValue > 0 && !(count.HideProgress && count.Hidden))
        {
            var progress = CreateText(rect, "Progress", $"{count.CurrentValue} / {count.RequiredValue}",
                22f, ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
            StretchRight(progress.rectTransform, nextY, 28f);

            var bar = CreateProgressBar(rect, RarityColor(achievement.Rarity), out var fill);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0f, 1f);
            bar.offsetMin = new Vector2(290f, nextY - 8f - 12f);
            bar.offsetMax = new Vector2(-60f, nextY - 8f);
            SetProgress(fill, (float) count.CurrentValue / count.RequiredValue);
        }

        var status = CreateText(rect, "Status", achievement.Unlocked ? "Unlocked" : "Locked",
            24f, achievement.Unlocked ? (Color) AccentHover : ValueColor, FontStyles.Bold);
        var statusRect = status.rectTransform;
        statusRect.anchorMin = statusRect.anchorMax = new Vector2(0f, 0f);
        statusRect.pivot = new Vector2(0f, 0f);
        statusRect.anchoredPosition = new Vector2(290f, 52f);
        statusRect.sizeDelta = new Vector2(300f, 30f);

        var back = CreateButton(rect, "Back", Vector2.zero);
        var backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(1f, 0.5f);
        backRect.anchoredPosition = new Vector2(-60f, 60f);
        back.onClick.AddListener((UnityAction)(() => onBack()));

        return panel;
    }

    // Stretches a rect across the right column: x 290 to -60, top `top`, height `h`.
    private static void StretchRight(RectTransform rect, float top, float h)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(290f, top - h);
        rect.offsetMax = new Vector2(-60f, top);
    }
}
