using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_SignalDelayNode : ITab_SignalNodeBase
    {
        private CompSignalDelayNode SelectedDelayNode => SelectedNode as CompSignalDelayNode;

        public override bool IsVisible => SelThing?.TryGetComp<CompSignalDelayNode>() != null;

        public ITab_SignalDelayNode()
        {
            labelKey = "Signal Delay";
            size = new Vector2(480f, 250f);
        }

        protected override void DrawMainContent(Rect rect)
        {
            if (SelectedDelayNode == null) return;

            var listing = new Listing_Standard();
            listing.Begin(rect);

            RimNetGUI.DrawHeader(listing.GetRect(30f), "Signal Delay Configuration");
            listing.Gap(12f);

            string delayLabel = $"Delay: {SelectedDelayNode.DelayTicks}t ({(SelectedDelayNode.DelayTicks / 60f):F2}s)";
            SelectedDelayNode.DelayTicks = (int)RimNetGUI.DrawStyledSlider(listing.GetRect(24f), SelectedDelayNode.DelayTicks, 1, 600, delayLabel);
            listing.Gap(12f);

            string modeLabel = "Mode: " + (SelectedDelayNode.UseCustomSignal ? "Custom Signal" : "Forward Signal");
            if (RimNetGUI.StyledButton(listing.GetRect(30f), modeLabel))
            {
                SelectedDelayNode.UseCustomSignal = !SelectedDelayNode.UseCustomSignal;
            }
            listing.Gap(6f);

            if (SelectedDelayNode.UseCustomSignal)
            {
                string valueLabel = $"Value: {SelectedDelayNode.CustomSignalValue:F2}";
                SelectedDelayNode.CustomSignalValue = RimNetGUI.DrawStyledSlider(listing.GetRect(24f), SelectedDelayNode.CustomSignalValue, 0f, 1f, valueLabel);
            }

            listing.End();
        }
    }
}
