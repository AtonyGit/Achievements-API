using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

namespace AchievementsAPI.API;

/// <summary>
/// Entry point for showing achievement toasts. Callers build an immutable
/// <see cref="ToastRequest"/> snapshot which is rendered by <see cref="XboxToast"/>.
/// </summary>
public class AchievementToast
{
    /// <summary>
    /// An immutable snapshot of a toast's contents, captured at enqueue time so
    /// deferred toasts display the values they were earned with.
    /// </summary>
    public sealed record ToastRequest(string Title, string Subtitle, Sprite? Icon, int Rarity, bool IsUnlock, Sprite? BgSprite, string AchievementId);

    public static void ShowAndDeleteToast(BaseAchievement achievement)
    {
        Show(new ToastRequest("Achievement Unlocked!", achievement.Name, achievement.Icon,
            achievement.Rarity, true, achievement.ToastBgSprite, achievement.Id));
    }

    public static void ShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        string title;
        string subtitle;
        if (achievement.Hidden && achievement.HideProgress && !unlocked)
        {
            title = "Achievement Progressed!";
            subtitle = "Hidden Achievement";
        }
        else
        {
            title = unlocked ? "Achievement Unlocked!" : "Achievement Progressed!";
            subtitle = $"{achievement.Name} ({achievement.CurrentValue}/{achievement.RequiredValue})";
        }

        Show(new ToastRequest(title, subtitle, achievement.Icon,
            achievement.Rarity, unlocked, achievement.ToastBgSprite, achievement.Id));
    }

    public static IEnumerator CoShowAndDeleteToast(BaseAchievement achievement)
    {
        ShowAndDeleteToast(achievement);
        yield break;
    }

    public static IEnumerator CoShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        ShowAndDeleteToast(achievement, unlocked);
        yield break;
    }

    /// <summary>
    /// Enqueues a toast request for display.
    /// </summary>
    public static void Show(ToastRequest request)
    {
        XboxToast.Enqueue(request);
    }

    /// <summary>
    /// Shows a preview toast, used by the settings panel. Uses the ExampleTab's
    /// first achievement when available, otherwise a hardcoded preview.
    /// </summary>
    public static void ShowPreview()
    {
        var achievement = AchievementsManager.GetTab("Example Tab")?.GetAchievements().FirstOrDefault();
        if (achievement != null)
        {
            ShowAndDeleteToast(achievement);
            return;
        }

        Show(new ToastRequest("Achievement Unlocked!", "Preview Achievement",
            SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", typeof(AchievementToast).Assembly, 100),
            0, true, null, "preview"));
    }

    /// <summary>
    /// Plays the unlock chime, if enabled in the config.
    /// </summary>
    public static void PlayUnlockSound()
    {
        if (!AchievementsConfig.ToastSoundEnabled.Value) return;
        var clip = Assets.LockChime;
        if (clip == null || SoundManager.Instance == null) return;
        SoundManager.Instance.PlaySound(clip, false, AchievementsConfig.ToastVolume.Value);
    }

    private static TMP_FontAsset? _font;

    /// <summary>
    /// Resolves the TMP font used by toasts: the font from the bundle toast prefab,
    /// falling back to <see cref="TMP_Settings.defaultFontAsset"/>.
    /// </summary>
    public static TMP_FontAsset? ResolveFont()
    {
        if (_font != null) return _font;
        var tmp = Assets.achievementToastPrefab != null
            ? Assets.achievementToastPrefab.transform.Find("AchievementName")?.GetComponent<TextMeshProUGUI>()
            : null;
        _font = tmp != null && tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
        return _font;
    }
}
