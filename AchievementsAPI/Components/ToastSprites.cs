using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

/// <summary>
/// Generates and caches the procedural sprites used by the toast UI, since the
/// asset bundles cannot be edited.
/// </summary>
public static class ToastSprites
{
    private const int Size = 128;
    private const float CornerRadius = 48f;
    private const float CornerRadiusSmall = 14f;
    private const float CornerRadiusTiny = 6f;
    private const float SquircleRadius = 28f;

    private static Sprite? _circle;
    private static Sprite? _roundedRect;
    private static Sprite? _roundedRectSmall;
    private static Sprite? _roundedRectTiny;
    private static Sprite? _squircle;
    private static Sprite? _gear;

    /// <summary>
    /// A 128x128 anti-aliased filled circle.
    /// </summary>
    public static Sprite Circle => _circle ??= CreateCircle();

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 48) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRect => _roundedRect ??= CreateRoundedRect(CornerRadius, "AchToastRoundedRect");

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 14) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRectSmall => _roundedRectSmall ??= CreateRoundedRect(CornerRadiusSmall, "AchToastRoundedRectSmall");

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 6) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRectTiny => _roundedRectTiny ??= CreateRoundedRect(CornerRadiusTiny, "AchToastRoundedRectTiny");

    /// <summary>
    /// A 128x128 anti-aliased squircle (superellipse corners, radius 28) with a
    /// 9-slice border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite Squircle => _squircle ??= CreateRoundedRect(SquircleRadius, "AchToastSquircle", 4f);

    /// <summary>
    /// A 128x128 anti-aliased gear with 8 teeth and an 18px center hole.
    /// </summary>
    public static Sprite Gear => _gear ??= CreateGear();

    private static readonly Dictionary<int, Sprite> _trimmed = new();

    /// <summary>
    /// Returns a copy of <paramref name="sprite"/> cropped to its opaque bounds so icons
    /// with different transparent padding render at a consistent size. Falls back to the
    /// original sprite when the texture is not readable. Results are cached per sprite.
    /// </summary>
    public static Sprite TrimTransparent(Sprite sprite)
    {
        if (_trimmed.TryGetValue(sprite.GetInstanceID(), out var cached)) return cached;

        var result = sprite;
        try
        {
            var pixels = sprite.texture.GetPixels32();
            var texW = sprite.texture.width;
            var x0 = Mathf.RoundToInt(sprite.rect.x);
            var y0 = Mathf.RoundToInt(sprite.rect.y);
            var x1 = x0 + Mathf.RoundToInt(sprite.rect.width);
            var y1 = y0 + Mathf.RoundToInt(sprite.rect.height);

            var minX = x1; var minY = y1; var maxX = x0; var maxY = y0;
            var found = false;
            for (var y = y0; y < y1; y++)
            {
                for (var x = x0; x < x1; x++)
                {
                    if (pixels[y * texW + x].a <= 10) continue;
                    found = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (found && (minX > x0 || minY > y0 || maxX < x1 - 1 || maxY < y1 - 1))
            {
                var trimmed = Sprite.Create(sprite.texture,
                    new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
                    new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);
                trimmed.name = $"{sprite.name}_trimmed";
                trimmed.DontUnload();
                result = trimmed;
            }
        }
        catch (System.Exception)
        {
            // Texture not readable; keep the original sprite
        }

        _trimmed[sprite.GetInstanceID()] = result;
        return result;
    }

    private static Sprite CreateCircle()
    {
        var tex = CreateTexture((x, y) =>
        {
            var d = Mathf.Sqrt((x - 63.5f) * (x - 63.5f) + (y - 63.5f) * (y - 63.5f));
            return Mathf.Clamp01(64f - d);
        }, "AchToastCircle");

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateGear()
    {
        var tex = CreateTexture((x, y) =>
        {
            var dx = x - 63.5f;
            var dy = y - 63.5f;
            var d = Mathf.Sqrt(dx * dx + dy * dy);
            var angle = Mathf.Atan2(dy, dx);
            var tooth = 0.5f + 0.5f * Mathf.Cos(angle * 8f);
            var outer = Mathf.Lerp(44f, 56f, Mathf.Clamp01((tooth - 0.5f) * 6f));
            return Mathf.Clamp01(outer - d) * Mathf.Clamp01(d - 18f);
        }, "AchToastGear");

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateRoundedRect(float radius, string name, float exponent = 2f)
    {
        var tex = CreateTexture((x, y) =>
        {
            // Distance to the inner rect inset by the corner radius, measured
            // with a superellipse norm (exponent 2 = circle, higher = squarer)
            var px = Mathf.Clamp(x, radius, Size - radius);
            var py = Mathf.Clamp(y, radius, Size - radius);
            var d = Mathf.Pow(Mathf.Pow(Mathf.Abs(x - px), exponent) + Mathf.Pow(Mathf.Abs(y - py), exponent), 1f / exponent);
            return Mathf.Clamp01(radius - d);
        }, name);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.DontUnload();
        return sprite;
    }

    private static Texture2D CreateTexture(System.Func<int, int, float> alphaAt, string name)
    {
        var tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = name
        };

        var pixels = new Color[Size * Size];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                pixels[y * Size + x] = new Color(1f, 1f, 1f, alphaAt(x, y));
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
