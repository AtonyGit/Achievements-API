using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AchievementsAPI.API;
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
            yield return Coroutines.Start(CoPlay(instance, request));
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

    private static Color RarityColor(int rarity)
    {
        return rarity switch
        {
            1 => new Color32(112, 208, 255, 255),
            2 => new Color32(187, 88, 255, 255),
            3 => new Color32(255, 226, 64, 255),
            _ => new Color32(16, 124, 16, 255)
        };
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
        public bool Dragging;
        public bool Dismissed;
        public float DismissDir;
        public float Velocity;
        public float SpringFrom;
        public float SpringT = 1f;
    }

    private static ToastInstance CreateToast(AchievementToast.ToastRequest request)
    {
        var canvas = EnsureCanvas();
        var instance = new ToastInstance();

        var root = new GameObject("Toast");
        instance.Root = root.AddComponent<RectTransform>();
        root.transform.SetParent(canvas.transform, false);
        instance.Root.anchorMin = instance.Root.anchorMax = new Vector2(0.5f, 0f);
        instance.Root.pivot = new Vector2(0.5f, 0.5f);
        instance.Root.anchoredPosition = new Vector2(0, BottomOffset);
        instance.Root.sizeDelta = new Vector2(PillWidth + CircleSize, PillHeight + 8);
        instance.Root.localScale = Vector3.one;
        instance.Group = root.AddComponent<CanvasGroup>();

        var rarity = RarityColor(request.Rarity);

        // Pill
        var pillGo = new GameObject("Pill");
        instance.Pill = pillGo.AddComponent<RectTransform>();
        pillGo.transform.SetParent(instance.Root, false);
        instance.Pill.anchorMin = instance.Pill.anchorMax = new Vector2(0.5f, 0.5f);
        instance.Pill.pivot = new Vector2(0f, 0.5f);
        instance.Pill.anchoredPosition = new Vector2(-PillWidth / 2f, 0f);
        instance.Pill.sizeDelta = new Vector2(CircleSize / 2f, PillHeight);
        // Hidden behind the disc until the spin completes
        pillGo.SetActive(false);

        // Rarity outline frame, 3px proud of the pill on every side. Created
        // first so it renders under the pill background; stretches with it.
        var frame = CreateImage(instance.Pill, "Frame", ToastSprites.RoundedRect, rarity);
        frame.type = Image.Type.Sliced;
        var frameRect = frame.rectTransform;
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-3f, -3f);
        frameRect.offsetMax = new Vector2(3f, 3f);

        // Pill background, drawn over the frame so only the 3px edge shows
        var pillImg = CreateImage(instance.Pill, "Background", null, Color.white, true);
        var pillBgRect = pillImg.rectTransform;
        pillBgRect.anchorMin = Vector2.zero;
        pillBgRect.anchorMax = Vector2.one;
        pillBgRect.offsetMin = Vector2.zero;
        pillBgRect.offsetMax = Vector2.zero;
        pillImg.sprite = ToastSprites.RoundedRect;
        pillImg.type = Image.Type.Sliced;
        pillImg.color = new Color32(30, 30, 30, 242);

        // Clip rect so text never overlaps the disc while the pill expands
        var clip = new GameObject("Clip");
        var clipRect = clip.AddComponent<RectTransform>();
        clip.AddComponent<RectMask2D>();
        clip.transform.SetParent(instance.Pill, false);
        clipRect.anchorMin = Vector2.zero;
        clipRect.anchorMax = Vector2.one;
        clipRect.offsetMin = new Vector2(CircleSize / 2f + 6f, 0f);
        clipRect.offsetMax = new Vector2(-16f, 0f);

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

        var title = CreateText(instance.TextRect, "Title", request.Title, 22f, new Color32(176, 176, 176, 255));
        var titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.anchoredPosition = new Vector2(6f, 15f);
        titleRect.sizeDelta = new Vector2(PillWidth, 26f);

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
            var icon = CreateImage(instance.Circle, "Icon", request.Icon, Color.white);
            icon.preserveAspect = true;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(44f, 44f);
            instance.IconObj = icon.gameObject;
            instance.IconObj.SetActive(false);
        }

        AddSwipeHandlers(instance);
        return instance;
    }

    private static void AddSwipeHandlers(ToastInstance instance)
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
            instance.Root.anchoredPosition = new Vector2(instance.OffsetX, BottomOffset);
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
    }

    private static float Smooth(float t) => t * t * (3f - 2f * t);

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

    private static float EaseOutBack(float t)
    {
        const float c = 1.70158f;
        return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
    }

    // Runs apply(k) with k in [0,1] eased by unscaled time; stops early when
    // instance is dismissed (if given).
    private static IEnumerator CoTween(float duration, System.Action<float> apply, ToastInstance? abortOn = null)
    {
        var t = 0f;
        while (t < duration && !(abortOn?.Dismissed ?? false))
        {
            t += Time.unscaledDeltaTime;
            apply(Mathf.Clamp01(t / duration));
            yield return null;
        }
        if (!(abortOn?.Dismissed ?? false)) apply(1f);
    }

    private static IEnumerator CoPlay(ToastInstance instance, AchievementToast.ToastRequest request)
    {
        // Step 1: pop (circle scales in with overshoot); chime fires here
        if (request.IsUnlock) AchievementToast.PlayUnlockSound(request.Rarity);
        yield return Coroutines.Start(CoTween(0.25f, k =>
        {
            var s = EaseOutBack(k);
            instance.Circle.localScale = new Vector3(s, s, 1f);
        }, instance));

        // Step 2: coin flip; the icon flips in at the halfway point. Pill stays
        // hidden behind the disc.
        var iconShown = false;
        yield return Coroutines.Start(CoTween(0.45f, k =>
        {
            var angle = k * 360f;
            instance.Circle.localEulerAngles = new Vector3(0f, angle, 0f);
            if (!iconShown && angle >= 180f && instance.IconObj != null)
            {
                iconShown = true;
                instance.IconObj.SetActive(true);
            }
        }, instance));
        instance.Circle.localEulerAngles = Vector3.zero;
        if (!iconShown && instance.IconObj != null)
        {
            instance.IconObj.SetActive(true);
        }

        // Step 3: pill expands from the circle outward; text fades/slides in
        // once the pill has mostly opened
        instance.Pill.gameObject.SetActive(true);
        yield return Coroutines.Start(CoTween(0.4f, k =>
        {
            var elapsed = k * 0.4f;
            var w = Mathf.Lerp(CircleSize / 2f, PillWidth, EaseOutCubic(Mathf.Clamp01(elapsed / 0.35f)));
            instance.Pill.sizeDelta = new Vector2(w, PillHeight);
            var tt = Mathf.Clamp01((elapsed - 0.12f) / 0.25f);
            instance.TextGroup.alpha = tt;
            instance.TextRect.anchoredPosition = new Vector2(Mathf.Lerp(-28f, 0f, EaseOutCubic(tt)), 0f);
        }, instance));

        // Step 4: hold (paused while dragging; spring-back applied after release)
        var t = 0f;
        var duration = request.Rarity > 0 ? AchievementsConfig.RareToastDuration.Value : AchievementsConfig.ToastDuration.Value;
        while (t < duration && !instance.Dismissed)
        {
            var dt = Time.unscaledDeltaTime;
            if (!instance.Dragging)
            {
                t += dt;
                if (instance.SpringT < 0.2f)
                {
                    instance.SpringT += dt;
                    instance.OffsetX = Mathf.Lerp(instance.SpringFrom, 0f, Smooth(Mathf.Clamp01(instance.SpringT / 0.2f)));
                    instance.Root.anchoredPosition = new Vector2(instance.OffsetX, BottomOffset);
                    instance.Group.alpha = Mathf.Max(1f - Mathf.Abs(instance.OffsetX) / 400f, 0.3f);
                }
            }
            yield return null;
        }

        if (instance.Dismissed)
        {
            // Swiped away: fly off in that direction and finish early
            var startX = instance.OffsetX;
            var startAlpha = instance.Group.alpha;
            yield return Coroutines.Start(CoTween(0.2f, k =>
            {
                instance.Root.anchoredPosition = new Vector2(Mathf.Lerp(startX, instance.DismissDir * 1600f, k), BottomOffset);
                instance.Group.alpha = Mathf.Lerp(startAlpha, 0f, k);
            }));
            yield break;
        }

        // Step 5: pill collapses back into the disc while the text snaps out,
        // then the whole toast scales out as one
        yield return Coroutines.Start(CoTween(0.3f, k =>
        {
            var elapsed = k * 0.3f;
            var w = Mathf.Lerp(PillWidth, CircleSize / 2f, EaseOutCubic(k));
            instance.Pill.sizeDelta = new Vector2(w, PillHeight);
            instance.TextGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.12f);
        }));
        instance.Pill.gameObject.SetActive(false);

        yield return Coroutines.Start(CoTween(0.2f, k =>
        {
            var s = 1f - Smooth(k);
            instance.Root.localScale = new Vector3(s, s, 1f);
        }));
    }
}
