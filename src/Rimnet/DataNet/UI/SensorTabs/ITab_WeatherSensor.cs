using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_WeatherSensor : ITab_SignalNodeBase
    {
        private Comp_SensorWeather SelectedPlate => SelectedNode as Comp_SensorWeather;

        public override bool IsVisible => SelThing?.TryGetComp<Comp_SensorWeather>() != null;

        public ITab_WeatherSensor()
        {
            labelKey = "Weather";
            size = new Vector2(480f, 320f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedPlate == null) return;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            RimNetGUI.DrawHeader(listing.GetRect(30f), "Weather Sensor Configuration");
            listing.Gap(10f);

            Rect triggerModeRect = listing.GetRect(30f);
            RimNetGUI.EnumSelectorButton(triggerModeRect, "Target: ", SelectedPlate.sensorMode, newMode => SelectedPlate.sensorMode = newMode);

            Rect weatherTypeRect = listing.GetRect(30f);
            RimNetGUI.DropdownSelectorButton(weatherTypeRect, $"Weather: {(SelectedPlate.targetWeather != null ? SelectedPlate.targetWeather.label : "none")}",
            ()=>
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (var item in DefDatabase<WeatherDef>.AllDefsListForReading)
                {
                    options.Add(new FloatMenuOption($"{item.LabelCap}", () =>
                    {
                        SelectedPlate.targetWeather = item;
                    }));
                }

                return options;
            });
            listing.End();
        }
    }
}
