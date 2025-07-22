using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_SignalSensor : ITab_SignalNodeBase
    {
        private Comp_SignalSensor SelectedSensor => (Comp_SignalSensor)SelectedNode;

        public override bool IsVisible => SelThing?.TryGetComp<Comp_SignalSensor>() != null;

        public ITab_SignalSensor()
        {
            labelKey = "Sensor";
            size = new Vector2(550f, 350f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedSensor == null) 
                return;
            var listing = new Listing_Standard();
            listing.Begin(rect);


            RimNetGUI.DrawHeader(listing.GetRect(30f), "Sensor Configuration");
            listing.Gap(10f);

            float sensorVal = SelectedSensor.GetSensorValue();
            Rect valueRect = listing.GetRect(24f);
            RimNetGUI.DrawLabeledValue(valueRect, "Current Output:", sensorVal.ToString("F3"), RimNetGUI.SignalOn);
            listing.Gap(4f);

          

            listing.End();
        }


        protected override void DrawSidePanelContent(Listing_Standard listing)
        {
            base.DrawSidePanelContent(listing);
            if (SelectedSensor == null) 
                return;


            bool shouldInvert = SelectedSensor.ShouldInvert;
            listing.CheckboxLabeled("Invert Output (1.0 - Value)", ref shouldInvert, null, 30f, 0.8f);

            if (shouldInvert != SelectedSensor.ShouldInvert)
                SelectedSensor.ShouldInvert = shouldInvert;

            listing.GapLine(12f);


            listing.Label("Update Interval:", -1f, new TipSignal("How often the sensor checks its state. Lower is faster but more performance intensive."));
            listing.Gap(2f);

            Rect sliderRect = listing.GetRect(28f);
            float newInterval = RimNetGUI.DrawStyledSlider(sliderRect, SelectedSensor.CurrentUpdateInterval, 30, 600, SelectedSensor.CurrentUpdateInterval.ToStringSecondsFromTicks());
            if (Mathf.Abs(newInterval - SelectedSensor.CurrentUpdateInterval) > 1)
            {
                SelectedSensor.customUpdateInterval = (int)newInterval;
            }

            Rect buttonRect = listing.GetRect(30f);
            if (RimNetGUI.StyledButton(buttonRect, "Reset to Default"))
            {
                SelectedSensor.customUpdateInterval = -1;
            }


            listing.GapLine();
            listing.Label("Connection Info", -1f, new TipSignal("Information about connected nodes."));
            listing.GapLine();

            if (SelectedSensor.AllConnectedPorts.Count == 0)
            {
                listing.Label("No active connections.");
            }
            else
            {
                foreach (var port in SelectedSensor.AllConnectedPorts)
                {
                    if (port.ConnectedNode != null)
                    {
                        listing.Label($"-> {port.ConnectedNode.parent.LabelCap}");
                    }
                }
            }

        }

        protected override void DrawToolbarContent(RowLayoutManager layout)
        {
            base.DrawToolbarContent(layout);
            if (Widgets.ButtonText(layout.NextRect(100f), "Connect"))
            {
                SelectedNode.ShowSourceSelectionMenu();
            }
        }
    }
}
