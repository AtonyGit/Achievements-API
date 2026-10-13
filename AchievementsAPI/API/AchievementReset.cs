namespace AchievementsAPI.API;

/// <summary>
/// Defines when an achievement is reset back to its locked/unprogressed state.
/// </summary>
public enum AchResetTrigger
{
    /// <summary>
    /// The achievement is never automatically reset.
    /// </summary>
    Never,

    /// <summary>
    /// The achievement is reset when the game ends or the player leaves the match.
    /// </summary>
    OnGameEnd,

    /// <summary>
    /// The achievement is reset when a meeting is called.
    /// </summary>
    OnMeetingStart,

    /// <summary>
    /// The achievement is reset when the meeting UI closes.
    /// </summary>
    OnMeetingEnd,

    /// <summary>
    /// The achievement is reset when a new round starts.
    /// </summary>
    OnRoundStart
}
