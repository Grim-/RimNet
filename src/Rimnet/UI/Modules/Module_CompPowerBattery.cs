using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_CompPowerBattery : NetworkUIModule
    {
        private CompPowerBattery battery;

        private static readonly Color ExpanseBlue = new Color(0.1f, 0.3f, 0.6f, 1f);
        private static readonly Color ExpanseOrange = new Color(1f, 0.5f, 0.1f, 1f);
        private static readonly Color ExpanseGreen = new Color(0.2f, 0.8f, 0.3f, 1f);
        private static readonly Color ExpanseGray = new Color(0.2f, 0.25f, 0.3f, 1f);
        private static readonly Color ExpanseText = new Color(0.8f, 0.9f, 1f, 1f);

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompPowerBattery>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            battery = node.parent.GetComp<CompPowerBattery>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            float currentY = rect.y;
            Color originalColor = GUI.color;
            Color originalBgColor = GUI.backgroundColor;

            GUI.backgroundColor = ExpanseBlue;
            GUI.Box(rect, "");
            GUI.backgroundColor = originalBgColor;

            Rect headerRect = new Rect(rect.x + 2, currentY + 2, rect.width - 4, 18);
            ExpanseUI.DrawAngularPanel(headerRect, ExpanseGray, ExpanseGray.ChangeAlpha(0.3f));

            GUI.color = ExpanseText;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(headerRect.x + 4, headerRect.y, headerRect.width - 8, headerRect.height), "PWR_CELL");
            Text.Anchor = TextAnchor.UpperLeft;

            currentY += 22;

            float barHeight = 24;
            Rect barRect = new Rect(rect.x + 6, currentY, rect.width - 12, barHeight);
            DrawBatteryBar(barRect, battery.StoredEnergyPct);

            currentY += barHeight + 4;

            GUI.color = ExpanseText;
            Text.Font = GameFont.Tiny;
            string statusText = $"STORED: {battery.StoredEnergy:F0}/{battery.Props.storedEnergyMax:F0} Wd";
            Widgets.Label(new Rect(rect.x + 6, currentY, rect.width - 12, 16), statusText);

            currentY += 16;
            string efficiencyText = $"EFF: {battery.Props.efficiency * 100f:F0}%";
            Color effColor = battery.Props.efficiency >= 0.8f ? ExpanseGreen : ExpanseOrange;
            GUI.color = effColor;
            Widgets.Label(new Rect(rect.x + 6, currentY, rect.width - 12, 16), efficiencyText);

            currentY += 16;

            string status = GetBatteryStatus();
            Color statusColor = GetStatusColor();
            GUI.color = statusColor;
            Widgets.Label(new Rect(rect.x + 6, currentY, rect.width - 12, 16), $"STS: {status}");

            GUI.color = originalColor;
        }

        private void DrawBatteryBar(Rect rect, float fillPct)
        {
            GUI.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            GUI.color = ExpanseGray;
            Widgets.DrawLine(new Vector2(rect.x + 2, rect.y), new Vector2(rect.xMax, rect.y), ExpanseGray, 1f);
            Widgets.DrawLine(new Vector2(rect.xMax, rect.y), new Vector2(rect.xMax - 2, rect.yMax), ExpanseGray, 1f);
            Widgets.DrawLine(new Vector2(rect.xMax - 2, rect.yMax), new Vector2(rect.x, rect.yMax), ExpanseGray, 1f);
            Widgets.DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.x + 2, rect.y), ExpanseGray, 1f);

 
            if (fillPct > 0f)
            {
                Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * fillPct, rect.height - 4);
                Color fillColor = GetBatteryFillColor(fillPct);
                GUI.color = fillColor;
                GUI.DrawTexture(fillRect, BaseContent.WhiteTex);

                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.3f);
                Rect glowRect = new Rect(fillRect.x - 1, fillRect.y - 1, fillRect.width + 2, fillRect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            GUI.color = ExpanseText;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            string percentText = $"{fillPct * 100f:F0}%";
            Widgets.Label(rect, percentText);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Color GetBatteryFillColor(float fillPct)
        {
            if (fillPct > 0.6f)
                return ExpanseGreen;
            else if (fillPct > 0.3f)
                return ExpanseOrange;
            else
                return new Color(0.8f, 0.2f, 0.2f, 1f);
        }

        private string GetBatteryStatus()
        {
            if (battery.parent.IsBrokenDown())
                return "FAULT";
            if (battery.StunnedByEMP)
                return "EMP_STUN";
            if (battery.StoredEnergyPct < 0.1f)
                return "CRITICAL";
            if (battery.StoredEnergyPct < 0.3f)
                return "LOW";
            return "NOMINAL";
        }

        private Color GetStatusColor()
        {
            string status = GetBatteryStatus();
            switch (status)
            {
                case "FAULT":
                case "CRITICAL":
                    return new Color(0.8f, 0.2f, 0.2f, 1f);
                case "EMP_STUN":
                case "LOW":
                    return ExpanseOrange;
                default:
                    return ExpanseGreen;
            }
        }

        public override void UpdateData()
        {
        }
    }
}