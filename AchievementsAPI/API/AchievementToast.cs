using UnityEngine;
using System.Collections;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Extensions;
using UnityEngine.UI;

namespace AchievementsAPI.API;

public class AchievementToast
{
    
    public static IEnumerator ShowAndDeleteToastBase(BaseAchievement achievement)
    {
        GameObject Canvas = GameObject.Find("ToastCanvas");
        if (Canvas == null)
        {
            var ToastCanvas = UnityEngine.Object.Instantiate(Assets.achievementToastCanvasPrefab);
            var Toast = ToastCanvas.transform.FindChild("Toast");
            Toast.FindChild("AchievementIcon").gameObject.GetComponent<Image>().sprite = SpriteTools.LoadSpriteFromPath(achievement.IconPath, achievement.Assembly, 100);;
            Toast.FindChild("AchievementName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = achievement.Name;
            Toast.FindChild("AchievementObtainedText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Achievement Obtained!";
            yield return new WaitForSeconds(5f);
            Toast.gameObject.Destroy();
        }
        else
        {
            var ToastGO = UnityEngine.Object.Instantiate(Assets.achievementToastPrefab);
            var Toast = ToastGO.transform;
            Toast.SetParent(Canvas.transform);
            foreach (Transform toast in Canvas.transform)
            {
                toast.position += new Vector3(0, -5f, 0);
            }
            Toast.FindChild("AchievementIcon").gameObject.GetComponent<Image>().sprite = SpriteTools.LoadSpriteFromPath(achievement.IconPath, achievement.Assembly, 100);;
            Toast.FindChild("AchievementName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = achievement.Name;
            Toast.FindChild("AchievementObtainedText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Achievement Obtained!";
            yield return new WaitForSeconds(5f);
            Toast.gameObject.Destroy();
        }
    }
    
    public static IEnumerator ShowAndDeleteToastCount(CountAchievement achievement, bool unlocked = false)
    {
        GameObject Canvas = GameObject.Find("ToastCanvas");
        if (Canvas == null)
        {
            var ToastCanvas = UnityEngine.Object.Instantiate(Assets.achievementToastCanvasPrefab);
            var Toast = ToastCanvas.transform.FindChild("Toast");
            Toast.FindChild("AchievementIcon").gameObject.GetComponent<Image>().sprite = SpriteTools.LoadSpriteFromPath(achievement.IconPath, achievement.Assembly, 100);;
            Toast.FindChild("AchievementName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = $"{achievement.Name} ({achievement.CurrentValue}/{achievement.RequiredValue})";
            Toast.FindChild("AchievementObtainedText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = (unlocked) ? "Achievement Obtained!" : "Achievement Progressed!";
            yield return new WaitForSeconds(5f);
            Toast.gameObject.Destroy();
        }
        else
        {
            var ToastGO = UnityEngine.Object.Instantiate(Assets.achievementToastPrefab);
            var Toast = ToastGO.transform;
            Toast.SetParent(Canvas.transform);
            foreach (Transform toast in Canvas.transform)
            {
                toast.position += new Vector3(0, -5f, 0);
            }
            Toast.FindChild("AchievementIcon").gameObject.GetComponent<Image>().sprite = SpriteTools.LoadSpriteFromPath(achievement.IconPath, achievement.Assembly, 100);;
            Toast.FindChild("AchievementName").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = achievement.Name;
            Toast.FindChild("AchievementObtainedText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = (unlocked) ? "Achievement Obtained!" : "Achievement Progressed!";
            yield return new WaitForSeconds(5f);
            Toast.gameObject.Destroy();
        }
    }
}