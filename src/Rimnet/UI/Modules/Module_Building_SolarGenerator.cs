using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_Building_SolarGenerator : NetworkUIModule
    {
        public override int Priority => 10; 

        private CompPowerPlantSolar solarComp;
        private CompPowerTrader powerTrader;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompPowerPlantSolar>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            solarComp = node.parent.GetComp<CompPowerPlantSolar>();
            powerTrader = node.parent.GetComp<CompPowerTrader>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 16, 16, 16, 16);

            ExpanseUI.DrawHeader(sections[0], "SOLAR_GEN");

            // Current output
            string outputText = $"OUT: {powerTrader.PowerOutput:F0}W";
            ExpanseUI.DrawStatusText(sections[1], "", outputText, ExpanseUI.Green);

            // Weather efficiency
            float weatherFactor = solarComp.parent.Map.skyManager.CurSkyGlow;
            string weatherText = $"WEATHER: {weatherFactor * 100f:F0}%";
            ExpanseUI.DrawStatusText(sections[2], "", weatherText, ExpanseUI.GetPercentageColor(weatherFactor));

            // Time efficiency 
            float timeEfficiency = Mathf.Clamp01(GenCelestial.CurCelestialSunGlow(solarComp.parent.Map));
            string timeText = $"SUN: {timeEfficiency * 100f:F0}%";
            ExpanseUI.DrawStatusText(sections[3], "", timeText, ExpanseUI.GetPercentageColor(timeEfficiency));

            string status = powerTrader.PowerOn ? "GENERATING" : "OFFLINE";
            ExpanseUI.DrawStatusText(sections[4], "STS: ", status, ExpanseUI.GetStatusColor(status));

            ExpanseUI.EndExpanseStyle();
        }

        public override void UpdateData()
        {
        }
    }
}