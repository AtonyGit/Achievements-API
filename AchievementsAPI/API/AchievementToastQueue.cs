using System.Collections.Generic;
using System.Linq;

namespace AchievementsAPI.API;

/// <summary>
/// Defines when an achievement toast is shown relative to game events.
/// </summary>
public enum AchToastTiming
{
    /// <summary>
    /// The toast is shown as soon as the achievement is unlocked or progresses.
    /// </summary>
    Immediate,

    /// <summary>
    /// The toast is deferred until a new round starts.
    /// </summary>
    OnRoundStart,

    /// <summary>
    /// The toast is deferred until a meeting is called.
    /// </summary>
    OnMeetingStart,

    /// <summary>
    /// The toast is deferred until the meeting UI closes.
    /// </summary>
    OnMeetingEnd,

    /// <summary>
    /// The toast is deferred until the game ends.
    /// </summary>
    OnGameEnd
}

/// <summary>
/// Queues achievement toasts so they can be deferred to a specific point in the game loop.
/// </summary>
public static class AchievementToastQueue
{
    private static readonly Dictionary<AchToastTiming, List<QueuedToast>> _queues = new();

    private readonly struct QueuedToast
    {
        public readonly BaseAchievement Achievement;
        public readonly bool Unlocked;

        public QueuedToast(BaseAchievement achievement, bool unlocked)
        {
            Achievement = achievement;
            Unlocked = unlocked;
        }
    }

    /// <summary>
    /// Enqueues a toast for an achievement. If the achievement's <see cref="BaseAchievement.ToastTiming"/>
    /// is <see cref="AchToastTiming.Immediate"/> the toast is shown right away, otherwise it is deferred
    /// until the matching timing is flushed. A pending toast for the same achievement is replaced,
    /// so repeated progress ticks collapse into a single toast with the final value.
    /// </summary>
    /// <param name="achievement">The achievement the toast is for.</param>
    /// <param name="unlocked">Whether this is an unlock toast; false means a progress toast.</param>
    public static void Enqueue(BaseAchievement achievement, bool unlocked)
    {
        if (!AchievementsConfig.ToastsEnabled.Value) return;

        if (achievement.ToastTiming == AchToastTiming.Immediate)
        {
            Show(achievement, unlocked);
            return;
        }

        var bucket = GetBucket(achievement.ToastTiming);
        bucket.RemoveAll(x => x.Achievement.Id == achievement.Id);
        bucket.Add(new QueuedToast(achievement, unlocked));
    }

    /// <summary>
    /// Removes any pending toast for the given achievement from every bucket.
    /// </summary>
    /// <param name="achievement">The achievement whose queued toasts should be dropped.</param>
    public static void Dequeue(BaseAchievement achievement)
    {
        foreach (var bucket in _queues.Values)
        {
            bucket.RemoveAll(x => x.Achievement.Id == achievement.Id);
        }
    }

    /// <summary>
    /// Shows all toasts pending for the given timing, in order.
    /// </summary>
    public static void Flush(AchToastTiming timing)
    {
        var bucket = GetBucket(timing);
        foreach (var toast in bucket)
        {
            Show(toast.Achievement, toast.Unlocked);
        }

        bucket.Clear();
    }

    /// <summary>
    /// Shows all pending toasts for every timing. Called when the game ends so nothing is lost.
    /// </summary>
    public static void FlushAll()
    {
        foreach (var timing in _queues.Keys.ToArray())
        {
            Flush(timing);
        }
    }

    private static List<QueuedToast> GetBucket(AchToastTiming timing)
    {
        if (!_queues.TryGetValue(timing, out var bucket))
        {
            bucket = new List<QueuedToast>();
            _queues[timing] = bucket;
        }

        return bucket;
    }

    private static void Show(BaseAchievement achievement, bool unlocked)
    {
        if (achievement is CountAchievement countAchievement)
        {
            AchievementToast.ShowAndDeleteToast(countAchievement, unlocked);
        }
        else
        {
            AchievementToast.ShowAndDeleteToast(achievement);
        }
    }
}
