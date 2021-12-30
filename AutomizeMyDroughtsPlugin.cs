using AutomizeMyDroughts.Components;
using AutomizeMyDroughts.Events;
using AutomizeMyDroughts.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberbornAPI;

namespace AutomizeMyDroughts
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.timberapi.timberapi")]
    [HarmonyPatch]
    public class AutomizeMyDroughtsPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource L;
        public void Awake() {
            L = Logger;

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            TimberAPI.DependencyRegistry.AddConfigurator(new AutomizationManager.Configurator());

            // Register event listener
            TimberAPI.DependencyRegistry.AddConfigurator(new GameEvents.Configurator());
            TimberAPI.DependencyRegistry.AddConfigurator(new WindEvents.Configurator());

            // Register fragments
            TimberAPI.DependencyRegistry.AddConfigurator(new AutomatedPausableFragmentConfigurator());
            TimberAPI.DependencyRegistry.AddConfigurator(new AutomatedFloodgateFragmentConfigurator());

            TimberAPI.DependencyRegistry.AddConfigurator(new AutomatedFloodgate.Configurator(), TimberbornAPI.Common.SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new AutomatedPausable.Configurator(), TimberbornAPI.Common.SceneEntryPoint.InGame);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
