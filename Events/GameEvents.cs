using AutomizeMyDroughts.Common;
using Bindito.Core;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using Timberborn.WeatherSystem;
using TimberbornAPI.EventSystem;

using Plugin = AutomizeMyDroughts.AutomizeMyDroughtsPlugin;
namespace AutomizeMyDroughts.Events
{
    public class GameEvents : EventListener, ILoadableSingleton
    {
        public class Configurator : IConfigurator
        {
            public void Configure(IContainerDefinition containerDefinition) {
                containerDefinition.Bind<GameEvents>().AsSingleton();
            }
        }

        private readonly DroughtService _droughtService;
        private readonly AutomizationManager _manager;

        public GameEvents(DroughtService droughtService, AutomizationManager manager) {
            _droughtService = droughtService;
            _manager = manager;
            _manager.BindData(this);
        }

        [OnEvent]
        public void OnDaytimeStart(DaytimeStartEvent _) {
            Plugin.L.LogInfo($"Daybreak once again, but it's " + (_droughtService.IsDrought ? "dry" : "wet") + ".");
            _manager.Automate(AutomizationEvent.Type.Day);
        }

        [OnEvent]
        public void OnNighttimeStart(NighttimeStartEvent _) {
            _manager.Automate(AutomizationEvent.Type.Night);
        }

        [OnEvent]
        public void OnDroughtEnded(DroughtEndedEvent _) {
            Plugin.L.LogInfo("Yo, it do be wet again.");
            _manager.Automate(AutomizationEvent.Type.Temperate);
        }

        [OnEvent]
        public void OnDroughtStarted(DroughtStartedEvent _) {
            Plugin.L.LogInfo("Yo, it be dry af.");
            _manager.Automate(AutomizationEvent.Type.Drought);
        }
    }
}
