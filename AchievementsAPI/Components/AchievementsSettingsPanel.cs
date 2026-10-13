using AchievementsAPI.API;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built toast settings panel shown inside the achievements menu,
/// bound directly to the <see cref="AchievementsConfig"/> entries.
/// </summary>
public static class AchievementsSettingsPanel
{
    /// <summary>
    /// Creates the settings gear button (top-right of the parent) and the settings
    /// panel (hidden until the gear is clicked).
    /// </summary>
    /// <param name="parent">The menu root transform.</param>
    /// <returns>The created settings panel.</returns>
    public static GameObject Create(Transform parent)
    {
        var panel = CreatePanel(parent);
        panel.SetActive(false);

        var gear = CreateGearButton(parent);
        gear.onClick.AddListener((UnityAction)(() => panel.SetActive(!panel.activeSelf)));

        return panel;
    }

    private static Image CreateImage(Transform parent, string name, Sprite? sprite, Color color)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        return img;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
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

    private static Button CreateGearButton(Transform parent)
    {
        var img = CreateImage(parent, "SettingsGear", ToastSprites.Circle, new Color32(30, 30, 30, 242));
        var rect = img.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-40f, -40f);
        rect.sizeDelta = new Vector2(56f, 56f);

        var font = AchievementToast.ResolveFont();
        var label = font != null && font.HasCharacter('⚙') ? "⚙" : "Settings";
        var text = CreateText(rect, "Label", label, label.Length > 1 ? 13f : 30f, Color.white,
            FontStyles.Bold, TextAlignmentOptions.Center);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return img.gameObject.AddComponent<Button>();
    }

    private static GameObject CreatePanel(Transform parent)
    {
        var bg = CreateImage(parent, "ToastSettingsPanel", ToastSprites.RoundedRect, new Color32(30, 30, 30, 242));
        bg.type = Image.Type.Sliced;
        var panel = bg.gameObject;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(640f, 480f);

        var title = CreateText(rect, "Title", "Toast Settings", 32f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(560f, 42f);

        var y = -90f;
        y = CreateToggleRow(rect, y, "Show toasts", AchievementsConfig.ToastsEnabled);
        y = CreateToggleRow(rect, y, "Play sound", AchievementsConfig.ToastSoundEnabled);
        y = CreateSliderRow(rect, y, "Volume", AchievementsConfig.ToastVolume, 0f, 1f,
            v => $"{Mathf.RoundToInt(v * 100f)}%");
        y = CreateSliderRow(rect, y, "Toast duration", AchievementsConfig.ToastDuration, 1f, 10f,
            v => $"{v:0.0}s");

        var preview = CreateButton(rect, "Preview", new Vector2(-120f, y - 10f));
        preview.onClick.AddListener((UnityAction)(() => AchievementToast.ShowPreview()));

        var close = CreateButton(rect, "Close", new Vector2(120f, y - 10f));
        close.onClick.AddListener((UnityAction)(() => panel.SetActive(false)));

        return panel;
    }

    private static TextMeshProUGUI CreateRowLabel(Transform panel, float y, string text)
    {
        var label = CreateText(panel, "Label", text, 24f, Color.white);
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(50f, y - 24f);
        rect.sizeDelta = new Vector2(240f, 48f);
        return label;
    }

    private static float CreateToggleRow(Transform panel, float y, string label, ConfigEntry<bool> entry)
    {
        CreateRowLabel(panel, y, label);

        var bg = CreateImage(panel, $"{label}Toggle", ToastSprites.RoundedRect, new Color32(60, 60, 60, 255));
        bg.type = Image.Type.Sliced;
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.pivot = new Vector2(1f, 0.5f);
        bgRect.anchoredPosition = new Vector2(-50f, y - 24f);
        bgRect.sizeDelta = new Vector2(64f, 32f);

        var check = CreateImage(bgRect, "Checkmark", ToastSprites.Circle, new Color32(16, 124, 16, 255));
        var checkRect = check.rectTransform;
        checkRect.anchorMin = checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = Vector2.zero;
        checkRect.sizeDelta = new Vector2(20f, 20f);

        var toggle = bg.gameObject.AddComponent<Toggle>();
        toggle.graphic = check;
        toggle.targetGraphic = bg;
        toggle.isOn = entry.Value;
        toggle.onValueChanged.AddListener((UnityAction<bool>)(v => entry.Value = v));

        y -= 56f;
        return y;
    }

    private static float CreateSliderRow(Transform panel, float y, string label, ConfigEntry<float> entry,
        float min, float max, System.Func<float, string> format)
    {
        CreateRowLabel(panel, y, label);

        var valueLabel = CreateText(panel, $"{label}Value", format(entry.Value), 22f,
            new Color32(176, 176, 176, 255), FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        var valueRect = valueLabel.rectTransform;
        valueRect.anchorMin = valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 0.5f);
        valueRect.anchoredPosition = new Vector2(-50f, y - 24f);
        valueRect.sizeDelta = new Vector2(80f, 48f);

        var bg = CreateImage(panel, $"{label}Slider", ToastSprites.RoundedRect, new Color32(60, 60, 60, 255));
        bg.type = Image.Type.Sliced;
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.pivot = new Vector2(1f, 0.5f);
        bgRect.anchoredPosition = new Vector2(-140f, y - 24f);
        bgRect.sizeDelta = new Vector2(200f, 16f);

        var fill = CreateImage(bgRect, "Fill", ToastSprites.RoundedRect, new Color32(16, 124, 16, 255));
        fill.type = Image.Type.Sliced;
        var fillRect = fill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 0f);

        var handle = CreateImage(bgRect, "Handle", ToastSprites.Circle, Color.white);
        var handleRect = handle.rectTransform;
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(24f, 24f);

        var slider = bg.gameObject.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
        slider.value = entry.Value;
        slider.onValueChanged.AddListener((UnityAction<float>)(v =>
        {
            entry.Value = v;
            valueLabel.text = format(v);
        }));

        y -= 56f;
        return y;
    }

    private static Button CreateButton(Transform panel, string label, Vector2 position)
    {
        var bg = CreateImage(panel, $"{label}Button", ToastSprites.RoundedRect, new Color32(16, 124, 16, 255));
        bg.type = Image.Type.Sliced;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(160f, 48f);

        var text = CreateText(rect, "Label", label, 22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return bg.gameObject.AddComponent<Button>();
    }
}
