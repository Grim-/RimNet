using UnityEngine;
using Verse;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public class Gizmo_SensorConfig : Gizmo
    {
        private Comp_SignalSensor sensor;
        private static readonly Vector2 GizmoSize = new Vector2(200f, 75f);

        public Gizmo_SensorConfig(Comp_SignalSensor sensor)
        {
            this.sensor = sensor;
            if (sensor.intervalBuffer == null)
            {
                sensor.intervalBuffer = sensor.CurrentUpdateInterval.ToString();
            }
            Order = -49f;
        }

        public override float GetWidth(float maxWidth) => GizmoSize.x;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), GizmoSize.y);
            Widgets.DrawWindowBackground(rect);
            string tip = $"Configure sensor settings.\n\nInvert: Toggles the sensor's output.\n\nUpdate Interval (ticks): How often the sensor checks its state. Default: {sensor.DefaultUpdateInterval}";
            TooltipHandler.TipRegion(rect, tip);
            Rect contentRect = rect.ContractedBy(5f);
            Text.Font = GameFont.Small;
            Widgets.Label(contentRect, "Sensor Configuration");
            float yPos = contentRect.y + 25f;

            // Check for checkbox changes
            Rect invertRect = new Rect(contentRect.x, yPos, contentRect.width, 24f);
            bool oldInvertValue = sensor.ShouldInvert;
            Widgets.CheckboxLabeled(invertRect, "Invert Output", ref sensor.ShouldInvert);
            if (oldInvertValue != sensor.ShouldInvert)
            {
                if (sensor.SignalGroup != null)
                {
                    sensor.SignalGroup.SyncGroup(sensor);
                }
            }

            yPos += 30f;

            // Interval controls with buttons
            Rect intervalLabelRect = new Rect(contentRect.x, yPos, contentRect.width, 24f);
            Widgets.Label(intervalLabelRect, $"Update Interval: {sensor.customUpdateInterval} ticks");

            yPos += 25f;
            float buttonWidth = 30f;
            float fieldWidth = contentRect.width - (buttonWidth * 2) - 10f;

            // Decrease button
            Rect decreaseRect = new Rect(contentRect.x, yPos, buttonWidth, 24f);
            if (Widgets.ButtonText(decreaseRect, "-"))
            {
                sensor.customUpdateInterval = Mathf.Max(0, sensor.customUpdateInterval - 10);
                sensor.intervalBuffer = sensor.customUpdateInterval.ToString();
                if (sensor.SignalGroup != null)
                {
                    sensor.SignalGroup.SyncGroup(sensor);
                }
            }

            // Text field
            Rect intervalFieldRect = new Rect(decreaseRect.xMax + 5f, yPos, fieldWidth, 24f);
            int oldValue = sensor.customUpdateInterval;
            Widgets.TextFieldNumeric(intervalFieldRect, ref sensor.customUpdateInterval, ref sensor.intervalBuffer, 0, 10000);
            if (oldValue != sensor.customUpdateInterval)
            {
                if (sensor.SignalGroup != null)
                {
                    sensor.SignalGroup.SyncGroup(sensor);
                }
            }

            // Increase button
            Rect increaseRect = new Rect(intervalFieldRect.xMax + 5f, yPos, buttonWidth, 24f);
            if (Widgets.ButtonText(increaseRect, "+"))
            {
                sensor.customUpdateInterval = Mathf.Min(10000, sensor.customUpdateInterval + 10);
                sensor.intervalBuffer = sensor.customUpdateInterval.ToString();
                if (sensor.SignalGroup != null)
                {
                    sensor.SignalGroup.SyncGroup(sensor);
                }
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }

}
