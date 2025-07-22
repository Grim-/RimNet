using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_PressurePlate : ITab_SignalNodeBase
    {
        private Comp_SensorPressurePlate SelectedPlate => SelectedNode as Comp_SensorPressurePlate;

        public override bool IsVisible => SelThing?.TryGetComp<Comp_SensorPressurePlate>() != null;

        public ITab_PressurePlate()
        {
            labelKey = "Pressure Plate";
            size = new Vector2(480f, 320f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedPlate == null) return;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            RimNetGUI.DrawHeader(listing.GetRect(30f), "Pressure Plate Configuration");
            listing.Gap(10f);

            bool isOccupied = SelectedPlate.GetSensorValue() > 0;
            string statusText = isOccupied ? "Occupied" : "Clear";
            Color statusColor = isOccupied ? RimNetGUI.SignalOn : RimNetGUI.TextColor;
            RimNetGUI.DrawLabeledValue(listing.GetRect(24f), "Current Status:", statusText, statusColor);
            listing.Gap(10f);

            listing.GapLine(12f);

            listing.Label("Trigger Logic:");
            listing.Gap(4f);

            Rect triggerModeRect = listing.GetRect(30f);
            RimNetGUI.EnumSelectorButton(triggerModeRect, "Mode: ", SelectedPlate.triggerMode, newMode => SelectedPlate.triggerMode = newMode);

            Rect targetTypeRect = listing.GetRect(30f);
            RimNetGUI.EnumSelectorButton(targetTypeRect, "Target: ", SelectedPlate.targetType, newType => SelectedPlate.targetType = newType);

            listing.End();
        }
    }
}
