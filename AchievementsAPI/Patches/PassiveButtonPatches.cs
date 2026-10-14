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
