using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_NetworkNode : CompProperties
    {
        public float defaultRange = 50f;
        public List<string> moduleTypes = new List<string>(); // Can be set in XML

        public CompProperties_NetworkNode()
        {
            compClass = typeof(Comp_NetworkNode);
        }
    }

    public class Comp_NetworkNode : ThingComp, ILoadReferenceable, IExposable
    {
        public CompProperties_NetworkNode Props => (CompProperties_NetworkNode)props;
        private float range = 50f;
        protected MapComp_NetworkManager NetworkRouter => MapComp_NetworkManager.GetNetworkManager(this.parent.Map);
        public RimNet _ConnectedNetwork = null;

        // Module management
        private List<Type> allowedModuleTypes = new List<Type>();
        private List<NetworkUIModule> cachedModules = null;

        // Events
        public event EventHandler<NetworkMessageEventArgs> MessageSent;
        public event EventHandler<NetworkMessageEventArgs> MessageReceived;
        public event EventHandler<NetworkStatusEventArgs> StatusChanged;

        public RimNet ConnectedNetwork
        {
            get => _ConnectedNetwork;
        }

        public ThingWithComps ParentThing => parent;

        public float Range
        {
            get => range;
            set => range = value;
        }

        private string nodeID = string.Empty;
        public string NodeID
        {
            get => nodeID;
            set => nodeID = value;
        }

        public bool IsConnectedToAnyNetwork => ConnectedNetwork != null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (String.IsNullOrEmpty(nodeID))
            {
                nodeID = NetworkRouter.GetUniqueNodeID();
            }

            SetupModules();
            NetworkRouter.RegisterNode(this);
        }

        private void SetupModules()
        {
            allowedModuleTypes.Clear();

            if (Props.moduleTypes != null && Props.moduleTypes.Count > 0)
            {
                foreach (string typeName in Props.moduleTypes)
                {
                    Type moduleType = GenTypes.GetTypeInAnyAssembly(typeName);
                    if (moduleType != null && typeof(NetworkUIModule).IsAssignableFrom(moduleType))
                    {
                        allowedModuleTypes.Add(moduleType);
                    }
                }
            }

            cachedModules = null; // Clear cache
        }


        public bool CanConnect(out string failReason)
        {
            failReason = string.Empty;

            if (ConnectedNetwork == null)
            {
                failReason = "No network";
                return false;
            }

            if (!CanReceive())
            {
                failReason = "Cannot recieve";
                return false;
            }
            if (!CanTransmit())
            {
                failReason = "Cannot transmit";
                return false;
            }

            return true;
        }

        public List<NetworkUIModule> GetUIModules()
        {
            if (cachedModules == null)
            {
                cachedModules = new List<NetworkUIModule>();

                foreach (Type moduleType in allowedModuleTypes)
                {
                    NetworkUIModule module = NetworkUIModuleRegistry.GetModule(moduleType);
                    if (module != null)
                    {
                        cachedModules.Add(module);
                    }
                }
            }

            return cachedModules;
        }

        public bool HasModule<T>() where T : NetworkUIModule
        {
            return allowedModuleTypes.Contains(typeof(T));
        }

        public override void PostDeSpawn(Map previousMap)
        {
            base.PostDeSpawn(previousMap);
            NetworkRouter.UnregisterNode(this);
        }

        public void JoinNetwork(RimNet network)
        {
            bool wasConnected = IsConnectedToAnyNetwork;
            _ConnectedNetwork = network;

            if (!wasConnected && IsConnectedToAnyNetwork)
            {
                StatusChanged?.Invoke(this, new NetworkStatusEventArgs { Node = this, IsOnline = true });
            }
        }

        public void LeaveNetwork()
        {
            bool wasConnected = IsConnectedToAnyNetwork;
            _ConnectedNetwork = null;

            if (wasConnected && !IsConnectedToAnyNetwork)
            {
                StatusChanged?.Invoke(this, new NetworkStatusEventArgs { Node = this, IsOnline = false });
            }
        }

        public void SendMessage(string targetNodeID)
        {
            if (!CanTransmit())
                return;

            var message = new NetworkMessage(NodeID, ConnectedNetwork);

            var targetNode = ConnectedNetwork?.NetworkNodes.FirstOrDefault(n => n.NodeID == targetNodeID);
            if (targetNode != null)
            {
                MessageSent?.Invoke(this, new NetworkMessageEventArgs
                {
                    Sender = this,
                    Receiver = targetNode,
                    Message = message
                });
            }

            NetworkRouter.SendMessage(this, targetNodeID, message);
        }

        public bool CanTransmit()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && ConnectedNetwork != null;
        }

        public bool CanReceive()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && ConnectedNetwork != null;
        }

        public void OnMessageReceived(NetworkMessage message)
        {
            MoteMaker.ThrowText(this.parent.DrawPos, this.parent.Map, $"Received network message {message}");

            var senderNode = ConnectedNetwork?.NetworkNodes.FirstOrDefault(n => n.NodeID == message.SenderId);
            if (senderNode != null)
            {
                MessageReceived?.Invoke(this, new NetworkMessageEventArgs
                {
                    Sender = senderNode,
                    Receiver = this,
                    Message = message
                });
            }
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"Network status : {(IsConnectedToAnyNetwork ? $"Connected Net ID {ConnectedNetwork.ID}" : "Disconnected")}";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref _ConnectedNetwork, "connectedNetwork");
            Scribe_Values.Look(ref range, "networkRange", 50f);
            Scribe_Values.Look(ref nodeID, "nodeID");
            Scribe_Collections.Look(ref allowedModuleTypes, "allowedModuleTypes", LookMode.Undefined);
        }

        public string GetUniqueLoadID()
        {
            if (string.IsNullOrEmpty(nodeID))
            {
                nodeID = NetworkRouter?.GetUniqueNodeID() ?? "NetworkNode_" + Find.UniqueIDsManager.GetNextThingID();
            }
            return nodeID;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref _ConnectedNetwork, "connectedNetwork");
            Scribe_Values.Look(ref range, "networkRange", 50f);
            Scribe_Values.Look(ref nodeID, "nodeID");
            Scribe_Collections.Look(ref allowedModuleTypes, "allowedModuleTypes", LookMode.Undefined);
        }
    }
}