using AutomizeMyDroughts.Common;
using Bindito.Core;
using Timberborn.Buildings;
using Timberborn.Persistence;
using Timberborn.TimeSystem;
using Timberborn.WeatherSystem;
using Timberborn.WindSystem;

namespace AutomizeMyDroughts.Components
{
    public sealed class AutomatedPausable : Automated, IPersistentEntity
    {
        public class Scheduling
        {
            public class SeasonSchedule
            {
                public bool WorkDuringNight = false;
                public bool WorkDuringDay = false;
                public bool WorkDuringLowWinds = false;

                public bool IsWorking(AutomizationManager m) {
                    var c = m.GetData<DayNightCycle>();
                    var w = m.GetData<WindService>();

                    return (c.IsDaytime && WorkDuringDay || c.IsNighttime && WorkDuringNight) && (w.WindStrength > 0.2 || WorkDuringLowWinds);
                }

                public string Summarize() {
                    if ((WorkDuringDay && WorkDuringDay && WorkDuringLowWinds))
                        return "always work";
                    if (!WorkDuringDay && !WorkDuringNight)
                        return "never work";
                    if (WorkDuringDay && WorkDuringNight && !WorkDuringLowWinds)
                        return "work with high winds";
                    return "work during " + (WorkDuringDay ? "daytime," : "nighttime") + (WorkDuringLowWinds ? "" : " with high winds");
                }

                public void Load(IObjectLoader loader, string prefix) {
                    loader.TryGet(new PropertyKey<bool>($"{prefix}:WorkNight"), ref WorkDuringNight);
                    loader.TryGet(new PropertyKey<bool>($"{prefix}:WorkDay"), ref WorkDuringDay);
                    loader.TryGet(new PropertyKey<bool>($"{prefix}:WorkLowWinds"), ref WorkDuringLowWinds);
                }

                public void Save(IObjectSaver saver, string prefix) {
                    saver.Set(new PropertyKey<bool>($"{prefix}:WorkNight"), WorkDuringNight);
                    saver.Set(new PropertyKey<bool>($"{prefix}:WorkDay"), WorkDuringDay);
                    saver.Set(new PropertyKey<bool>($"{prefix}:WorkLowWinds"), WorkDuringLowWinds);
                }
            }

            public bool Automate = false;
            public SeasonSchedule Temperate = new();
            public SeasonSchedule Drought = new();

            public bool IsWorking(AutomizationManager m) {
                var d = m.GetData<DroughtService>();
                return d.IsDrought ? Drought.IsWorking(m) : Temperate.IsWorking(m);
            }

            public void Load(IObjectLoader loader) {
                loader.TryGet(new PropertyKey<bool>($"Automate"), ref Automate);
                Temperate.Load(loader, "Temperate");
                Drought.Load(loader, "Drought");
            }

            public void Save(IObjectSaver saver) {
                saver.Set(new PropertyKey<bool>($"Automate"), Automate);
                Temperate.Save(saver, "Temperate");
                Drought.Save(saver, "Drought");
            }
        }

        public class Configurator : IConfigurator
        {
            public void Configure(IContainerDefinition containerDefinition) {
                containerDefinition.Bind<AutomatedPausable>().AsTransient();
                containerDefinition.ExtendComponent<PausableBuilding>().With<AutomatedPausable>();
            }
        }

        public Scheduling Schedule = new();

        public override void Automate(AutomizationEvent.Type e) {
            PausableBuilding building = GetComponent<PausableBuilding>();

            // If we can't pause, we exit early
            if (building == null || !building.IsPausable() || !Schedule.Automate)
                return;

            bool shouldBeWorking = Schedule.IsWorking(Manager);

            // Don't do anything if nothing needs to change
            if (shouldBeWorking == !building.Paused)
                return;

            if (shouldBeWorking) {
                building.Resume();
            } else {
                building.Pause();
            }
        }

        public void Load(IEntityLoader entityLoader) {
            if (!entityLoader.HasComponent(new ComponentKey(typeof(AutomatedPausable).Name)))
                return;

            var component = entityLoader.GetComponent(new ComponentKey(typeof(AutomatedPausable).Name));
            Schedule.Load(component);
        }

        public void Save(IEntitySaver entitySaver) {
            var component = entitySaver.GetComponent(new ComponentKey(typeof(AutomatedPausable).Name));
            Schedule.Save(component);
        }
    }
}
