using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        public List<SignalPort> ConnectionPorts = new List<SignalPort>();
        public List<Comp_SignalNode> ParentNodes = new List<Comp_SignalNode>();
        public List<Comp_SignalNode> ChildNodes = new List<Comp_SignalNode>();

        public CompProperties_SignalNode NodeProps => (CompProperties_SignalNode)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                SetupDefaultPorts();
                SignalConnectionMaker.RecursivelyConnectForward(this);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            if (ParentNodes.Count > 0)
            {
                foreach (var parent in ParentNodes.ToArray())
                {
                    DisconnectParent(parent);
                }   
            }

            if (ChildNodes.Count > 0)
            {
                foreach (var child in ChildNodes.ToArray())
                {
                    DisconnectChild(child);
                }
            }
        }

        protected virtual void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(SignalPortType.OUT, IntVec3.Zero));
            ConnectionPorts.Add(new SignalPort(SignalPortType.IN, IntVec3.Zero));
        }

        public virtual bool MakeConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string reason)
        {
            reason = string.Empty;
            ConnectChild(otherNode);
            otherNode.ConnectParent(this);

            return true;
        }

        public virtual bool VerifyConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string cantConnectReason, bool ignoreConnectionChecks = false)
        {
            cantConnectReason = string.Empty;

            if (myPort == null || otherNode == null || otherPort == null)
            {
                cantConnectReason = "self port, other node other nodes port is null";
                return false;
            }

            if (myPort.Type != SignalPortType.OUT || otherPort.Type != SignalPortType.IN)
            {
                cantConnectReason = "Incompatible port type";
                return false;
            }

            if (otherNode == this)
            {
                cantConnectReason = "cant connect to self";
                return false;
            }


            if (myPort.ConnectedNode != null)
            {
                cantConnectReason = "Source node already has connection";
                return false;
            }

            if (!ignoreConnectionChecks)
            {
                if (otherPort.ConnectedNode != null)
                {
                    cantConnectReason = "Target node already has connection";
                    return false;
                }
            }


            if (ParentNodes.Contains(otherNode))
            {
                cantConnectReason = "Target node is already a parent node";
                return false;
            }

            if (ChildNodes.Contains(otherNode))
            {
                cantConnectReason = "Target node is already a child node";
                return false;
            }

            if (this == otherNode)
            {
                cantConnectReason = "Cant connect to self";
                return false;
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

        public Comp_SignalNode GetNodeInDirection(IntVec3 direction)
        {
            IntVec3 cell = parent.Position + direction.ClampMagnitude(1);
            return GetNodeAt(cell);
        }

        public virtual void ConnectParent(Comp_SignalNode Other)
        {
            if (!TryGetConnectionPort(SignalPortType.IN, out SignalPort port))
            {
                Log.Message($"Cant connect to {Other.parent.Label} {this.parent.Label} has no available IN signal ports");
                //cant connect no free IN nodes
                return;
            }

            if (!ParentNodes.Contains(Other))
            {
                ParentNodes.Add(Other);
                port.Connect(Other);
            }
        }
        public virtual void DisconnectParent(Comp_SignalNode Other)
        {
            if (ParentNodes.Contains(Other))
            {
                ParentNodes.Remove(Other);
                Other.DisconnectChild(this);
            }
        }

        public virtual void ConnectChild(Comp_SignalNode Other)
        {
            if (!TryGetConnectionPort(SignalPortType.OUT, out SignalPort port))
            {
                Log.Message($"Cant connect to {Other.parent.Label} {this.parent.Label} has no available OUT signal ports");
                //cant connect no free IN nodes
                return;
            }
            if (!ChildNodes.Contains(Other))
            {
                ChildNodes.Add(Other);
                port.Connect(Other);
            }
        }

        public bool TryGetConnectionPort(SignalPortType signalPortType, out SignalPort foundPort)
        {
            foundPort = null;
            if (HasFreeConnectionPort(signalPortType))
            {
                foundPort = GetConnectionPort(signalPortType);
                return true;
            }

            return false;
        }

        public SignalPort GetConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.FirstOrDefault(x => x.Type == signalPortType);
        }

        public bool HasFreeConnectionPort(SignalPortType signalPortType)
        {
            return ConnectionPorts.Any(x => x.Type == signalPortType);
        }
        public virtual void DisconnectChild(Comp_SignalNode Other)
        {
            if (ChildNodes.Contains(Other))
            {
                ChildNodes.Remove(Other);
                Other.DisconnectParent(this);
            }
        }

        public bool IsNodeDescendantOf(Comp_SignalNode target)
        {
            foreach (var child in ChildNodes)
            {
                if (child == target || child.IsNodeDescendantOf(target))
                    return true;
            }
            return false;
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

            Log.Message($"-- {this.parent.Label} recieved signal");
            OnSignalRecieved(signal);

            foreach (var child in ChildNodes)
            {
                Log.Message($"    -- >> {child.parent.Label}");
                child.Propagate(signal, visited);
            }
        }

        public virtual void OnSignalRecieved(Signal signal)
        {
            MoteMaker.ThrowText(this.parent.DrawPos, this.parent.Map, $"Signal recieved!", 3);
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

            if (ChildNodes != null)
            {
                GenDraw.DrawCircleOutline(this.parent.DrawPos, 0.5f, SimpleColor.Green);
                foreach (var c in ChildNodes)
                {
                    GenDraw.DrawCircleOutline(c.parent.DrawPos, 0.5f, SimpleColor.Red);
                    GenDraw.DrawLineBetween(this.parent.DrawPos, c.parent.DrawPos);
                }
            }
        }



        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();

            if (ParentNodes != null)
            {
                foreach (var parentNode in ParentNodes)
                {
                    if (parentNode != null)
                    {
                        baseString += $"Parent : {parentNode.parent.Label}\r\n";
                    }              
                }
            }
            if (ChildNodes != null)
            {
                foreach (var childNode in ChildNodes)
                {
                    if (childNode != null)
                    {
                        baseString += $"Child : {childNode.parent.Label}\r\n";
                    }
                }
            }
            return baseString.TrimEndNewlines();
        }


        protected List<ThingWithComps> ParentThings = new List<ThingWithComps>();
        protected List<ThingWithComps> ChildThings = new List<ThingWithComps>();

        public override void PostExposeData()
        {
            base.PostExposeData();


            ParentThings = ParentNodes.Select(x => x.parent).ToList();
            ChildThings = ChildNodes.Select(x => x.parent).ToList();


            Scribe_Collections.Look(ref ParentThings, "parentThings", LookMode.Reference);
            Scribe_Collections.Look(ref ChildThings, "childThings", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (var p in ParentThings)
                {
                    Comp_SignalNode _SignalNode = p.TryGetComp<Comp_SignalNode>();
                    if (_SignalNode != null)
                    {
                        ConnectParent(_SignalNode);
                    }
                }

                foreach (var c in ChildThings)
                {
                    Comp_SignalNode _SignalNode = c.TryGetComp<Comp_SignalNode>();
                    if (_SignalNode != null)
                    {
                        ConnectChild(_SignalNode);
                    }
                }    
            }

            Scribe_Collections.Look(ref ConnectionPorts, "connectionPorts", LookMode.Deep);
        }
    }
}
