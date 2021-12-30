using AutomizeMyDroughts.Common;
using AutomizeMyDroughts.Components;
using Bindito.Core;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using TimberbornAPI.UIBuilderSystem;
using UnityEngine;
using UnityEngine.UIElements;

using Plugin = AutomizeMyDroughts.AutomizeMyDroughtsPlugin;

namespace AutomizeMyDroughts.UI
{
    public class AutomatedPausableFragment : IEntityPanelFragment
    {
        private readonly UIBuilder _uibuilder;

        private VisualElement _root;
        private PausableBuilding _building;

        public AutomatedPausableFragment(UIBuilder builder) {
            _uibuilder = builder;
        }

        public VisualElement InitializeFragment() {
            _root = _uibuilder.CreateComponentBuilder().CreateVisualElement()
                .AddComponent(_uibuilder.CreateFragmentBuilder()
                    .AddPreset(f => f.Toggles().Checkmark(name: "automate", text: "Automate"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Temperate Weather"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "tWorkDay"    , text: "Work during day"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "tWorkNight"  , text: "Work during night"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "tWorkLowWind", text: "Work during low winds"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Dry Weather"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "dWorkDay"    , text: "Work during day"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "dWorkNight"  , text: "Work during night"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "dWorkLowWind", text: "Work during low winds"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Summary"))
                    .AddPreset(f => f.Labels().GameText(name: "summary", text: "Hello World!"))
                    .Build()
                ).BuildAndInitialize();
            return _root;
        }

        public void ShowFragment(GameObject entity) {
            _building = entity.GetComponent<PausableBuilding>();

            var config = _building?.GetComponent<AutomatedPausable>()?.Schedule;
            _root.Q<Toggle>("automate").value = config?.Automate ?? false;

            _root.Q<Toggle>("tWorkDay").value = config?.Temperate?.WorkDuringDay ?? false;
            _root.Q<Toggle>("dWorkDay").value = config?.Drought?.WorkDuringDay ?? false;

            _root.Q<Toggle>("tWorkNight").value = config?.Temperate?.WorkDuringNight ?? false;
            _root.Q<Toggle>("dWorkNight").value = config?.Drought?.WorkDuringNight ?? false;

            _root.Q<Toggle>("tWorkLowWind").value = config?.Temperate?.WorkDuringLowWinds ?? false;
            _root.Q<Toggle>("dWorkLowWind").value = config?.Drought?.WorkDuringLowWinds ?? false;
        }

        public void ClearFragment() {
            // Not sure if this is needed.
            if (!IsValidBuilding()) {
                _root.ToggleDisplayStyle(false);
                return;
            }

            OnConfigurationChanged();
            
            _root.ToggleDisplayStyle(false);
        }

        public void UpdateFragment() {
            if (IsValidBuilding()) {
                _root.ToggleDisplayStyle(true);

                OnConfigurationChanged();
            } else {
                _root.ToggleDisplayStyle(false);
                return;
            }
        }

        private bool IsValidBuilding() {
            return _building is not null && _building.IsPausable() && _building.enabled;
        }

        private void OnConfigurationChanged() {
            // Parse UI stuff
            var ap = _building.GetComponent<AutomatedPausable>();
            
            if (ap is null) {
                Plugin.L.LogInfo("Adding AutomatedPausable component.");
                ap = _building.gameObject.AddComponent<AutomatedPausable>();
            }
            var config = ap.Schedule;

            config.Automate = _root.Q<Toggle>("automate").value;
            config.Temperate.WorkDuringDay = _root.Q<Toggle>("tWorkDay").value;
            config.Drought  .WorkDuringDay = _root.Q<Toggle>("dWorkDay").value;
            config.Temperate.WorkDuringNight = _root.Q<Toggle>("tWorkNight").value;
            config.Drought  .WorkDuringNight = _root.Q<Toggle>("dWorkNight").value;
            config.Temperate.WorkDuringLowWinds = _root.Q<Toggle>("tWorkLowWind").value;
            config.Drought  .WorkDuringLowWinds = _root.Q<Toggle>("dWorkLowWind").value;

            _root.Q<Label>("summary").text = $"With temperate weather {config.Temperate.Summarize()}. With dry weather {config.Drought.Summarize()}.";
        }
    }

    public class AutomatedPausableFragmentConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition) {
            containerDefinition.Bind<AutomatedPausableFragment>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly AutomatedPausableFragment _fragment;

            public EntityPanelModuleProvider(AutomatedPausableFragment fragment) {
                _fragment = fragment;
            }

            public EntityPanelModule Get() {
                EntityPanelModule.Builder builder = new();
                builder.AddMiddleFragment(_fragment);
                return builder.Build();
            }
        }
    }
}
