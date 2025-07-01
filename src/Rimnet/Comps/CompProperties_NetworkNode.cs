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
        public List<string> moduleTypes = new List<string>();

        public CompProperties_NetworkNode()
        {
            compClass = typeof(Comp_NetworkNode);
        }
    }

    public class Comp_NetworkNode : ThingComp, ILoadReferenceable
    {
        public CompProperties_NetworkNode Props => (CompProperties_NetworkNode)props;
        private float range = 50f;
        protected MapComp_NetworkManager NetworkRouter => MapComp_NetworkManager.GetNetworkManager(this.parent.Map);
        protected RimNet _ConnectedNetwork = null;

        private List<Type> allowedModuleTypes = new List<Type>();
        private List<NetworkUIModule> cachedModules = null;

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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (String.IsNullOrEmpty(nodeID))
            {
                nodeID = NetworkRouter.GetUniqueNodeID();
            }

            SetupModules();
            TryJoinNetwork();
        }

        public override void PostDeSpawn(Map previousMap)
        {
            base.PostDeSpawn(previousMap);
            NetworkConnectionMaker.DisconnectFromNetwork(this);
        }

        public override void CompTickLong()
        {
            base.CompTickLong();

            var currentServer = NetworkConnectionMaker.FindServerViaCables(this);

            if (currentServer == null && ConnectedNetwork != null)
            {
                TryJoinNetwork();
            }
            else if (currentServer != null && ConnectedNetwork != currentServer.HostedNetwork)
            {
                TryJoinNetwork();
            }
            else if (currentServer != null && ConnectedNetwork == null)
            {
                TryJoinNetwork();
            }
        }

        protected virtual void TryJoinNetwork()
        {
            var foundServer = NetworkConnectionMaker.FindServerViaCables(this);

            if (foundServer != null)
            {
                if (ConnectedNetwork != foundServer.HostedNetwork)
                {
                    if (ConnectedNetwork != null)
                    {
                        ConnectedNetwork.UnregisterNode(this);
                    }
                    foundServer.ConnectNode(this);
                }
            }
            else if (ConnectedNetwork != null)
            {
                ConnectedNetwork.UnregisterNode(this);
            }
        }

        public void JoinNetwork(RimNet network)
        {
            _ConnectedNetwork = network;
            Log.Message($"{this.parent.Label} joined {this._ConnectedNetwork.ID} network.");
        }

        public void LeaveNetwork()
        {
            _ConnectedNetwork = null;
        }

        public bool CanTransmit()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && IsConnectedToServer(out _);
        }

        public bool CanReceive()
        {
            var power = parent.TryGetComp<CompPowerTrader>();
            return (power == null || power.PowerOn) && IsConnectedToServer(out _);
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
            cachedModules = null;
        }

        public bool CanConnect(out string failReason)
        {
            failReason = string.Empty;

            if (!IsConnectedToServer(out _))
            {
                failReason = "No server connection";
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

        public virtual bool IsConnectedToServer(out Comp_NetworkServer foundServer)
        {
            foundServer = NetworkConnectionMaker.FindServerViaCables(this);
            return foundServer != null;
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "force connect",
                defaultDesc = "force connect",
                action = () =>
                {
                    NetworkConnectionMaker.TryConnectToAnyNetwork(this);
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Debug Network",
                defaultDesc = "Show network debug info",
                action = () =>
                {
                    var server = NetworkConnectionMaker.FindServerViaCables(this);
                    Log.Message($"Node {this.NodeID}:");
                    Log.Message($"  - Connected to network: {ConnectedNetwork?.ID ?? "None"}");
                    Log.Message($"  - Server found: {server?.NodeID ?? "None"}");
                    if (server?.HostedNetwork != null)
                    {
                        Log.Message($"  - Server's network: {server.HostedNetwork.ID}");
                        Log.Message($"  - Nodes in server's network: {server.HostedNetwork.NetworkNodes.Count}");
                        Log.Message($"  - Is this node in server's network: {server.HostedNetwork.NetworkNodes.Contains(this)}");
                    }
                }
            };
        }

        public override string CompInspectStringExtra()
        {
            string serverStatus = IsConnectedToServer(out var server) ? $"Server: {server.NodeID}" : "No connection";
            return base.CompInspectStringExtra() + $"{serverStatus}";
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
    }
}