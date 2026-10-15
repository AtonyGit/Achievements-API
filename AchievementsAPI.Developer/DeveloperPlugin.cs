using AchievementsAPI;
using AchievementsAPI.API;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace AchievementsAPI.Developer;

/// Marks the local player as a developer. Build one of these per developer (change DeveloperName) and
/// only give the DLL to that developer; nobody else needs it (ModFlags.None).
[BepInAutoPlugin("achievementsapi.developer", "Achievements API Developer")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(AchievementsAPIPlugin.Id)]
[ReactorModFlags(ModFlags.None)]
public partial class DeveloperPlugin : BasePlugin
{
    private const string DeveloperName = "satire";

    public override void Load() => DeveloperPresence.RegisterLocalDeveloper(DeveloperName);
}
