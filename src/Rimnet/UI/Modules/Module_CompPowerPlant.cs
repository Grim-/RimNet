using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_CompPowerPlant : NetworkUIModule
    {
        private CompPowerPlant powerPlant;
        private CompRefuelable refuelableComp;
        private CompBreakdownable breakdownableComp;
        private CompAutoPowered autoPoweredComp;
        private CompFlickable flickableComp;
        private CompToxifier toxifier;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompPowerPlant>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            powerPlant = node.parent.GetComp<CompPowerPlant>();
            refuelableComp = node.parent.GetComp<CompRefuelable>();
            breakdownableComp = node.parent.GetComp<CompBreakdownable>();
            autoPoweredComp = node.parent.GetComp<CompAutoPowered>();
            toxifier = node.parent.GetComp<CompToxifier>();
            flickableComp = node.parent.GetComp<CompFlickable>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections;

            if (refuelableComp != null)
            {
                sections = ExpanseUI.SplitRectVertical(rect.Inset(2), 18, 24, 16, 16, 16, 16, 16);
            }
            else
            {
                sections = ExpanseUI.SplitRectVertical(rect.Inset(2), 18, 16, 16, 16, 16, 16);
            }

            ExpanseUI.DrawHeader(sections[0], "POWER_GEN");

            float currentOutput = powerPlant.PowerOutput;
            float maxOutput = Mathf.Abs(powerPlant.Props.PowerConsumption);

            if (refuelableComp != null)
            {
                float fuelPercent = refuelableComp.FuelPercentOfMax;
                ExpanseUI.DrawSectionedProgressBar(
                    sections[1],
                    fuelPercent * refuelableComp.Props.fuelCapacity,
                    refuelableComp.Props.fuelCapacity,
                    0f,
                    refuelableComp.Props.fuelCapacity,
                    "FUEL",
                    "",
                    false,
                    GetFuelColor(fuelPercent)
                );

                string outputText = $"OUT: {currentOutput:F0}W";
                ExpanseUI.DrawStatusText(sections[2], "", outputText, GetPowerColor(currentOutput, maxOutput));

                string maxText = $"MAX: {maxOutput:F0}W";
                ExpanseUI.DrawStatusText(sections[3], "", maxText);

                string efficiencyText = $"EFFC: {GetEfficiency():F0}%";
                ExpanseUI.DrawStatusText(sections[4], "", efficiencyText, GetEfficiencyColor());

                string uptimeText = $"UPTIME: {GetUptimeDisplay()}";
                ExpanseUI.DrawStatusText(sections[5], "", uptimeText, ExpanseUI.Gray);

                string status = GetSystemStatus();
                ExpanseUI.DrawStatusText(sections[6], "STS: ", status, ExpanseUI.GetStatusColor(status));
            }
            else
            {
                string outputText = $"OUT: {currentOutput:F0}W";
                ExpanseUI.DrawStatusText(sections[1], "", outputText, GetPowerColor(currentOutput, maxOutput));

                string maxText = $"MAX: {maxOutput:F0}W";
                ExpanseUI.DrawStatusText(sections[2], "", maxText);

                string efficiencyText = $"EFFC: {GetEfficiency():F0}%";
                ExpanseUI.DrawStatusText(sections[3], "", efficiencyText, GetEfficiencyColor());

                string uptimeText = $"UPTIME: {GetUptimeDisplay()}";
                ExpanseUI.DrawStatusText(sections[4], "", uptimeText, ExpanseUI.Gray);

                string status = GetSystemStatus();
                ExpanseUI.DrawStatusText(sections[5], "STS: ", status, ExpanseUI.GetStatusColor(status));
            }

            ExpanseUI.EndExpanseStyle();
        }

        private Color GetFuelColor(float fuelPercent)
        {
            if (fuelPercent > 0.5f)
                return ExpanseUI.Green;
            else if (fuelPercent > 0.2f)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Red;
        }

        private Color GetPowerColor(float current, float max)
        {
            if (current <= 0f)
                return ExpanseUI.Gray;

            float ratio = current / max;
            if (ratio >= 0.9f)
                return ExpanseUI.Green;
            else if (ratio >= 0.5f)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Red;
        }

        private float GetEfficiency()
        {
            if (powerPlant.Props.PowerConsumption == 0f)
                return 100f;

            float maxOutput = Mathf.Abs(powerPlant.Props.PowerConsumption);
            float currentOutput = powerPlant.PowerOutput;

            if (maxOutput == 0f)
                return 100f;

            return (currentOutput / maxOutput) * 100f;
        }

        private Color GetEfficiencyColor()
        {
            float efficiency = GetEfficiency();
            return ExpanseUI.GetPercentageColor(efficiency / 100f);
        }

        private string GetUptimeDisplay()
        {
            CompGlower glowerComp = powerPlant.parent.GetComp<CompGlower>();
            if (glowerComp != null && powerPlant.PowerOutput > 0f)
            {
                return "ACTIVE";
            }

            if (powerPlant.PowerOutput > 0f)
            {
                int ticks = Find.TickManager.TicksGame;
                int hours = ticks / 2500;
                int days = hours / 24;

                if (days > 0)
                    return $"{days}d";
                else
                    return $"{hours % 24}h";
            }

            return "OFFLINE";
        }

        private string GetSystemStatus()
        {
            if (breakdownableComp != null && breakdownableComp.BrokenDown)
                return "FAULT";

            if (refuelableComp != null && !refuelableComp.HasFuel)
                return "NO_FUEL";

            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return "OFFLINE";

            if (autoPoweredComp != null && !autoPoweredComp.WantsToBeOn)
                return "AUTO_OFF";

            if (toxifier != null && !toxifier.CanPolluteNow)
                return "POLLUTION";

            if (!powerPlant.PowerOn)
                return "INACTIVE";

            if (powerPlant.PowerOutput > 0f)
                return "GENERATING";

            return "STANDBY";
        }

        public override void UpdateData()
        {
        }
    }
}