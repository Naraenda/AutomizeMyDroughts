using AutomizeMyDroughts.Components;
using Bindito.Core;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.WaterBuildings;
using TimberbornAPI.UIBuilderSystem;
using UnityEngine;
using UnityEngine.UIElements;

using Plugin = AutomizeMyDroughts.AutomizeMyDroughtsPlugin;

namespace AutomizeMyDroughts.UI
{
    class AutomatedFloodgateFragment : IEntityPanelFragment
    {
        private readonly UIBuilder _uibuilder;

        private VisualElement _root;
        private Floodgate _building;

        public AutomatedFloodgateFragment(UIBuilder builder) {
            _uibuilder = builder;
        }

        public VisualElement InitializeFragment() {
            _root = _uibuilder.CreateComponentBuilder().CreateVisualElement()
                .AddComponent(_uibuilder.CreateFragmentBuilder()
                    .AddPreset(f => f.Toggles().Checkmark(name: "automate", text: "Automate"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Temperate Weather"))
                    .AddPreset(f => f.Sliders().SliderInt(0, 1, 0, name: "tHeight", text: "Height"))
                    .AddPreset(f => f.Sliders().SliderInt(0, 10, 0, name: "tLowerInterval", text: "Lower every x days"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "tRaiseInstead", text: "Raise instead of lower"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "tLowerWinds", text: "Lower with low winds (experimental)"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Dry Weather"))
                    .AddPreset(f => f.Sliders().SliderInt(0, 1, 0, name: "dHeight", text: "Height"))
                    .AddPreset(f => f.Sliders().SliderInt(0, 10, 0, name: "dLowerInterval", text: "Lower every x days"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "dRaiseInstead", text: "Raise instead of lower"))
                    .AddPreset(f => f.Toggles().Checkmark(name: "dLowerWinds", text: "Lower with low winds (experimental)"))
                    .AddPreset(f => f.Labels().DefaultBold(text: "Summary"))
                    .AddPreset(f => f.Labels().GameText(name: "summary", text: "Hello World!"))
                    .Build()
                ).BuildAndInitialize();
            return _root;
        }

        public void ShowFragment(GameObject entity) {
            _building = entity?.GetComponent<Floodgate>();

            var config = _building?.GetComponent<AutomatedFloodgate>();

            if (config == null)
                return;

            _root.Q<Toggle>("automate").value = config.Schedule.Automate;

            _root.Q<SliderInt>("dHeight").highValue = 2 * _building.MaxHeight;
            _root.Q<SliderInt>("tHeight").highValue = 2 * _building.MaxHeight;
            
            _root.Q<SliderInt>("dHeight").value = Mathf.RoundToInt(2 * config.Schedule.Drought.Height);
            _root.Q<SliderInt>("tHeight").value = Mathf.RoundToInt(2 * config.Schedule.Temperate.Height);

            _root.Q<SliderInt>("dLowerInterval").value = config.Schedule.Drought.LowerInterval;
            _root.Q<SliderInt>("tLowerInterval").value = config.Schedule.Temperate.LowerInterval;

            _root.Q<Toggle>("dRaiseInstead").value = config.Schedule.Drought.RaiseInsteadOfLower;
            _root.Q<Toggle>("tRaiseInstead").value = config.Schedule.Temperate.RaiseInsteadOfLower;

            _root.Q<Toggle>("dLowerWinds").value = config.Schedule.Drought.LowerWithLowWinds;
            _root.Q<Toggle>("tLowerWinds").value = config.Schedule.Temperate.LowerWithLowWinds;
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
            return _building is not null && _building.enabled;
        }

        private void OnConfigurationChanged() {
            var automateUI = _root.Q<Toggle>("automate");

            var dHeightUI = _root.Q<SliderInt>("dHeight");
            var tHeightUI = _root.Q<SliderInt>("tHeight");

            var dLoweringUI = _root.Q<SliderInt>("dLowerInterval");
            var tLoweringUI = _root.Q<SliderInt>("tLowerInterval");

            var dRaiseInsteadUI = _root.Q<Toggle>("dRaiseInstead");
            var tRaiseInsteadUI = _root.Q<Toggle>("tRaiseInstead");

            var dLowerWindsUI = _root.Q<Toggle>("dLowerWinds");
            var tLowerWindsUI = _root.Q<Toggle>("tLowerWinds");

            var config = _building.GetComponent<AutomatedFloodgate>();

            if (config is null) {
                Plugin.L.LogInfo("Adding AutomatedFloodgate component.");
                config = _building.gameObject.AddComponent<AutomatedFloodgate>();
            }

            dHeightUI.label = string.Format("Set height to {0}", dHeightUI.value / 2f);
            tHeightUI.label = string.Format("Set height to {0}", tHeightUI.value / 2f);

            dLoweringUI.label = string.Format(dLoweringUI.value > 0 ? "Lower every {0} days" : "Lower never", dLoweringUI.value);
            tLoweringUI.label = string.Format(tLoweringUI.value > 0 ? "Lower every {0} days" : "Lower never", tLoweringUI.value);

            config.Schedule.Automate = automateUI.value;

            config.Schedule.Drought.Height = dHeightUI.value / 2f;
            config.Schedule.Temperate.Height = tHeightUI.value / 2f;

            config.Schedule.Drought.LowerInterval = dLoweringUI.value;
            config.Schedule.Temperate.LowerInterval = tLoweringUI.value;

            config.Schedule.Drought.RaiseInsteadOfLower = dRaiseInsteadUI.value;
            config.Schedule.Temperate.RaiseInsteadOfLower = tRaiseInsteadUI.value;

            config.Schedule.Drought.LowerWithLowWinds = dLowerWindsUI.value;
            config.Schedule.Temperate.LowerWithLowWinds = tLowerWindsUI.value;

            _root.Q<Label>("summary").text = $"With temperate weather {config.Schedule.Temperate.Summarize()}. With dry weather {config.Schedule.Drought.Summarize()}";
        }
    }

    public class AutomatedFloodgateFragmentConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition) {
            containerDefinition.Bind<AutomatedFloodgateFragment>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly AutomatedFloodgateFragment _fragment;

            public EntityPanelModuleProvider(AutomatedFloodgateFragment fragment) {
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