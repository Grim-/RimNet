using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_ProximitySensor : ITab_SignalNodeBase
    {
        private Comp_SensorProximity SelectedPlate => SelectedNode as Comp_SensorProximity;

        public override bool IsVisible => SelThing?.TryGetComp<Comp_SensorProximity>() != null;

        public ITab_ProximitySensor()
        {
            labelKey = "Proximity";
            size = new Vector2(480f, 320f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedPlate == null) 
                return;

            var listing = new Listing_Standard();
            listing.Begin(rect);

            RimNetGUI.DrawHeader(listing.GetRect(30f), "Proximity Sensor Configuration");
            listing.Gap(10f);

            bool isOccupied = SelectedPlate.GetSensorValue() > 0;
            string statusText = isOccupied ? "Target found" : "Clear";
            Color statusColor = isOccupied ? RimNetGUI.SignalOn : RimNetGUI.TextColor;
            RimNetGUI.DrawLabeledValue(listing.GetRect(24f), "Current Status:", statusText, statusColor);
            listing.Gap(10f);

            listing.GapLine(12f);

            listing.Label("Trigger Logic:");
            listing.Gap(4f);

            float oldRadius = RimNetGUI.DrawStyledSlider(listing.GetRect(30f), SelectedPlate.ActiveRadius, 1, 10, $"Radius {SelectedPlate.ActiveRadius}");
            if (Mathf.Abs(oldRadius - SelectedPlate.ActiveRadius) > 1)
            {
                SelectedPlate.ActiveRadius = oldRadius;
            }

            Rect triggerModeRect = listing.GetRect(30f);
            RimNetGUI.EnumSelectorButton(triggerModeRect, "Target: ", SelectedPlate.TargetType, newMode => SelectedPlate.TargetType = newMode);

            listing.End();
        }
    }
}
