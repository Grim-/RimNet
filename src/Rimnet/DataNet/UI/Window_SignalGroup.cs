using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Window_SignalGroup : Window
    {
        private readonly SignalGroup group;
        private readonly Comp_SignalNode selectedNode;
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(400f, 500f);

        public Window_SignalGroup(SignalGroup signalGroup, Comp_SignalNode currentlySelectedNode)
        {
            group = signalGroup;
            selectedNode = currentlySelectedNode;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (group == null)
            {
                Widgets.Label(inRect, "Error: No Signal Group provided.");
                Close();
                return;
            }

            var listing = new Listing_Standard();
            listing.Begin(inRect);

            Text.Font = GameFont.Medium;
            listing.Label($"Group: {group.GroupLabel}");
            Text.Font = GameFont.Small;
            listing.GapLine();

            listing.Label($"Current Leader: {group.OwnerNode?.parent.LabelCap ?? "None"}");
            listing.Gap(12f);

            if (selectedNode != null && group.IsPartOfGroup(selectedNode))
            {
                if (!group.IsGroupOwner(selectedNode))
                {
                    if (listing.ButtonText("Set this node as Leader"))
                    {
                        group.PromoteToOwner(selectedNode);
                    }
                }

                if (listing.ButtonText("Sync Settings from this Node to Group"))
                {
                    group.SyncGroup(selectedNode);
                    Messages.Message($"Settings from {selectedNode.parent.LabelCap} synced to the group.", MessageTypeDefOf.PositiveEvent);
                }
            }

            listing.GapLine();


            listing.Label("Group Members:");

            Rect scrollViewRect = listing.GetRect(250f);
            Widgets.DrawBoxSolidWithOutline(scrollViewRect, new Color(0.15f, 0.15f, 0.15f), new Color(0.4f, 0.4f, 0.4f));

            float viewHeight = group.AllNodes.Count * 30f;
            Rect viewRect = new Rect(0, 0, scrollViewRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, viewRect);

            var innerListing = new Listing_Standard();
            innerListing.Begin(viewRect);

            foreach (var node in group.AllNodes)
            {
                string label = node.parent.LabelCap;
                if (group.IsGroupOwner(node))
                {
                    label += " (Leader)";
                }

                Rect rowRect = innerListing.GetRect(28f);

                if (node == selectedNode)
                {
                    Widgets.DrawHighlight(rowRect);
                }
                else if(node != selectedNode && Mouse.IsOver(rowRect))
                {
                    Widgets.DrawStrongHighlight(rowRect, Color.yellow);
                }

                Widgets.Label(rowRect.ContractedBy(4f), label);
            }

            innerListing.End();
            Widgets.EndScrollView();
            listing.End();
        }
    }
}
