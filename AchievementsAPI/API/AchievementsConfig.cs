using BepInEx.Configuration;

namespace AchievementsAPI.API;

/// <summary>
/// Configuration for achievement toast notifications, backed by the plugin's BepInEx config file.
/// </summary>
public static class AchievementsConfig
{
    /// <summary>
    /// Whether achievement toasts are shown at all. Unlocking still happens when disabled.
    /// </summary>
    public static ConfigEntry<bool> ToastsEnabled { get; private set; } = null!;

    /// <summary>
    /// Seconds a toast stays fully visible on screen.
    /// </summary>
    public static ConfigEntry<float> ToastDuration { get; private set; } = null!;

    /// <summary>
    /// Seconds a rare (rarity above 0) toast stays fully visible on screen.
    /// </summary>
    public static ConfigEntry<float> RareToastDuration { get; private set; } = null!;

    /// <summary>
    /// Whether a sound plays when an achievement unlock toast appears.
    /// </summary>
    public static ConfigEntry<bool> ToastSoundEnabled { get; private set; } = null!;

    /// <summary>
    /// Volume of the achievement unlock sound.
    /// </summary>
    public static ConfigEntry<float> ToastVolume { get; private set; } = null!;

    internal static void Bind(ConfigFile config)
    {
        const string section = "Toasts";

        ToastsEnabled = config.Bind(section, nameof(ToastsEnabled), true,
            "Whether achievement toast popups are shown.");

        ToastDuration = config.Bind(section, nameof(ToastDuration), 4f,
            new ConfigDescription(
                "Seconds an achievement toast stays fully visible on screen.",
                new AcceptableValueRange<float>(1f, 10f)));

        RareToastDuration = config.Bind(section, nameof(RareToastDuration), 6f,
            new ConfigDescription(
                "Seconds a rare (rarity above 0) achievement toast stays fully visible on screen.",
                new AcceptableValueRange<float>(1f, 10f)));

        ToastSoundEnabled = config.Bind(section, nameof(ToastSoundEnabled), true,
            "Whether a sound plays when an achievement unlock toast appears.");

        ToastVolume = config.Bind(section, nameof(ToastVolume), 0.8f,
            new ConfigDescription(
                "Volume of the achievement unlock sound.",
                new AcceptableValueRange<float>(0f, 1f)));
    }
}
