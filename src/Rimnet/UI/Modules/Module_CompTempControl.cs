using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_CompTempControl : NetworkUIModule
    {
        private CompTempControl tempControl;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompTempControl>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            tempControl = node.parent.GetComp<CompTempControl>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = ExpanseUI.SplitRectVertical(rect.Inset(2), 18, 24, 16, 16, 16, 16);

            ExpanseUI.DrawHeader(sections[0], "TEMP_CTRL");

            // Interactive temperature control bar
            float currentTemp = tempControl.parent.Position.GetTemperature(tempControl.parent.Map);
            float newTarget = ExpanseUI.DrawSectionedProgressBar(
                sections[1],
                currentTemp,
                tempControl.targetTemperature,
                -50f,
                50f,
                "TEMP",
                "°C",
                true,
                GetTemperatureColor(currentTemp)
            );

            // Apply target change if clicked
            if (Mathf.Abs(newTarget - tempControl.targetTemperature) > 0.1f)
            {
                tempControl.targetTemperature = newTarget;
            }

            string modeText = $"MODE: {GetOperatingMode()}";
            ExpanseUI.DrawStatusText(sections[2], "", modeText, GetModeColor());

            string powerText = $"POWR: {tempControl.Props.energyPerSecond:F0}W";
            Color powerColor = tempControl.operatingAtHighPower ? ExpanseUI.Orange : ExpanseUI.Green;
            ExpanseUI.DrawStatusText(sections[3], "", powerText, powerColor);

            string efficiencyText = $"EFFC: {GetEfficiency():F0}%";
            ExpanseUI.DrawStatusText(sections[4], "", efficiencyText, GetEfficiencyColor());

            string status = GetSystemStatus();
            ExpanseUI.DrawStatusText(sections[5], "STS: ", status, ExpanseUI.GetStatusColor(status));

            ExpanseUI.EndExpanseStyle();
        }

        private Color GetTemperatureColor(float temp)
        {
            if (temp < -10f)
                return new Color(0.4f, 0.7f, 1f, 1f);
            else if (temp < 10f)
                return ExpanseUI.Blue;
            else if (temp < 25f)
                return ExpanseUI.Green;
            else if (temp < 35f)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Red;
        }

        private string GetOperatingMode()
        {
            float currentTemp = tempControl.parent.Position.GetTemperature(tempControl.parent.Map);
            float targetTemp = tempControl.targetTemperature;

            if (!tempControl.operatingAtHighPower)
                return "STANDBY";

            if (currentTemp < targetTemp - 1f)
                return "HEATING";
            else if (currentTemp > targetTemp + 1f)
                return "COOLING";
            else
                return "MAINTAIN";
        }

        private Color GetModeColor()
        {
            string mode = GetOperatingMode();
            switch (mode)
            {
                case "HEATING":
                    return ExpanseUI.Orange;
                case "COOLING":
                    return new Color(0.4f, 0.7f, 1f, 1f);
                case "MAINTAIN":
                    return ExpanseUI.Green;
                case "STANDBY":
                    return ExpanseUI.Gray;
                default:
                    return ExpanseUI.TextColor;
            }
        }

        private float GetEfficiency()
        {
            float currentTemp = tempControl.parent.Position.GetTemperature(tempControl.parent.Map);
            float targetTemp = tempControl.targetTemperature;
            float tempDiff = Mathf.Abs(currentTemp - targetTemp);

            if (tempDiff < 1f)
                return 100f;
            else if (tempDiff < 5f)
                return Mathf.Lerp(100f, 75f, (tempDiff - 1f) / 4f);
            else if (tempDiff < 10f)
                return Mathf.Lerp(75f, 50f, (tempDiff - 5f) / 5f);
            else
                return Mathf.Max(25f, 50f - tempDiff);
        }

        private Color GetEfficiencyColor()
        {
            float efficiency = GetEfficiency();
            return ExpanseUI.GetPercentageColor(efficiency / 100f);
        }

        private string GetSystemStatus()
        {
            if (tempControl.parent.IsBrokenDown())
                return "FAULT";

            CompPowerTrader powerComp = tempControl.parent.GetComp<CompPowerTrader>();
            if (powerComp != null && !powerComp.PowerOn)
                return "OFFLINE";

            float currentTemp = tempControl.parent.Position.GetTemperature(tempControl.parent.Map);
            float targetTemp = tempControl.targetTemperature;
            float tempDiff = Mathf.Abs(currentTemp - targetTemp);

            if (tempDiff > 10f)
                return "CRITICAL";
            else if (tempDiff > 5f)
                return "WARNING";
            else if (tempControl.operatingAtHighPower)
                return "ACTIVE";
            else
                return "NOMINAL";
        }
    }
}