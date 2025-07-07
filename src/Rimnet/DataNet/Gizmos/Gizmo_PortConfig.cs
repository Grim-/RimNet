using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public class Gizmo_PortConfig : Gizmo
    {
        private Comp_SignalNode parentNode;
        private SignalPort port;
        private static readonly Vector2 GizmoSize = new Vector2(150f, 75f);
        private static readonly Color ConnectedColor = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color DisconnectedColor = new Color(0.8f, 0.2f, 0.2f);
        private static readonly Texture2D PortIcon = TexButton.Add ?? BaseContent.BadTex;

        public Gizmo_PortConfig(Comp_SignalNode node, SignalPort port)
        {
            this.parentNode = node;
            this.port = port;
            Order = -50f;
        }

        public override float GetWidth(float maxWidth)
        {
            return GizmoSize.x;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GizmoSize.x, GizmoSize.y);
            bool mouseOver = Mouse.IsOver(rect);

            // Background
            GUI.color = mouseOver ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            Widgets.DrawWindowBackground(rect);
            GUI.color = Color.white;

            // Port icon
            Rect iconRect = new Rect(rect.x + 5f, rect.y + 5f, 32f, 32f);
            GUI.DrawTexture(iconRect, PortIcon);

            // Port name
            Text.Font = GameFont.Small;
            Rect labelRect = new Rect(iconRect.xMax + 5f, rect.y + 5f, rect.width - iconRect.width - 15f, 20f);
            Widgets.Label(labelRect, port.PortName);

            // Connection status
            Text.Font = GameFont.Tiny;
            Rect statusRect = new Rect(rect.x + 5f, iconRect.yMax + 2f, rect.width - 10f, 20f);
            GUI.color = port.HasConnectTarget ? ConnectedColor : DisconnectedColor;
            string statusText = port.HasConnectTarget
                ? $"→ {port.ConnectedNode.parent.Label}"
                : "Not connected";
            Widgets.Label(statusRect, statusText);
            GUI.color = Color.white;

            // Current value if connected
            if (port.HasConnectTarget && parentNode is Comp_SignalLogicGate logicGate)
            {
                Rect valueRect = new Rect(rect.x + 5f, statusRect.yMax, rect.width - 10f, 15f);
                float value = logicGate.GetPortValue(port);
                Widgets.Label(valueRect, $"Value: {value:F2}");
            }

            Text.Font = GameFont.Small;

            // Tooltip
            if (mouseOver)
            {
                string tip = port.HasConnectTarget
                    ? $"Connected to {port.ConnectedNode.parent.Label}\nClick to change connection\nRight-click to disconnect"
                    : "Click to select input source";
                TooltipHandler.TipRegion(rect, tip);
            }

            // Handle clicks
            if (Widgets.ButtonInvisible(rect))
            {
                if (Event.current.button == 0) // Left click
                {
                    parentNode.ShowSourceSelectionMenu();
                    return new GizmoResult(GizmoState.Interacted);
                }
                else if (Event.current.button == 1 && port.HasConnectTarget)
                {
                    DisconnectPort();
                    return new GizmoResult(GizmoState.Interacted);
                }
            }

            return new GizmoResult(GizmoState.Clear);
        }




        private void DisconnectPort()
        {
            if (port.HasConnectTarget)
            {
                port.ConnectedNode.DisconnectFromChild(parentNode);
                port.Disconnect();

                var signalManager = parentNode.parent.Map?.GetComponent<SignalManager>();
                signalManager?.MarkNetworksDirty();

                Messages.Message($"Disconnected {port.PortName}", MessageTypeDefOf.NeutralEvent, false);
            }
        }
    }
}
