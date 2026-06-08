using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;

namespace AchievementsAPI.API;

public static class AchievementsManager
{
    public static List<AchievementsTab> Tabs = new();
    public static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (info, assembly, arg3) => InitializeForAssembly(assembly);
    }

    private static void InitializeForAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(AchievementsTab)) && !x.IsAbstract);
        foreach (var type in types)
        {
            Tabs.Add(Activator.CreateInstance(type) as AchievementsTab);
        }
    }
}