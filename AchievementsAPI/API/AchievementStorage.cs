using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using UnityEngine;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
    public static string JsonPath => OperatingSystem.IsAndroid() ? Environment.GetEnvironmentVariable("SL_DATA_PATH") : Path.Combine(Application.persistentDataPath, "AchievementsAPIData/achievements.json");
    
    public static void AchievementStorageUpdate(BaseAchievement achievement, bool unlocked)
    {
        var data = GetData(achievement);
        data.Unlocked = unlocked;

        Save();
    }

    public static void AchievementStorageUpdate(CountAchievement achievement, int progress, bool unlocked)
    {
        var data = GetData(achievement);
        data.Unlocked = unlocked;
        if (achievement.ProgressPersists) data.Progress = progress;

        Save();
    }

    public static AchievementData GetData(BaseAchievement achievement)
    {
        var data =  BaseAchievements.Find(x =>
            x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName);
        if (data == null)
        {
            data = new AchievementData { TabQualifiedName = achievement.GetType().AssemblyQualifiedName,  Name = achievement.Name, Unlocked = false };
            BaseAchievements.Add(data);
        }
        return data;
    }
    
    public static AchievementData GetData(CountAchievement achievement)
    {
        var data =  BaseAchievements.Find(x =>
            x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName);
        if (data == null)
        {
            data = new AchievementData { TabQualifiedName = achievement.GetType().AssemblyQualifiedName,  Name = achievement.Name, Unlocked = false, Progress = achievement.CurrentValue };
            BaseAchievements.Add(data);
        }
        return data;
    }

    public static void AchievementStorageGet(AchievementsTab tab)
    {

        foreach (var propInfo in tab.GetType().GetProperties().Where(x =>
                                 x.PropertyType == typeof(BaseAchievement)))
        {
            var achievement = propInfo.GetValue(tab) as BaseAchievement;
            if (achievement == null) return;
            var data =  BaseAchievements.Find(x =>
                x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName);
            if (data == null) return;
            achievement.Unlocked = data.Unlocked;
        }

        foreach (var propInfo in tab.GetType().GetProperties().Where(x =>
                                 x.PropertyType == typeof(CountAchievement)))
        {
            var achievement = propInfo.GetValue(tab) as CountAchievement;
            if (achievement == null) return;
            var data =  BaseAchievements.Find(x =>
                x.Name == achievement.Name && x.TabQualifiedName == achievement.GetType().AssemblyQualifiedName);
            if (data == null) return;
            achievement.CurrentValue = data.Progress;
            achievement.Unlocked = data.Unlocked;
        }
    }

    public static void Save()
    {

        var directory = Path.GetDirectoryName(JsonPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(
            JsonPath,
            JsonSerializer.Serialize(
                BaseAchievements,
                new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(JsonPath, JsonSerializer.Serialize(BaseAchievements, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Load()
    {
        if (!File.Exists(JsonPath))
        {
            BaseAchievements = new List<AchievementData>();
            return;
        }

        var json = File.ReadAllText(JsonPath);

        if (string.IsNullOrWhiteSpace(json))
        {
            BaseAchievements = new List<AchievementData>();
            return;
        }

        BaseAchievements =
            JsonSerializer.Deserialize<List<AchievementData>>(json)
            ?? new List<AchievementData>();
    }
}