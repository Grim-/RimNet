using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{

    public class Module_CompPowerTrader : NetworkUIModule
    {
        private CompPowerTrader powerTrader;
        private CompFlickable flickableComp;
        private CompStunnable stunnableComp;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompPowerTrader>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            if (node?.parent == null)
                return;

            powerTrader = node.parent.GetComp<CompPowerTrader>();
            flickableComp = node.parent.GetComp<CompFlickable>();
            stunnableComp = node.parent.GetComp<CompStunnable>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 24, 16, 16, 16, 16, 20);

            ExpanseUI.DrawHeader(sections[0], "PWR_TRADER");

            float powerValue = Mathf.Abs(powerTrader.PowerOutput);
            float maxPower = Mathf.Abs(powerTrader.Props.PowerConsumption);
            bool isOutputting = powerTrader.PowerOutput != 0;

            ExpanseUI.DrawSectionedProgressBar(
                sections[1],
                powerValue,
                maxPower,
                0f,
                maxPower,
                isOutputting ? "OUT" : "CONS",
                "W",
                true,
                GetPowerColor(isOutputting, powerTrader.PowerOn)
            );

            string powerText = $"{(isOutputting ? "OUT" : "CONS")}: {powerValue:F0}W";
            ExpanseUI.DrawStatusText(sections[2], "", powerText, GetPowerColor(isOutputting, powerTrader.PowerOn));

            string maxText = $"MAX: {maxPower:F0}W";
            ExpanseUI.DrawStatusText(sections[3], "", maxText);

            string energyText = $"E/T: {powerTrader.EnergyOutputPerTick:F3}";
            ExpanseUI.DrawStatusText(sections[4], "", energyText, ExpanseUI.Gray);

            string status = GetSystemStatus();
            ExpanseUI.DrawStatusText(sections[5], "STS: ", status, ExpanseUI.GetStatusColor(status));

            CompFlickable flickableComp = powerTrader.parent.GetComp<CompFlickable>();
            if (flickableComp != null)
            {
                string buttonText = powerTrader.PowerOn ? "SHUTDOWN" : "STARTUP";
                Color buttonColor = powerTrader.PowerOn ? ExpanseUI.Red : ExpanseUI.Green;

                if (ExpanseUI.DrawButton(sections[6].InsetBy(2, 2, 2, 2), buttonText, buttonColor))
                {
                    if (powerTrader.PowerOn)
                    {
                        flickableComp.DoFlick();
                    }
                    else
                    {
                        if (flickableComp.SwitchIsOn)
                        {
                            powerTrader.PowerOn = true;
                        }
                        else
                        {
                            flickableComp.DoFlick();
                        }
                    }
                }
            }

            ExpanseUI.EndExpanseStyle();
        }

        private Color GetPowerColor(bool isOutputting, bool powerOn)
        {
            if (!powerOn)
                return ExpanseUI.Gray;

            if (isOutputting)
                return ExpanseUI.Green;
            else
                return ExpanseUI.Orange;
        }

        private string GetSystemStatus()
        {
            if (powerTrader.parent.IsBrokenDown())
                return "FAULT";

            if (stunnableComp != null && stunnableComp.StunHandler.Stunned && stunnableComp.StunHandler.StunFromEMP)
                return "EMP_STUN";

            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return "OFFLINE";

            if (!powerTrader.PowerOn)
                return "INACTIVE";

            if (powerTrader.PowerOutput != 0)
                return "GENERATING";
            else
                return "CONSUMING";
        }

        public override void UpdateData()
        {
        }
    }
}