using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AchievementsAPI.API;
using AchievementsAPI.Patches.MainMenu;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Procedural Xbox-360 style achievement toast renderer. Shows one toast at a
/// time, FIFO, and survives scene loads.
/// </summary>
public static class XboxToast
{
    private const float PillHeight = 96f;
    private const float PillWidth = 560f;
    private const float CircleSize = PillHeight + 4f;
    private const float PillCollapsedWidth = 36f;
    private const float IconSize = 76f;
    private const float BottomOffset = 140f;
    private const float DismissThreshold = 140f;
    private const float DismissVelocity = 1500f;

    private static readonly Queue<AchievementToast.ToastRequest> _queue = new();
    private static GameObject? _canvasObject;
    private static Canvas? _canvas;
    private static bool _running;

    /// <summary>
    /// Enqueues a toast for display. An unlock request drops a pending progress
    /// request for the same achievement.
    /// </summary>
    public static void Enqueue(AchievementToast.ToastRequest request)
    {
        if (request.IsUnlock)
        {
            var kept = _queue.Where(x => x.AchievementId != request.AchievementId || x.IsUnlock).ToList();
            _queue.Clear();
            foreach (var toast in kept) _queue.Enqueue(toast);
        }

        _queue.Enqueue(request);

        if (!_running)
        {
            Coroutines.Start(CoRunQueue());
        }
    }

    private static IEnumerator CoRunQueue()
    {
        _running = true;
        while (_queue.Count > 0)
        {
            var request = _queue.Dequeue();
            var instance = CreateToast(request);
            yield return CoPlay(instance, request);
            instance.Root.gameObject.Destroy();
            yield return new WaitForSecondsRealtime(0.2f);
        }

        _running = false;
    }

    private static Canvas EnsureCanvas()
    {
        if (_canvas == null)
        {
            var existing = GameObject.Find("AchievementToastCanvas");
            _canvasObject = existing != null ? existing : new GameObject("AchievementToastCanvas");
            _canvas = _canvasObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = _canvasObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 32000;
            }

            if (_canvasObject.GetComponent<CanvasScaler>() == null)
            {
                var scaler = _canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (_canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                _canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (EventSystem.current == null && _canvasObject.GetComponent<EventSystem>() == null)
            {
                _canvasObject.AddComponent<EventSystem>();
                _canvasObject.AddComponent<StandaloneInputModule>();
            }

            _canvasObject.DontDestroyOnLoad();
        }

        return _canvas;
    }

    private static Image CreateImage(Transform parent, string name, Sprite? sprite, Color color, bool raycast = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var font = AchievementToast.ResolveFont();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        return tmp;
    }

    private sealed class ToastInstance
    {
        public RectTransform Root = null!;
        public RectTransform Pill = null!;
        public RectTransform Circle = null!;
        public Image Disc = null!;
        public GameObject? IconObj;
        public CanvasGroup Group = null!;
        public RectTransform TextRect = null!;
        public CanvasGroup TextGroup = null!;
        public float OffsetX;
        public float SlideX;
        public bool Dragging;
        public bool Dismissed;
        public float DismissDir;
        public float Velocity;
        public float SpringFrom;
        public float SpringT = 1f;
        public TextMeshProUGUI Title = null!;
        public TextMeshProUGUI Hint = null!;
        public bool Opened;
    }

    private static void ApplyPosition(ToastInstance i) => i.Root.anchoredPosition = new Vector2(i.OffsetX + i.SlideX, BottomOffset);

    private static ToastInstance CreateToast(AchievementToast.ToastRequest request)
    {
        var canvas = EnsureCanvas();
        var instance = new ToastInstance();

        var root = new GameObject("Toast");
        instance.Root = root.AddComponent<RectTransform>();
        root.transform.SetParent(canvas.transform, false);
        instance.Root.anchorMin = instance.Root.anchorMax = new Vector2(0.5f, 0f);
        instance.Root.pivot = new Vector2(0.5f, 0.5f);
        // Disc starts at screen center; the toast slides left as the pill expands
        instance.SlideX = PillWidth / 2f;
        ApplyPosition(instance);
        instance.Root.sizeDelta = new Vector2(PillWidth + CircleSize, PillHeight + 8);
        instance.Root.localScale = Vector3.one;
        instance.Group = root.AddComponent<CanvasGroup>();

        var rarity = MenuUi.RarityColor(request.Rarity);

        // Pill
        var pillGo = new GameObject("Pill");
        instance.Pill = pillGo.AddComponent<RectTransform>();
        pillGo.transform.SetParent(instance.Root, false);
        instance.Pill.anchorMin = instance.Pill.anchorMax = new Vector2(0.5f, 0.5f);
        instance.Pill.pivot = new Vector2(0f, 0.5f);
        instance.Pill.anchoredPosition = new Vector2(-PillWidth / 2f, 0f);
        instance.Pill.sizeDelta = new Vector2(PillCollapsedWidth, PillHeight);
        // Hidden behind the disc until the spin completes
        pillGo.SetActive(false);

        // Pill background: the achievement card frame; the disc carries the
        // rarity color
        var pillImg = CreateImage(instance.Pill, "Background", ToastSprites.Card, Color.white, true);
        pillImg.type = Image.Type.Sliced;
        var pillBgRect = pillImg.rectTransform;
        pillBgRect.anchorMin = Vector2.zero;
        pillBgRect.anchorMax = Vector2.one;
        pillBgRect.offsetMin = Vector2.zero;
        pillBgRect.offsetMax = Vector2.zero;

        // Clip rect so text never overlaps the disc while the pill expands
        var clip = new GameObject("Clip");
        var clipRect = clip.AddComponent<RectTransform>();
        clip.AddComponent<RectMask2D>();
        clip.transform.SetParent(instance.Pill, false);
        clipRect.anchorMin = Vector2.zero;
        clipRect.anchorMax = Vector2.one;
        clipRect.offsetMin = new Vector2(CircleSize / 2f + 6f, 0f);
        clipRect.offsetMax = new Vector2(-28f, 0f);

        // Text container, faded/slid in once the pill has expanded
        var textGo = new GameObject("Text");
        instance.TextRect = textGo.AddComponent<RectTransform>();
        textGo.transform.SetParent(clipRect, false);
        instance.TextRect.anchorMin = Vector2.zero;
        instance.TextRect.anchorMax = Vector2.one;
        instance.TextRect.pivot = new Vector2(0f, 0.5f);
        instance.TextRect.offsetMin = Vector2.zero;
        instance.TextRect.offsetMax = Vector2.zero;
        instance.TextGroup = textGo.AddComponent<CanvasGroup>();
        instance.TextGroup.alpha = 0f;

        var title = CreateText(instance.TextRect, "Title", request.Title, 22f, MenuUi.ValueColor);
        instance.Title = title;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.anchoredPosition = new Vector2(6f, 15f);
        titleRect.sizeDelta = new Vector2(PillWidth, 26f);

        // Alternates with the title while the toast is held, prompting how to
        // open the achievement in the menu
        var hint = CreateText(instance.TextRect, "Hint",
            HasController() ? "Hold Y to view" : "Middle-click to view",
            22f, MenuUi.ValueColor);
        instance.Hint = hint;
        hint.alpha = 0f;
        var hintRect = hint.rectTransform;
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(0f, 0.5f);
        hintRect.pivot = new Vector2(0f, 0.5f);
        hintRect.anchoredPosition = new Vector2(6f, 15f);
        hintRect.sizeDelta = new Vector2(PillWidth, 26f);

        var subtitle = CreateText(instance.TextRect, "Subtitle", request.Subtitle, 30f, Color.white, FontStyles.Bold);
        var subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = subtitleRect.anchorMax = new Vector2(0f, 0.5f);
        subtitleRect.pivot = new Vector2(0f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(6f, -16f);
        subtitleRect.sizeDelta = new Vector2(PillWidth, 38f);

        // Circle, centered on the pill's left end
        var circleGo = new GameObject("Circle");
        instance.Circle = circleGo.AddComponent<RectTransform>();
        circleGo.transform.SetParent(instance.Root, false);
        instance.Circle.anchorMin = instance.Circle.anchorMax = new Vector2(0.5f, 0.5f);
        instance.Circle.pivot = new Vector2(0.5f, 0.5f);
        instance.Circle.anchoredPosition = new Vector2(-PillWidth / 2f, 0f);
        instance.Circle.sizeDelta = new Vector2(CircleSize, CircleSize);
        instance.Circle.localScale = Vector3.zero;
        instance.Disc = circleGo.AddComponent<Image>();
        instance.Disc.sprite = ToastSprites.Circle;
        instance.Disc.color = rarity;
        instance.Disc.raycastTarget = false;
        var mask = circleGo.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        if (request.Icon != null)
        {
            var icon = CreateImage(instance.Circle, "Icon", ToastSprites.TrimTransparent(request.Icon), Color.white);
            icon.preserveAspect = true;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            instance.IconObj = icon.gameObject;
            instance.IconObj.SetActive(false);
        }

        AddSwipeHandlers(instance, request);
        return instance;
    }

    private static bool HasController() => Input.GetJoystickNames().Any(n => !string.IsNullOrEmpty(n));

    private static void Open(ToastInstance instance, AchievementToast.ToastRequest request)
    {
        if (instance.Opened) return;
        instance.Opened = true;
        instance.Dismissed = true;
        // Zero direction: the fly-off just fades in place
        instance.DismissDir = 0f;
        AchievementsMenuOpen.OpenMenu(request.AchievementId);
    }

    private static void AddSwipeHandlers(ToastInstance instance, AchievementToast.ToastRequest request)
    {
        var trigger = instance.Pill.gameObject.AddComponent<EventTrigger>();
        var scale = EnsureCanvas().scaleFactor;

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((UnityAction<BaseEventData>)(_ =>
        {
            instance.Dragging = true;
            instance.Velocity = 0f;
            instance.SpringT = 1f;
        }));
        trigger.triggers.Add(down);

        var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener((UnityAction<BaseEventData>)(data =>
        {
            var pointer = data.TryCast<PointerEventData>();
            if (pointer == null) return;
            var dx = pointer.delta.x / scale;
            instance.OffsetX += dx;
            if (Time.unscaledDeltaTime > 0f) instance.Velocity = dx / Time.unscaledDeltaTime;
            ApplyPosition(instance);
            instance.Group.alpha = Mathf.Max(1f - Mathf.Abs(instance.OffsetX) / 400f, 0.3f);
        }));
        trigger.triggers.Add(drag);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((UnityAction<BaseEventData>)(_ =>
        {
            instance.Dragging = false;
            if (Mathf.Abs(instance.OffsetX) > DismissThreshold || Mathf.Abs(instance.Velocity) > DismissVelocity)
            {
                instance.Dismissed = true;
                instance.DismissDir = instance.OffsetX != 0f ? Mathf.Sign(instance.OffsetX) : Mathf.Sign(instance.Velocity);
            }
            else
            {
                instance.SpringFrom = instance.OffsetX;
                instance.SpringT = 0f;
            }
        }));
        trigger.triggers.Add(up);

        var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        click.callback.AddListener((UnityAction<BaseEventData>)(data =>
        {
            var pointer = data.TryCast<PointerEventData>();
            if (pointer == null || pointer.button != PointerEventData.InputButton.Middle) return;
            Open(instance, request);
        }));
        trigger.triggers.Add(click);
    }

    private static float Smooth(float t) => t * t * (3f - 2f * t);

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

    private static float EaseOutBack(float t)
    {
        const float c = 1.70158f;
        return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
    }

    private static float EaseInCubic(float t) => t * t * t;

    private static float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    // Runs apply(k) with k in [0,1] eased by unscaled time; stops early when
    // instance is dismissed or destroyed (if given).
    private static IEnumerator CoTween(float duration, System.Action<float> apply, ToastInstance? abortOn = null)
    {
        bool Aborted() => abortOn != null && (abortOn.Dismissed || abortOn.Root == null);
        var t = 0f;
        while (t < duration && !Aborted())
        {
            t += Time.unscaledDeltaTime;
            apply(Mathf.Clamp01(t / duration));
            yield return null;
        }
        if (!Aborted()) apply(1f);
    }

    // Flips the disc 360° around Y; the icon appears (or, reversed, disappears)
    // at the halfway point.
    private static IEnumerator CoFlip(ToastInstance instance, bool reverse)
    {
        var iconToggled = false;
        yield return CoTween(0.45f, k =>
        {
            var angle = reverse ? 360f - k * 360f : k * 360f;
            instance.Circle.localEulerAngles = new Vector3(0f, angle, 0f);
            if (!iconToggled && k >= 0.5f && instance.IconObj != null)
            {
                iconToggled = true;
                instance.IconObj.SetActive(!reverse);
            }
        }, reverse ? null : instance);
        instance.Circle.localEulerAngles = Vector3.zero;
        if (!reverse && !iconToggled && instance.IconObj != null)
        {
            instance.IconObj.SetActive(true);
        }
    }

    private static IEnumerator CoPlay(ToastInstance instance, AchievementToast.ToastRequest request)
    {
        // Step 1: pop (circle scales in with overshoot); chime fires here
        if (request.IsUnlock) AchievementToast.PlayUnlockSound(request.Rarity);
        yield return CoTween(0.25f, k =>
        {
            var s = EaseOutBack(k);
            instance.Circle.localScale = new Vector3(s, s, 1f);
        }, instance);

        // Step 2: coin flip; the icon flips in at the halfway point. Pill stays
        // hidden behind the disc.
        yield return CoFlip(instance, reverse: false);

        // Step 3: pill expands from the circle outward while the toast slides
        // left so the pill ends centered; text fades/slides in once the pill
        // has mostly opened
        instance.Pill.gameObject.SetActive(true);
        yield return CoTween(0.4f, k =>
        {
            var elapsed = k * 0.4f;
            var e = EaseOutCubic(Mathf.Clamp01(elapsed / 0.35f));
            var w = Mathf.Lerp(PillCollapsedWidth, PillWidth, e);
            instance.Pill.sizeDelta = new Vector2(w, PillHeight);
            instance.SlideX = Mathf.Lerp(PillWidth / 2f, 0f, e);
            ApplyPosition(instance);
            var tt = Mathf.Clamp01((elapsed - 0.12f) / 0.25f);
            instance.TextGroup.alpha = tt;
            instance.TextRect.anchoredPosition = new Vector2(Mathf.Lerp(-28f, 0f, EaseOutCubic(tt)), 0f);
        }, instance);

        // Step 4: hold (paused while dragging; spring-back applied after
        // release). The title alternates with an "open in menu" hint, and
        // holding Y / middle-clicking opens it.
        var t = 0f;
        var holdT = 0f;
        instance.Title.alpha = 1f;
        var duration = request.Rarity > 0 ? AchievementsConfig.RareToastDuration.Value : AchievementsConfig.ToastDuration.Value;
        while (t < duration && !instance.Dismissed)
        {
            var dt = Time.unscaledDeltaTime;
            holdT = Input.GetKey(KeyCode.JoystickButton3) ? holdT + dt : 0f;
            if (holdT >= 0.6f) Open(instance, request);
            if (!instance.Dragging)
            {
                t += dt;
                if (instance.SpringT < 0.2f)
                {
                    instance.SpringT += dt;
                    instance.OffsetX = Mathf.Lerp(instance.SpringFrom, 0f, Smooth(Mathf.Clamp01(instance.SpringT / 0.2f)));
                    ApplyPosition(instance);
                    instance.Group.alpha = Mathf.Max(1f - Mathf.Abs(instance.OffsetX) / 400f, 0.3f);
                }

                var cycle = t % 4f;
                if (t < 2f)
                {
                    instance.Title.alpha = 1f;
                    instance.Hint.alpha = 0f;
                }
                else
                {
                    var showHint = cycle >= 2f;
                    var b = Mathf.Clamp01((cycle - (showHint ? 2f : 0f)) / 0.3f);
                    instance.Title.alpha = showHint ? 1f - b : b;
                    instance.Hint.alpha = showHint ? b : 1f - b;
                }
            }
            yield return null;
        }

        if (instance.Dismissed)
        {
            // Swiped away: fly off in that direction and finish early
            var startX = instance.OffsetX;
            var startAlpha = instance.Group.alpha;
            yield return CoTween(0.2f, k =>
            {
                instance.OffsetX = Mathf.Lerp(startX, instance.DismissDir * 1600f, k);
                ApplyPosition(instance);
                instance.Group.alpha = Mathf.Lerp(startAlpha, 0f, k);
            });
            yield break;
        }

        // Step 5: the intro in reverse — pill collapses into the disc while the
        // toast slides back to center and the text slides out, the disc flips
        // back, then shrinks away
        yield return CoTween(0.35f, k =>
        {
            var elapsed = k * 0.35f;
            var e = EaseInCubic(k);
            var w = Mathf.Lerp(PillWidth, PillCollapsedWidth, e);
            instance.Pill.sizeDelta = new Vector2(w, PillHeight);
            instance.SlideX = Mathf.Lerp(0f, PillWidth / 2f, e);
            ApplyPosition(instance);
            var tt = Mathf.Clamp01(elapsed / 0.2f);
            instance.TextGroup.alpha = 1f - tt;
            instance.TextRect.anchoredPosition = new Vector2(Mathf.Lerp(0f, -28f, EaseInCubic(tt)), 0f);
        });
        instance.Pill.gameObject.SetActive(false);

        // Disc alone for a beat
        yield return new WaitForSecondsRealtime(0.15f);

        yield return CoFlip(instance, reverse: true);

        // Mirror of step 1: the disc shrinks back to nothing
        yield return CoTween(0.25f, k =>
        {
            var s = Mathf.Max(1f - EaseInBack(k), 0f);
            instance.Circle.localScale = new Vector3(s, s, 1f);
        });
    }
}
