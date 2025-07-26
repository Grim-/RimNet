using UnityEngine;
using Verse;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyStatus : Gizmo
    {
        public CompDronePowerSource EnergyComp;
        public string customLabel;
        public Color barColor = new Color(0.8f, 0.8f, 0.2f);
        public bool LerpColor = true;
        public Color EmptyColor = Color.red;
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        private static readonly float CollapsedHeight = 24f;
        private static readonly float ExpandedHeight = 75f;

        public Gizmo_EnergyStatus(CompDronePowerSource compDronePowerSource)
        {
            EnergyComp = compDronePowerSource;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            float height = ExpandedHeight;
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), height);
            Rect innerRect = rect.ContractedBy(6f);

            Widgets.DrawWindowBackground(rect);

            DrawExpanded(rect, innerRect, maxWidth, parms);

            string tooltipText = $"Energy: {EnergyComp.Current:F0} / {EnergyComp.Max:F0}";
            TooltipHandler.TipRegion(rect, tooltipText);

            return new GizmoResult(GizmoState.Clear);
        }

        public virtual void DrawCollapsed(Rect innerRect, float maxWidth, GizmoRenderParms parms)
        {
            Text.Font = GameFont.Tiny;
            Rect miniLabelRect = new Rect(innerRect.x, innerRect.y, innerRect.width - 26f, CollapsedHeight);
            Widgets.Label(miniLabelRect, $"{customLabel ?? EnergyComp.parent.LabelCap}: {(EnergyComp.AsPercent * 100f):F0}%");
        }

        public virtual void DrawExpanded(Rect rect, Rect innerRect, float maxWidth, GizmoRenderParms parms)
        {
            Rect labelRect = innerRect;
            labelRect.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, customLabel ?? EnergyComp.parent.LabelCap);

            Rect barRect = innerRect;
            barRect.yMin = labelRect.yMax;
            Color lerpColored = Color.Lerp(EmptyColor, barColor, EnergyComp.AsPercent);
            var barTex = SolidColorMaterials.NewSolidColorTexture(lerpColored);
            Widgets.FillableBar(barRect, EnergyComp.AsPercent, barTex, EmptyBarTex, false);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, $"{(EnergyComp.AsPercent * 100f):F0}%");
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }


}