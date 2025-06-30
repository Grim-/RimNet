using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public class Window_NetworkView : Window
    {
        public override Vector2 InitialSize => new Vector2(800, 800);

        private readonly Comp_NetworkNode NetworkNode;
        public RimNet TargetNetwork { get; private set; }

        private readonly List<NetworkMessageCache> RecentMessages = new List<NetworkMessageCache>();
        private readonly List<NodeStatusAnimation> StatusAnimations = new List<NodeStatusAnimation>();
        private readonly List<NodePulseAnimation> PulseAnimations = new List<NodePulseAnimation>();

        private Vector2 NodeBoxSize = new Vector2(60f, 60f);
        private const float CanvasMargin = 40f;

        private const int MessageFadeTicks = 120;
        private const int StatusAnimationTicks = 60;
        private const int PulseAnimationTicks = 30;

        private Color NodeBackground = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        private Color ActiveConnection = new Color(0, 0.6f, 0, 0.5f);
        private Color NoActiveConnection = new Color(0.6f, 0, 0, 0.5f);
        private Color ConnectionWarning = new Color(0.6f, 0.4f, 0, 0.5f);
        private Color MessageSentColor = new Color(0.3f, 0.8f, 1f, 1f);
        private Color MessageReceivedColor = new Color(1f, 0.8f, 0.3f, 1f);

        private int lastUpdateTick;

        public Window_NetworkView(RimNet targetNetwork, Comp_NetworkNode networkNode)
        {
            TargetNetwork = targetNetwork;
            NetworkNode = networkNode;
            draggable = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            preventCameraMotion = false;
            absorbInputAroundWindow = true;
            lastUpdateTick = Find.TickManager.TicksGame;

            SubscribeToNetworkEvents();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
            {
                Widgets.Label(inRect, "No network or nodes found.");
                return;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Mouse1 && Event.current.type == EventType.MouseDown)
            {
                this.Close();
                Event.current.Use();
                return;
            }


            Dictionary<Comp_NetworkNode, NodeVisual> nodeVisuals = CalculateNodePositions(inRect);

            DrawConnections(nodeVisuals);
            DrawNodes(nodeVisuals);
            DrawMessageFlow();

            CleanupOldAnimations();
        }

        private void SubscribeToNetworkEvents()
        {
            if (TargetNetwork != null)
            {
                TargetNetwork.NetworkMessageSent += OnNetworkMessageSent;
                TargetNetwork.NetworkMessageReceived += OnNetworkMessageReceived;
                TargetNetwork.NodeStatusChanged += OnNetworkNodeStatusChanged;
            }
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (TargetNetwork != null)
            {
                TargetNetwork.NetworkMessageSent -= OnNetworkMessageSent;
                TargetNetwork.NetworkMessageReceived -= OnNetworkMessageReceived;
                TargetNetwork.NodeStatusChanged -= OnNetworkNodeStatusChanged;
            }
        }

        public override void PreClose()
        {
            base.PreClose();
            UnsubscribeFromNetworkEvents();
        }

        private void OnNetworkMessageSent(object sender, NetworkMessageEventArgs e)
        {
            Dictionary<Comp_NetworkNode, NodeVisual> visuals = CalculateNodePositions(windowRect);
            if (visuals.ContainsKey(e.Sender) && visuals.ContainsKey(e.Receiver))
            {
                RecentMessages.Add(new NetworkMessageCache(
                    visuals[e.Sender].Center,
                    visuals[e.Receiver].Center,
                    Find.TickManager.TicksGame,
                    MessageSentColor
                ));
                PulseAnimations.Add(new NodePulseAnimation(e.Sender, Find.TickManager.TicksGame, MessageSentColor));
            }
        }

        private void OnNetworkMessageReceived(object sender, NetworkMessageEventArgs e)
        {
            Dictionary<Comp_NetworkNode, NodeVisual> visuals = CalculateNodePositions(windowRect);
            if (visuals.ContainsKey(e.Sender) && visuals.ContainsKey(e.Receiver))
            {
                RecentMessages.Add(new NetworkMessageCache(
                    visuals[e.Sender].Center,
                    visuals[e.Receiver].Center,
                    Find.TickManager.TicksGame,
                    MessageReceivedColor
                ));
                PulseAnimations.Add(new NodePulseAnimation(e.Receiver, Find.TickManager.TicksGame, MessageReceivedColor));
            }
        }

        private void OnNetworkNodeStatusChanged(object sender, NetworkStatusEventArgs e)
        {
            if (TargetNetwork.NetworkNodes.Contains(e.Node))
            {
                StatusAnimations.Add(new NodeStatusAnimation(e.Node, Find.TickManager.TicksGame, e.IsOnline));
                if (!e.IsOnline)
                {
                    PulseAnimations.Add(new NodePulseAnimation(e.Node, Find.TickManager.TicksGame, Color.red));
                }
            }
        }

        private Dictionary<Comp_NetworkNode, NodeVisual> CalculateNodePositions(Rect inRect)
        {
            Dictionary<Comp_NetworkNode, NodeVisual> nodeVisuals = new Dictionary<Comp_NetworkNode, NodeVisual>();
            Rect canvas = inRect.ContractedBy(CanvasMargin);

            Vector2 center = canvas.center;
            float angleStep = 137.5f * Mathf.Deg2Rad;

            for (int i = 0; i < TargetNetwork.NetworkNodes.Count; i++)
            {
                Comp_NetworkNode node = TargetNetwork.NetworkNodes[i];

                float angle = i * angleStep;
                float radius = 100f + 35f * Mathf.Sqrt(i);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                Vector2 nodeCenter = center + offset;
                Rect nodeRect = new Rect(
                    nodeCenter.x - NodeBoxSize.x / 2,
                    nodeCenter.y - NodeBoxSize.y / 2,
                    NodeBoxSize.x,
                    NodeBoxSize.y
                );

                nodeVisuals[node] = new NodeVisual
                {
                    Center = nodeCenter,
                    Rect = nodeRect
                };
            }

            return nodeVisuals;
        }

        private void DrawConnections(Dictionary<Comp_NetworkNode, NodeVisual> visuals)
        {
            if (TargetNetwork?.NetworkNodes == null)
            {
                return;
            }

            for (int i = 0; i < TargetNetwork.NetworkNodes.Count; i++)
            {
                Comp_NetworkNode nodeA = TargetNetwork.NetworkNodes[i];
                if (!visuals.TryGetValue(nodeA, out NodeVisual visualA)) 
                    continue;

                for (int j = i + 1; j < TargetNetwork.NetworkNodes.Count; j++)
                {
                    Comp_NetworkNode nodeB = TargetNetwork.NetworkNodes[j];
                    if (!visuals.TryGetValue(nodeB, out NodeVisual visualB)) 
                        continue;

                    Vector2 start = visualA.Center;
                    Vector2 end = visualB.Center;

                    Color connectionColor = NoActiveConnection; 

                    bool aIsConnectedToTargetNet = (nodeA.ConnectedNetwork == TargetNetwork);
                    bool bIsConnectedToTargetNet = (nodeB.ConnectedNetwork == TargetNetwork);

                    string aFailReason, bFailReason;
                    bool aIsOperational = nodeA.CanConnect(out aFailReason);
                    bool bIsOperational = nodeB.CanConnect(out bFailReason);

                    if (aIsConnectedToTargetNet && bIsConnectedToTargetNet)
                    {
                        if (aIsOperational && bIsOperational)
                        {
                            connectionColor = ActiveConnection;
                        }
                        else
                        {
                            connectionColor = ConnectionWarning; 
                        }
                    }
                    else
                    {
                        connectionColor = NoActiveConnection; 
                    }

                    // Draw the line between the nodes
                    Widgets.DrawLine(start, end, connectionColor, 1f);
                }
            }
        }

        private void DrawNodes(Dictionary<Comp_NetworkNode, NodeVisual> visuals)
        {
            int currentTick = Find.TickManager.TicksGame;

            foreach ((Comp_NetworkNode node, NodeVisual visual) in visuals)
            {
                Color baseColor = node.IsConnectedToAnyNetwork ? ActiveConnection : NoActiveConnection;
                Color currentBgColor = NodeBackground;
                float currentScale = 1f;

                var statusAnim = StatusAnimations.FirstOrDefault(a => a.Node == node);
                if (statusAnim.Node != null)
                {
                    int ticksElapsed = currentTick - statusAnim.StartTick;
                    float progress = Mathf.Clamp01((float)ticksElapsed / StatusAnimationTicks);

                    if (statusAnim.GoingOnline)
                    {
                        baseColor = Color.Lerp(NoActiveConnection, ActiveConnection, progress);
                        currentScale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
                    }
                    else
                    {
                        baseColor = Color.Lerp(ActiveConnection, NoActiveConnection, progress);
                        currentScale = 1f - Mathf.Sin(progress * Mathf.PI) * 0.1f;
                    }
                }

                var pulseAnim = PulseAnimations.FirstOrDefault(a => a.Node == node);
                if (pulseAnim.Node != null)
                {
                    int ticksElapsed = currentTick - pulseAnim.StartTick;
                    float progress = Mathf.Clamp01((float)ticksElapsed / PulseAnimationTicks);

                    float pulse = Mathf.Sin(progress * Mathf.PI);
                    currentBgColor = Color.Lerp(NodeBackground, pulseAnim.PulseColor * 0.3f, pulse);
                    currentScale *= 1f + pulse * 0.15f;
                }

                Rect animatedRect = visual.Rect;
                //if (currentScale != 1f)
                //{
                //    float sizeDiff = NodeBoxSize * (currentScale - 1f);
                //    animatedRect = animatedRect.ExpandedBy(sizeDiff / 2f);
                //}

                if (Mouse.IsOver(animatedRect))
                {
                    Widgets.DrawHighlight(animatedRect);
                }

                Widgets.DrawBoxSolidWithOutline(animatedRect, currentBgColor, baseColor);

                Rect innerRect = animatedRect.ContractedBy(2);
                Widgets.DrawBoxSolidWithOutline(innerRect, Color.clear, Color.white);

                if (Widgets.ButtonInvisible(animatedRect))
                {
                    Find.WindowStack.Add(new Window_NetworkControlPanel(node));
                }

                Rect iconRect = animatedRect.ContractedBy(4);
                Widgets.ThingIcon(iconRect, node.parent);
                TooltipHandler.TipRegion(animatedRect,
                    $"Node: {node.parent.LabelShort}\nStatus: {(node.IsConnectedToAnyNetwork ? "Connected" : "Disconnected")}");
            }
        }

        private void DrawMessageFlow()
        {
            int currentTick = Find.TickManager.TicksGame;

            foreach (NetworkMessageCache msg in RecentMessages)
            {
                int ticksElapsed = currentTick - msg.StartTick;
                float progress = Mathf.Clamp01((float)ticksElapsed / MessageFadeTicks);

                float alpha = 1f - progress;
                Color fadeColor = msg.MessageColor * new Color(1f, 1f, 1f, alpha);

                float moveProgress = Mathf.Clamp01(progress * 2f);
                Vector2 currentPos = Vector2.Lerp(msg.Start, msg.End, moveProgress);
                Vector2 trailStart = Vector2.Lerp(msg.Start, currentPos, Mathf.Max(0, moveProgress - 0.3f));

                Widgets.DrawLine(trailStart, currentPos, fadeColor, 3f);

                if (moveProgress < 1f)
                {
                    GUI.color = fadeColor;
                    Widgets.DrawTextureFitted(
                        new Rect(currentPos.x - 4, currentPos.y - 4, 8, 8),
                        BaseContent.WhiteTex,
                        1f
                    );
                    GUI.color = Color.white;
                }
            }
        }

        private void CleanupOldAnimations()
        {
            int currentTick = Find.TickManager.TicksGame;
            RecentMessages.RemoveAll(m => currentTick - m.StartTick > MessageFadeTicks);
            StatusAnimations.RemoveAll(a => currentTick - a.StartTick > StatusAnimationTicks);
            PulseAnimations.RemoveAll(a => currentTick - a.StartTick > PulseAnimationTicks);
        }

        private struct NodeVisual
        {
            public Vector2 Center;
            public Rect Rect;
        }
    }
}