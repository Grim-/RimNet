using System.Collections.Generic;
using UnityEngine;

namespace RimNet
{
    public static class ExpanseAnimations
    {
        private static Dictionary<string, AnimationState> animations = new Dictionary<string, AnimationState>();
        private static float lastCleanupTime = 0f;
        private const float CLEANUP_INTERVAL = 30f;

        private class AnimationState
        {
            public float value;
            public float target;
            public float speed;
            public AnimationType type;
            public float lastAccessTime;
            public float phaseOffset;
        }

        public enum AnimationType
        {
            Linear,
            EaseOut,
            EaseInOut,
            Spring,
            Pulse,
            PingPong
        }

        public static float Hover(string key, bool isHovering, float speed = 8f, float maxValue = 1f)
        {
            var state = GetOrCreateState(key, AnimationType.EaseOut);
            state.target = isHovering ? maxValue : 0f;
            state.speed = speed;
            UpdateAnimation(state);
            return state.value;
        }

        public static float Pulse(string key, float speed = 2f, float minValue = 0f, float maxValue = 1f)
        {
            var state = GetOrCreateState(key, AnimationType.Pulse);
            state.speed = speed;
            float pulse = (Mathf.Sin(Time.realtimeSinceStartup * speed + state.phaseOffset) + 1f) * 0.5f;
            state.value = Mathf.Lerp(minValue, maxValue, pulse);
            return state.value;
        }

        public static float Fade(string key, bool fadeIn, float speed = 10f)
        {
            var state = GetOrCreateState(key, AnimationType.Linear);
            state.target = fadeIn ? 1f : 0f;
            state.speed = speed;
            UpdateAnimation(state);
            return state.value;
        }

        public static float SmoothScroll(string key, float currentValue, float targetValue, float speed = 8f, float threshold = 0.1f)
        {
            var state = GetOrCreateState(key, AnimationType.Spring);
            state.value = currentValue;
            state.target = targetValue;
            state.speed = speed;

            float diff = state.target - state.value;
            if (Mathf.Abs(diff) > threshold)
            {
                state.value += diff * Time.deltaTime * speed;
                return state.value;
            }
            return targetValue;
        }

        public static float Scale(string key, bool scaleUp, float baseScale = 1f, float maxScale = 1.05f, float speed = 8f)
        {
            float hoverProgress = Hover(key, scaleUp, speed);
            return Mathf.Lerp(baseScale, maxScale, hoverProgress);
        }

        public static Color GlowColor(string key, Color baseColor, bool active, float maxAlpha = 0.3f, float speed = 8f)
        {
            float progress = Hover(key, active, speed);
            return new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * progress * maxAlpha);
        }

        public static Color PulseColor(string key, Color color1, Color color2, float speed = 2f)
        {
            float pulse = Pulse(key, speed);
            return Color.Lerp(color1, color2, pulse);
        }

        public static float Typewriter(string key, float totalDuration, int maxChars)
        {
            var state = GetOrCreateState(key, AnimationType.Linear);
            state.value = Mathf.Min(state.value + Time.deltaTime, totalDuration);
            int visibleChars = Mathf.FloorToInt((state.value / totalDuration) * maxChars);
            return visibleChars;
        }

        public static float StaggeredFade(string key, int index, float delayPerItem = 0.05f, float fadeSpeed = 2f)
        {
            var state = GetOrCreateState(key + "_" + index, AnimationType.EaseOut);
            float startTime = index * delayPerItem;
            float elapsed = Time.realtimeSinceStartup - state.phaseOffset;

            if (elapsed < startTime)
                return 0f;

            float fadeProgress = Mathf.Clamp01((elapsed - startTime) * fadeSpeed);
            state.value = fadeProgress;
            return state.value;
        }

        public static bool IsAnimating(string key)
        {
            if (!animations.ContainsKey(key))
                return false;

            var state = animations[key];
            return Mathf.Abs(state.value - state.target) > 0.01f || state.type == AnimationType.Pulse;
        }

        public static void Reset(string key)
        {
            if (animations.ContainsKey(key))
            {
                animations.Remove(key);
            }
        }

        public static void ResetAll()
        {
            animations.Clear();
        }

        public static void ResetGroup(string groupPrefix)
        {
            List<string> toRemove = new List<string>();
            foreach (var key in animations.Keys)
            {
                if (key.StartsWith(groupPrefix))
                    toRemove.Add(key);
            }
            foreach (var key in toRemove)
            {
                animations.Remove(key);
            }
        }

        private static AnimationState GetOrCreateState(string key, AnimationType type)
        {
            CleanupOldAnimations();

            if (!animations.ContainsKey(key))
            {
                animations[key] = new AnimationState
                {
                    value = 0f,
                    target = 0f,
                    speed = 8f,
                    type = type,
                    lastAccessTime = Time.realtimeSinceStartup,
                    phaseOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2f)
                };
            }

            var state = animations[key];
            state.lastAccessTime = Time.realtimeSinceStartup;
            return state;
        }

        private static void UpdateAnimation(AnimationState state)
        {
            float deltaTime = Time.deltaTime;

            switch (state.type)
            {
                case AnimationType.Linear:
                    float diff = state.target - state.value;
                    state.value += Mathf.Sign(diff) * state.speed * deltaTime;
                    if (Mathf.Abs(diff) < deltaTime * state.speed)
                        state.value = state.target;
                    break;

                case AnimationType.EaseOut:
                case AnimationType.Spring:
                    float springDiff = state.target - state.value;
                    state.value += springDiff * deltaTime * state.speed;
                    if (Mathf.Abs(springDiff) < 0.001f)
                        state.value = state.target;
                    break;

                case AnimationType.EaseInOut:
                    float t = state.value;
                    float easeInOutDiff = state.target - state.value;
                    float easeT = t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                    state.value = Mathf.Lerp(state.value, state.target, easeT * deltaTime * state.speed);
                    break;
            }

            state.value = Mathf.Clamp01(state.value);
        }

        private static void CleanupOldAnimations()
        {
            if (Time.realtimeSinceStartup - lastCleanupTime < CLEANUP_INTERVAL)
                return;

            lastCleanupTime = Time.realtimeSinceStartup;

            List<string> toRemove = new List<string>();
            foreach (var kvp in animations)
            {
                if (Time.realtimeSinceStartup - kvp.Value.lastAccessTime > CLEANUP_INTERVAL * 2f)
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
            {
                animations.Remove(key);
            }
        }
    }

    //[StaticConstructorOnStartup]
    //public class Window_NetworkGridView : Window
    //{
    //    private const float WINDOW_WIDTH = 900f;
    //    private const float WINDOW_HEIGHT = 700f;
    //    private const float WINDOW_MARGIN = 8f;

    //    private const float TOOLBAR_HEIGHT = 40f;
    //    private const float TOOLBAR_SPACING = 4f;
    //    private const float TOOLBAR_BUTTON_WIDTH = 80f;
    //    private const float CATEGORY_BUTTON_WIDTH = 70f;
    //    private const float SEARCH_BOX_WIDTH = 120f;

    //    private const float NODE_BUTTON_WIDTH = 270f;
    //    private const float NODE_BUTTON_HEIGHT = 80f;
    //    private const float GRID_PADDING = 8f;
    //    private const float SCROLLBAR_WIDTH = 16f;

    //    private const float STATUS_STRIP_WIDTH = 4f;
    //    private const float CONTENT_MARGIN = 8f;
    //    private const float CONTENT_VERTICAL_MARGIN = 2f;
    //    private const float TEXT_FIELD_MARGIN = 4f;
    //    private const float ANGULAR_CORNER_SIZE = 2f;
    //    private const float BORDER_THICKNESS = 4f;
    //    private const float BORDER_WIDTH = 2f;
    //    private const int MAX_CATEGORY_LABEL_LENGTH = 8;
    //    private const int MAX_VISIBLE_CATEGORIES = 4;
    //    private const float NAME_RECT_WIDTH_PERCENT = 0.6f;
    //    private const float NAME_RECT_HEIGHT_PERCENT = 0.6f;
    //    private const float TYPE_RECT_HEIGHT_PERCENT = 0.4f;
    //    private const float STATUS_RECT_WIDTH_PERCENT = 0.4f;

    //    public override Vector2 InitialSize => new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

    //    private readonly Comp_NetworkNode NetworkNode;
    //    public RimNet TargetNetwork { get; private set; }

    //    private Vector2 scrollPosition = Vector2.zero;
    //    private HashSet<NetworkFilterType> activeFilters = new HashSet<NetworkFilterType> { NetworkFilterType.All };
    //    private HashSet<DesignationCategoryDef> activeCategoryFilters = new HashSet<DesignationCategoryDef>();
    //    private string searchText = "";
    //    private List<Comp_NetworkNode> filteredNodes = new List<Comp_NetworkNode>();
    //    private List<DesignationCategoryDef> availableCategories = new List<DesignationCategoryDef>();

    //    //private Font Font;

    //    private NetworkFilterType[] logicalFilters = {
    //            NetworkFilterType.All,
    //            NetworkFilterType.Servers,
    //            NetworkFilterType.Clients,
    //            NetworkFilterType.Online,
    //            NetworkFilterType.Offline
    //        };

    //    public Window_NetworkGridView(RimNet targetNetwork, Comp_NetworkNode networkNode)
    //    {
    //        TargetNetwork = targetNetwork;
    //        NetworkNode = networkNode;
    //        draggable = true;
    //        doCloseX = true;
    //        closeOnClickedOutside = false;
    //        preventCameraMotion = false;
    //        absorbInputAroundWindow = true;
    //        drawShadow = false;
    //        doWindowBackground = false;
    //       // Font = LoadedModManager.GetMod<Mod_RimNet>().Font;
    //        RefreshFilteredNodes();
    //        RefreshAvailableCategories();
    //        SubscribeToNetworkEvents();
    //    }

    //    public override void DoWindowContents(Rect inRect)
    //    {
    //        ExpanseUI.DrawBackground(inRect, ExpanseUI.DarkGray);
    //        GUI.DrawTexture(new Rect(inRect.width / 2 - 150, inRect.height / 2 - 150, 300, 300), ExpanseUI.UIBGIcon);
    //        ExpanseUI.DrawAngularBorder(inRect, ExpanseUI.Blue, BORDER_THICKNESS, BORDER_WIDTH);

    //        if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
    //        {
    //            ExpanseUI.BeginExpanseStyle();
    //            GUI.color = ExpanseUI.TextColor;
    //            Text.Anchor = TextAnchor.MiddleCenter;


    //            ExpanseUI.DrawFontLabel(inRect, "NO NETWORK OR NODES FOUND", GameFont.Small);
    //            Text.Anchor = TextAnchor.UpperLeft;
    //            ExpanseUI.EndExpanseStyle();
    //            return;
    //        }

    //        Rect workingRect = inRect.Inset(WINDOW_MARGIN);

    //        Rect toolbarRect = new Rect(workingRect.x, workingRect.y, workingRect.width, TOOLBAR_HEIGHT);
    //        Rect contentRect = new Rect(workingRect.x, workingRect.y + TOOLBAR_HEIGHT + GRID_PADDING,
    //                                  workingRect.width, workingRect.height - TOOLBAR_HEIGHT - GRID_PADDING);

    //        DrawToolbar(toolbarRect);
    //        DrawNodeGrid(contentRect);
    //    }

    //    private void DrawToolbar(Rect toolbarRect)
    //    {
    //        float currentX = toolbarRect.x;

    //        foreach (var filter in logicalFilters)
    //        {
    //            Rect buttonRect = new Rect(currentX, toolbarRect.y, TOOLBAR_BUTTON_WIDTH, toolbarRect.height);
    //            bool isActive = activeFilters.Contains(filter);

    //            Color buttonColor = isActive ? ExpanseUI.Blue : ExpanseUI.Gray;

    //            if (ExpanseUI.DrawButton(buttonRect, filter.ToString().ToUpper(), buttonColor))
    //            {
    //                ToggleLogicalFilter(filter);
    //            }

    //            currentX += TOOLBAR_BUTTON_WIDTH + TOOLBAR_SPACING;
    //        }

    //        foreach (var category in availableCategories.Take(MAX_VISIBLE_CATEGORIES))
    //        {
    //            if (currentX + CATEGORY_BUTTON_WIDTH > toolbarRect.xMax - SEARCH_BOX_WIDTH - TOOLBAR_SPACING)
    //                break;

    //            Rect buttonRect = new Rect(currentX, toolbarRect.y, CATEGORY_BUTTON_WIDTH, toolbarRect.height);
    //            bool isActive = activeCategoryFilters.Contains(category);

    //            Color buttonColor = isActive ? ExpanseUI.Orange : ExpanseUI.Gray;
    //            string labelText = category.ToString().ToUpper();
    //            if (labelText.Length > MAX_CATEGORY_LABEL_LENGTH)
    //                labelText = labelText.Substring(0, MAX_CATEGORY_LABEL_LENGTH);

    //            if (ExpanseUI.DrawButton(buttonRect, labelText, buttonColor))
    //            {
    //                ToggleCategoryFilter(category);
    //            }

    //            currentX += CATEGORY_BUTTON_WIDTH + TOOLBAR_SPACING;
    //        }

    //        Rect searchRect = new Rect(toolbarRect.xMax - SEARCH_BOX_WIDTH, toolbarRect.y, SEARCH_BOX_WIDTH, toolbarRect.height);

    //        string newSearchText = GUI.TextField(searchRect.ContractedBy(TEXT_FIELD_MARGIN), searchText);
    //        if (newSearchText != searchText)
    //        {
    //            searchText = newSearchText;
    //            RefreshFilteredNodes();
    //        }
    //    }


    //    private Rect viewRect;

    //    private void DrawNodeGrid(Rect contentRect)
    //    {
    //        if (filteredNodes.Count == 0)
    //        {
    //            ExpanseUI.BeginExpanseStyle();
    //            GUI.color = ExpanseUI.TextColor;
    //            Text.Anchor = TextAnchor.MiddleCenter;
    //            ExpanseUI.DrawFontLabel(contentRect, "NO NODES MATCH CURRENT FILTERS", GameFont.Medium);
    //            Text.Anchor = TextAnchor.UpperLeft;
    //            ExpanseUI.EndExpanseStyle();
    //            return;
    //        }

    //        int columns = Mathf.FloorToInt((contentRect.width - GRID_PADDING) / (NODE_BUTTON_WIDTH + GRID_PADDING));
    //        if (columns < 1) columns = 1;

    //        int rows = Mathf.CeilToInt((float)filteredNodes.Count / columns);
    //        float totalHeight = rows * (NODE_BUTTON_HEIGHT + GRID_PADDING) - GRID_PADDING;

    //        viewRect = new Rect(0, 0, contentRect.width - SCROLLBAR_WIDTH, Mathf.Max(totalHeight, contentRect.height));
    //        Rect scrollRect = contentRect;




    //        scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, viewRect);

    //        for (int i = 0; i < filteredNodes.Count; i++)
    //        {
    //            int row = i / columns;
    //            int col = i % columns;

    //            float x = col * (NODE_BUTTON_WIDTH + GRID_PADDING);
    //            float y = row * (NODE_BUTTON_HEIGHT + GRID_PADDING);

    //            Rect nodeRect = new Rect(x, y, NODE_BUTTON_WIDTH, NODE_BUTTON_HEIGHT);

    //            if (nodeRect.yMax >= scrollPosition.y && nodeRect.y <= scrollPosition.y + contentRect.height)
    //            {
    //                DrawNodeButton(filteredNodes[i], nodeRect);
    //            }
    //        }

    //        GUI.EndScrollView();
    //    }

    //    private void DrawNodeButton(Comp_NetworkNode node, Rect rect)
    //    {
    //        bool isServer = IsServerNode(node);
    //        bool isConnected = node.IsConnectedToServer(out Comp_NetworkServer _);
    //        string failReason;
    //        bool isOperational = node.CanConnect(out failReason);

    //        Color bgColor = ExpanseUI.DarkGray;
    //        Color borderColor = ExpanseUI.Gray;
    //        Color statusColor = ExpanseUI.Red;

    //        if (isServer)
    //        {
    //            bgColor = new Color(ExpanseUI.Blue.r * 0.3f, ExpanseUI.Blue.g * 0.3f, ExpanseUI.Blue.b * 0.8f, 0.4f);
    //            borderColor = ExpanseUI.Blue;
    //        }

    //        if (isConnected && isOperational)
    //        {
    //            statusColor = ExpanseUI.Green;
    //        }
    //        else if (isConnected && !isOperational)
    //        {
    //            statusColor = ExpanseUI.Orange;
    //        }

    //        ExpanseUI.DrawAngularPanel(rect, bgColor, borderColor, ANGULAR_CORNER_SIZE);

    //        Rect statusStrip = new Rect(rect.x, rect.y, STATUS_STRIP_WIDTH, rect.height);
    //        GUI.color = statusColor;
    //        GUI.DrawTexture(statusStrip, BaseContent.WhiteTex);
    //        GUI.color = Color.white;

    //        Rect contentRect = new Rect(rect.x + CONTENT_MARGIN, rect.y + CONTENT_VERTICAL_MARGIN,
    //                                  rect.width - (CONTENT_MARGIN * 2), rect.height - (CONTENT_VERTICAL_MARGIN * 2));

    //        Rect nameRect = new Rect(contentRect.x, contentRect.y, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * NAME_RECT_HEIGHT_PERCENT);
    //        Rect typeRect = new Rect(contentRect.x, nameRect.yMax, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * TYPE_RECT_HEIGHT_PERCENT);
    //        Rect statusRect = new Rect(nameRect.xMax, contentRect.y, contentRect.width * STATUS_RECT_WIDTH_PERCENT, contentRect.height);

    //        ExpanseUI.BeginExpanseStyle();
    //        Text.Font = GameFont.Small;
    //        GUI.color = ExpanseUI.TextColor;
    //        Text.Anchor = TextAnchor.MiddleLeft;
    //        ExpanseUI.DrawFontLabel(nameRect, node.parent.LabelShort.ToUpper(), GameFont.Medium);

    //        Text.Font = GameFont.Tiny;
    //        GUI.color = isServer ? ExpanseUI.Blue : ExpanseUI.Gray;
    //        string nodeType = isServer ? "SERVER" : "CLIENT";
    //        //Widgets.Label(typeRect, nodeType);

    //        ExpanseUI.DrawFontLabel(typeRect, nodeType, GameFont.Medium);
    //        Text.Anchor = TextAnchor.MiddleRight;
    //        GUI.color = statusColor;
    //        string statusText = isConnected ? "ONLINE" : "OFFLINE";
    //        if (!isOperational && isConnected)
    //        {
    //            statusText = "ERROR";
    //            statusText += $"\nISSUE: {failReason}";
    //        }

    //        ExpanseUI.DrawFontLabel(statusRect, statusText, GameFont.Small);

    //        ExpanseUI.EndExpanseStyle();

    //        if (Widgets.ButtonInvisible(rect))
    //        {
    //            Find.WindowStack.Add(new Window_NetworkControlPanel(node));
    //        }

    //        string tooltipText = $"{nodeType}: {node.parent.LabelCap}\n" +
    //                           $"STATUS: {statusText}\n" +
    //                           $"NETWORK: {node.ConnectedNetwork?.ID ?? "NONE"}";

    //        TooltipHandler.TipRegion(rect, tooltipText);
    //    }

    //    private void ToggleLogicalFilter(NetworkFilterType filter)
    //    {
    //        if (filter == NetworkFilterType.All)
    //        {
    //            activeFilters.Clear();
    //            activeFilters.Add(NetworkFilterType.All);
    //        }
    //        else
    //        {
    //            activeFilters.Remove(NetworkFilterType.All);

    //            if (activeFilters.Contains(filter))
    //            {
    //                activeFilters.Remove(filter);
    //            }
    //            else
    //            {
    //                activeFilters.Add(filter);
    //            }

    //            if (activeFilters.Count == 0)
    //            {
    //                activeFilters.Add(NetworkFilterType.All);
    //            }
    //        }

    //        RefreshFilteredNodes();
    //    }

    //    private void ToggleCategoryFilter(DesignationCategoryDef category)
    //    {
    //        if (activeCategoryFilters.Contains(category))
    //        {
    //            activeCategoryFilters.Remove(category);
    //        }
    //        else
    //        {
    //            activeCategoryFilters.Add(category);
    //        }

    //        RefreshFilteredNodes();
    //    }

    //    private void RefreshAvailableCategories()
    //    {
    //        if (TargetNetwork?.NetworkNodes == null)
    //        {
    //            availableCategories.Clear();
    //            return;
    //        }

    //        availableCategories = TargetNetwork.NetworkNodes
    //            .Select(node => node.parent.def.designationCategory)
    //            .Distinct()
    //            .OrderBy(cat => cat.ToString())
    //            .ToList();
    //    }

    //    private void RefreshFilteredNodes()
    //    {
    //        if (TargetNetwork?.NetworkNodes == null)
    //        {
    //            filteredNodes.Clear();
    //            return;
    //        }

    //        filteredNodes = TargetNetwork.NetworkNodes.Where(node =>
    //        {
    //            if (!string.IsNullOrEmpty(searchText) &&
    //                !node.parent.LabelShort.ToLower().Contains(searchText.ToLower()) &&
    //                !node.NodeLabel.ToLower().Contains(searchText.ToLower()))
    //            {
    //                return false;
    //            }

    //            if (!activeFilters.Contains(NetworkFilterType.All))
    //            {
    //                bool matchesLogicalFilter = false;

    //                if (activeFilters.Contains(NetworkFilterType.Servers) && IsServerNode(node))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Clients) && !IsServerNode(node))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Online) && node.IsConnectedToServer(out _))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Offline) && !node.IsConnectedToServer(out _))
    //                    matchesLogicalFilter = true;

    //                if (!matchesLogicalFilter)
    //                    return false;
    //            }

    //            if (activeCategoryFilters.Count > 0)
    //            {
    //                if (!activeCategoryFilters.Contains(node.parent.def.designationCategory))
    //                    return false;
    //            }

    //            return true;
    //        }).ToList();

    //        filteredNodes = filteredNodes.OrderBy(n => !IsServerNode(n))
    //                                   .ThenBy(n => n.parent.LabelShort)
    //                                   .ToList();
    //    }

    //    private bool IsServerNode(Comp_NetworkNode node)
    //    {
    //        return node.parent.TryGetComp<Comp_NetworkServer>() != null;
    //    }

    //    private void SubscribeToNetworkEvents()
    //    {
    //        if (TargetNetwork != null)
    //        {
    //            TargetNetwork.NetworkMessageSent += OnNetworkEvent;
    //            TargetNetwork.NetworkMessageReceived += OnNetworkEvent;
    //            TargetNetwork.NodeStatusChanged += OnNodeStatusChanged;
    //        }
    //    }

    //    private void UnsubscribeFromNetworkEvents()
    //    {
    //        if (TargetNetwork != null)
    //        {
    //            TargetNetwork.NetworkMessageSent -= OnNetworkEvent;
    //            TargetNetwork.NetworkMessageReceived -= OnNetworkEvent;
    //            TargetNetwork.NodeStatusChanged -= OnNodeStatusChanged;
    //        }
    //    }

    //    private void OnNetworkEvent(object sender, NetworkMessageEventArgs e)
    //    {
    //        RefreshFilteredNodes();
    //    }

    //    private void OnNodeStatusChanged(object sender, NetworkStatusEventArgs e)
    //    {
    //        RefreshFilteredNodes();
    //        RefreshAvailableCategories();
    //    }

    //    public override void PreClose()
    //    {
    //        base.PreClose();
    //        UnsubscribeFromNetworkEvents();
    //    }
    //}
}