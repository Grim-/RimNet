using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_CompFlickable : NetworkUIModule
    {
        private CompFlickable Flickable;

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing.TryGetComp<CompFlickable>() != null;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            Flickable = node.parent.TryGetComp<CompFlickable>();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 24);

            ExpanseUI.DrawHeader(sections[0], "SWITCH");

            if (Flickable != null)
            {
                string buttonText = Flickable.SwitchIsOn ? "SHUTDOWN" : "STARTUP";
                Color buttonColor = Flickable.SwitchIsOn ? ExpanseUI.Red : ExpanseUI.Green;

                if (ExpanseUI.DrawButton(sections[1].InsetBy(2, 2, 2, 2), buttonText, buttonColor))
                {
                    Flickable.DoFlick();
                }
            }

            ExpanseUI.EndExpanseStyle();
        }

        public override void UpdateData()
        {
        }
    }
}