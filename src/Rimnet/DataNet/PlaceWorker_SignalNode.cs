using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    //not very good or useful, yet
    public class PlaceWorker_SignalNode : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);

            var comp = def.GetCompProperties<CompProperties_SignalNode>();
            if (comp == null) return;

            Map map = Find.CurrentMap;
            if (map == null) return;

            var nearbyNodes = GetNearbySignalNodes(center, map, 10f);

            foreach (var node in nearbyNodes)
            {
                DrawPotentialConnections(center, node, ghostCol);
            }

            DrawPortIndicators(center, rot, comp);
        }

        private List<Comp_SignalNode> GetNearbySignalNodes(IntVec3 center, Map map, float radius)
        {
            return map.listerThings.AllThings
                .Where(t => t.Position.DistanceTo(center) <= radius)
                .Select(t => t.TryGetComp<Comp_SignalNode>())
                .Where(n => n != null && !n.ExcludeFromNetworkDiscovery)
                .ToList();
        }

        private void DrawPotentialConnections(IntVec3 fromPos, Comp_SignalNode toNode, Color ghostCol)
        {
            var canConnect = CanAutoConnect(fromPos, toNode);
            var lineColor = canConnect ? Color.green : Color.red;
            lineColor.a = ghostCol.a * 0.5f;

            GenDraw.DrawLineBetween(fromPos.ToVector3Shifted(), toNode.parent.DrawPos);
        }

        private bool CanAutoConnect(IntVec3 fromPos, Comp_SignalNode toNode)
        {
            return toNode.GetUnconnectedPorts(SignalPortType.IN).Any() ||
                   toNode.GetUnconnectedPorts(SignalPortType.OUT).Any();
        }

        private void DrawPortIndicators(IntVec3 center, Rot4 rot, CompProperties_SignalNode props)
        {
            if (props.portsAvailable != null)
            {
                foreach (var port in props.portsAvailable)
                {
                    Vector3 portPos = center.ToVector3Shifted() + port.LocalOffset.RotatedBy(rot).ToVector3() * 0.5f;
                    Color portColor = port.Type == SignalPortType.IN ? Color.cyan : Color.green;
                    portColor.a = 0.8f;

                    GenDraw.DrawCircleOutline(portPos, 0.2f, SimpleColor.White);

                    if (port.Type == SignalPortType.IN)
                    {
                        GenDraw.DrawArrowPointingAt(portPos, false);
                    }
                    else
                    {
                        GenDraw.DrawArrowPointingAt(portPos, true);
                    }
                }
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var result = base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
            if (!result.Accepted) return result;

            var nearbyNodes = GetNearbySignalNodes(loc, map, 2f);

            foreach (var node in nearbyNodes)
            {
                if (node.parent.def == checkingDef && node.CanFormSignalGroup)
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return result;
        }
    }
}
