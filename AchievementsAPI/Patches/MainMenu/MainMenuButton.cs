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
using Action = Il2CppSystem.Action;
using Debug = Il2CppSystem.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace AchievementsAPI.MainMenu;

public static class MainMenuButtons
{
    public static PassiveButton button;

    public static void SetUp(MainMenuManager menu)
    {
        var sprit = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.AchievementsButtonActive.png", Assembly.GetCallingAssembly(), 100);
        var sprit2 = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.AchievementsButton.png", Assembly.GetCallingAssembly(), 100);
        button = PassiveButtonStuff.CreatePassiveButton("AchievementsMenuButton", sprit2, sprit, new Vector2(1f, 1f), () => AchievementsMenuOpen.OpenMenu(menu));
        menu.StartCoroutine(Effects.ActionAfterDelay(0.1f, new System.Action(() =>
        {
            button.activeSprites.GetComponent<SpriteRenderer>().color = Color.white;
            button.inactiveSprites.GetComponent<SpriteRenderer>().color = Color.white;
        })));
        button.transform.SetParent(menu.transform.FindChild("MainUI/AspectScaler"));
        button.transform.localPosition = new Vector3(5.6f, 1.55f, 5);
    }
}