using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public abstract class ITab_SignalNodeBase : ITab
    {
        #region Constants
        private const float ToolbarHeight = 30f;
        private const float SidePanelWidth = 200f;
        private const float Spacing = 10f;
        private const float ToggleButtonWidth = 30f;
        #endregion

        #region Fields
        private bool sidePanelIsOpen = false;
        protected Comp_SignalNode selectedNode;
        public static readonly Texture2D SidePanelToggleIcon = TexButton.OpenInspectSettings;
        #endregion

        #region Properties
        public override bool IsVisible => SelThing?.TryGetComp<Comp_SignalNode>() != null;

        protected Comp_SignalNode SelectedNode => SelThing.TryGetComp<Comp_SignalNode>();
        #endregion

        public ITab_SignalNodeBase()
        {
            size = new Vector2(500f, 450f);
            labelKey = "SignalNode";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            selectedNode = SelThing.TryGetComp<Comp_SignalNode>();
        }

        protected override void FillTab()
        {
            if (SelectedNode == null)
            {
                Log.Error("ITab_SignalNodeBase could not find Comp_SignalNode on selected thing.");
                return;
            }

            Rect mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(Spacing);

            Rect toolbarRect = new Rect(mainRect.x, mainRect.y, mainRect.width, ToolbarHeight);
            DrawToolbar(toolbarRect);

            float mainContentY = toolbarRect.yMax + Spacing;
            float mainContentHeight = mainRect.height - ToolbarHeight - Spacing;
            float mainContentWidth = sidePanelIsOpen ? mainRect.width - SidePanelWidth - Spacing : mainRect.width;

            Rect mainContentRect = new Rect(mainRect.x, mainContentY, mainContentWidth, mainContentHeight);
            Widgets.DrawBoxSolidWithOutline(mainContentRect, new Color(0.15f, 0.15f, 0.15f), new Color(0.4f, 0.4f, 0.4f));
            DrawMainContent(mainContentRect.ContractedBy(Spacing));


            if (sidePanelIsOpen)
            {
                Rect sidePanelRect = new Rect(mainContentRect.xMax + Spacing, mainContentY, SidePanelWidth, mainContentHeight);
                Widgets.DrawBoxSolidWithOutline(sidePanelRect, new Color(0.18f, 0.18f, 0.18f), new Color(0.4f, 0.4f, 0.4f));


                Listing_Standard sidePanelLister = new Listing_Standard();
                sidePanelLister.Begin(sidePanelRect.ContractedBy(Spacing));
                DrawSidePanelContent(sidePanelLister);
                sidePanelLister.End();
            }
        }


        protected virtual void DrawToolbar(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.25f, 0.25f, 0.25f));
            var layout = new RowLayoutManager(rect, Spacing);

            DrawToolbarContent(layout);
  
            Rect toggleButtonRect = new Rect(rect.xMax - ToggleButtonWidth - Spacing, rect.y, ToggleButtonWidth, rect.height);
            if (Widgets.ButtonImage(toggleButtonRect, SidePanelToggleIcon))
            {
                sidePanelIsOpen = !sidePanelIsOpen;
            }
            TooltipHandler.TipRegion(toggleButtonRect, "Toggle Details Panel");
        }


        protected virtual void DrawToolbarContent(RowLayoutManager layout)
        {

        }

        protected virtual void DrawMainContent(Rect rect)
        {

        }

        protected virtual void DrawSidePanelContent(Listing_Standard listing)
        {
            if (SelectedNode != null && SelectedNode.BelongsToGroup)
            {
                Rect syncButtonRect = listing.GetRect(30f);

                if (RimNetGUI.StyledButton(syncButtonRect, "Sync to group"))
                {
                    SelectedNode.SignalGroup.SyncGroup(SelectedNode);
                }

                Rect manageGroupRect = listing.GetRect(30f);
                if (RimNetGUI.StyledButton(manageGroupRect, "Manage signal group"))
                {
                    Find.WindowStack.Add(new Window_SignalGroup(SelectedNode.SignalGroup, SelectedNode));
                }
            }
            listing.End();
        }
    }
}
