using Bindito.Core;
using HarmonyLib;
using Timberborn.SingletonSystem;
using Timberborn.WindSystem;
using UnityEngine;

namespace AutomizeMyDroughts.Events
{
    class WindEvents : ILoadableSingleton
    {
        private float _lastWindStrength;
        public class Configurator : IConfigurator
        {
            public void Configure(IContainerDefinition containerDefinition) {
                containerDefinition.Bind<WindEvents>().AsSingleton();
            }
        }

        [HarmonyPatch(typeof(WindService), "Tick")]
        public static class WindServicePatches
        {
            [HarmonyPostfix]
            public static void Postfix() {
                Instance?.OnWindUpdate();
            }
        }

        public static WindEvents Instance { get; private set; }
        public float WindStrength => _windservice.WindStrength;
        public Vector2 WindDirection => _windservice.WindDirection;

        readonly WindService _windservice;
        readonly AutomizationManager _manager;

        public WindEvents(WindService windservce, AutomizationManager manager) {
            _windservice = windservce;
            _manager = manager;
            Instance = this;
            manager.BindData(this);
        }

        private void OnWindUpdate() {
            var windStrength = _windservice.WindStrength;

            if (windStrength == _lastWindStrength)
                return;

            _manager.Automate(Common.AutomizationEvent.Type.Wind);
            _lastWindStrength = windStrength;
        }

        public void Load() { }
    }
}
