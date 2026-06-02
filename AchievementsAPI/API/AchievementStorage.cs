using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
namespace AchievementsAPI.API;

[Serializable]
public class AchievementData
{
    public string TabQualifiedName { get; set; }
    public string Name { get; set; }
    public bool Unlocked { get; set; }
    public int Progress { get; set; }
}


public class AchievementStorage
{
    public static List<AchievementData> BaseAchievements = new List<AchievementData>();
    public static string JsonPath => Path.Combine(Application.persistentDataPath, "AchievementsAPIData/achievements.json");


    public static void AchievementStorageUpdateBase(BaseAchievement achievement, bool unlocked)
    {
        foreach (var data in BaseAchievements.Where(x => x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName))
        {
            data.Unlocked = unlocked;
            return;
        }
        BaseAchievements.Add(new AchievementData { TabQualifiedName = achievement.GetType().AssemblyQualifiedName, Name = achievement.Name, Unlocked = unlocked, Progress = 0 });
    }

    public static void AchievementStorageUpdateCount(CountAchievement achievement, int progress, bool unlocked)
    {
        foreach (var data in BaseAchievements.Where(x => x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName))
        {
            data.Unlocked = unlocked;
            data.Progress = progress;
            return;
        }
        BaseAchievements.Add(new AchievementData { TabQualifiedName = achievement.GetType().AssemblyQualifiedName, Name = achievement.Name, Unlocked = unlocked, Progress = progress });
    }



    public static void AchievementStorageGet(AchievementsTab tab)
    {

        foreach (var propInfo in tab.GetType().GetProperties().Where(x =>
                                 x.PropertyType.IsSubclassOf(typeof(BaseAchievement)) ||
                                 x.PropertyType == typeof(BaseAchievement)))
        {
            var achievement = propInfo.GetValue(tab) as BaseAchievement;
            foreach (var data in BaseAchievements.Where(x => x.Name == achievement.Name))
            {
                achievement.Unlocked = data.Unlocked;
            }
        }

        foreach (var propInfo in tab.GetType().GetProperties().Where(x =>
                                 x.PropertyType.IsSubclassOf(typeof(CountAchievement)) ||
                                 x.PropertyType == typeof(CountAchievement)))
        {
            var achievement = propInfo.GetValue(tab) as CountAchievement;
            foreach (var data in BaseAchievements.Where(x => x.Name == achievement.Name))
            {
                achievement.CurrentValue = data.Progress;
                achievement.Unlocked = data.Unlocked;
            }
        }
    }

    public static void Save()
    {
        var il2cppList = new Il2CppSystem.Collections.Generic.List<AchievementData>();

        foreach (var item in BaseAchievements)
        {
            
            var ilItem = new AchievementData();
            ilItem.TabQualifiedName = item.TabQualifiedName;
            ilItem.Name = item.Name;
            ilItem.Unlocked = item.Unlocked;
            ilItem.Progress = item.Progress;
            il2cppList.Add(ilItem);
        }

        File.WriteAllText(JsonPath, JsonConvert.SerializeObject(il2cppList, Formatting.Indented));
    }

    public static void Load()
    {
        if (!File.Exists(JsonPath)) return;
        var json = File.ReadAllText(JsonPath);
        var data = JsonConvert.DeserializeObject<Il2CppSystem.Collections.Generic.List<AchievementData>>(json);
        BaseAchievements.Clear();
        foreach (var thing in data)
        {
            var otherthing = new AchievementData();
            otherthing.TabQualifiedName = thing.TabQualifiedName;
            otherthing.Name = thing.Name;
            otherthing.Unlocked = thing.Unlocked;
            otherthing.Progress = thing.Progress;
            BaseAchievements.Add(otherthing);
        }
    }
}