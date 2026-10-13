using AchievementsAPI.API;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built toast settings panel shown inside the achievements menu,
/// bound directly to the <see cref="AchievementsConfig"/> entries. Styled to
/// match the toast: dark surfaces, green accents, white text.
/// </summary>
public static class AchievementsSettingsPanel
{
    private static readonly Color32 PanelColor = new(26, 26, 26, 250);
    private static readonly Color32 StripColor = new(42, 42, 42, 255);
    private static readonly Color32 AccentColor = new(16, 124, 16, 255);
    private static readonly Color32 PressedColor = new(12, 90, 12, 255);
    private static readonly Color32 TrackOffColor = new(70, 70, 70, 255);
    private static readonly Color32 ValueColor = new(176, 176, 176, 255);

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
        var img = CreateImage(parent, "SettingsGear", ToastSprites.Circle, new Color32(26, 26, 26, 242));
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
        var bg = CreateImage(parent, "ToastSettingsPanel", ToastSprites.RoundedRect, PanelColor);
        bg.type = Image.Type.Sliced;
        var panel = bg.gameObject;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700f, 560f);

        var title = CreateText(rect, "Title", "Toast Settings", 32f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);
        titleRect.sizeDelta = new Vector2(560f, 40f);

        var divider = CreateImage(rect, "Divider", ToastSprites.RoundedRect, AccentColor);
        divider.type = Image.Type.Sliced;
        var dividerRect = divider.rectTransform;
        dividerRect.anchorMin = dividerRect.anchorMax = new Vector2(0.5f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 0.5f);
        dividerRect.anchoredPosition = new Vector2(0f, -72f);
        dividerRect.sizeDelta = new Vector2(300f, 3f);

        // Full-width row strips, 76px pitch, first strip 40px below the divider
        const float rowsTop = -114f;
        CreateToggleRow(CreateRowStrip(rect, rowsTop), "Show toasts", AchievementsConfig.ToastsEnabled);
        CreateToggleRow(CreateRowStrip(rect, rowsTop - 76f), "Play sound", AchievementsConfig.ToastSoundEnabled);
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 152f), "Volume", AchievementsConfig.ToastVolume, 0f, 1f,
            v => $"{Mathf.RoundToInt(v * 100f)}%");
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 228f), "Toast duration", AchievementsConfig.ToastDuration, 1f, 10f,
            v => $"{v:0.0}s");

        var preview = CreateButton(rect, "Preview", new Vector2(-110f, 64f));
        preview.onClick.AddListener((UnityAction)(() => AchievementToast.ShowPreview()));

        var close = CreateButton(rect, "Close", new Vector2(110f, 64f));
        close.onClick.AddListener((UnityAction)(() => panel.SetActive(false)));

        return panel;
    }

    private static RectTransform CreateRowStrip(Transform panel, float topY)
    {
        var strip = CreateImage(panel, "Row", ToastSprites.RoundedRect, StripColor);
        strip.type = Image.Type.Sliced;
        var rect = strip.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, topY);
        rect.sizeDelta = new Vector2(620f, 64f);
        return rect;
    }

    private static void CreateRowLabel(Transform strip, string text)
    {
        var label = CreateText(strip, "Label", text, 24f, Color.white);
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(24f, 0f);
        rect.sizeDelta = new Vector2(320f, 32f);
    }

    private static void ApplySwitchState(RectTransform knob, Image track, bool on)
    {
        knob.anchorMin = knob.anchorMax = new Vector2(on ? 1f : 0f, 0.5f);
        knob.pivot = new Vector2(on ? 1f : 0f, 0.5f);
        knob.anchoredPosition = new Vector2(on ? -3f : 3f, 0f);
        track.color = on ? AccentColor : TrackOffColor;
    }

    private static void CreateToggleRow(Transform strip, string label, ConfigEntry<bool> entry)
    {
        CreateRowLabel(strip, label);

        var track = CreateImage(strip, $"{label}Track", ToastSprites.RoundedRect, TrackOffColor);
        track.type = Image.Type.Sliced;
        var trackRect = track.rectTransform;
        trackRect.anchorMin = trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.pivot = new Vector2(1f, 0.5f);
        trackRect.anchoredPosition = new Vector2(-24f, 0f);
        trackRect.sizeDelta = new Vector2(64f, 32f);

        var knob = CreateImage(trackRect, "Knob", ToastSprites.Circle, Color.white);
        knob.raycastTarget = false;
        var knobRect = knob.rectTransform;
        knobRect.sizeDelta = new Vector2(26f, 26f);

        var toggle = track.gameObject.AddComponent<Toggle>();
        toggle.graphic = null;
        toggle.targetGraphic = track;
        toggle.transition = Selectable.Transition.ColorTint;
        var colors = toggle.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        toggle.colors = colors;
        toggle.isOn = entry.Value;
        ApplySwitchState(knobRect, track, entry.Value);
        toggle.onValueChanged.AddListener((UnityAction<bool>)(v =>
        {
            entry.Value = v;
            ApplySwitchState(knobRect, track, v);
        }));
    }

    private static void CreateSliderRow(Transform strip, string label, ConfigEntry<float> entry,
        float min, float max, System.Func<float, string> format)
    {
        CreateRowLabel(strip, label);

        var valueLabel = CreateText(strip, $"{label}Value", format(entry.Value), 22f,
            ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        var valueRect = valueLabel.rectTransform;
        valueRect.anchorMin = valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.pivot = new Vector2(1f, 0.5f);
        valueRect.anchoredPosition = new Vector2(-24f, 0f);
        valueRect.sizeDelta = new Vector2(72f, 28f);

        var bg = CreateImage(strip, $"{label}Slider", ToastSprites.RoundedRect, TrackOffColor);
        bg.type = Image.Type.Sliced;
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = bgRect.anchorMax = new Vector2(1f, 0.5f);
        bgRect.pivot = new Vector2(1f, 0.5f);
        bgRect.anchoredPosition = new Vector2(-112f, 0f);
        bgRect.sizeDelta = new Vector2(220f, 10f);

        var fill = CreateImage(bgRect, "Fill", ToastSprites.RoundedRect, AccentColor);
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
        handleRect.sizeDelta = new Vector2(22f, 22f);

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
    }

    private static Button CreateButton(Transform panel, string label, Vector2 position)
    {
        var bg = CreateImage(panel, $"{label}Button", ToastSprites.RoundedRect, Color.white);
        bg.type = Image.Type.Sliced;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(180f, 48f);

        var text = CreateText(rect, "Label", label, 22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var button = bg.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = StripColor;
        colors.highlightedColor = AccentColor;
        colors.pressedColor = PressedColor;
        colors.selectedColor = AccentColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        return button;
    }
}
