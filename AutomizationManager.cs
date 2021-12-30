using AutomizeMyDroughts.Common;
using AutomizeMyDroughts.Components;
using Bindito.Core;
using System;
using System.Collections.Generic;
using Timberborn.TimeSystem;
using Timberborn.WeatherSystem;
using Timberborn.WindSystem;
using Plugin = AutomizeMyDroughts.AutomizeMyDroughtsPlugin;
namespace AutomizeMyDroughts
{
    public class AutomizationManager
    {
        public class Data
        {
            private readonly Dictionary<Type, object> _data = new();
            public void BindData<T>(T data)
                => _data[data.GetType()] = data;

            public T GetData<T>()
                => (T)_data[typeof(T)];
        }

        public class Configurator : IConfigurator
        {
            public void Configure(IContainerDefinition containerDefinition) {
                containerDefinition.Bind<AutomizationManager>().AsSingleton();
            }
        }

        public static AutomizationManager Instance;

        private readonly HashSet<IAutomatable> automatables = new();
        private readonly Data _data = new();

        public AutomizationManager(IDayNightCycle dayNightCycle, DroughtService droughtService, WeatherService weatherService, WindService windService) {

            BindData(dayNightCycle);
            BindData(droughtService);
            BindData(weatherService);
            BindData(windService);
            Instance = this;
            Plugin.L.LogInfo($"AutomizationManager initialized.");
        }

        public void BindData<T>(T data) {
            if (data == null)
                Plugin.L.LogWarning($"Could not bind {data.GetType().Name} to AutomizationManager");
            _data.BindData(data);
            Plugin.L.LogInfo($"Bound {data.GetType().Name}");
        }

        public T GetData<T>() {
            return _data.GetData<T>();
        }

        public void Register(IAutomatable automatable) {
            automatables.Add(automatable);
        }

        public void Unregister(IAutomatable automatable) {
            automatables.Remove(automatable);
        }

        public void Automate(AutomizationEvent.Type e) {
            Plugin.L.LogInfo($"Received [{e}]! Propagating event to {automatables.Count} listeners.");
            foreach (var automatable in automatables) {
                automatable.Automate(e);
            }
        }
    }
}
