using HarmonyLib;

namespace AchievementsAPI.Patches;

// Our menu and toasts are uGUI overlays; the game's world-space PassiveButtons would
// otherwise still get clicks and hovers through them.
[HarmonyPatch(typeof(PassiveButton))]
public static class PassiveButtonPatches
{
    [HarmonyPatch(nameof(PassiveButton.ReceiveClickDown))]
    [HarmonyPatch(nameof(PassiveButton.ReceiveClickUp))]
    [HarmonyPatch(nameof(PassiveButton.ReceiveRepeatDown))]
    [HarmonyPatch(nameof(PassiveButton.ReceiveMouseOver))]
    [HarmonyPrefix]
    public static bool BlockWhileOverlayOpen() => !AchievementsMenu.IsOpen && !XboxToast.PointerOverToast;
}

// While our menu is up the game's own controller UI navigation must not move
// its selection or fire its confirm/cancel underneath it.
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
public static class ControllerManagerBlock
{
    [HarmonyPrefix]
    public static bool Prefix() => !AchievementsMenu.IsOpen;
}
