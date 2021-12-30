using AutomizeMyDroughts.Common;
using Bindito.Core;
using Timberborn.Persistence;
using Timberborn.WaterBuildings;
using Timberborn.WeatherSystem;
using Timberborn.WindSystem;
using UnityEngine;

namespace AutomizeMyDroughts.Components
{
    public sealed class AutomatedFloodgate : Automated, IPersistentEntity
    {
        public class Scheduling
        {
            public class SeasonSchedule
            {
                public float Height = 0.0f;
                public int LowerInterval = 0;
                public bool RaiseInsteadOfLower = false;
                public bool LowerWithLowWinds = false;

                public string Summarize() {
                    var res = $"set height to {Height}";
                    if (LowerInterval > 0)
                        res += " and " + (RaiseInsteadOfLower ? "raise" : "lower") + $" every {LowerInterval} day(s)";
                    if (LowerWithLowWinds)
                        res += " and lower with low winds";
                    return res;
                }
                public void Load(IObjectLoader loader, string prefix) {
                    loader.TryGet(new PropertyKey<float>($"{prefix}:Height"), ref Height);
                    loader.TryGet(new PropertyKey<int>($"{prefix}:LowerInterval"), ref LowerInterval);
                    loader.TryGet(new PropertyKey<bool>($"{prefix}:RaiseInsteadOfLower"), ref RaiseInsteadOfLower);
                    loader.TryGet(new PropertyKey<bool>($"{prefix}:LowerWithLowWinds"), ref LowerWithLowWinds);
                }
                public void Save(IObjectSaver saver, string prefix) {
                    saver.Set(new PropertyKey<float>($"{prefix}:Height"), Height);
                    saver.Set(new PropertyKey<int>($"{prefix}:LowerInterval"), LowerInterval);
                    saver.Set(new PropertyKey<bool>($"{prefix}:RaiseInsteadOfLower"), RaiseInsteadOfLower);
                    saver.Set(new PropertyKey<bool>($"{prefix}:LowerWithLowWinds"), LowerWithLowWinds);
                }
            }

            public bool Automate = false;
            public int DaysSinceLastAction = 0;

            public SeasonSchedule Temperate = new();
            public SeasonSchedule Drought = new();
            public void Load(IObjectLoader loader) {
                loader.TryGet(new PropertyKey<bool>($"Automate"), ref Automate);
                loader.TryGet(new PropertyKey<int>($"DaysSinceLastAction"), ref DaysSinceLastAction);
                Temperate.Load(loader, "Temperate");
                Drought.Load(loader, "Drought");
            }

            public void Save(IObjectSaver saver) {
                saver.Set(new PropertyKey<bool>($"Automate"), Automate);
                saver.Set(new PropertyKey<int>($"DaysSinceLastAction"), DaysSinceLastAction);
                Temperate.Save(saver, "Temperate");
                Drought.Save(saver, "Drought");
            }
        }
        public class Configurator : IConfigurator
        {
            public void Configure(IContainerDefinition containerDefinition) {
                containerDefinition.Bind<AutomatedFloodgate>().AsTransient();
                containerDefinition.ExtendComponent<Floodgate>().With<AutomatedFloodgate>();
            }
        }

        public Scheduling Schedule = new();

        public override void Automate(AutomizationEvent.Type e) {
            Floodgate building = GetComponent<Floodgate>();

            var d = Manager.GetData<DroughtService>();
            var w = Manager.GetData<WindService>();

            if (building == null || !Schedule.Automate)
                return;

            switch (e) {
                case AutomizationEvent.Type.Drought:
                    building.SetHeight(Schedule.Drought.Height);
                    break;
                case AutomizationEvent.Type.Temperate:
                    building.SetHeight(Schedule.Temperate.Height);
                    break;
                case AutomizationEvent.Type.Day:
                    Schedule.DaysSinceLastAction++;
                    var schedule = (d.IsDrought ? Schedule.Drought : Schedule.Temperate);

                    if (schedule.LowerInterval == 0)
                        break;

                    if (Schedule.DaysSinceLastAction % schedule.LowerInterval == 0) {
                        var isRaising = schedule.RaiseInsteadOfLower;
                        var dHeight = isRaising ? 0.5f : -0.5f;
                        building.SetHeight(building.Height + dHeight);
                    }
                    break;
                case AutomizationEvent.Type.Wind:
                    if (!(d.IsDrought ? Schedule.Drought : Schedule.Temperate).LowerWithLowWinds)
                        break;
                    if (w.WindStrength < 0.2)
                        building.SetHeight(building.Height - 0.5f);
                    break;
            }
        }

        public void Load(IEntityLoader entityLoader) {
            if (!entityLoader.HasComponent(new ComponentKey(typeof(AutomatedFloodgate).Name)))
                return;

            var component = entityLoader.GetComponent(new ComponentKey(typeof(AutomatedFloodgate).Name));
            Schedule.Load(component);
        }

        public void Save(IEntitySaver entitySaver) {
            var component = entitySaver.GetComponent(new ComponentKey(typeof(AutomatedFloodgate).Name));
            Schedule.Save(component);
        }
    }

}
