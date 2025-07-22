using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_DaylightSensor : ITab_SignalNodeBase
    {
        private Comp_SensorDaylight SelectedSensor => SelectedNode as Comp_SensorDaylight;

        public override bool IsVisible => SelThing?.TryGetComp<Comp_SensorDaylight>() != null;

        public ITab_DaylightSensor()
        {
            labelKey = "Daylight Sensor";
            size = new Vector2(480f, 320f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedSensor == null)
                return;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            RimNetGUI.DrawHeader(listing.GetRect(30f), "Daylight Sensor Configuration");
            listing.Gap(10f);

            float currentGlow = SelectedSensor.GetSensorValue();
            RimNetGUI.DrawLabeledValue(listing.GetRect(24f), "Current Light Level:", currentGlow.ToStringPercent(), RimNetGUI.WarningColor);
            listing.Gap(10f);

            RimNetGUI.EnumSelectorButton(
                listing.GetRect(30f),
                "Mode: ",
                SelectedSensor.currentMode,
                newMode => SelectedSensor.SetMode(newMode)
            );

            if (SelectedSensor.currentMode == DayLightSensorMode.CUSTOM)
            {
                listing.Gap(10f);
                listing.Label("Custom Trigger Threshold:");
                listing.Gap(4f);

                Rect sliderRect = listing.GetRect(28f);
                float newThreshold = RimNetGUI.DrawStyledSlider(
                    sliderRect,
                    SelectedSensor.customThreshold,
                    0f,
                    1f,
                    SelectedSensor.customThreshold.ToStringPercent()
                );

                if (Mathf.Abs(newThreshold - SelectedSensor.customThreshold) > 0.01f)
                {
                    SelectedSensor.SetCustomThreshold(newThreshold);
                }
            }

            listing.End();
        }
    }
}
