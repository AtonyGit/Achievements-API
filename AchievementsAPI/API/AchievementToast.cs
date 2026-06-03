using UnityEngine;
using System.Collections;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Rewired.Utils;
using UnityEngine.UI;

namespace AchievementsAPI.API;
public class AchievementToast
{
    private static Transform currentToast;

    public static void ShowAndDeleteToast(BaseAchievement achievement)
    {
        Coroutines.Start(CoShowAndDeleteToast(achievement));
    }

    public static void ShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        Coroutines.Start(CoShowAndDeleteToast(achievement, unlocked));
    }

    private static Transform GetOrCreateToast()
    {
        GameObject canvas = GameObject.Find("ToastCanvas");
        if (canvas == null)
        {
            var toastCanvas = UnityEngine.Object.Instantiate(Assets.achievementToastCanvasPrefab);
            return toastCanvas.transform.FindChild("Toast");
        }
        else
        {
            var toastGO = UnityEngine.Object.Instantiate(Assets.achievementToastPrefab);
            var toast = toastGO.transform;
            toast.SetParent(canvas.transform);
            foreach (Transform t in canvas.transform)
            {
                t.position += new Vector3(0, -5f, 0);
            }
            return toast;
        }
    }

    private static void PopulateToast(Transform toast, string iconPath, System.Reflection.Assembly assembly, string title, string subtitle)
    {
        toast.FindChild("AchievementIcon").gameObject.GetComponent<Image>().sprite =
            SpriteTools.LoadSpriteFromPath(iconPath, assembly, 100);
        toast.FindChild("AchievementName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = title;
        toast.FindChild("AchievementObtainedText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = subtitle;
    }

    private static IEnumerator CoAnimateAndDestroyToast()
    {
        Vector3 onScreenPos = currentToast.localPosition;
        Vector3 offScreenRight = onScreenPos + new Vector3(1500, 0, 0);
        
        yield return TransitionFade.Instance.StartCoroutine(
            Effects.Slide2D(currentToast, offScreenRight, onScreenPos, 0.7f));

        float time = 0;
        while (time <= 3)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        yield return TransitionFade.Instance.StartCoroutine(
            Effects.Slide2D(currentToast, onScreenPos, offScreenRight, 0.3f));

        currentToast.gameObject.Destroy();
        yield break;
    }

    public static IEnumerator CoShowAndDeleteToast(BaseAchievement achievement)
    {
        while (!currentToast.IsNullOrDestroyed())
        {
            yield return null;
        }

        currentToast = GetOrCreateToast();
        PopulateToast(currentToast, achievement.IconPath, achievement.Assembly,
            title: "Achievement Obtained!",
            subtitle: achievement.Name);

        yield return Coroutines.Start(CoAnimateAndDestroyToast());
    }

    public static IEnumerator CoShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        while (!currentToast.IsNullOrDestroyed())
        {
            yield return null;
        }

        currentToast = GetOrCreateToast();
        PopulateToast(currentToast, achievement.IconPath, achievement.Assembly,
            title: $"{achievement.Name} ({achievement.CurrentValue}/{achievement.RequiredValue})",
            subtitle: unlocked ? "Achievement Obtained!" : "Achievement Progressed!");

        yield return Coroutines.Start(CoAnimateAndDestroyToast());
    }
}