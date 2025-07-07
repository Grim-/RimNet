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
            Rect invertRect = new Rect(contentRect.x, yPos, contentRect.width, 24f);
            Widgets.CheckboxLabeled(invertRect, "Invert Output", ref sensor.ShouldInvert);

            yPos += 30f;
            Rect intervalLabelRect = new Rect(contentRect.x, yPos, contentRect.width * 0.6f, 24f);
            Widgets.Label(intervalLabelRect, "Update Interval (ticks):");

            Rect intervalFieldRect = new Rect(intervalLabelRect.xMax, yPos, contentRect.width * 0.4f, 24f);
            Widgets.TextFieldNumeric(intervalFieldRect, ref sensor.customUpdateInterval, ref sensor.intervalBuffer, 0, 10000);

            return new GizmoResult(GizmoState.Clear);
        }
    }

}
