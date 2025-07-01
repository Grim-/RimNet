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

        private Vector2 NodeBoxSize = new Vector2(140f, 25f);
        private const float CanvasMargin = 40f;

        private const int MessageFadeTicks = 120;
        private const int StatusAnimationTicks = 60;
        private const int PulseAnimationTicks = 30;

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
            drawShadow = false;
            doWindowBackground = false;
            lastUpdateTick = Find.TickManager.TicksGame;

            SubscribeToNetworkEvents();
        }

        public override void DoWindowContents(Rect inRect)
        {
            ExpanseUI.DrawBackground(inRect, ExpanseUI.DarkGray);
            ExpanseUI.DrawAngularBorder(inRect, ExpanseUI.Blue, 4f, 2f);

            if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
            {
                ExpanseUI.BeginExpanseStyle();
                GUI.color = ExpanseUI.TextColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(inRect, "NO NETWORK OR NODES FOUND");
                Text.Anchor = TextAnchor.UpperLeft;
                ExpanseUI.EndExpanseStyle();
                return;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape || Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                this.Close();
                Event.current.Use();
                return;
            }

            Rect workingRect = inRect.Inset(8f);
            Dictionary<Comp_NetworkNode, NodeVisual> nodeVisuals = CalculateNodePositions(workingRect);

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
            Dictionary<Comp_NetworkNode, NodeVisual> visuals = CalculateNodePositions(windowRect.Inset(8f));
            if (visuals.ContainsKey(e.Sender) && visuals.ContainsKey(e.Receiver))
            {
                RecentMessages.Add(new NetworkMessageCache(
                    visuals[e.Sender].Center,
                    visuals[e.Receiver].Center,
                    Find.TickManager.TicksGame,
                    ExpanseUI.Blue
                ));
                PulseAnimations.Add(new NodePulseAnimation(e.Sender, Find.TickManager.TicksGame, ExpanseUI.Blue));
            }
        }

        private void OnNetworkMessageReceived(object sender, NetworkMessageEventArgs e)
        {
            Dictionary<Comp_NetworkNode, NodeVisual> visuals = CalculateNodePositions(windowRect.Inset(8f));
            if (visuals.ContainsKey(e.Sender) && visuals.ContainsKey(e.Receiver))
            {
                RecentMessages.Add(new NetworkMessageCache(
                    visuals[e.Sender].Center,
                    visuals[e.Receiver].Center,
                    Find.TickManager.TicksGame,
                    ExpanseUI.Orange
                ));
                PulseAnimations.Add(new NodePulseAnimation(e.Receiver, Find.TickManager.TicksGame, ExpanseUI.Orange));
            }
        }

        private void OnNetworkNodeStatusChanged(object sender, NetworkStatusEventArgs e)
        {
            if (TargetNetwork.NetworkNodes.Contains(e.Node))
            {
                StatusAnimations.Add(new NodeStatusAnimation(e.Node, Find.TickManager.TicksGame, e.IsOnline));
                if (!e.IsOnline)
                {
                    PulseAnimations.Add(new NodePulseAnimation(e.Node, Find.TickManager.TicksGame, ExpanseUI.Red));
                }
            }
        }

        private bool IsServerNode(Comp_NetworkNode node)
        {
            return node.parent.TryGetComp<Comp_NetworkServer>() != null;
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
                float radius = IsServerNode(node) ? 50f : 100f + 35f * Mathf.Sqrt(i);
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
                    Rect = nodeRect,
                    IsServer = IsServerNode(node)
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

            var servers = TargetNetwork.NetworkNodes.Where(IsServerNode).ToList();
            var clients = TargetNetwork.NetworkNodes.Where(n => !IsServerNode(n)).ToList();

            foreach (Comp_NetworkNode client in clients)
            {
                if (!visuals.TryGetValue(client, out NodeVisual clientVisual))
                    continue;

                foreach (Comp_NetworkNode server in servers)
                {
                    if (!visuals.TryGetValue(server, out NodeVisual serverVisual))
                        continue;

                    bool clientIsConnectedToTargetNet = (client.ConnectedNetwork == TargetNetwork);
                    bool serverIsConnectedToTargetNet = (server.ConnectedNetwork == TargetNetwork);

                    string clientFailReason, serverFailReason;
                    bool clientIsOperational = client.CanConnect(out clientFailReason);
                    bool serverIsOperational = server.CanConnect(out serverFailReason);

                    Color connectionColor = ExpanseUI.Red;

                    if (clientIsConnectedToTargetNet && serverIsConnectedToTargetNet)
                    {
                        if (clientIsOperational && serverIsOperational)
                        {
                            connectionColor = ExpanseUI.Green;
                        }
                        else
                        {
                            connectionColor = ExpanseUI.Orange;
                        }
                    }

                    DrawDualChannelConnection(clientVisual.Center, serverVisual.Center, connectionColor);
                }
            }
        }

        private void DrawDualChannelConnection(Vector2 start, Vector2 end, Color baseColor)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float channelOffset = 2f;

            Vector2 sendStart = start + perpendicular * channelOffset;
            Vector2 sendEnd = end + perpendicular * channelOffset;
            Vector2 receiveStart = start - perpendicular * channelOffset;
            Vector2 receiveEnd = end - perpendicular * channelOffset;

            Color sendColor = Color.Lerp(baseColor, ExpanseUI.Blue, 0.7f);
            Color receiveColor = Color.Lerp(baseColor, ExpanseUI.Orange, 0.7f);

            Widgets.DrawLine(sendStart, sendEnd, sendColor, 1.5f);
            Widgets.DrawLine(receiveStart, receiveEnd, receiveColor, 1.5f);
        }

        private void DrawNodes(Dictionary<Comp_NetworkNode, NodeVisual> visuals)
        {
            int currentTick = Find.TickManager.TicksGame;

            foreach ((Comp_NetworkNode node, NodeVisual visual) in visuals)
            {
                Color baseColor = node.IsConnectedToServer(out Comp_NetworkServer _) ? ExpanseUI.Green : ExpanseUI.Red;
                Color currentBgColor = ExpanseUI.DarkGray;
                float currentScale = 1f;

                if (visual.IsServer)
                {
                    currentBgColor = new Color(ExpanseUI.Blue.r * 0.3f, ExpanseUI.Blue.g * 0.3f, ExpanseUI.Blue.b * 0.8f, 0.4f);
                    baseColor = new Color(baseColor.r * 1.2f, baseColor.g * 1.2f, baseColor.b * 0.8f, baseColor.a);
                }

                var statusAnim = StatusAnimations.FirstOrDefault(a => a.Node == node);
                if (statusAnim.Node != null)
                {
                    int ticksElapsed = currentTick - statusAnim.StartTick;
                    float progress = Mathf.Clamp01((float)ticksElapsed / StatusAnimationTicks);

                    if (statusAnim.GoingOnline)
                    {
                        baseColor = Color.Lerp(ExpanseUI.Red, ExpanseUI.Green, progress);
                        currentScale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
                    }
                    else
                    {
                        baseColor = Color.Lerp(ExpanseUI.Green, ExpanseUI.Red, progress);
                        currentScale = 1f - Mathf.Sin(progress * Mathf.PI) * 0.1f;
                    }
                }

                var pulseAnim = PulseAnimations.FirstOrDefault(a => a.Node == node);
                if (pulseAnim.Node != null)
                {
                    int ticksElapsed = currentTick - pulseAnim.StartTick;
                    float progress = Mathf.Clamp01((float)ticksElapsed / PulseAnimationTicks);

                    float pulse = Mathf.Sin(progress * Mathf.PI);
                    currentBgColor = Color.Lerp(ExpanseUI.DarkGray, pulseAnim.PulseColor * 0.3f, pulse);
                    currentScale *= 1f + pulse * 0.15f;
                }

                Rect animatedRect = visual.Rect;
                if (currentScale != 1f)
                {
                    Vector2 sizeDiff = NodeBoxSize * (currentScale - 1f);
                    animatedRect = new Rect(animatedRect);
                    animatedRect.width = animatedRect.width + sizeDiff.x;
                    animatedRect.height = animatedRect.height + sizeDiff.y;
                }

                if (Mouse.IsOver(animatedRect))
                {
                    GUI.color = new Color(ExpanseUI.Blue.r, ExpanseUI.Blue.g, ExpanseUI.Blue.b, 0.2f);
                    GUI.DrawTexture(animatedRect, BaseContent.WhiteTex);
                    GUI.color = Color.white;
                }

                GUI.color = currentBgColor;
                GUI.DrawTexture(animatedRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                //ExpanseUI.DrawAngularBorder(animatedRect, baseColor, 3f, 2f);

                Rect innerRect = animatedRect.ContractedBy(4);
                ExpanseUI.DrawCutAngularPanel(animatedRect, baseColor, 5f);

                if (Widgets.ButtonInvisible(animatedRect))
                {
                    Find.WindowStack.Add(new Window_NetworkControlPanel(node));
                }

                Rect iconRect = animatedRect.ContractedBy(8);
                //Widgets.ThingIcon(iconRect, node.parent);
                GUI.color = Color.white;
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(animatedRect, $"{node.parent.LabelCap}");
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                string nodeType = visual.IsServer ? "SERVER" : "CLIENT";
                string connectionStatus = node.IsConnectedToServer(out Comp_NetworkServer _) ? "CONNECTED" : "DISCONNECTED";

                ExpanseUI.BeginExpanseStyle();
                string tooltipText = $"{nodeType}: {node.parent.LabelShort.ToUpper()}\nSTATUS: {connectionStatus}";
                TooltipHandler.TipRegion(animatedRect, tooltipText);
                ExpanseUI.EndExpanseStyle();
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
            public bool IsServer;
        }
    }
}