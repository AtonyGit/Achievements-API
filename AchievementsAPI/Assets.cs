using System.Reflection;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

public class Assets
{
    public static AssetBundle assetBundle { get; set; } = AssetBundleManager.Load("achievements");
    public static GameObject achievementPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("AchievementsMenu").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static GameObject achievementToastCanvasPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("ToastCanvas").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static GameObject achievementToastPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("Toast").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static Sprite StarSprite { get; set; } = assetBundle.LoadAsset<Sprite>("Star");
    
}