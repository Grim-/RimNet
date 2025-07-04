using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_CompPowerTransmitter : NetworkUIModule
    {
        private CompPowerTransmitter transmitter;
        private CompFlickable flickable;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.GetComp<CompPowerTransmitter>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            transmitter = node.parent.GetComp<CompPowerTransmitter>();
            flickable = node.parent.GetComp<CompFlickable>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 18, 16, 16, 16, 16, 16);

            ExpanseUI.DrawHeader(sections[0], "PWR_TRANS");


            if (flickable != null)
            {
                string enabledText = $"PWR: {(flickable.SwitchIsOn ? "ON" : "OFF")}";

                if (ExpanseUI.DrawButton(sections[1], enabledText))
                {
                    flickable.DoFlick();
                }
            }



            string connectionText = $"CONN: {GetConnectionCount()}";
            ExpanseUI.DrawStatusText(sections[2], "", connectionText, GetConnectionColor());

            string flowText = $"FLOW: {GetPowerFlow():F0}W";
            ExpanseUI.DrawStatusText(sections[3], "", flowText, GetFlowColor());

            string loadText = $"LOAD: {GetLoadPercentage():F0}%";
            ExpanseUI.DrawStatusText(sections[4], "", loadText, GetLoadColor());

            string efficiencyText = $"EFFC: {GetEfficiency():F0}%";
            ExpanseUI.DrawStatusText(sections[5], "", efficiencyText, GetEfficiencyColor());

            string status = GetSystemStatus();
            ExpanseUI.DrawStatusText(sections[6], "STS: ", status, ExpanseUI.GetStatusColor(status));

            ExpanseUI.EndExpanseStyle();
        }

        private int GetConnectionCount()
        {
            if (transmitter?.PowerNet == null)
                return 0;

            return transmitter.PowerNet.powerComps.Count;
        }

        private Color GetConnectionColor()
        {
            int connections = GetConnectionCount();
            if (connections == 0)
                return ExpanseUI.Red;
            else if (connections < 3)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Green;
        }

        private float GetPowerFlow()
        {
            if (transmitter?.PowerNet == null)
                return 0f;

            return Mathf.Abs(transmitter.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick);
        }

        private Color GetFlowColor()
        {
            float flow = GetPowerFlow();
            if (flow == 0f)
                return ExpanseUI.Gray;
            else if (flow < 1000f)
                return ExpanseUI.Green;
            else if (flow < 5000f)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Red;
        }

        private float GetLoadPercentage()
        {
            if (transmitter?.PowerNet == null)
                return 0f;

            float stored = transmitter.PowerNet.CurrentStoredEnergy();
            float capacity = transmitter.transNet.CurrentStoredEnergy();

            if (capacity <= 0f)
                return 0f;

            return (stored / capacity) * 100f;
        }

        private Color GetLoadColor()
        {
            float load = GetLoadPercentage();
            if (load < 25f)
                return ExpanseUI.Red;
            else if (load < 75f)
                return ExpanseUI.Orange;
            else
                return ExpanseUI.Green;
        }

        private float GetEfficiency()
        {
            if (transmitter?.PowerNet == null)
                return 0f;

            if (transmitter.parent.IsBrokenDown())
                return 0f;

            return 100f;
        }

        private Color GetEfficiencyColor()
        {
            float efficiency = GetEfficiency();
            return ExpanseUI.GetPercentageColor(efficiency / 100f);
        }

        private string GetSystemStatus()
        {
            if (transmitter.parent.IsBrokenDown())
                return "FAULT";

            if (transmitter.PowerNet == null)
                return "ISOLATED";

            int connections = GetConnectionCount();
            if (connections == 0)
                return "NO_CONN";

            float flow = GetPowerFlow();
            if (flow > 10000f)
                return "OVERLOAD";
            else if (flow > 5000f)
                return "HIGH_LOAD";
            else if (flow > 0f)
                return "ACTIVE";
            else
                return "STANDBY";
        }

        public override void UpdateData()
        {
        }
    }
}