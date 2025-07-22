using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalNode : CompProperties
    {
        public List<SignalPort> portsAvailable;

        public CompProperties_SignalNode()
        {
            compClass = typeof(Comp_SignalNode);
        }
    }

    public class Comp_SignalNode : ThingComp, ITileGroupedSignalNode
    {
        #region Props      
        public CompProperties_SignalNode NodeProps => (CompProperties_SignalNode)props;


        protected List<SignalPort> ConnectionPorts = new List<SignalPort>();
        public IEnumerable<Comp_SignalNode> Neighbors => AllConnectedPorts
            .Select(p => p.ConnectedNode)
            .Where(n => n != null)
            .Distinct();


        public List<SignalPort> AllPorts => ConnectionPorts.ToList();
        public List<SignalPort> AllConnectedPorts => ConnectionPorts.Where(x=> x.HasConnectTarget).ToList();
        public List<SignalPort> AllUnConnectedPorts => ConnectionPorts.Where(x => !x.HasConnectTarget).ToList();
        public List<SignalPort> AllOutPorts => ConnectionPorts.Where(x=> x.Type == SignalPortType.OUT || x.Type == SignalPortType.BOTH).ToList();
        public List<SignalPort> AllInPorts => ConnectionPorts.Where(x => x.Type == SignalPortType.IN || x.Type == SignalPortType.BOTH).ToList();

  
        public List<Comp_SignalNode> ConnectedParents => ConnectionPorts.Where(x => x.Type == SignalPortType.IN || x.Type == SignalPortType.BOTH).Select(p => p.ConnectedNode).ToList();
        public List<Comp_SignalNode> ConnectedChildren => ConnectionPorts.Where(x => x.Type == SignalPortType.OUT || x.Type == SignalPortType.BOTH).Select(p => p.ConnectedNode).ToList();
        public List<Comp_SignalNode> EnabledConnectedChildren => ConnectionPorts.Where(x => x.Type == SignalPortType.OUT || x.Type == SignalPortType.BOTH && x.Enabled).Select(p => p.ConnectedNode).ToList();



        public virtual bool IsPassiveNode => false;

        public virtual bool ExcludeFromNetworkDiscovery => false;

        #endregion

        protected SignalGroup signalGroup;
        public SignalGroup SignalGroup => signalGroup;
        public virtual bool CanFormSignalGroup => false;

        public virtual bool BelongsToGroup => signalGroup != null && signalGroup.IsPartOfGroup(this) && CanFormSignalGroup;



        public virtual bool CanSendSignal => true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad && ConnectionPorts != null)
            {
                foreach (var port in ConnectionPorts)
                {
                    port.OwnerNode = this;
                }
            }

            if (!respawningAfterLoad)
            {
                SetupDefaultPorts();
            }

            JoinOrFormSignalGroup();
            SignalConnectionMaker.UpdateConnectionsFor(this);
            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            Log.Message("signal node placed, rebuilding network");
            signalManager?.MarkNetworksDirty();
        }


        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            ClearPorts();

            if (CanFormSignalGroup && SignalGroup != null)
            {
                SignalGroup.LeaveGroup(this);
            }

            var signalManager = map?.GetComponent<SignalManager>();
            Log.Message("signal node removed, rebuilding network");
            signalManager?.MarkNetworksDirty();
        }


        public virtual void SendSignal(Signal signal)
        {
            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            if (signalManager != null)
            {
                signalManager.SendSignal(signal, this);
            }

            if (SignalGroup != null)
            {
                SignalGroup.PropagateSignal(signal, this);
            }
        }

        public virtual void TriggerSignal(float value)
        {
            var signal = new Signal
            {
                Value = value,
                LastChangeTick = Find.TickManager.TicksGame,
                SignalSource = this
            };

            SendSignal(signal);
        }

        public virtual void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            if (SignalGroup != null)
            {
                Comp_SignalNode senderNode = receivingPort.ConnectedNode;
                if (senderNode != null && SignalGroup.IsPartOfGroup(senderNode))
                {
                    return;
                }
                SignalGroup.PropagateSignal(signal, this);
            }
        }

        /// <summary>
        /// Called when a signal is received from another member of the same SignalGroup.
        /// </summary>
        public virtual void OnGroupSignalReceived(Signal signal, SignalGroup signalGroup)
        {

        }

        #region Port

        protected virtual void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            CreatePort(SignalPortType.OUT, IntVec3.Zero, "OUT", 0);
            CreatePort(SignalPortType.IN, IntVec3.Zero, "IN", 0);
        }
        public virtual void CreatePort(SignalPortType type, IntVec3 offset, string portName, int portIndex, bool showPort = true, bool autoConnectable = true)
        {
            ConnectionPorts.Add(new SignalPort(this, type, offset, null, portName, portIndex, showPort, autoConnectable));
        }
        public virtual void ClearPorts()
        {
            foreach (var item in ConnectionPorts.ToArray())
            {
                if (item.HasConnectTarget)
                {
                    item.Disconnect();
                }
            }

            ConnectionPorts.Clear();
        }
        #endregion


        #region Signal Group

        private void JoinOrFormSignalGroup()
        {
            if (!CanFormSignalGroup)
            {
                return;
            }
            foreach (var item in GetCardinalNodes())
            {
                if (item.FoundNode.CanFormSignalGroup && item.FoundNode.SignalGroup != null && item.FoundNode.SignalGroup.CanJoinGroup(this))
                {
                    JoinSignalGroup(item.FoundNode.SignalGroup);
                    return; 
                }
            }

            SignalGroup newGroup = new SignalGroup(this, $"{this.GetType()}");
            JoinSignalGroup(newGroup);
        }
        public void JoinSignalGroup(SignalGroup group)
        {
            if (signalGroup != null && signalGroup.IsPartOfGroup(this))
            {
                signalGroup.LeaveGroup(this);
            }

            signalGroup = group;
            signalGroup.JoinGroup(this);
            signalGroup.SelectBestOwner();
            SyncNetWorkToGroupOwner();
        }

        public virtual void SyncWithGroupNode(Comp_SignalNode senderNode)
        {
            if (senderNode == this)
            {
                return;
            }
        }

        private void SyncNetWorkToGroupOwner()
        {

            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            if (signalManager != null)
            {
                if (SignalGroup.OwnerNode != null)
                {
                    SignalNetwork ownerNetwork = signalManager.GetNetworkFor(SignalGroup.OwnerNode);

                    if (ownerNetwork != null)
                    {
                        SignalNetwork network = signalManager.GetNetworkFor(this);

                        if (network != null)
                        {
                            network.TransferTo(this, ownerNetwork);
                        }
                    }
                }
            }
        }

        #endregion

        public virtual bool IsSignalTerminal()
        {
            return ConnectedChildren.Count == 0;
        }

        public virtual bool IsSplitterNode()
        {
            return false;
        }


        #region Connections
        public virtual bool MakeConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string reason)
        {
            reason = string.Empty;
            myPort.Connect(otherPort);
            otherPort.Connect(myPort);
            return true;
        }
        public void MakeConnection(SignalPort myPort, SignalPort targetPort)
        {
            this.MakeConnection(myPort, targetPort.OwnerNode, targetPort, out _);
        }

        public virtual bool CanConnectTo(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string cantConnectReason, bool ignoreConnectionChecks = false)
        {
            cantConnectReason = string.Empty;

            if (myPort == null || otherNode == null || otherPort == null)
            {
                cantConnectReason = "Port or node is null.";
                return false;
            }

            bool myPortCanSend = myPort.Type == SignalPortType.OUT || myPort.Type == SignalPortType.BOTH;
            bool otherPortCanReceive = otherPort.Type == SignalPortType.IN || otherPort.Type == SignalPortType.BOTH;

            if (!myPortCanSend || !otherPortCanReceive)
            {
                cantConnectReason = "Port type mismatch: Connection must be from a sending port (OUT/BOTH) to a receiving port (IN/BOTH).";
                return false;
            }

            if (otherNode == this)
            {
                cantConnectReason = "Cannot connect to self.";
                return false;
            }

            if (myPort.HasConnectTarget || (!ignoreConnectionChecks && otherPort.HasConnectTarget))
            {
                cantConnectReason = "A port is already connected.";
                return false;
            }

            if (this.IsNodeDescendantOf(otherNode))
            {
                cantConnectReason = "Creates a circular dependency.";
                return false;
            }

            return true;
        }

        public virtual int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            return 1;
        }
 
        public bool IsAlreadyConnected(Comp_SignalNode otherNode)
        {
            return ConnectionPorts.Any(x => x.ConnectedNode == otherNode);
        }

        public virtual void DisconnectFromChild(Comp_SignalNode Other)
        {
            if (!TryGetConnectionPortForNode(Other, out SignalPort port))
            {
                Log.Message($"Cannot find");
                return;
            }
            port.Disconnect();
        }

        public bool TryGetConnectionPort(SignalPortType signalPortType, out SignalPort foundPort)
        {
            foundPort = null;
            if (HasConnectionPort(signalPortType))
            {
                foundPort = GetConnectionPort(signalPortType);
                return true;
            }

            return false;
        }
        public bool TryGetConnectionPortForNode(Comp_SignalNode node, out SignalPort foundPort)
        {
            foundPort = null;
            if (ConnectionPorts.Any(x => x.ConnectedNode == node))
            {
                foundPort = ConnectionPorts.FirstOrDefault(x => x.ConnectedNode == node);
                return true;
            }

            return false;
        }
        public virtual List<SignalPort> GetUnconnectedPorts(SignalPortType signalPortType)
        {
            return ConnectionPorts.Where(x => x.Type == signalPortType && !x.HasConnectTarget).ToList();
        }
        public SignalPort GetConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.FirstOrDefault(x => x.Type == signalPortType);
        }
        public bool HasConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.Any(x => x.Type == signalPortType);
        }

 
        #endregion


        public virtual IEnumerable<PropagationTarget> GetPropagationTargets()
        {
            foreach (var sendingPort in AllOutPorts.Where(x=> x.Enabled && x.HasConnectTarget && !x.ConnectedNode.IsSignalTerminal()))
            {
                var neighborNode = sendingPort.ConnectedNode;
                var receivingPort = sendingPort.ConnectedPort;

                // Check if the connection is valid and the neighbor can receive the signal.
                if (neighborNode != null && receivingPort != null && receivingPort.Enabled && CanSendSignal)
                {
                    yield return new PropagationTarget(neighborNode, receivingPort);
                }
            }
        }

        public Comp_SignalNode GetNodeAt(IntVec3 position)
        {
            if (!parent.Spawned || parent.Map == null || !position.InBounds(parent.Map))
            {
                return null;
            }

            List<Thing> thingList = position.GetThingList(parent.Map);
            foreach (Thing thing in thingList)
            {
                Comp_SignalNode comp = thing.TryGetComp<Comp_SignalNode>();
                if (comp != null && comp != this)
                    return comp;
            }
            return null;
        }
        public List<Comp_SignalNode> GetNodesAt(IntVec3 position)
        {
            if (!parent.Spawned || parent.Map == null || !position.InBounds(parent.Map))
            {
                return null;
            }

            List<Comp_SignalNode> foundNodes = new List<Comp_SignalNode>();

            List<Thing> thingList = position.GetThingList(parent.Map);
            foreach (Thing thing in thingList)
            {
                Comp_SignalNode comp = thing.TryGetComp<Comp_SignalNode>();
                if (comp != null && comp != this)
                {
                    foundNodes.Add(comp);
                }
            }

            return foundNodes;
        }
        public HashSet<SpatialNodeData> GetCardinalNodes()
        {
            HashSet<SpatialNodeData> foundNodes = new HashSet<SpatialNodeData>();

            Map map = this.parent.Map;

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(this.parent.Position))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                foreach (var item in GetNodesAt(cell))
                {
                    if (item != null && item != this)
                    {
                        IntVec3 directionToNode = (item.parent.Position - this.parent.Position).ClampMagnitude(1);
                        foundNodes.Add(new SpatialNodeData(item, directionToNode, Rot4.FromIntVec3(directionToNode)));
                    }
                }
            }
            return foundNodes;
        }

        public bool IsNodeDescendantOf(Comp_SignalNode target)
        {
            return IsNodeDescendantOf(target, new HashSet<Comp_SignalNode>());
        }
        private bool IsNodeDescendantOf(Comp_SignalNode target, HashSet<Comp_SignalNode> visited)
        {
            visited.Add(this);

            foreach (var child in ConnectedChildren)
            {
                if (child == null)
                    continue;

                if (child == target) 
                    return true;

                if (visited.Contains(child)) 
                    continue;

                if (child.IsNodeDescendantOf(target, visited))
                    return true;
            }
            return false;
        }


        protected virtual bool IsValidSelectionTarget(SignalPort sourcePort, SignalPort targetPort, out string notValidSelectionTargetReason)
        {
            notValidSelectionTargetReason = string.Empty;
            return sourcePort.OwnerNode.CanConnectTo(sourcePort, targetPort.OwnerNode, targetPort, out _);
        }
        public void ShowSourceSelectionMenu(Func<SignalPort, SignalPort, bool> validator = null)
        {
            var potentialSourcePorts = this.AllUnConnectedPorts
                .Where(p => p.Type == SignalPortType.OUT || p.Type == SignalPortType.BOTH)
                .ToList();

            if (!potentialSourcePorts.Any())
            {
                Messages.Message("No Available Output Ports", MessageTypeDefOf.RejectInput, false);
                return;
            }

            TargetingParameters parameters = new TargetingParameters
            {
                canTargetBuildings = true,
                canTargetPawns = false
            };

            Find.Targeter.BeginTargeting(parameters, (LocalTargetInfo target) =>
            {
                if (!target.HasThing || !target.Thing.TryGetComp(out Comp_SignalNode targetNode)) return;

                var potentialTargetPorts = targetNode.AllUnConnectedPorts
                    .Where(p => p.Type == SignalPortType.IN || p.Type == SignalPortType.BOTH)
                    .ToList();

                var validConnections = new List<(SignalPort source, SignalPort target)>();
                foreach (var sourcePort in potentialSourcePorts)
                {
                    foreach (var targetPort in potentialTargetPorts)
                    {
                        if (IsValidSelectionTarget(sourcePort, targetPort, out string notValidSelectionReason) && (validator == null || validator(sourcePort, targetPort)))
                        {
                            validConnections.Add((sourcePort, targetPort));
                        }
                        else Messages.Message(notValidSelectionReason, MessageTypeDefOf.RejectInput, false);
                    }
                }

                if (!validConnections.Any())
                {
                    Messages.Message("No Valid Connections", MessageTypeDefOf.RejectInput, false);
                    return;
                }

                var options = new List<FloatMenuOption>();
                foreach (var connection in validConnections)
                {
                    string label = $"Connect {connection.source.PortName} to {connection.target.OwnerNode.parent.LabelCap}'s {connection.target.PortName}";
                    var option = new FloatMenuOption(label, () => MakeConnection(connection.source, connection.target));
                    options.Add(option);
                }

                Find.WindowStack.Add(new FloatMenu(options));
            });
        }


        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            DrawConnectionLine();
            DrawPortIndicators();


            if (SignalGroup != null)
            {
                SignalGroup.OnGroupMemberSelected(this);
            }
        }
 
        private void DrawConnectionLine()
        {

            if (ConnectedChildren != null && ConnectedChildren.Count > 0)
            {
                GenDraw.DrawCircleOutline(this.parent.DrawPos, 0.5f, SimpleColor.Green);

                foreach (var child in ConnectedChildren)
                {
                    if (child == null) continue;

                    var childPort = child.AllInPorts
                        .FirstOrDefault(p => p.ConnectedNode == this);
                    var isEnabled = childPort != null && childPort.Enabled;

                    var color = isEnabled ? SimpleColor.Green : SimpleColor.Red;

                    GenDraw.DrawCircleOutline(child.parent.DrawPos, 0.5f, color);

                    DrawSignalFlowLine(this.parent.DrawPos, child.parent.DrawPos, color);
                }
            }
        }
        private void DrawPortIndicators()
        {
            foreach (var port in ConnectionPorts)
            {
                Vector3 portWorldPos = port.WorldPosition.ToVector3Shifted();
                portWorldPos.y = parent.DrawPos.y;

                SimpleColor color = port.Type == SignalPortType.IN ? SimpleColor.Blue : SimpleColor.Yellow;
                float size = 0.2f;

                GenDraw.DrawCircleOutline(portWorldPos, size, color);

                if (!port.PortName.NullOrEmpty())
                {
                    Vector3 labelPos = portWorldPos + new Vector3(0, 0, 0.3f);
                    GenDraw.DrawFieldEdges(new List<IntVec3>()
                    {
                        this.parent.Position + port.LocalOffset
                    }, port.Type == SignalPortType.IN ? Color.green : Color.yellow);
                }

                if (port.HasConnectTarget)
                {
                    var targetPort = port.ConnectedNode.ConnectionPorts
                        .FirstOrDefault(p => p.ConnectedNode == this);

                    if (targetPort != null)
                    {
                        Vector3 targetPos = targetPort.WorldPosition.ToVector3Shifted();
                        targetPos.y = port.ConnectedNode.parent.DrawPos.y;

                        GenDraw.DrawLineBetween(portWorldPos, targetPos,
                            port.Enabled ? SimpleColor.Green : SimpleColor.Red);
                    }
                }
            }
        }
        private void DrawSignalFlowLine(Vector3 start, Vector3 end, SimpleColor color)
        {
            var midPoint = (start + end) * 0.5f;
            var direction = (end - start).normalized;

            GenDraw.DrawLineBetween(start, end, color);

            var arrowSize = 0.15f;
            var arrowAngle = 30f;

            var perpendicular = Vector3.Cross(direction, Vector3.up);
            var arrowLeft = Quaternion.AngleAxis(arrowAngle, Vector3.up) * -direction * arrowSize;
            var arrowRight = Quaternion.AngleAxis(-arrowAngle, Vector3.up) * -direction * arrowSize;

            GenDraw.DrawLineBetween(midPoint, midPoint + arrowLeft, color);
            GenDraw.DrawLineBetween(midPoint, midPoint + arrowRight, color);
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra() ?? "";

            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            if (signalManager != null)
            {
               SignalNetwork network = signalManager.GetNetworkFor(this);

                if (network != null)
                {
                    baseString += $"{network.networkID}\r\n";
                }
            }
            return baseString.TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var port in AllPorts)
            {
                if (port.ShowPort)
                {
                    yield return new Gizmo_PortConfig(this, port);
                }        
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref ConnectionPorts, "connectionPorts", LookMode.Deep);
        }
    }
}
