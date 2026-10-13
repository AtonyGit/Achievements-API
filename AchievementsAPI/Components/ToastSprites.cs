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

    private static Sprite? _circle;
    private static Sprite? _roundedRect;

    /// <summary>
    /// A 128x128 anti-aliased filled circle.
    /// </summary>
    public static Sprite Circle => _circle ??= CreateCircle();

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 48) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRect => _roundedRect ??= CreateRoundedRect();

    private static Sprite CreateCircle()
    {
        var tex = CreateTexture((x, y) =>
        {
            var d = Mathf.Sqrt((x - 63.5f) * (x - 63.5f) + (y - 63.5f) * (y - 63.5f));
            return Mathf.Clamp01(64f - d);
        }, "AchToastCircle");

        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateRoundedRect()
    {
        var tex = CreateTexture((x, y) =>
        {
            // Signed distance to the inner rect inset by the corner radius
            var px = Mathf.Clamp(x, CornerRadius, Size - CornerRadius);
            var py = Mathf.Clamp(y, CornerRadius, Size - CornerRadius);
            var d = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
            return Mathf.Clamp01(CornerRadius - d);
        }, "AchToastRoundedRect");

        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(CornerRadius, CornerRadius, CornerRadius, CornerRadius));
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
