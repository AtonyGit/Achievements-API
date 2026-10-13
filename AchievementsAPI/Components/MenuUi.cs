using AchievementsAPI.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Shared procedural UI helpers (colors, images, text, buttons, progress bars)
/// used by the achievements menu panels. Styled to match the toast: dark
/// surfaces, green accents, white text.
/// </summary>
public static class MenuUi
{
    internal static readonly Color32 StripColor = new(71, 72, 71, 255);      // row strip, from the game's settings rows
    internal static readonly Color32 AccentColor = new(74, 127, 121, 255);   // AU teal
    internal static readonly Color32 AccentHover = new(126, 203, 195, 255);
    internal static readonly Color32 PressedColor = new(56, 96, 92, 255);
    internal static readonly Color32 TrackOffColor = new(112, 112, 112, 255);
    internal static readonly Color32 ValueColor = new(200, 200, 200, 255);

    internal static Image CreateImage(Transform parent, string name, Sprite? sprite, Color color)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        return img;
    }

    internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
        Color color, FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var font = AchievementToast.ResolveFont();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        return tmp;
    }

    internal static Image CreateCard(Transform parent, string name)
    {
        var img = CreateImage(parent, name, ToastSprites.Card, Color.white);
        img.type = Image.Type.Sliced;
        return img;
    }

    internal static void Outline(TextMeshProUGUI tmp)
    {
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
    }

    internal static Button CreateButton(Transform panel, string label, Vector2 position)
    {
        var bg = CreateImage(panel, $"{label}Button", ToastSprites.RoundedRectSmall, Color.white);
        bg.type = Image.Type.Sliced;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(180f, 48f);

        var text = CreateText(rect, "Label", label, 22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        Outline(text);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var button = bg.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = AccentColor;
        colors.highlightedColor = AccentHover;
        colors.pressedColor = PressedColor;
        colors.selectedColor = AccentColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        return button;
    }

    // Thin horizontal progress bar; returns the fill rect so callers can set its anchorMax.x.
    internal static RectTransform CreateProgressBar(Transform parent, Color fillColor, out RectTransform fill)
    {
        var bg = CreateImage(parent, "ProgressBar", ToastSprites.RoundedRectTiny, TrackOffColor);
        bg.type = Image.Type.Sliced;

        var fillImg = CreateImage(bg.rectTransform, "Fill", ToastSprites.RoundedRectTiny, fillColor);
        fillImg.type = Image.Type.Sliced;
        fill = fillImg.rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        return bg.rectTransform;
    }

    internal static void SetProgress(RectTransform fill, float t) => fill.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);

    // Re-parents `target` next to `source` and copies its anchors/pivot/offsets so it covers the same area.
    internal static void MatchRect(RectTransform target, RectTransform source)
    {
        target.SetParent(source.parent, false);
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
        // Draw over the lists
        target.SetAsLastSibling();
    }

    internal static Color RarityColor(int rarity)
    {
        return rarity switch
        {
            1 => new Color32(112, 208, 255, 255),
            2 => new Color32(187, 88, 255, 255),
            3 => new Color32(255, 226, 64, 255),
            _ => new Color32(16, 124, 16, 255)
        };
    }

    internal static string RarityName(int rarity)
    {
        return rarity switch
        {
            1 => "Rare",
            2 => "Epic",
            3 => "Legendary",
            _ => "Common"
        };
    }
}
