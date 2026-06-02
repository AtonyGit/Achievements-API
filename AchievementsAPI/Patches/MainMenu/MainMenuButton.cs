// here too, this wouldn't have existed without Pix!


using System;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMono.Unity;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = Il2CppSystem.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace AchievementsAPI.MainMenu;

public static class MainMenuButtons
{
    public static PassiveButton button;

    public static void SetUp(MainMenuManager menu)
    {
        var sprit = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", Assembly.GetCallingAssembly(), 100);
        var sprit2 = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIconSecond.png", Assembly.GetCallingAssembly(), 100);
        button = PassiveButtonStuff.CreatePassiveButton("AchievementsMenu", sprit2, sprit, new Vector2(1f, 1f), () => AchievementsMenuOpen.OpenMenu(menu));
        button.transform.SetParent(menu.transform.FindChild("MainUI/AspectScaler"));
        button.transform.localPosition = new Vector3(5.6f, 1.55f, 0);
    }
    
    
}