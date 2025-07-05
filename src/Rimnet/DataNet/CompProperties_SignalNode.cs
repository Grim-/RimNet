using RimWorld;
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

    public class Comp_SignalNode : ThingComp
    {
        protected List<SignalPort> ConnectionPorts = new List<SignalPort>();


        public List<SignalPort> AllPorts => ConnectionPorts.ToList();
        public List<SignalPort> AllConnectedPorts => ConnectionPorts.Where(x=> x.HasConnectTarget).ToList();
        public List<SignalPort> AllOutPorts => ConnectionPorts.Where(x=> x.Type == SignalPortType.OUT).ToList();

        public List<SignalPort> AllInPorts => ConnectionPorts.Where(x => x.Type == SignalPortType.IN).ToList();

        public CompProperties_SignalNode NodeProps => (CompProperties_SignalNode)props;
        public List<Comp_SignalNode> ConnectedParents => GetConnectedPorts(SignalPortType.IN).Select(p => p.ConnectedNode).ToList();
        public List<Comp_SignalNode> ConnectedChildren => GetConnectedPorts(SignalPortType.OUT).Select(p => p.ConnectedNode).ToList();
        public List<Comp_SignalNode> EnabledConnectedChildren => GetConnectedPorts(SignalPortType.OUT).Where(x=> x.Enabled).Select(p => p.ConnectedNode).ToList();

        public virtual bool IsPassiveNode => false;

        public virtual bool ExcludeFromNetworkDiscovery => false;

        public virtual int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            return 1;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                SetupDefaultPorts();
            }

            SignalConnectionMaker.AutoConnectAllOnMap(this.parent.Map);

            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            Log.Message("signal node placed, rebuilding network");
            signalManager?.MarkNetworksDirty();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);


            foreach (var item in ConnectedParents)
            {
                item.DisconnectFromChild(this);
            }

            foreach (var item in ConnectedChildren)
            {
                item.DisconnectFromParent(this);
            }

            var signalManager = map?.GetComponent<SignalManager>();
            Log.Message("signal node removed, rebuilding network");
             signalManager?.MarkNetworksDirty();
        }

        protected virtual void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.IN, IntVec3.Zero));
        }

        public virtual bool MakeConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string reason)
        {
            reason = string.Empty;

            myPort.Connect(otherNode);
            otherPort.Connect(this);
            //var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            //signalManager?.MarkNetworksDirty();
            return true;
        }

        public virtual void Propagate(Signal signal, HashSet<Comp_SignalNode> visited = null)
        {
            if (visited == null)
            {
                visited = new HashSet<Comp_SignalNode>();
            }

            if (visited.Contains(this))
                return;

            visited.Add(this);

            //Log.Message($"-- {this.parent.Label} recieved signal");
            OnSignalRecieved(signal);

            foreach (var child in EnabledConnectedChildren)
            {
                //Log.Message($"    -- >> {child.parent.Label}");
                child.Propagate(signal, visited);
            }
        }
        public virtual void SendSignal(Signal signal)
        {
            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            if (signalManager != null)
            {
                signalManager.SendSignal(signal, this);
            }
            else
            {
                var visited = new HashSet<Comp_SignalNode>();
                Propagate(signal, visited);
            }
        }




        public virtual void OnSignalRecieved(Signal signal)
        {
           // MoteMaker.ThrowText(this.parent.DrawPos, this.parent.Map, $"Signal recieved!", 3);
        }

        public virtual bool IsSignalTerminal()
        {
            return ConnectedChildren.Count == 0;
        }

        public virtual bool CanConnectTo(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string cantConnectReason, bool ignoreConnectionChecks = false)
        {
            cantConnectReason = string.Empty;
            if (myPort == null)
            {
                cantConnectReason = $"cannot connect to {otherNode.parent.Label} port on source node is null";
                return false;
            }

            if (otherNode == null)
            {
                cantConnectReason = $"cannot connect target node is null";
                return false;
            }

            if (otherPort == null)
            {
                cantConnectReason = $"cannot connect target node port is null";
                return false;
            }


            if (myPort.Type == SignalPortType.OUT && otherPort.Type != SignalPortType.IN)
            {
                cantConnectReason = "Source port is an OUT port, and the target port is not an IN port.";
                return false;
            }

            if (myPort.Type == SignalPortType.IN && otherPort.Type != SignalPortType.OUT)
            {
                cantConnectReason = "Source port is an IN port, and the target port is not an OUT port.";
                return false;
            }

            if (otherNode == this)
            {
                cantConnectReason = "cant connect to self";
                return false;
            }

            if (IsAlreadyConnected(otherNode))
            {
                cantConnectReason = "target node is already connected";
                return false;
            }


            if (myPort.HasConnectTarget)
            {
                cantConnectReason = "Source node already has connection";
                return false;
            }

            if (!ignoreConnectionChecks)
            {
                if (otherPort.HasConnectTarget)
                {
                    cantConnectReason = "Target node already has connection";
                    return false;
                }
            }

            if (IsNodeDescendantOf(otherNode))
            {
                cantConnectReason = "Already connected";
                return false;
            }

            return true;
        }

        public Comp_SignalNode GetNodeAt(IntVec3 position)
        {
            if (!position.InBounds(parent.Map))
                return null;

            List<Thing> thingList = position.GetThingList(parent.Map);
            foreach (Thing thing in thingList)
            {
                Comp_SignalNode comp = thing.TryGetComp<Comp_SignalNode>();
                if (comp != null)
                    return comp;
            }
            return null;
        }


        public bool IsAlreadyConnected(Comp_SignalNode otherNode)
        {
            return ConnectionPorts.Any(x => x.ConnectedNode == otherNode);
        }

        public virtual void ConnectToParent(Comp_SignalNode Other)
        {
            if (!TryGetFreeConnectionPort(SignalPortType.IN, out SignalPort port))
            {
                Log.Message($"Cant connect to {Other.parent.Label} {this.parent.Label} has no available IN signal ports");
 
                return;
            }
            port.Connect(Other);
        }

        public virtual void DisconnectFromParent(Comp_SignalNode Other)
        {
            if (!TryGetConnectionPortForNode(Other, out SignalPort port))
            {
                Log.Message($"Cant connect to {Other.parent.Label} {this.parent.Label} has no available IN signal ports");
                //cant connect no free IN nodes
                return;
            }

            port.Disconnect();
            //var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            //signalManager?.MarkNetworksDirty();
        }

        public virtual void ConnectToChild(Comp_SignalNode Other)
        {
            if (!TryGetFreeConnectionPort(SignalPortType.OUT, out SignalPort port))
            {
                Log.Message($"Cant connect to {Other.parent.Label} {this.parent.Label} has no available OUT signal ports");
                //cant connect no free IN nodes
                return;
            }

            port.Connect(Other);
        }

        public virtual void DisconnectFromChild(Comp_SignalNode Other)
        {
            if (!TryGetConnectionPortForNode(Other, out SignalPort port))
            {
                Log.Message($"Cannot find");
                return;
            }
            port.Disconnect();
            //var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            //signalManager?.MarkNetworksDirty();
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

                Comp_SignalNode signalNode = GetNodeAt(cell);
                if (signalNode != null)
                {
                    IntVec3 directionToNode = (signalNode.parent.Position - this.parent.Position).ClampMagnitude(1);
                    foundNodes.Add(new SpatialNodeData(signalNode, directionToNode, Rot4.FromIntVec3(directionToNode)));
                    break;
                }
            }
            return foundNodes;
        }


        public bool TryGetFreeConnectionPort(SignalPortType signalPortType, out SignalPort foundPort)
        {
            foundPort = null;
            if (HasFreeConnectionPort(signalPortType))
            {
                foundPort = GetFreeConnectionPort(signalPortType);
                return true;
            }

            return false;
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
            if (ConnectionPorts.Any(x=> x.ConnectedNode == node))
            {
                foundPort = ConnectionPorts.FirstOrDefault(x=> x.ConnectedNode == node);
                return true;
            }

            return false;
        }


        public SignalPort GetFreeConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.FirstOrDefault(x => x.Type == signalPortType && !x.HasConnectTarget);
        }
        public SignalPort GetConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.FirstOrDefault(x => x.Type == signalPortType);
        }

        public bool HasFreeConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.Any(x => x.Type == signalPortType);
        }
        public bool HasConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.Any(x => x.Type == signalPortType);
        }



        public bool IsNodeDescendantOf(Comp_SignalNode target)
        {
            foreach (var child in ConnectedChildren)
            {
                if (child == target || child.IsNodeDescendantOf(target))
                    return true;
            }
            return false;
        }

        public virtual List<SignalPort> GetUnconnectedPorts(SignalPortType signalPortType)
        {
            return ConnectionPorts.Where(x => x.Type == signalPortType && x.ConnectedNode == null).ToList();
        }
        public virtual List<SignalPort> GetConnectedPorts(SignalPortType signalPortType)
        {
            return ConnectionPorts.Where(x => x.Type == signalPortType && x.ConnectedNode != null).ToList();
        }
        public List<SignalPort> GetPorts(SignalPortType signalPortType)
        {
            return ConnectionPorts.Where(x => x.Type == signalPortType).ToList();
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            if (ConnectedChildren != null && ConnectedChildren.Count > 0)
            {
                GenDraw.DrawCircleOutline(this.parent.DrawPos, 0.5f, SimpleColor.Green);

                foreach (var child in ConnectedChildren)
                {
                    if (child == null) continue;

                    var childPort = child.GetConnectedPorts(SignalPortType.IN)
                        .FirstOrDefault(p => p.ConnectedNode == this);
                    var isEnabled = childPort != null && childPort.Enabled;

                    var color = isEnabled ? SimpleColor.Green : SimpleColor.Red;

                    GenDraw.DrawCircleOutline(child.parent.DrawPos, 0.5f, color);

                    DrawSignalFlowLine(this.parent.DrawPos, child.parent.DrawPos, color);
                }
            }
        }
        public virtual bool IsSplitterNode()
        {
            return false;
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
            if (ConnectedParents != null)
            {
                foreach (var parentNode in ConnectedParents)
                {
                    if (parentNode != null)
                    {
                        baseString += $"Parent : {parentNode.parent.Label}\r\n";
                    }
                }
            }
            if (ConnectedChildren != null)
            {
                foreach (var childNode in ConnectedChildren)
                {
                    if (childNode != null)
                    {
                        baseString += $"Child : {childNode.parent.Label}\r\n";
                    }
                }
            }

            var signalManager = this.parent.Map?.GetComponent<SignalManager>();
            if (signalManager != null)
            {
               SignalNetwork network = signalManager.GetNetworkFor(this);

                if (network != null)
                {
                    baseString += $"Network {network.networkID}";
                }
            }
            return baseString.TrimEndNewlines();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref ConnectionPorts, "connectionPorts", LookMode.Deep);
        }
    }


}
