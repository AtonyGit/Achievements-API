using System;
using AchievementsAPI.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Shared procedural UI helpers (colors, images, text, buttons, progress bars)
/// used by the achievements menu panels. Styled to match the toast: dark
/// surfaces, green accents, white text.
/// </summary>
public static class MenuUi
{
    internal static readonly Color32 AccentColor = new(74, 127, 121, 255);   // AU teal
    internal static readonly Color32 AccentHover = new(126, 203, 195, 255);
    internal static readonly Color32 PressedColor = new(56, 96, 92, 255);
    internal static readonly Color32 TrackOffColor = new(112, 112, 112, 255);
    internal static readonly Color32 ValueColor = new(190, 190, 190, 255);
    internal static readonly Color32 RimColor = new(112, 114, 118, 255);     // neutral ring for common/no-rarity

    internal static Image CreateImage(Transform parent, string name, Sprite? sprite, Color color)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = SpriteTools.PreserveSprite(sprite);
        img.color = color;
        return img;
    }

    internal static void ApplyGameFont(TextMeshProUGUI tmp)
    {
        if (DisconnectPopup.Instance && DisconnectPopup.Instance._textArea)
        {
            var source = DisconnectPopup.Instance._textArea;
            tmp.font = source.font;
            tmp.fontMaterial = source.fontMaterial;
            return;
        }

        if (HudManager.InstanceExists && HudManager.Instance.TaskPanel && HudManager.Instance.TaskPanel.taskText)
        {
            var source = HudManager.Instance.TaskPanel.taskText;
            tmp.font = source.font;
            tmp.fontMaterial = source.fontMaterial;
            return;
        }

        var prefabText = Assets.achievementToastPrefab != null
            ? Assets.achievementToastPrefab.transform.Find("AchievementName")?.GetComponent<TextMeshProUGUI>()
            : null;
        if (prefabText != null && prefabText.font != null)
        {
            tmp.font = prefabText.font;
            tmp.fontMaterial = prefabText.fontMaterial;
            return;
        }

        var font = AchievementToast.ResolveFont();
        if (font != null) tmp.font = font;
    }

    internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
        Color color, FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyGameFont(tmp);
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

    // Card frame with a rarity-tinted rim (common keeps the neutral rim). Returns the card image; the rim overlay is a stretched child.
    internal static Image CreateRarityCard(Transform parent, string name, int rarity)
    {
        var card = CreateCard(parent, name);
        if (rarity > 0)
        {
            card.color = Color.Lerp(Color.white, RarityColor(rarity), 0.12f);
            var c = RarityColor(rarity);
            var rim = CreateImage(card.rectTransform, "Rim", ToastSprites.CardRim, new Color(c.r, c.g, c.b, 0.65f));
            rim.type = Image.Type.Sliced;
            rim.raycastTarget = false;
            var rimRect = rim.rectTransform;
            rimRect.anchorMin = Vector2.zero;
            rimRect.anchorMax = Vector2.one;
            rimRect.offsetMin = Vector2.zero;
            rimRect.offsetMax = Vector2.zero;
        }
        return card;
    }

    // Dark disc with a rarity-colored ring (4px). Returns the ring (outer) rect; `inner` is the dark face to parent icons to.
    internal static RectTransform CreateRarityDisc(Transform parent, string name, float size, Color ringColor, out RectTransform inner)
    {
        var ring = CreateImage(parent, name, ToastSprites.Circle, ringColor);
        ring.raycastTarget = false;
        var ringRect = ring.rectTransform;
        ringRect.sizeDelta = new Vector2(size, size);

        var face = CreateImage(ringRect, "Inner", ToastSprites.Circle, new Color32(41, 40, 41, 255));
        face.raycastTarget = false;
        inner = face.rectTransform;
        inner.anchorMin = Vector2.zero;
        inner.anchorMax = Vector2.one;
        inner.offsetMin = new Vector2(4f, 4f);
        inner.offsetMax = new Vector2(-4f, -4f);
        return ringRect;
    }

    internal static void Outline(TextMeshProUGUI tmp)
    {
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
    }

    private static Sprite _trophy;

    internal static Sprite AchievementIcon
    {
        get
        {
            if (_trophy == null)
            {
                _trophy = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.AchievementsIcon.png",
                    typeof(MenuUi).Assembly, 100);
            }
            return ToastSprites.TrimTransparent(_trophy);
        }
    }

    // Skeld Score emblem: the achievements trophy icon (same sprite as the
    // in-game settings tab), untinted.
    internal static RectTransform CreateScoreEmblem(Transform parent, float size)
    {
        var icon = CreateImage(parent, "ScoreEmblem", AchievementIcon, Color.white);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var rect = icon.rectTransform;
        rect.sizeDelta = new Vector2(size, size);
        return rect;
    }

    /// <summary>
    /// Small teal "xN" tag pinned to the top-right corner of <paramref name="parent"/>, used to
    /// show a points multiplier next to the base score rather than inflating the number.
    /// </summary>
    internal static RectTransform CreateMultiplierBadge(Transform parent, int multiplier, Vector2 overhang)
    {
        var badge = CreateImage(parent, "MultiplierBadge", ToastSprites.ButtonBox, AccentHover);
        badge.type = Image.Type.Sliced;
        badge.raycastTarget = false;
        var rect = badge.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = overhang;
        rect.sizeDelta = new Vector2(34f, 20f);

        var text = CreateText(rect, "Label", $"x{multiplier}", 14f, Color.black, FontStyles.Bold,
            TextAlignmentOptions.Center);
        text.overflowMode = TextOverflowModes.Overflow;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return rect;
    }

    private static AudioClip _clickSound;
    private static AudioClip _hoverSound;
    private static Sprite _closeButtonSprite;

    /// <summary>
    /// The game's own close-button sprite (the options menu's Close button),
    /// scraped at runtime so it matches the shipped UI exactly. Null when no
    /// options menu exists in the scene yet or none could be found.
    /// </summary>
    internal static Sprite CloseButtonSprite
    {
        get
        {
            if (_closeButtonSprite == null) ResolveCloseButtonSprite();
            return _closeButtonSprite;
        }
    }

    // Scans the first options menu's buttons for a SpriteRenderer whose own,
    // parent's or grandparent's name contains "close" (the back button's
    // renderers live under activeSprites and their parents are unnamed).
    private static void ResolveCloseButtonSprite()
    {
        bool IsCloseName(string n) => n != null && n.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0;

        var menus = UnityEngine.Object.FindObjectsByType<OptionsMenuBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (menus == null || menus.Length == 0) return;

        foreach (var button in menus[0].GetComponentsInChildren<PassiveButton>(true))
        {
            foreach (var renderer in button.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.sprite == null) continue;
                var t = renderer.transform;
                if (IsCloseName(t.name) || (t.parent != null && (IsCloseName(t.parent.name)
                    || (t.parent.parent != null && IsCloseName(t.parent.parent.name)))))
                {
                    _closeButtonSprite = renderer.sprite;
                    return;
                }
            }
        }
    }

    // Lazily resolved game UI clips; cached, but `== null` is a Unity null
    // check so a destroyed clip is re-resolved.
    internal static AudioClip ClickSound
    {
        get
        {
            if (_clickSound == null) ResolveUiSounds();
            return _clickSound;
        }
    }

    internal static AudioClip HoverSound
    {
        get
        {
            if (_hoverSound == null) ResolveUiSounds();
            return _hoverSound;
        }
    }

    // HudManager does not exist at the main menu, so prefer its clips when the
    // instance is up and fall back to any button in the scene (options menus
    // have no hover clip, but the main menu's own buttons do).
    private static void ResolveUiSounds()
    {
        if (HudManager.InstanceExists && HudManager.Instance.MapButton)
        {
            _clickSound = HudManager.Instance.MapButton.ClickSound;
            _hoverSound = HudManager.Instance.MapButton.HoverSound;
        }

        if (_clickSound && _hoverSound) return;

        foreach (var button in UnityEngine.Object.FindObjectsByType<PassiveButton>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (_clickSound == null && button.ClickSound) _clickSound = button.ClickSound;
            if (_hoverSound == null && button.HoverSound) _hoverSound = button.HoverSound;
            if (_clickSound && _hoverSound) break;
        }
    }

    internal static void PlayUiSound(bool click)
    {
        var clip = click ? ClickSound : HoverSound;
        if (clip && Constants.ShouldPlaySfx() && SoundManager.Instance)
        {
            SoundManager.Instance.PlaySound(clip, false);
        }
    }

    /// <summary>
    /// Adds an <see cref="EventTrigger"/> that plays the game's click sound on
    /// pointer down and its hover sound on pointer enter (unless
    /// <paramref name="hoverSound"/> is false). Silently does nothing when no
    /// game clips could be found.
    /// </summary>
    internal static void AttachGameSounds(GameObject go, bool hoverSound = true)
    {
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        var press = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        press.callback.AddListener((UnityAction<BaseEventData>)(_ => PlayUiSound(true)));
        trigger.triggers.Add(press);

        // EventTrigger implements IScrollHandler, so event propagation stops at
        // the button and the mouse wheel never reaches the ScrollRect while
        // hovering it; forward the scroll explicitly
        var scroll = new EventTrigger.Entry { eventID = EventTriggerType.Scroll };
        scroll.callback.AddListener((UnityAction<BaseEventData>)(data =>
        {
            var pointer = data.TryCast<PointerEventData>();
            var scrollRect = go.GetComponentInParent<ScrollRect>();
            if (pointer != null && scrollRect != null) scrollRect.OnScroll(pointer);
        }));
        trigger.triggers.Add(scroll);

        if (hoverSound)
        {
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((UnityAction<BaseEventData>)(_ => PlayUiSound(false)));
            trigger.triggers.Add(enter);
        }
    }

    // Charcoal, white-outline pill button in the game's settings style (back/preview).
    internal static Button CreateButton(Transform panel, string label, Vector2 position)
    {
        var bg = CreateImage(panel, $"{label}Button", ToastSprites.ButtonBox, Color.white);
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
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(200, 200, 200, 255);
        colors.pressedColor = new Color32(150, 150, 150, 255);
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        return button;
    }

    // Il2CppInterop's Transform enumerator yields Il2CppSystem.Object wrappers
    // (foreach + cast throws); detach so layout groups don't draw pending-destroy
    // children for a frame.
    internal static void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.SetParent(null, false);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    internal static void EnsureEventSystem(GameObject host)
    {
        if (EventSystem.current == null && host.GetComponent<EventSystem>() == null)
        {
            host.AddComponent<EventSystem>();
            host.AddComponent<StandaloneInputModule>();
        }
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

    internal static string FormatUnlockTime(DateTime utc) => utc.ToLocalTime().ToString("g");

    internal static void Prewarm()
    {
        _ = AchievementIcon;
        _ = CloseButtonSprite;
        _ = ClickSound;
        _ = HoverSound;
    }

    // "BottomCenter" -> "Bottom Center"
    internal static string EnumDisplayName(Enum value)
        => System.Text.RegularExpressions.Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");

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
            1 => new Color32(104, 172, 212, 255),
            2 => new Color32(158, 112, 214, 255),
            3 => new Color32(214, 184, 92, 255),
            _ => RimColor
        };
    }

    // Progress-bar fills: the rarity color, but the teal AccentHover for
    // common so the fill does not sit gray-on-gray against the ring/track.
    internal static Color ProgressColor(int rarity) => rarity > 0 ? RarityColor(rarity) : (Color) AccentHover;

    // 0-100 completion percentage for a countable progress pair.
    internal static int Percent(int current, int required)
        => required > 0 ? Mathf.Clamp(Mathf.RoundToInt(100f * current / required), 0, 100) : 100;

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
