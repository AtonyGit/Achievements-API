using System.Reflection;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

public class Assets
{
    public static AssetBundle assetBundle { get; set; } = AssetBundleManager.Load("achievements");
    public static GameObject achievementPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("AchievementsMenu");
    
    public static Sprite StarSprite { get; set; } = assetBundle.LoadAsset<Sprite>("Star");
    
}