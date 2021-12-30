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

        private Toggle    _uiAutomate;
        private SliderInt _uiHeightD;
        private SliderInt _uiHeightT;
        private SliderInt _uiLowerIntervalD;
        private SliderInt _uiLowerIntervalT;
        private Toggle    _uiRaiseInsteadD;
        private Toggle    _uiRaiseInsteadT;
        private Toggle    _uiLowerWindsD;
        private Toggle    _uiLowerWindsT;
        private Label     _uiSummary;

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

            _uiAutomate = _root.Q<Toggle>("automate");
            _uiSummary = _root.Q<Label>("summary");

            _uiHeightD = _root.Q<SliderInt>("dHeight");
            _uiHeightT = _root.Q<SliderInt>("tHeight");

            _uiLowerIntervalD = _root.Q<SliderInt>("dLowerInterval");
            _uiLowerIntervalT = _root.Q<SliderInt>("tLowerInterval");

            _uiRaiseInsteadD =_root.Q<Toggle>("dRaiseInstead");
            _uiRaiseInsteadT =_root.Q<Toggle>("tRaiseInstead");

            _uiLowerWindsD = _root.Q<Toggle>("dLowerWinds");
            _uiLowerWindsT = _root.Q<Toggle>("tLowerWinds");

            return _root;
        }

        public void ShowFragment(GameObject entity) {
            _building = entity?.GetComponent<Floodgate>();
            if (_building is null)
                return;

            var config = _building.GetComponent<AutomatedFloodgate>() ??
                _building.gameObject.AddComponent<AutomatedFloodgate>();
            var schedule = config.Schedule;

            _uiAutomate.value = schedule.Automate;

            _uiHeightD.highValue = 2 * _building.MaxHeight;
            _uiHeightT.highValue = 2 * _building.MaxHeight;

            _uiHeightD.value = Mathf.RoundToInt(2 * schedule.Drought.Height);
            _uiHeightT.value = Mathf.RoundToInt(2 * schedule.Temperate.Height);

            _uiLowerIntervalD.value = schedule.Drought.LowerInterval;
            _uiLowerIntervalT.value = schedule.Temperate.LowerInterval;

            _uiRaiseInsteadD.value = schedule.Drought.RaiseInsteadOfLower;
            _uiRaiseInsteadT.value = schedule.Temperate.RaiseInsteadOfLower;

            _uiLowerWindsD.value = schedule.Drought.LowerWithLowWinds;
            _uiLowerWindsT.value = schedule.Temperate.LowerWithLowWinds;
        }

        public void ClearFragment() {
            if (IsValidBuilding())
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
            var schedule = _building.GetComponent<AutomatedFloodgate>().Schedule;

            _uiHeightD.label = string.Format("Set height to {0}", _uiHeightD.value / 2f);
            _uiHeightT.label = string.Format("Set height to {0}", _uiHeightT.value / 2f);

            _uiLowerIntervalD.label = string.Format(_uiLowerIntervalD.value > 0 ? "Lower every {0} days" : "Lower never", _uiLowerIntervalD.value);
            _uiLowerIntervalT.label = string.Format(_uiLowerIntervalT.value > 0 ? "Lower every {0} days" : "Lower never", _uiLowerIntervalT.value);

            schedule.Automate = _uiAutomate.value;

            schedule.Drought.Height = _uiHeightD.value / 2f;
            schedule.Temperate.Height = _uiHeightT.value / 2f;

            schedule.Drought.LowerInterval = _uiLowerIntervalD.value;
            schedule.Temperate.LowerInterval = _uiLowerIntervalT.value;

            schedule.Drought.RaiseInsteadOfLower = _uiRaiseInsteadD.value;
            schedule.Temperate.RaiseInsteadOfLower = _uiRaiseInsteadT.value;

            schedule.Drought.LowerWithLowWinds = _uiLowerWindsD.value;
            schedule.Temperate.LowerWithLowWinds = _uiLowerWindsT.value;

            _uiSummary.text = $"With temperate weather {schedule.Temperate.Summarize()}. With dry weather {schedule.Drought.Summarize()}";
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