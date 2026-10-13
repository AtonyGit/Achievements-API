using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;

namespace AchievementsAPI.API;

public static class AchievementsManager
{
    public static List<AchievementsTab> Tabs = new();

    /// <summary>
    /// Registers an achievements tab. Does nothing if a tab of the same type is already
    /// registered (unless it is a <see cref="SoftAchievementsTab"/>).
    /// </summary>
    /// <param name="tab">The tab to register.</param>
    public static void RegisterTab(AchievementsTab tab)
    {
        if (tab is not SoftAchievementsTab && Tabs.Any(x => x.GetType() == tab.GetType())) return;
        Tabs.Add(tab);
    }

    /// <summary>
    /// Gets a registered tab by its display name.
    /// </summary>
    /// <param name="name">The <see cref="AchievementsTab.Name"/> of the tab.</param>
    /// <returns>The tab, or null if no tab with that name is registered.</returns>
    public static AchievementsTab? GetTab(string name)
    {
        return Tabs.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Finds a registered achievement by Id across all tabs.
    /// </summary>
    public static (AchievementsTab Tab, BaseAchievement Achievement)? FindAchievement(string id)
    {
        foreach (var tab in Tabs)
        {
            var achievement = tab.GetAchievements().FirstOrDefault(x => x.Id == id);
            if (achievement != null) return (tab, achievement);
        }
        return null;
    }

    public static void Initialize()
    {
        RegisterTab(new ExampleTab());
        IL2CPPChainloader.Instance.PluginLoad += (info, assembly, arg3) => InitializeForAssembly(assembly);
    }

    private static void InitializeForAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(AchievementsTab)) && !x.IsAbstract);
        foreach (var type in types)
        {
            RegisterTab(Activator.CreateInstance(type) as AchievementsTab);
        }
    }
}