using System.Linq;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace AchievementsAPI.API;

/// <summary>
/// Lets a developer-only plugin announce to the lobby that the local player is a developer,
/// which unlocks DeveloperTab.PlayedWithDeveloper for everyone else.
/// </summary>
public static class DeveloperPresence
{
    public static string? LocalDeveloperName { get; private set; }

    private static bool _developerInMatch;

    /// <summary>
    /// Call once from a developer-only plugin's Load(). The API then announces it to every lobby/game automatically.
    /// </summary>
    public static void RegisterLocalDeveloper(string name) { LocalDeveloperName = name; }

    internal static void Announce()
    {
        if (string.IsNullOrEmpty(LocalDeveloperName)) return;
        if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected) return;
        Rpc<AnnounceDeveloperRpc>.Instance.Send(PlayerControl.LocalPlayer, LocalDeveloperName);
    }

    internal static void OnDeveloperAnnounced(PlayerControl sender, string name)
    {
        if (sender == null || sender.AmOwner) return;
        AchievementsAPIPlugin.Instance.Log.LogInfo($"Developer '{name}' is in this game.");
        _developerInMatch = true;
    }

    internal static void ResetMatch() => _developerInMatch = false;

    internal static void OnMatchEnded()
    {
        if (!_developerInMatch) return;
        AchievementsManager.Tabs.OfType<DeveloperTab>().FirstOrDefault()?.PlayedWithDeveloper.Unlock();
        _developerInMatch = false;
    }
}

[RegisterCustomRpc(0)]
internal sealed class AnnounceDeveloperRpc(AchievementsAPIPlugin plugin, uint id)
    : PlayerCustomRpc<AchievementsAPIPlugin, string>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, string? data) => writer.Write(data ?? string.Empty);

    public override string Read(MessageReader reader) => reader.ReadString();

    public override void Handle(PlayerControl innerNetObject, string? data) =>
        DeveloperPresence.OnDeveloperAnnounced(innerNetObject, data ?? string.Empty);
}
