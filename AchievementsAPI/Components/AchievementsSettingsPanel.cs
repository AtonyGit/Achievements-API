using AchievementsAPI.API;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built toast settings panel shown as a tab inside the
/// achievements menu, bound directly to the <see cref="AchievementsConfig"/>
/// entries.
/// </summary>
public static class AchievementsSettingsPanel
{
    /// <summary>
    /// Creates the settings panel (hidden until toggled by the caller).
    /// </summary>
    /// <param name="parent">The menu root transform.</param>
    /// <returns>The created settings panel.</returns>
    public static GameObject Create(Transform parent)
    {
        var panel = CreatePanel(parent);
        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        var bg = CreateCard(parent, "ToastSettingsPanel");
        var panel = bg.gameObject;
        // The menu positions the card (MenuUi.MatchRect); rows stretch with it
        var rect = bg.rectTransform;

        // Full-width row strips, 76px pitch, first strip below the frame edge
        const float rowsTop = -40f;
        CreateToggleRow(CreateRowStrip(rect, rowsTop), "Show toasts", AchievementsConfig.ToastsEnabled);
        CreateToggleRow(CreateRowStrip(rect, rowsTop - 76f), "Play sound", AchievementsConfig.ToastSoundEnabled);
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 152f), "Volume", AchievementsConfig.ToastVolume, 0f, 1f,
            v => $"{Mathf.RoundToInt(v * 100f)}%");
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 228f), "Toast duration", AchievementsConfig.ToastDuration, 1f, 10f,
            v => $"{v:0.0}s");
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 304f), "Rare duration", AchievementsConfig.RareToastDuration, 1f, 10f,
            v => $"{v:0.0}s");

        var preview = CreateButton(rect, "Preview", Vector2.zero);
        var previewRect = preview.GetComponent<RectTransform>();
        previewRect.anchorMin = previewRect.anchorMax = new Vector2(1f, 0f);
        previewRect.pivot = new Vector2(1f, 0.5f);
        previewRect.anchoredPosition = new Vector2(-60f, 60f);
        preview.onClick.AddListener((UnityAction)(() => AchievementToast.ShowPreview()));

        return panel;
    }

    private static RectTransform CreateRowStrip(Transform panel, float topY)
    {
        var strip = CreateImage(panel, "Row", ToastSprites.RoundedRectSmall, StripColor);
        strip.type = Image.Type.Sliced;
        var rect = strip.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, topY);
        rect.sizeDelta = new Vector2(-80f, 64f);
        return rect;
    }

    private static void CreateRowLabel(Transform strip, string text)
    {
        var label = CreateText(strip, "Label", text, 24f, Color.white);
        Outline(label);
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

        var track = CreateImage(strip, $"{label}Track", ToastSprites.RoundedRectTiny, TrackOffColor);
        track.type = Image.Type.Sliced;
        var trackRect = track.rectTransform;
        trackRect.anchorMin = trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.pivot = new Vector2(1f, 0.5f);
        trackRect.anchoredPosition = new Vector2(-24f, 0f);
        trackRect.sizeDelta = new Vector2(64f, 32f);

        var knob = CreateImage(trackRect, "Knob", ToastSprites.RoundedRectTiny, Color.white);
        knob.type = Image.Type.Sliced;
        knob.raycastTarget = false;
        var knobRect = knob.rectTransform;
        knobRect.sizeDelta = new Vector2(34f, 24f);

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

        var bg = CreateImage(strip, $"{label}Slider", ToastSprites.RoundedRectTiny, TrackOffColor);
        bg.type = Image.Type.Sliced;
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = bgRect.anchorMax = new Vector2(1f, 0.5f);
        bgRect.pivot = new Vector2(1f, 0.5f);
        bgRect.anchoredPosition = new Vector2(-112f, 0f);
        bgRect.sizeDelta = new Vector2(220f, 10f);

        var fill = CreateImage(bgRect, "Fill", ToastSprites.RoundedRectTiny, AccentColor);
        fill.type = Image.Type.Sliced;
        var fillRect = fill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 0f);

        var handle = CreateImage(bgRect, "Handle", ToastSprites.RoundedRectTiny, Color.white);
        handle.type = Image.Type.Sliced;
        var handleRect = handle.rectTransform;
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(26f, 26f);

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
}
