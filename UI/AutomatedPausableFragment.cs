using AutomizeMyDroughts.Components;
using Bindito.Core;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using TimberbornAPI.UIBuilderSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace AutomizeMyDroughts.UI
{
    public class AutomatedPausableFragment : IEntityPanelFragment
    {
        private readonly UIBuilder _uibuilder;

        private VisualElement _root;
        private PausableBuilding _building;

        private Toggle _uiAutomate;
        private Toggle _uiWorkDayD;
        private Toggle _uiWorkDayT;
        private Toggle _uiWorkNightD;
        private Toggle _uiWorkNightT;
        private Toggle _uiWorkLowWindD;
        private Toggle _uiWorkLowWindT;
        private Label  _uiSummary;

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

            _uiAutomate = _root.Q<Toggle>("automate");
            _uiSummary = _root.Q<Label>("summary");

            _uiWorkDayD = _root.Q<Toggle>("dWorkDay");
            _uiWorkDayT = _root.Q<Toggle>("tWorkDay");

            _uiWorkNightD = _root.Q<Toggle>("dWorkNight");
            _uiWorkNightT = _root.Q<Toggle>("tWorkNight");

            _uiWorkLowWindD = _root.Q<Toggle>("dWorkLowWind");
            _uiWorkLowWindT = _root.Q<Toggle>("tWorkLowWind");

            return _root;
        }

        public void ShowFragment(GameObject entity) {
            _building = entity.GetComponent<PausableBuilding>();
            if (_building is null || !_building.IsPausable())
                return;

            var config = _building.GetComponent<AutomatedPausable>() ?? 
                _building.gameObject.AddComponent<AutomatedPausable>();
            var schedule = config.Schedule;

            _uiAutomate.value = schedule.Automate;

            _uiWorkDayD.value = schedule.Drought.WorkDuringDay;
            _uiWorkDayT.value = schedule.Temperate.WorkDuringDay;

            _uiWorkNightD.value = schedule.Drought.WorkDuringNight;
            _uiWorkNightT.value = schedule.Temperate.WorkDuringNight;

            _uiWorkLowWindD.value = schedule.Drought.WorkDuringLowWinds;
            _uiWorkLowWindT.value = schedule.Temperate.WorkDuringLowWinds;
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
            return _building is not null && _building.IsPausable() && _building.enabled;
        }

        private void OnConfigurationChanged() {
            var schedule = _building.GetComponent<AutomatedPausable>().Schedule;

            schedule.Automate = _uiAutomate.value;
            schedule.Temperate.WorkDuringDay = _uiWorkDayT.value;
            schedule.Drought.WorkDuringDay = _uiWorkDayD.value;
            schedule.Temperate.WorkDuringNight = _uiWorkNightT.value;
            schedule.Drought.WorkDuringNight = _uiWorkNightD.value;
            schedule.Temperate.WorkDuringLowWinds = _uiWorkLowWindT.value;
            schedule.Drought.WorkDuringLowWinds = _uiWorkLowWindD.value;

            _uiSummary.text = $"With temperate weather {schedule.Temperate.Summarize()}. With dry weather {schedule.Drought.Summarize()}.";
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
